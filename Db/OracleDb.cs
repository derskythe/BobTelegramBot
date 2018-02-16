using System;
using NLog;
using Oracle.DataAccess.Client;

namespace Db
{
    public static partial class OracleDb
    {
        private static string _ConnectionString;
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming

        public static void Init(
            string user,
            string pass,
            string dbase,
            int minPoolSize,
            int maxPoolSize,
            int connectionLifetime,
            int connectionTimeout)
        {
            if (_ConnectionString == null)
            {
                _ConnectionString = "User ID=" + user + ";Data Source=" + dbase + ";Password=" + pass +
                                    ";Min Pool Size=" + minPoolSize + ";Max Pool Size=" + maxPoolSize +
                                    ";Pooling=True;" +
                                    "Validate Connection=true;Connection Lifetime=" + connectionLifetime +
                                    ";Connection Timeout=" + connectionTimeout;
            }

            Log.Debug(_ConnectionString);
        }

        public static bool CheckConnection()
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();
                using (OracleCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1 FROM dual";
                    cmd.ExecuteScalar();
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return false;
        }

        private static OracleConnection GetConnection()
        {
            if (String.IsNullOrEmpty(_ConnectionString))
            {
                throw new Exception("Connection not initialized!");
            }

            var connection = new OracleConnection(_ConnectionString);
            connection.Open();

            return connection;
        }
    }
}
