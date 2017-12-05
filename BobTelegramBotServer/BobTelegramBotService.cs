using System;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using BobTelegramBot;
using BobTelegramBotServer.Properties;
using Db;
using NLog;

namespace BobTelegramBotServer
{
    public partial class BobTelegramBotService : ServiceBase
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        private ServiceHost _SoapService;
        private BotService _Bot;

        public BobTelegramBotService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                OracleDb.Init(
                              Settings.Default.OracleUsername,
                              Settings.Default.OraclePassword,
                              Settings.Default.OracleHost,
                              Settings.Default.OracleMinPoolSize,
                              Settings.Default.OracleMaxPoolSize,
                              Settings.Default.OracleConnectionLifetime,
                              Settings.Default.OracleConnectionTimeout);
                if (!OracleDb.CheckConnection())
                {
                    throw new Exception("Connection failed!");
                }

#if DEBUG
                _Bot = new BotService(
                                         "387574122:AAGcSL1BNU2enJhjOKSAU1tZ2-fEM6JlUko");
#else
                _Bot = new BotService(
                                         "407407246:AAH74nLyEpiMISiLV6JXNByO4i8bWkHN3kI");
#endif
                Log.Info("Trying to start FacebookService");
                _SoapService = new ServiceHost(typeof(FacebookSoapService.FacebookService));
                _SoapService.Open();
                Log.Info("FacebookService started");
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
                Thread.Sleep(1000);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (_Bot != null)
            {
                _Bot.Dispose();

                Thread.Sleep(2000);
            }

            if (_SoapService != null)
            {
                _SoapService.Close();
            }
        }
    }
}
