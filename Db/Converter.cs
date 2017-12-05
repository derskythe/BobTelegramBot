using System;
using Containers;
using Containers.Enums;

namespace Db
{
    internal static class Converter
    {
        public static BotUser ToChatBotUser(ds.V_TELEGRAM_KNOWN_USERSRow row)
        {
            return new BotUser(
                                    Convert.ToString(row.USER_ID),
                                    row.IsPHONE_NUMBERNull() ? String.Empty : row.PHONE_NUMBER,
                                    row.ADD_DATE,
                                    row.UPDATE_DATE,
                                    row.RECEIVE_SETTINGS,
                                    row.IsUSERNAMENull() ? String.Empty : row.USERNAME,
                                    row.IsLAST_FEEDBACKNull() ? DateTime.MinValue : row.LAST_FEEDBACK,
                                    row.IsFEEDBACK_COUNTNull() ? 0 : Convert.ToInt32(row.FEEDBACK_COUNT),
                                    row.IsLAST_CREDIT_REQUESTNull() ? DateTime.MinValue : row.LAST_CREDIT_REQUEST,
                                    Convert.ToInt32(row.PLASTIC_CARD_COUNT),
                                    row.IsLAST_PLASTIC_CARDNull() ? DateTime.MinValue : row.LAST_PLASTIC_CARD,
                                    BotUserType.Telegram,
                                    Language.En);
        }

        public static BotUser ToChatBotUser(ds.V_FACEBOOK_KNOWN_USERSRow row)
        {
            Language lang;
            switch (row.LANGUAGE)
            {
                case "en":
                    lang = Language.En;
                    break;

                case "az":
                    lang = Language.Az;
                    break;

                case "ru":
                    lang = Language.Ru;
                    break;

                default:
                    lang = Language.En;
                    break;
            }

            return new BotUser(
                               row.USER_ID,
                               row.IsPHONE_NUMBERNull() ? String.Empty : row.PHONE_NUMBER,
                               row.ADD_DATE,
                               row.UPDATE_DATE,
                               row.RECEIVE_SETTINGS,
                               row.IsUSERNAMENull() ? String.Empty : row.USERNAME,
                               row.IsLAST_FEEDBACKNull() ? DateTime.MinValue : row.LAST_FEEDBACK,
                               row.IsFEEDBACK_COUNTNull() ? 0 : Convert.ToInt32(row.FEEDBACK_COUNT),
                               row.IsLAST_CREDIT_REQUESTNull() ? DateTime.MinValue : row.LAST_CREDIT_REQUEST,
                               Convert.ToInt32(row.PLASTIC_CARD_COUNT),
                               row.IsLAST_PLASTIC_CARDNull() ? DateTime.MinValue : row.LAST_PLASTIC_CARD,
                               BotUserType.Facebook,
                               lang);
        }

        public static TelegramMessage ToTelegramMessage(ds.V_TELEGRAM_MESSAGES_ACTIVERow row)
        {
            return new TelegramMessage(row.ID, row.USER_ID, row.MESSAGE_TEXT, row.IS_SENT > 0, row.UPDATE_DATE);
        }

        public static DbCreditItem ToCreditItem(ds.V_TELEGRAM_SAVED_CREDITRow row)
        {
            return new DbCreditItem(row.ID, Convert.ToString(row.USER_ID), row.CLIENT_ID, row.BIRTHDATE);
        }

        public static DbCreditItem ToCreditItem(ds.V_FACEBOOK_SAVED_CREDITRow row)
        {
            return new DbCreditItem(row.ID, row.USER_ID, row.CLIENT_ID, row.BIRTHDATE);
        }
    }
}
