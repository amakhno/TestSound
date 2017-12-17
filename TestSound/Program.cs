using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSound
{
    class Program
    {
        static void Main(string[] args)
        {
            TelegramMessager.InsertNewApi("440765294:AAG7iqMKrsr6vB2px37ZTTk35m0K7Z902Ag");
            TelegramMessager.IsStop = false;
            TelegramMessager.RunAsync().GetAwaiter();
            Console.ReadKey();
        }
    }
}
