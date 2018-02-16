using System;
using System.Reflection;
using BobTelegramBot;
using Db;
using NLog;

namespace BobTelegramBotConsole
{
    class Program
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        static void Main(string[] args)
        {
            Log.Info(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            OracleDb.Init("bob_smsc", "bob_smsc", "DWNEW", 1, 10, 300, 300);
            OracleDb.CheckConnection();
#if DEBUG            
            var bot = new BotService(
                                     "387574122:AAGcSL1BNU2enJhjOKSAU1tZ2-fEM6JlUko");
#else
            var bot = new BotService(
                                     "407407246:AAH74nLyEpiMISiLV6JXNByO4i8bWkHN3kI");
#endif

            Console.WriteLine(@"Press ENTER to exit");
            Console.ReadLine();

            bot.Dispose();
        }
    }
}
