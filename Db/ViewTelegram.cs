using System;
using System.Collections.Generic;
using Containers;
using Db.dsTableAdapters;
using Oracle.DataAccess.Client;

namespace Db
{
    public static partial class OracleDb
    {
        public static List<TelegramMessage> ListMessages()
        {
            OracleConnection connection = null;
            var result = new List<TelegramMessage>();

            try
            {
                connection = GetConnection();

                using (var adapter = new V_TELEGRAM_MESSAGES_ACTIVETableAdapter { Connection = connection })
                {
                    using (var table = new ds.V_TELEGRAM_MESSAGES_ACTIVEDataTable())
                    {
                        adapter.Fill(table);

                        foreach (ds.V_TELEGRAM_MESSAGES_ACTIVERow row in table.Rows)
                        {
                            result.Add(Converter.ToTelegramMessage(row));
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

        public static List<BotUser> ListTelegramUsers()
        {
            OracleConnection connection = null;
            var result = new List<BotUser>();

            try
            {
                connection = GetConnection();

                using (var adapter = new V_TELEGRAM_KNOWN_USERSTableAdapter { Connection = connection })
                {
                    using (var table = new ds.V_TELEGRAM_KNOWN_USERSDataTable())
                    {
                        adapter.Fill(table);

                        foreach (ds.V_TELEGRAM_KNOWN_USERSRow row in table.Rows)
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

        public static BotUser GetTelegramUser(String phoneNumber)
        {
            OracleConnection connection = null;

            try
            {
                connection = GetConnection();

                using (var adapter = new V_TELEGRAM_KNOWN_USERSTableAdapter { Connection = connection })
                {
                    using (var table = new ds.V_TELEGRAM_KNOWN_USERSDataTable())
                    {
                        adapter.FillByPhoneNumber(table, phoneNumber);

                        foreach (ds.V_TELEGRAM_KNOWN_USERSRow row in table.Rows)
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

        public static BotUser GetTelegramUser(long userId)
        {
            OracleConnection connection = null;

            try
            {
                connection = GetConnection();

                using (var adapter = new V_TELEGRAM_KNOWN_USERSTableAdapter { Connection = connection })
                {
                    using (var table = new ds.V_TELEGRAM_KNOWN_USERSDataTable())
                    {
                        adapter.FillByUserId(table, userId);

                        foreach (ds.V_TELEGRAM_KNOWN_USERSRow row in table.Rows)
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

        public static List<DbCreditItem> GetSavedCreditTelegram(long userId)
        {
            OracleConnection connection = null;
            var result = new List<DbCreditItem>();

            try
            {
                connection = GetConnection();

                using (var adapter = new V_TELEGRAM_SAVED_CREDITTableAdapter { Connection = connection })
                {
                    using (var table = new ds.V_TELEGRAM_SAVED_CREDITDataTable())
                    {
                        adapter.FillByUserId(table, userId);

                        foreach (ds.V_TELEGRAM_SAVED_CREDITRow row in table.Rows)
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
