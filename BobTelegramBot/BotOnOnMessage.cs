using System;
using BobTelegramBot.Properties;
using BobTelegramBot.Structures;
using Containers.Enums;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace BobTelegramBot
{
    public partial class BotService
    {
        private void BotOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                if (messageEventArgs.Message.Type == MessageType.LocationMessage)
                {
                    Log.Info(
                             "Received location: {0} ({1})\n",
                             messageEventArgs.Message.Chat.Id,
                             messageEventArgs.Message.From.Username);

                    var text = GetLastCommand(messageEventArgs.Message.Chat.Id);
                    if (text.StartsWith(Emoji.Atm))
                    {
                        SendLocationList(messageEventArgs, false, LocationListType.Atm);
                    }
                    else if (text.StartsWith(Emoji.Bank))
                    {
                        SendLocationList(messageEventArgs, false, LocationListType.Bank);
                    }
                    else if (text.StartsWith(Emoji.DesktopComputer))
                    {
                        SendLocationList(messageEventArgs, false, LocationListType.CashIn);
                    }
                }
                else if (messageEventArgs.Message.Type == MessageType.ContactMessage)
                {
                    Log.Info(
                             "Received contact: {0} ({1})\nMessage: {2}",
                             messageEventArgs.Message.Chat.Id,
                             messageEventArgs.Message.From.Username,
                             messageEventArgs.Message.Text);

                    var text = GetLastCommand(messageEventArgs.Message.Chat.Id);
                    if (text.Contains(Emoji.Sms))
                    {
                        StartBroadcastFinalization(messageEventArgs);
                    }
                    else if (text.Contains(Emoji.MoneyWings))
                    {
                        SendMoneyPreWings(messageEventArgs);
                    }
                    else if (text.Contains(Emoji.CreditCard))
                    {
                        SendCreditCardRequest(
                                              messageEventArgs.Message.Chat.Id,
                                              messageEventArgs.Message.Contact.PhoneNumber);
                    }
                    else
                    {
                        SendFeedBackSequence(messageEventArgs);
                    }
                }
                else if ((messageEventArgs.Message.Type == MessageType.PhotoMessage ||
                          messageEventArgs.Message.Type == MessageType.DocumentMessage) &&
                         GetLastCommand(messageEventArgs.Message.Chat.Id) == Emoji.CreditCard)
                {
                    SendCreditCardRequest(
                                          messageEventArgs.Message.Chat.Id,
                                          messageEventArgs.Message.Text,
                                          messageEventArgs.Message.Photo,
                                          messageEventArgs.Message.Document);
                }
                else if (messageEventArgs.Message.Type != MessageType.TextMessage)
                {
                    SendDefaultMessage(messageEventArgs);
                }
                else
                {
                    Log.Info(
                             "Received message: {0} ({1})\nMessage: {2}",
                             messageEventArgs.Message.Chat.Id,
                             messageEventArgs.Message.From.Username,
                             messageEventArgs.Message.Text);

                    var text = messageEventArgs.Message.Text.Trim();
                    if (text.StartsWith(CommandsList.Help))
                    {
                        SendHelpMessage(messageEventArgs.Message.Chat.Id);
                    }
                    else if (text.StartsWith(CommandsList.Start))
                    {
                        StartBroadcast(messageEventArgs);
                    }
                    else if (text.StartsWith(CommandsList.Stop))
                    {
                        StopBroadcast(messageEventArgs);
                    }
                    else if (text.StartsWith(Emoji.Rate))
                    {
                        SendCurrencyRates(messageEventArgs);
                    }
                    else if (text.StartsWith(Emoji.CurrencyExchange) ||
                             GetLastCommand(messageEventArgs.Message.Chat.Id) == Resources.SelectBuyCurrency)
                    {
                        SendCurrencyRateCalculator(
                                                   messageEventArgs.Message.Chat.Id,
                                                   String.Empty,
                                                   String.Empty,
                                                   messageEventArgs.Message.Text);
                    }
                    else if (text.StartsWith(Emoji.Atm))
                    {
                        if (!text.Contains(Resources.AllList))
                        {
                            SendPreLocationKeyboard(messageEventArgs, LocationListType.Atm);
                        }
                        else
                        {
                            SendLocationList(messageEventArgs, true, LocationListType.Atm);
                        }
                    }
                    else if (text.StartsWith(Emoji.Bank))
                    {
                        if (!text.Contains(Resources.AllList))
                        {
                            SendPreLocationKeyboard(messageEventArgs, LocationListType.Bank);
                        }
                        else
                        {
                            SendLocationList(messageEventArgs, true, LocationListType.Bank);
                        }
                    }
                    else if (text.StartsWith(Emoji.DesktopComputer))
                    {
                        if (!text.Contains(Resources.AllList))
                        {
                            SendPreLocationKeyboard(messageEventArgs, LocationListType.CashIn);
                        }
                        else
                        {
                            SendLocationList(messageEventArgs, true, LocationListType.CashIn);
                        }
                    }
                    else if (text.StartsWith(Emoji.MoneyWings))
                    {
                        SendMoneyPreWings(messageEventArgs);
                    }
                    else if (text.StartsWith(Emoji.Briefcase) ||
                             GetLastCommand(messageEventArgs.Message.Chat.Id) == Emoji.Briefcase)
                    {
                        SendCreditBriefcase(messageEventArgs);
                    }
                    else if (text.StartsWith(Emoji.FeedBack))
                    {
                        SendFeedBackSequence(messageEventArgs);
                    }
                    else if (text.StartsWith(Emoji.CreditCard) ||
                             GetLastCommand(messageEventArgs.Message.Chat.Id) == Emoji.CreditCard &&
                             !messageEventArgs.Message.Text.Contains(Emoji.Back))
                    {
                        SendCreditCardRequest(messageEventArgs.Message.Chat.Id, messageEventArgs.Message.Text);
                    }
                    else if (_Feedback.ContainsKey(messageEventArgs.Message.Chat.Id) &&
                             GetLastCommand(messageEventArgs.Message.Chat.Id) == Emoji.FeedBack)
                    {
                        SendFeedBackSequence(messageEventArgs);
                    }                    
                    else if (_RegEx != null &&
                             _RegEx.IsMatch(text))
                    {
                        ProcessSmsBankingMessage(messageEventArgs);
                    }
                    else
                    {
                        if (text.StartsWith(Emoji.Back))
                        {
                            RemoveLastCommand(messageEventArgs.Message.Chat.Id);
                        }
                        SendHelpMessage(messageEventArgs.Message.Chat.Id);
                    }
                }
            }
            catch (ArgumentException)
            {
                SendSimpleResponse(
                                   messageEventArgs.Message.Chat.Id,
                                   Emoji.Shrugging + " " + Resources.InvalidArguments);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }


    }
}
