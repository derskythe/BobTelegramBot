using System;
using System.Data;
using Oracle.DataAccess.Client;

namespace Db
{
    public partial class OracleDb
    {
        public static void DeleteFacebookUser(string userId)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin facebook.delete_user(v_user_id => :v_user_id); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Varchar2, ParameterDirection.Input).Value = userId;
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

        public static void UpdateFacebookFeedback(string userId)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin facebook.update_feedback(v_user_id => :v_user_id); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Varchar2, ParameterDirection.Input).Value = userId;
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

        public static void SaveFacebookUser(string userId, string username, string phoneNumber)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin facebook.save_user(" +
                        "v_user_id => :v_user_id, " +
                        "v_username => :v_username, " +
                        "v_phone_number => :v_phone_number" +
                        "); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Varchar2, ParameterDirection.Input).Value = userId;
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

        public static void UpdateFacebookReceiveSettings(string userId, int value, string lang)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin facebook.update_receive_settings(v_user_id => :v_user_id, v_receive_settings => :v_receive_settings, v_language => :v_language); end; ";


                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Varchar2, ParameterDirection.Input).Value = userId;
                    cmd.Parameters.Add("v_receive_settings", OracleDbType.Int16, ParameterDirection.Input).Value =
                            value;
                    cmd.Parameters.Add("v_language", OracleDbType.Varchar2, ParameterDirection.Input).Value =
                            lang;
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

        public static void UpdateFacebookPlasticCardRequest(string userId)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin facebook.update_plastic_card_request(v_user_id => :v_user_id); end; ";

                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Varchar2, ParameterDirection.Input).Value = userId;
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

        public static void SaveFacebookCreditRequest(string userId)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin facebook.save_credit_request(v_user_id => :v_user_id); end; ";

                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Varchar2, ParameterDirection.Input).Value = userId;

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

        public static void SaveFacebookCredit(string userId, string clientId, DateTime birthDate)
        {
            OracleConnection connection = null;
            try
            {
                connection = GetConnection();

                const string cmdText =
                        "begin facebook.save_credit(v_user_id => :v_user_id, v_client_id => :v_client_id, v_birthdate => :v_birthdate); end; ";

                using (var cmd = new OracleCommand(cmdText, connection))
                {
                    cmd.Parameters.Add("v_user_id", OracleDbType.Varchar2, ParameterDirection.Input).Value = userId;
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
