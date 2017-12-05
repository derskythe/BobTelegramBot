using System;
using BobTelegramBot.Properties;
using BobTelegramBot.Structures;
using Containers.Enums;
using Db;
using Telegram.Bot.Args;

namespace BobTelegramBot
{
    public partial class BotService
    {
        private void BotOnOnCallbackQuery(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                if (String.IsNullOrEmpty(callbackQueryEventArgs.CallbackQuery.Data))
                {
                    Log.Error("CallbackQuery.Data is NULL");
                    SendInvalidResponse(callbackQueryEventArgs);
                    return;
                }
                Log.Info("Received data: {0}", callbackQueryEventArgs.CallbackQuery.Data);
                var cmdList = callbackQueryEventArgs.CallbackQuery.Data.Split(';');

                if (cmdList.Length <= 0)
                {
                    Log.Error("cmdList is empty");
                    SendInvalidResponse(callbackQueryEventArgs);
                    return;
                }

                //_Bot.AnswerCallbackQueryAsync(
                //                              callbackQueryEventArgs.CallbackQuery.Id,
                //                              Resources.ResponseReceived);

                var chatId = Convert.ToInt64(cmdList[1]);
                switch (cmdList[0])
                {
                    case CallbackTypes.Auth:
                        {
                            RemoveLastCommand(chatId);
                            var response = _Bot.AnswerCallbackQueryAsync(
                                                                         callbackQueryEventArgs.CallbackQuery.Id,
                                                                         Resources.ResponseReceived);

                            Wait(response);
                            OracleDb.UpdateTelegramReceiveSettings(chatId, Convert.ToInt32(cmdList[2]));

                            SendSimpleResponse(chatId, Emoji.Ok + " " + Resources.Saved);
                        }
                        break;
                    case CallbackTypes.ListAtmOrBranch:
                    case CallbackTypes.ListAllAtmOrBranch:
                        {
                            var type = (LocationListType)Convert.ToInt32(cmdList[2]);
                            var page = Convert.ToInt32(cmdList[3]);

                            var response = _Bot.AnswerCallbackQueryAsync(
                                                                         callbackQueryEventArgs.CallbackQuery.Id,
                                                                         String.Empty);
                            Wait(response);

                            if (page == -1)
                            {
                                // Nothing to do
                                return;
                            }

                            SendLocationByPage(chatId, page, type, cmdList[0] == CallbackTypes.ListAllAtmOrBranch);
                            return;
                        }
                    case CallbackTypes.FeedBack:
                        _Bot.AnswerCallbackQueryAsync(
                                                      callbackQueryEventArgs.CallbackQuery.Id,
                                                      String.Empty);
                        SendFeedBackSequence(null, chatId, cmdList[2]);
                        return;

                    case CallbackTypes.CreditRequest:
                        if (!String.IsNullOrEmpty(cmdList[3]))
                        {
                            _Bot.AnswerCallbackQueryAsync(
                                                          callbackQueryEventArgs.CallbackQuery.Id,
                                                          String.Empty);
                            SendMoneyWings(chatId, cmdList[3]);
                        }
                        break;

                    case CallbackTypes.CreditInfo:
                        _Bot.AnswerCallbackQueryAsync(
                                                      callbackQueryEventArgs.CallbackQuery.Id,
                                                      String.Empty);
                        if (!String.IsNullOrEmpty(cmdList[2]) &&
                            !String.IsNullOrEmpty(cmdList[3]) &&
                            !String.IsNullOrEmpty(cmdList[4]))
                        {
                            SendCreditBriefcaseCallback(chatId, cmdList[2], cmdList[3], cmdList[4]);
                            return;
                        }

                        break;

                    case CallbackTypes.Sex:
                    case CallbackTypes.CardType:
                    case CallbackTypes.Currency:
                    case CallbackTypes.OrderType:
                    case CallbackTypes.CardPeriod:
                    case CallbackTypes.BranchId:
                        _Bot.AnswerCallbackQueryAsync(
                                                      callbackQueryEventArgs.CallbackQuery.Id,
                                                      String.Empty);
                        if (!String.IsNullOrEmpty(cmdList[2]))
                        {
                            SendCreditCardRequest(chatId, cmdList[2]);
                            return;
                        }
                        break;

                    case CallbackTypes.CurrencyCalculator:
                        _Bot.AnswerCallbackQueryAsync(
                                                      callbackQueryEventArgs.CallbackQuery.Id,
                                                      String.Empty);
                        var lastCommand = GetLastCommand(chatId);
                        if (cmdList.Length == 5)
                        {
                            SendCurrencyRateCalculator(chatId, cmdList[3], cmdList[4]);
                        }
                        else
                        {
                            SendCurrencyRateCalculator(chatId, cmdList[3]);                            
                        }
                        return;
                }

                SendHelpMessage(chatId);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }
    }
}
