using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Taikandi.Telebot;
using Taikandi.Telebot.Types;
using ITCC.YandexSpeechKitClient;
using ITCC.YandexSpeechKitClient.Enums;
using System.Net;

namespace TestSound
{
    public static class TelegramMessager
    {
        public static string InviterName = "";
        public static string WriterName = "";

        public static void InsertNewApi(string apiKey)
        {
            _telebot = new Telebot(apiKey)
            {
                Timeout = new TimeSpan(2, 0, 0)
            };
        }

        //____________________
        private static Telebot _telebot;
        private static List<long> chatsId = new List<long>();

        public static Task Run;


        public static bool IsStop = true;
        public static bool IsWriteToChatMode = false;
        public static bool isInviterMode = false;
        public static bool IsLaunchPilotMode = false;
        public static CancellationToken Token = new CancellationToken(true);
        public static int TimeBeforeUpdate = 0;
        public static async Task RunAsync()
        {
            // Used for getting only the unconfirmed updates.
            // It is recommended to stored this value between sessions. 
            // More information at https://core.telegram.org/bots/api#getting-updates
            var offset = 0L;

            while (!IsStop)
            {
                // Use this method to receive incoming updates using long polling.
                // Or use Telebot.SetWebhook() method to specify a URL to receive incoming updates.
                TimeBeforeUpdate = 0;
                Token = new CancellationToken(false);

                try
                {
                    List<Update> updates = (await _telebot.GetUpdatesAsync(offset: offset, limit: 2, timeout: 2, cancellationToken: Token).ConfigureAwait(false)).ToList();
                    if (updates.Any())
                    {
                        offset = updates.Max(u => u.Id) + 1;

                        foreach (Update update in updates)
                        {
                            switch (update.Type)
                            {
                                case UpdateType.Message:
                                    await TelegramMessager.CheckMessagesAsync(update.Message).ConfigureAwait(false);
                                    break;
                                case UpdateType.InlineQuery:
                                    await TelegramMessager.CheckInlineQueryAsync(update).ConfigureAwait(false);
                                    break;
                                case UpdateType.ChosenInlineResult:
                                    TelegramMessager.CheckChosenInlineResult(update);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(5000);
                }
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }

        private static async Task CheckMessagesAsync(Taikandi.Telebot.Types.Message message)
        {
            try
            {

                var fileTask = _telebot.GetFileAsync(message.Voice.FileId).GetAwaiter().GetResult();
                _telebot.DownloadFileAsync(fileTask, Path.GetFullPath(@".\\voice.ogg"), overwrite: true).GetAwaiter().GetResult();
                var apiSetttings = new SpeechKitClientOptions("1774531e-6a7c-4973-878c-d84d121d9ae1", "Key #1", Guid.Empty, "pc");
                FileStream stream = new FileStream(@".\\voice.ogg", FileMode.Open);
                using (var client = new SpeechKitClient(apiSetttings))
                {
                    var speechRecognitionOptions = new SpeechRecognitionOptions(SpeechModel.Queries, RecognitionAudioFormat.Ogg, RecognitionLanguage.Russian);
                    try
                    {
                        var result = await client.SpeechToTextAsync(speechRecognitionOptions, stream, CancellationToken.None).ConfigureAwait(false);
                        if (result.TransportStatus != TransportStatus.Ok || result.StatusCode != HttpStatusCode.OK)
                        {
                            await TelegramMessager._telebot.SendMessageAsync(message.Chat.Id, "Ошибка передачи").ConfigureAwait(false);
                            return;
                        }

                        if (!result.Result.Success)
                        {
                            await TelegramMessager._telebot.SendMessageAsync(message.Chat.Id, "Ошибка распознавания").ConfigureAwait(false);
                            return;
                        }

                        var utterances = result.Result.Variants;
                        string text = utterances[0].Text;
                        text = text.First().ToString().ToUpper() + text.Substring(1);
                        await TelegramMessager._telebot.SendMessageAsync(message.Chat.Id, text).ConfigureAwait(false);
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        //Handle operation cancellation
                    }
                }

            }
            catch (Exception exc)
            {
                ;
            }
        }


        private static async Task CheckInlineQueryAsync(Update update)
        {
            // Telebot will support all 19 types of InlineQueryResult.
            // To see available inline query results:
            // https://core.telegram.org/bots/api#answerinlinequery
            var articleResult = new InlineQueryResultArticle
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = "This is a title",
                Url = "https://core.telegram.org/bots/api#inlinequeryresultarticle"
            };

            var photoResult = new InlineQueryResultPhoto
            {
                Id = Guid.NewGuid().ToString("N"),
                Url = "https://telegram.org/file/811140636/1/hzUbyxse42w/4cd52d0464b44e1e5b",
                ThumbnailUrl = "https://telegram.org/file/811140636/1/hzUbyxse42w/4cd52d0464b44e1e5b"
            };


            var gifResult = new InlineQueryResultGif
            {
                Id = Guid.NewGuid().ToString("N"),
                Url = "http://i.giphy.com/ya4eevXU490Iw.gif",
                ThumbnailUrl = "http://i.giphy.com/ya4eevXU490Iw.gif"
            };

            var results = new InlineQueryResult[] { articleResult, photoResult, gifResult };
            await TelegramMessager._telebot.AnswerInlineQueryAsync(update.InlineQuery.Id, results).ConfigureAwait(false);
        }

        private static void CheckChosenInlineResult(Update update)
        {
            Console.WriteLine("Received ChosenInlineResult.");
        }
    }
}
