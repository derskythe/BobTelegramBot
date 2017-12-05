using System;
using System.Collections.Generic;
using Containers;
using Db.dsTableAdapters;
using Oracle.DataAccess.Client;

namespace Db
{
    public partial class OracleDb
    {
        public static List<BotUser> ListFacebookUsers()
        {
            OracleConnection connection = null;
            var result = new List<BotUser>();

            try
            {
                connection = GetConnection();

                using (var adapter = new V_FACEBOOK_KNOWN_USERSTableAdapter { Connection = connection })
                {
                    using (var table = new ds.V_FACEBOOK_KNOWN_USERSDataTable())
                    {
                        adapter.Fill(table);

                        foreach (ds.V_FACEBOOK_KNOWN_USERSRow row in table.Rows)
                        {
                            result.Add(Converter.ToChatBotUser(row));
                        }
                    }
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

            return result;
        }

        public static BotUser GetFacebookUser(String number, bool seekByUserId)
        {
            OracleConnection connection = null;

            try
            {
                connection = GetConnection();

                using (var adapter = new V_FACEBOOK_KNOWN_USERSTableAdapter { Connection = connection })
                {
                    using (var table = new ds.V_FACEBOOK_KNOWN_USERSDataTable())
                    {
                        if (seekByUserId)
                        {
                            adapter.FillByUserId(table, number);
                        }
                        else
                        {
                            adapter.FillByPhoneNumber(table, number);
                        }

                        foreach (ds.V_FACEBOOK_KNOWN_USERSRow row in table.Rows)
                        {
                            return Converter.ToChatBotUser(row);
                        }
                    }
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

            return null;
        }      

        public static List<DbCreditItem> GetSavedCreditFacebook(string userId)
        {
            OracleConnection connection = null;
            var result = new List<DbCreditItem>();

            try
            {
                connection = GetConnection();

                using (var adapter = new V_FACEBOOK_SAVED_CREDITTableAdapter { Connection = connection })
                {
                    using (var table = new ds.V_FACEBOOK_SAVED_CREDITDataTable())
                    {
                        adapter.FillByUserId(table, userId);

                        foreach (ds.V_FACEBOOK_SAVED_CREDITRow row in table.Rows)
                        {
                            result.Add(Converter.ToCreditItem(row));
                        }
                    }
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

            return result;
        }
    }
}
