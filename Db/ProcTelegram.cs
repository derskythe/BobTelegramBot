using System;
using System.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Db
{
    public partial class OracleDb
    {
        public static bool CheckAuthKey(long userId, string phoneNumber, string auth)
        {
            OracleConnection connection = null;
            object result;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin " +
                        "telegram.check_auth_key(" +
                        "v_user_id => :v_user_id, " +
                        "v_phone_number => :v_phone_number, " +
                        "v_auth_key => :v_auth_key, " +
                        "v_result_code => :v_result_code" +
                        "); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {

                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;
                    cmd.Parameters.Add("v_phone_number", OracleDbType.Varchar2, ParameterDirection.Input).Value =
                            phoneNumber;
                    cmd.Parameters.Add("v_auth_key", OracleDbType.Varchar2, ParameterDirection.Input).Value = auth;
                    cmd.Parameters.Add("v_result_code", OracleDbType.Int16, ParameterDirection.Output);

                    cmd.ExecuteNonQuery();
                    result = cmd.Parameters["v_result_code"].Value;
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return result != null && ((OracleDecimal) (result)).ToInt16() > 0;
        }

        public static string GetSmsBankingSettings()
        {
            OracleConnection connection = null;
            object result;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.get_sms_banking_settings(v_settings => :v_settings); end; ";

                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    OracleParameter settings =
                            new OracleParameter("v_settings", OracleDbType.Varchar2)
                            {
                                Direction = ParameterDirection.Output,
                                Size = 100
                            };

                    cmd.Parameters.Add(settings);

                    cmd.ExecuteNonQuery();
                    result = cmd.Parameters["v_settings"].Value;

                    settings.Dispose();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return result != null ? result.ToString() : String.Empty;
        }

        public static void AddIncomingMessage(long userId, string message)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.add_incoming_message(v_user_id => :v_user_id, v_message => :v_message); end; ";

                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;
                    cmd.Parameters.Add("v_message", OracleDbType.Varchar2, ParameterDirection.Input).Value = message;

                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static void DeleteTelegramUser(long userId)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.delete_user(v_user_id => :v_user_id); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static void UpdateTelegramFeedback(long userId)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.update_feedback(v_user_id => :v_user_id); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static void SaveTelegramUser(long userId, string username, string phoneNumber)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.save_user(" +
                        "v_user_id => :v_user_id, " +
                        "v_username => :v_username, " +
                        "v_phone_number => :v_phone_number" +
                        "); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;
                    cmd.Parameters.Add("v_username", OracleDbType.Varchar2, ParameterDirection.Input).Value =
                            username;
                    cmd.Parameters.Add("v_phone_number", OracleDbType.Varchar2, ParameterDirection.Input).Value = phoneNumber;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static void UpdateMessage(long id, int value)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.update_message(v_id => :v_id, v_is_sent => :v_is_sent); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_id", OracleDbType.Int64, ParameterDirection.Input).Value = id;
                    cmd.Parameters.Add("v_is_sent", OracleDbType.Int16, ParameterDirection.Input).Value =
                            value;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static void UpdateTelegramReceiveSettings(long userId, int value)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.update_receive_settings(v_user_id => :v_user_id, v_receive_settings => :v_receive_settings); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;
                    cmd.Parameters.Add("v_receive_settings", OracleDbType.Int16, ParameterDirection.Input).Value =
                            value;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static void UpdateTelegramPlasticCardRequest(long userId)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.update_plastic_card_request(v_user_id => :v_user_id); end; ";

                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static void SaveTelegramCreditRequest(long userId)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.save_credit_request(v_user_id => :v_user_id); end; ";

                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;

                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static void SaveTelegramCredit(long userId, string clientId, DateTime birthDate)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin telegram.save_credit(v_user_id => :v_user_id, v_client_id => :v_client_id, v_birthdate => :v_birthdate); end; ";

                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Int64, ParameterDirection.Input).Value = userId;
                    cmd.Parameters.Add("v_client_id", OracleDbType.Varchar2, ParameterDirection.Input).Value = clientId;
                    cmd.Parameters.Add("v_birthdate", OracleDbType.Date, ParameterDirection.Input).Value = birthDate;

                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
    }
}
