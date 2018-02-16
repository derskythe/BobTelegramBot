using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BobTelegramBot.InternetBankingMobileServiceReference;
using BobTelegramBot.Properties;
using BobTelegramBot.ServiceHelper;
using BobTelegramBot.Structures;
using BobTelegramBot.VirtualCashInServiceReference;
using Containers;
using Containers.Enums;
using Db;
using Geolocation;
using NLog;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using FeedbackMessage = BobTelegramBot.ServiceHelper.FeedbackMessage;
using File = Telegram.Bot.Types.File;
using PersonSex = BobTelegramBot.ServiceHelper.PersonSex;
using ResultCode = BobTelegramBot.ServiceHelper.ResultCode;
using ResultCodes = BobTelegramBot.VirtualCashInServiceReference.ResultCodes;

namespace BobTelegramBot
{
    public partial class BotService : IDisposable
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        private readonly Telegram.Bot.TelegramBotClient _Bot;
        private readonly Thread _SendThread;
        private const int TIMEOUT = 2 * 1000;
        private bool _Running;

        // TODO: Change this to Memory Cache
        private readonly Dictionary<long, Feedback> _Feedback = new Dictionary<long, Feedback>();

        private readonly Dictionary<long, string> _Auth = new Dictionary<long, string>();

        private readonly Dictionary<long, string> _LastCommand = new Dictionary<long, string>();

        private readonly Dictionary<long, Structures.ClientInfo> _ClientInfos =
                new Dictionary<long, Structures.ClientInfo>();

        private readonly Dictionary<long, ChatBotCreditCardRequest> _CardRequests = new Dictionary<long, ChatBotCreditCardRequest>();

        private readonly Dictionary<long, CurrencyCalculator> _CurrencyCalculators =
                new Dictionary<long, CurrencyCalculator>();

        private readonly Regex _RegEx;
        private const int TERMINAL_ID = 496;
        private readonly CultureInfo _Culture = new CultureInfo("ru-RU");
        private const int CREDIT_CARD_MAX_STEPS = 24;
        private const int MAX_SIZE = 1280;

        private readonly List<String> _AcceptMimeTypes =
                new List<string> {"image/jpeg", "image/pjpeg", "image/png", "image/tiff"};

        public BotService(string apiId)
        {
            _Bot = new Telegram.Bot.TelegramBotClient(apiId);
            var me = _Bot.GetMeAsync();
            while (!me.IsCompleted)
            {
                Thread.Sleep(100);
            }

            Log.Info("Init bot: " + me.Result.FirstName);

            _Bot.OnMessage += BotOnOnMessage;
            _Bot.OnCallbackQuery += BotOnOnCallbackQuery;
            _Bot.StartReceiving();

            Log.Info("Init CheckMessagesForSend thread");
            _SendThread = new Thread(CheckMessagesForSend);
#if !DEBUG
            Log.Info("Trying CheckMessagesForSend thread");
            _SendThread.Start();
            Log.Info("CheckMessagesForSend thread started");
#endif
            _Running = true;

            Log.Info("Init GetSmsBankingSettings");
            var pattern = OracleDb.GetSmsBankingSettings();
            _RegEx = !String.IsNullOrEmpty(pattern) ? new Regex(pattern, RegexOptions.IgnoreCase) : null;

            Log.Info("Successful init");
        }

        private string GetLastCommand(long chatId)
        {
            lock (_LastCommand)
            {
                if (_LastCommand.ContainsKey(chatId))
                {
                    return _LastCommand[chatId];
                }
            }

            return String.Empty;
        }

        private void AddLastCommand(long chatId, string command)
        {
            lock (_LastCommand)
            {
                if (_LastCommand.ContainsKey(chatId))
                {
                    _LastCommand.Remove(chatId);
                }

                _LastCommand.Add(chatId, command);
            }
        }

        private void RemoveLastCommand(long chatId)
        {
            lock (_LastCommand)
            {
                if (_LastCommand.ContainsKey(chatId))
                {
                    _LastCommand.Remove(chatId);
                }
            }
        }

        private void CheckMessagesForSend()
        {
            while (_Running)
            {
                try
                {
                    var list = OracleDb.ListMessages();

                    foreach (var message in list)
                    {
                        try
                        {
                            OracleDb.UpdateMessage(
                                                   message.Id,
                                                   SendSimpleResponse(message.UserId, message.MessageText) ? 1 : 0);
                        }
                        catch (Exception exp)
                        {
                            Log.Error(exp, exp.Message);
                        }
                    }
                }
                catch (Exception exp)
                {
                    Log.Error(exp, exp.Message);
                }

                Thread.Sleep(TIMEOUT);
            }
        }

        private void SendInvalidResponse(CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                var response = _Bot.AnswerCallbackQueryAsync(
                                                             callbackQueryEventArgs.CallbackQuery.Id,
                                                             Resources.InvalidResponse);
                Wait(response);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private void ProcessSmsBankingMessage(MessageEventArgs messageEventArgs)
        {
            try
            {
                OracleDb.AddIncomingMessage(messageEventArgs.Message.Chat.Id, messageEventArgs.Message.Text.Trim());
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                SendErrorToUser(messageEventArgs.Message.Chat.Id);
            }
        }

        #region SendFeedBackSequence

        private void SendFeedBackSequence(MessageEventArgs messageEventArgs, long chatId = 0, string selectedTypeValue = "")
        {
            try
            {
                chatId = messageEventArgs != null ? messageEventArgs.Message.Chat.Id : chatId;
                var user = OracleDb.GetTelegramUser(chatId);
                if (user == null)
                {
                    throw new Exception("Can't get user from DB!");
                }

                if (user.FeedBackCount > 5)
                {
                    SendSimpleResponse(chatId, Emoji.Shrugging + " " + Resources.TooManyFeedbacks);
                    return;
                }

                if (!String.IsNullOrEmpty(selectedTypeValue) &&
                    _Feedback.ContainsKey(chatId))
                {
                    _Feedback[chatId].Type = selectedTypeValue;
                    SendSimpleResponse(chatId, Resources.EnterFeedback);
                }
                else if (_Feedback.ContainsKey(chatId))
                {
                    using (var client = new TelegramBotHelperServiceClient())
                    {
                        var values = _Feedback[chatId];
                        if (values == null ||
                            String.IsNullOrEmpty(values.Type))
                        {
                            _Feedback.Remove(chatId);
                            throw new Exception("Something wrong in feedback");
                        }

                        var list = client.GetFeedbackCategories();
                        var selectedType = list.First(v => v.id == values.Type);
                        var message = new FeedbackMessage
                        {
                            Subject = "Telegram Bot",
                            Message = messageEventArgs.Message.Text
                        };
                        _Feedback.Remove(chatId);
                        OracleDb.UpdateTelegramFeedback(chatId);
                        var result = client.RegisterUserFeedback(values.Phone, message, selectedType);

                        if (result.Code != ResultCode.Success)
                        {
                            Log.Warn("Not success! " + result.Code + "\t" + result.Message);

                            //throw new Exception(result.Message);
                        }
                        client.Close();
                        SendSimpleResponse(chatId, Emoji.Ok + " " + Resources.FeedbackSaved);
                        SendHelpMessage(chatId);
                    }
                }
                else
                {
                    if (messageEventArgs.Message.Contact == null)
                    {
                        SendPhoneRequest(chatId);
                        AddLastCommand(chatId, Emoji.FeedBack);
                        return;
                    }

                    _Feedback.Add(
                                  chatId,
                                  new Feedback(messageEventArgs.Message.Contact.PhoneNumber, String.Empty));

                    using (var client = new TelegramBotHelperServiceClient())
                    {
                        var list = client.GetFeedbackCategories();

                        var values = new Dictionary<string, string>();

                        foreach (var value in list)
                        {
                            if (value.id == "1")
                            {
                                continue;
                            }
                            values.Add(value.value, value.id);
                        }

                        var keyboard = Keyboards.GetInlineKeyboard(
                                                         CallbackTypes.FeedBack,
                                                         chatId.ToString(),
                                                         values);

                        var keyboardMarkup = new InlineKeyboardMarkup(keyboard);
                        AddLastCommand(chatId, Emoji.FeedBack);

                        var selectButton = _Bot.SendTextMessageAsync(
                                                                     chatId,
                                                                     Resources.SelectType,
                                                                     false,
                                                                     false,
                                                                     0,
                                                                     keyboardMarkup,
                                                                     ParseMode.Markdown);
                        Wait(selectButton);
                        client.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                SendErrorToUser(messageEventArgs == null ? chatId : messageEventArgs.Message.Chat.Id);
            }
        }

        #endregion

        #region Location

        private void SendLocationByPage(
            long chatId,
            int page,
            LocationListType type,
            bool allList,
            List<LocationItem> itemsList = null)
        {
            if (itemsList == null)
            {
                if (allList)
                {
                    switch (type)
                    {
                        case LocationListType.Atm:
                            itemsList = GetLocationObjectList().Atms;
                            break;

                        case LocationListType.Bank:
                            itemsList = GetLocationObjectList().Branches;
                            break;

                        case LocationListType.CashIn:
                            itemsList = GetLocationObjectList().CashIn;
                            break;

                        default:
                            itemsList = new List<LocationItem>();
                            break;
                    }
                }
                else
                {
                    var cacheKey = string.Format(
                                                 MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums
                                                                                       .GetLocalBranchAndAtmList],
                                                 chatId,
                                                 type);
                    if (MemoryCacheUtil.Contains(cacheKey))
                    {
                        itemsList = MemoryCacheUtil.Get<List<LocationItem>>(cacheKey);
                    }
                    else
                    {
                        SendErrorToUser(chatId);
                        return;
                    }
                }
            }

            if (itemsList.Count == 0)
            {
                SendErrorToUser(chatId);
                return;
            }

            if (page + 1 > itemsList.Count)
            {
                page = itemsList.Count - 1;
            }
            if (page == -1)
            {
                page = 0;
            }

            var stringArray = new Dictionary<string, string>();
            if (page > 0)
            {
                stringArray.Add(Emoji.Prev + " " + Resources.Prev, (page - 1).ToString());
            }
            stringArray.Add(String.Format(Resources.PageNavigation, page + 1, itemsList.Count), "-1");
            if (page + 1 < itemsList.Count)
            {
                stringArray.Add(Resources.Next + " " + Emoji.Next, (page + 1).ToString());
            }

            var keyboard = Keyboards.GetInlineKeyboardSingleLine(
                                                       allList ? CallbackTypes.ListAllAtmOrBranch : CallbackTypes.ListAtmOrBranch,
                                                       chatId.ToString(),
                                                       ((int)type).ToString(),
                                                       stringArray);

            var keyboardMarkup = new InlineKeyboardMarkup(keyboard);

            var item = itemsList[page];

            var response = _Bot.SendLocationAsync(
                                                  chatId,
                                                  Convert.ToSingle(item.Latitude),
                                                  Convert.ToSingle(item.Longitude),
                                                  false,
                                                  0,
                                                  null,
                                                  CancellationToken.None);
            Wait(response);

            var selectButton = _Bot.SendTextMessageAsync(
                                                         chatId,
                                                         item.Title.Az + "\n" + item.Desc.Az,
                                                         false,
                                                         false,
                                                         0,
                                                         keyboardMarkup,
                                                         ParseMode.Markdown);
            Wait(selectButton);
        }

        private void SendLocationList(MessageEventArgs messageEventArgs, bool allList, LocationListType type)
        {
            try
            {
                if (!allList)
                {
                    if (messageEventArgs.Message.Location == null)
                    {
                        SendSimpleResponse(messageEventArgs.Message.Chat.Id, Emoji.Shrugging + " " + Resources.NoLocation);
                        return;
                    }
                }

                List<LocationItem> totalList;
                switch (type)
                {
                    case LocationListType.Atm:
                        totalList = GetLocationObjectList().Atms;
                        break;

                    case LocationListType.Bank:
                        totalList = GetLocationObjectList().Branches;
                        break;

                    case LocationListType.CashIn:
                        totalList = GetLocationObjectList().CashIn;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (allList)
                {
                    SendLocationByPage(messageEventArgs.Message.Chat.Id, 0, type, true, totalList);
                }
                else
                {
                    var origin = new Coordinate
                    {
                        Longitude = messageEventArgs.Message.Location.Longitude,
                        Latitude = messageEventArgs.Message.Location.Latitude
                    };

                    var boundaries = new CoordinateBoundaries(origin, 5);
                    var minLatitude = boundaries.MinLatitude;
                    var maxLatitude = boundaries.MaxLatitude;
                    var minLongitude = boundaries.MinLongitude;
                    var maxLongitude = boundaries.MaxLongitude;

                    var resultList = totalList
                            .Where(x => x.Latitude >= minLatitude && x.Latitude <= maxLatitude)
                            .Where(x => x.Longitude >= minLongitude && x.Longitude <= maxLongitude)
                            .Select(
                                    item => new
                                    {
                                        item.Title,
                                        item.Desc,
                                        item.Latitude,
                                        item.Longitude,
                                        Distance = GeoCalculator.GetDistance(
                                                                             origin.Latitude,
                                                                             origin.Longitude,
                                                                             item.Latitude,
                                                                             item.Longitude),
                                        Direction = GeoCalculator.GetDirection(
                                                                               origin.Latitude,
                                                                               origin.Longitude,
                                                                               item.Latitude,
                                                                               item.Longitude)
                                    })
                            .Where(x => x.Distance <= 5)
                            .OrderBy(x => x.Distance).Select(
                                                             item => new LocationItem
                                                             {
                                                                 Desc = item.Desc,
                                                                 Title = item.Title,
                                                                 Latitude = item.Latitude,
                                                                 Longitude =
                                                                         item.Longitude
                                                             }).ToList();

                    if (resultList.Count <= 0)
                    {
                        SendSimpleResponse(
                                           messageEventArgs.Message.Chat.Id,
                                           Emoji.Shrugging + " " + Resources.NoNearestLocation);
                        return;
                    }

                    var cacheKey = string.Format(
                                                 MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums
                                                                                       .GetLocalBranchAndAtmList],
                                                 messageEventArgs.Message.Chat.Id,
                                                 type);
                    MemoryCacheUtil.Add(resultList, cacheKey, 60);

                    SendLocationByPage(messageEventArgs.Message.Chat.Id, 0, type, false, resultList);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                SendErrorToUser(messageEventArgs.Message.Chat.Id);
            }
        }

        private static CompositeList GetLocationObjectList()
        {
            CompositeList result;
            var cacheKey = string.Format(
                                         MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums.GetBranchAndAtmList]);

            if (MemoryCacheUtil.Contains(cacheKey))
            {
                result = MemoryCacheUtil.Get<CompositeList>(cacheKey);
                return result;
            }

            using (var client = new TelegramBotHelperServiceClient())
            {
                var response = client.GetBranchAndAtmList();
                if (response.Code != ResultCode.Success)
                {
                    throw new Exception(response.Message);
                }

                result = new CompositeList(
                                           response.BranchList.Select(LocationHelper.FromBranch)
                                                   .ToList(),
                                           response.AtmList.Select(LocationHelper.FromAtm).ToList(),
                                           response.PaymentTerminalList.Select(LocationHelper.FromCashIn).ToList());

                client.Close();
            }
            MemoryCacheUtil.Add(result, cacheKey, 120);

            return result;
        }

        private static InternetBankingMobileServiceReference.BobSiteCardAndBranchListResult
                GetBobSiteCardAndBranchList()
        {
            InternetBankingMobileServiceReference.BobSiteCardAndBranchListResult result;
            var cacheKey = string.Format(
                                         MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums
                                                                               .GetBobSiteCardAndBranchList]);

            if (MemoryCacheUtil.Contains(cacheKey))
            {
                result =
                        MemoryCacheUtil
                                .Get<InternetBankingMobileServiceReference.BobSiteCardAndBranchListResult>(cacheKey);
                return result;
            }

            using (var client = new InternetBankingMobileServiceClient())
            {
                var response = client.GetCardsAndBranches();
                if (response.Code != InternetBankingMobileServiceReference.ResultCode.Success)
                {
                    throw new Exception(response.Message);
                }

                result = response;
                client.Close();
            }

            MemoryCacheUtil.Add(result, cacheKey, 240);

            return result;
        }

        private void SendPreLocationKeyboard(MessageEventArgs messageEventArgs, LocationListType listType)
        {
            try
            {
                string type = String.Empty;
                switch (listType)
                {
                    case LocationListType.Atm:
                        type = Emoji.Atm + " " + Resources.ATM;
                        break;

                    case LocationListType.Bank:
                        type = Emoji.Bank + " " + Resources.Bank;
                        break;

                    case LocationListType.CashIn:
                        type = Emoji.DesktopComputer + " " + Resources.CashIn;
                        break;
                }

                AddLastCommand(messageEventArgs.Message.Chat.Id, type);
                var list = new List<ButtonType>
                {
                    new ButtonType(type + " " + Resources.Nearest, true, false),
                    new ButtonType(type + " " + Resources.AllList, false, false),
                    new ButtonType(Emoji.Back + " " + Resources.Back, false, false)
                };

                var keyboardMarkup =
                        new ReplyKeyboardMarkup(Keyboards.GetKeyboard(list), true, true);

                var selectButton = _Bot.SendTextMessageAsync(
                                                             messageEventArgs.Message.Chat.Id,
                                                             type + "\n" + Resources.SelectType,
                                                             false,
                                                             false,
                                                             0,
                                                             keyboardMarkup,
                                                             ParseMode.Markdown);
                Wait(selectButton);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                SendErrorToUser(messageEventArgs.Message.Chat.Id);
            }
        }

        #endregion

        private void SendErrorToUser(long id)
        {
            SendSimpleResponse(id, Emoji.Failed + " " + Resources.ErrorOccured);
        }

        #region SendCurrencyRates

        private void SendCurrencyRates(MessageEventArgs messageEventArgs)
        {
            try
            {
                var str = new StringBuilder();
                var list = GetCurrencyRates();

                foreach (var rate in list)
                {
                    str.Append(rate.Flag).Append(rate.Currency).Append("\n");
                    str.Append(Resources.BuyRate).Append(": ")
                            .Append(rate.BuyRate.ToString("F4")).Append("\n").Append(Resources.SellRate)
                            .Append(": ").Append(rate.SellRate.ToString("F4")).Append("\n\n");
                }
                SendSimpleResponse(messageEventArgs.Message.Chat.Id, str.ToString());

                var listButtons = new List<ButtonType>
                {
                    new ButtonType(Emoji.CurrencyExchange + " " + Resources.CurrencyCalculator, false, false),
                    new ButtonType(Emoji.Back + " " + Resources.Back, false, false)
                };

                var keyboardMarkup =
                        new ReplyKeyboardMarkup(Keyboards.GetKeyboardByRows(listButtons), true, true);

                var selectButton = _Bot.SendTextMessageAsync(
                                                             messageEventArgs.Message.Chat.Id,
                                                             Resources.SelectType,
                                                             false,
                                                             false,
                                                             0,
                                                             keyboardMarkup,
                                                             ParseMode.Markdown);
                Wait(selectButton);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                SendErrorToUser(messageEventArgs.Message.Chat.Id);
            }
        }

        private void SendCurrencyRateCalculator(long chatId, string fromCurrency = "", string toCurrency = "", string value = "")
        {
            try
            {
                var lastCommand = GetLastCommand(chatId);

                if (!String.IsNullOrEmpty(fromCurrency) && String.IsNullOrEmpty(toCurrency))
                {
                    var list = GetCurrencyRates();
                    list.Add(new CurrencyRate("AZN", GetFlag("AZN"), 1M, 1M));

                    if (list.All(rate => rate.Currency != fromCurrency))
                    {
                        RemoveLastCommand(chatId);
                        SendCurrencyRateCalculator(chatId);
                        return;
                    }

                    var currencyList = list.FindAll(rate => rate.Currency != fromCurrency);

                    var listButtons = currencyList.ToDictionary(
                                                        item => item.Flag + " " +
                                                                item.Currency,
                                                        item => chatId + ";" + fromCurrency + ";" + item.Currency);
                    AddLastCommand(chatId, Resources.SelectBuyCurrency);

                    var keyboardMarkup =
                            new InlineKeyboardMarkup(
                                                     Keyboards.GetInlineKeyboard(
                                                                                 CallbackTypes.CurrencyCalculator,
                                                                                 chatId.ToString(),
                                                                                 listButtons));

                    var selectButton = _Bot.SendTextMessageAsync(
                                                                 chatId,
                                                                 Emoji.CurrencyExchange + " " + Resources
                                                                         .SelectBuyCurrency,
                                                                 false,
                                                                 false,
                                                                 0,
                                                                 keyboardMarkup,
                                                                 ParseMode.Markdown);
                    Wait(selectButton);
                }
                else if (!String.IsNullOrEmpty(fromCurrency) &&
                         !String.IsNullOrEmpty(toCurrency))
                {
                    if (String.IsNullOrEmpty(fromCurrency) ||
                        String.IsNullOrEmpty(toCurrency))
                    {
                        RemoveLastCommand(chatId);
                        SendCurrencyRateCalculator(chatId);
                        return;
                    }

                    var list = GetCurrencyRates();
                    list.Add(new CurrencyRate("AZN", GetFlag("AZN"), 1M, 1M));

                    if (_CurrencyCalculators.ContainsKey(chatId))
                    {
                        _CurrencyCalculators.Remove(chatId);
                    }

                    if (list.All(rate => rate.Currency != toCurrency))
                    {
                        RemoveLastCommand(chatId);
                        SendCurrencyRateCalculator(chatId);
                        return;
                    }

                    _CurrencyCalculators.Add(chatId, new CurrencyCalculator(fromCurrency, toCurrency));
                    SendSimpleResponse(chatId, Emoji.CurrencyExchange + " " + Resources.EnterValue);
                    AddLastCommand(chatId, Resources.SelectBuyCurrency);
                }
                else if (lastCommand.Contains(Resources.SelectBuyCurrency) &&
                         !String.IsNullOrEmpty(value) &&
                         !value.Contains(Emoji.CurrencyExchange))
                {
                    if (value.Contains(Emoji.Back))
                    {
                        SendHelpMessage(chatId);
                        return;
                    }
                    var list = GetCurrencyRates();
                    decimal enteredValue;
                    var item = _CurrencyCalculators[chatId];
                    toCurrency = item.To;
                    fromCurrency = item.From;

                    try
                    {
                        value = value.Replace(".", ",");
                        enteredValue = decimal.Parse(value, NumberStyles.Any, _Culture);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, e.Message);
                        enteredValue = 1M;
                    }

                    if (fromCurrency == "AZN")
                    {
                        var selectedRate = list.FirstOrDefault(rate => rate.Currency == toCurrency);
                        enteredValue = enteredValue / selectedRate.SellRate;
                    }
                    else if (toCurrency != "AZN")
                    {
                        var selectedRate = list.FirstOrDefault(rate => rate.Currency == toCurrency);
                        enteredValue = enteredValue / selectedRate.BuyRate;

                        selectedRate = list.FirstOrDefault(rate => rate.Currency == fromCurrency);
                        enteredValue = enteredValue * selectedRate.BuyRate;
                    }
                    else
                    {
                        var selectedRate = list.FirstOrDefault(rate => rate.Currency == fromCurrency);
                        enteredValue = enteredValue * selectedRate.BuyRate;
                    }

                    SendSimpleResponse(
                                       chatId,
                                       enteredValue.ToString("F4") + " " + toCurrency);
                    SendSimpleResponse(chatId, Emoji.CurrencyExchange + " " + Resources.EnterValue);
                }
                else
                {
                    var list = GetCurrencyRates();
                    list.Add(new CurrencyRate("AZN", GetFlag("AZN"), 1M, 1M));

                    var listButtons = list.ToDictionary(
                                                        item => item.Flag + " " +
                                                                item.Currency,
                                                        item => chatId + ";" + item.Currency);
                    AddLastCommand(chatId, Resources.SelectSellCurrency);

                    var keyboardMarkup =
                            new InlineKeyboardMarkup(
                                                     Keyboards.GetInlineKeyboard(
                                                                                 CallbackTypes.CurrencyCalculator,
                                                                                 chatId.ToString(),
                                                                                 listButtons));

                    var selectButton = _Bot.SendTextMessageAsync(
                                                                 chatId,
                                                                 Emoji.CurrencyExchange + " " + Resources
                                                                         .SelectSellCurrency,
                                                                 false,
                                                                 false,
                                                                 0,
                                                                 keyboardMarkup,
                                                                 ParseMode.Markdown);
                    Wait(selectButton);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                SendErrorToUser(chatId);
            }
        }

        private static List<T> Clone<T>(IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        private List<CurrencyRate> GetCurrencyRates()
        {
            var cacheKey = string.Format(
                                         MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums.CurrencyRate]);
            var result = new List<CurrencyRate>();

            if (MemoryCacheUtil.Contains(cacheKey))
            {
                result = MemoryCacheUtil.Get<List<CurrencyRate>>(cacheKey);
                if (result.Count > 0)
                {
                    return Clone(result);
                }
            }

            using (var client = new TelegramBotHelperServiceClient())
            {
                var response = client.GetCurrencyRates();
                if (response.Code != ResultCode.Success)
                {
                    throw new Exception(response.Message);
                }

                foreach (var rate in response.CurrencyRates)
                {
                    var flag = GetFlag(rate.currency);
                    result.Add(
                               new CurrencyRate(
                                                rate.currency,
                                                flag,
                                                Convert.ToDecimal(rate.BuyRate),
                                                Convert.ToDecimal(rate.SellRate)));
                }
                client.Close();
            }
            MemoryCacheUtil.Add(result, cacheKey, 10);

            return Clone(result);
        }

        private string GetFlag(string rateCurrency)
        {
            switch (rateCurrency.ToUpperInvariant())
            {
                case "EUR":
                    return "\U0001F1EA\U0001F1FA ";

                case "USD":
                    return "\U0001F1FA\U0001F1F8 ";

                case "GBP":
                    return "\U0001F1EC\U0001F1E7 ";

                case "RUB":
                    return "\U0001F1F7\U0001F1FA ";

                default:
                    return "\U0001F1E6\U0001F1FF ";
            }
        }

        #endregion

        #region  Wait

        private static void Wait(Task<Message> selectButton)
        {
            while (!selectButton.IsCompleted &&
                   !selectButton.IsFaulted &&
                   !selectButton.IsCanceled)
            {
                Thread.Sleep(100);
            }
        }

        private static void Wait(Task selectButton)
        {
            while (!selectButton.IsCompleted &&
                   !selectButton.IsFaulted &&
                   !selectButton.IsCanceled)
            {
                Thread.Sleep(100);
            }
        }

        #endregion

        #region StartBroadcast

        private void StartBroadcast(MessageEventArgs messageEventArgs)
        {
            try
            {
                SendSimpleResponse(
                                   messageEventArgs.Message.Chat.Id,
                                   Emoji.Welcome + " " + Resources.Welcome);
                var user = OracleDb.GetTelegramUser(messageEventArgs.Message.Chat.Id);
                if (user == null)
                {
                    OracleDb.SaveTelegramUser(
                                      messageEventArgs.Message.Chat.Id,
                                      messageEventArgs.Message.Chat.Username,
                                      String.Empty);
                }

                var list = messageEventArgs.Message.Text.Split(' ');
                if (list.Length == 2)
                {
                    Log.Info(
                             "SMS Register! Received from: {0} ({1}), Key: {2}",
                             messageEventArgs.Message.Chat.Id,
                             messageEventArgs.Message.Chat.Username,
                             list[1]);
                    lock (_Auth)
                    {
                        if (!_Auth.ContainsKey(messageEventArgs.Message.Chat.Id))
                        {
                            _Auth.Add(messageEventArgs.Message.Chat.Id, list[1]);
                        }
                        else
                        {
                            _Auth.Remove(messageEventArgs.Message.Chat.Id);
                            _Auth.Add(messageEventArgs.Message.Chat.Id, list[1]);
                        }
                    }

                    AddLastCommand(messageEventArgs.Message.Chat.Id, Emoji.Sms);

                    SendPhoneRequest(messageEventArgs.Message.Chat.Id);
                }
                else
                {
                    SendHelpMessage(messageEventArgs.Message.Chat.Id);
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private void StartBroadcastFinalization(MessageEventArgs messageEventArgs)
        {
            try
            {
                string authKey;
                var user = OracleDb.GetTelegramUser(messageEventArgs.Message.Chat.Id);

                lock (_Auth)
                {
                    if (!_Auth.ContainsKey(messageEventArgs.Message.Chat.Id) ||
                        user == null)
                    {
                        Log.Error("Didn't find chatId: {0} or user: {1}", messageEventArgs.Message.Chat.Id, user);
                        SendSimpleResponse(
                                           messageEventArgs.Message.Chat.Id,
                                           Emoji.Shrugging + " " + Resources.InvalidArguments);
                        return;
                    }

                    authKey = _Auth[messageEventArgs.Message.Chat.Id];
                    _Auth.Remove(messageEventArgs.Message.Chat.Id);
                }

                var phoneNumber = messageEventArgs.Message.Contact.PhoneNumber.IndexOf("+", StringComparison.Ordinal) >=
                                  0
                    ? messageEventArgs.Message.Contact.PhoneNumber
                    : "+" + messageEventArgs.Message.Contact.PhoneNumber;

                var result = OracleDb.CheckAuthKey(
                                                   messageEventArgs.Message.Chat.Id,
                                                   phoneNumber,
                                                   authKey);
                if (!result)
                {
                    Log.Error(
                              "Check auth key failed\nAuthKey: {0}\nPhoneNumber: {1}",
                              authKey,
                              phoneNumber);
                    SendSimpleResponse(
                                       messageEventArgs.Message.Chat.Id,
                                       Emoji.Shrugging + " " + Resources.InvalidArguments);
                    return;
                }

                var listButtons = new Dictionary<string, string>
                {
                    {Resources.SendMessageDisable, "0"},
                    {Resources.SendMessagesTelegram, "1"},
                    {Resources.SendMessagesBoth, "2"}
                };

                var keyboardMarkup =
                        new InlineKeyboardMarkup(
                                                 Keyboards.GetInlineKeyboard(
                                                                             CallbackTypes.Auth,
                                                                             messageEventArgs.Message.Chat.Id.ToString(),
                                                                             listButtons));


                var selectButton = _Bot.SendTextMessageAsync(
                                                             messageEventArgs.Message.Chat.Id,
                                                             Resources.SelectSendType,
                                                             false,
                                                             false,
                                                             messageEventArgs.Message.MessageId,
                                                             keyboardMarkup,
                                                             ParseMode.Markdown);

                Wait(selectButton);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        #endregion

        private void SendPhoneRequest(long chatId)
        {
            var listButtons = new List<ButtonType>
            {
                new ButtonType(Emoji.Phone + " " + Resources.SendPhoneNumber, false, true),
                new ButtonType(Emoji.Back + " " + Resources.Back, false, false)
            };

            var keyboardMarkup =
                    new ReplyKeyboardMarkup(Keyboards.GetKeyboardByRows(listButtons), true, true);

            var selectButton = _Bot.SendTextMessageAsync(
                                                         chatId,
                                                         Resources.SendPhoneNumber,
                                                         false,
                                                         false,
                                                         0,
                                                         keyboardMarkup,
                                                         ParseMode.Markdown);
            Wait(selectButton);
        }

        private void StopBroadcast(MessageEventArgs messageEventArgs)
        {
            try
            {
                SendSimpleResponse(
                                   messageEventArgs.Message.Chat.Id,
                                   Emoji.Stop + " " + Resources.BroadcastStopped);
                OracleDb.DeleteTelegramUser(messageEventArgs.Message.Chat.Id);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private void SendDefaultMessage(MessageEventArgs messageEventArgs)
        {
            try
            {
                SendSimpleResponse(
                                   messageEventArgs.Message.Chat.Id,
                                   String.Format(
                                                 Resources.DefaultMessage,
                                                 messageEventArgs.Message.From.Username));
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private bool SendSimpleResponse(long chatId, String message)
        {
            try
            {
                var response = _Bot.SendTextMessageAsync(
                                                       chatId,
                                                       message);
                Wait(response);
                return true;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }

        private void SendHelpMessage(long chatId)
        {
            RemoveLastCommand(chatId);
            var list = new List<ButtonType>
            {
                new ButtonType(Emoji.Rate + " " + Resources.CurrencyRates, false, false),
                new ButtonType(Emoji.Atm + " " + Resources.ATM, false, false),
                new ButtonType(Emoji.Bank + " " + Resources.Bank, false, false),
                new ButtonType(Emoji.DesktopComputer + " " + Resources.CashIn, false, false),
                new ButtonType(Emoji.FeedBack + " " + Resources.Feedback, false, false),
                new ButtonType(Emoji.Briefcase + " " + Resources.MyCredits, false, false),
                new ButtonType(Emoji.MoneyWings + " " + Resources.CreditRequest, false, false),
                new ButtonType(Emoji.CreditCard + " " + Resources.CardRequest, false, false)
            };

            var keyboardMarkup =
                    new ReplyKeyboardMarkup(Keyboards.GetKeyboard(list), true);


            var selectButton = _Bot.SendTextMessageAsync(
                                                         chatId,
                                                         Resources.SelectAction,
                                                         false,
                                                         false,
                                                         0,
                                                         keyboardMarkup,
                                                         ParseMode.Markdown);
            Wait(selectButton);
        }

        private void SendMoneyPreWings(MessageEventArgs messageEventArgs)
        {
            try
            {
                if (messageEventArgs.Message.Contact == null)
                {
                    SendPhoneRequest(messageEventArgs.Message.Chat.Id);
                    AddLastCommand(messageEventArgs.Message.Chat.Id, Emoji.MoneyWings);
                    return;
                }

                var user = OracleDb.GetTelegramUser(messageEventArgs.Message.Chat.Id);
                if (user.LastCreditRequest > DateTime.Today)
                {
                    Log.Warn("{0} {1} {2}", Resources.TooManyRequests, user.LastCreditRequest, DateTime.Today);
                    SendSimpleResponse(
                                       messageEventArgs.Message.Chat.Id,
                                       Emoji.Shrugging + " " + Resources.TooManyRequests);
                    return;
                }

                var phoneNumber = FormatPhoneNumber(messageEventArgs.Message.Contact.PhoneNumber);
                var list = new Dictionary<string, string>
                {
                    {Emoji.Yes + " " + Resources.Yes, phoneNumber},
                    {Emoji.No + " " + Resources.No, String.Empty}
                };

                var keyboardMarkup =
                        new InlineKeyboardMarkup(
                                                 Keyboards.GetInlineKeyboardSingleLine(
                                                                             CallbackTypes.CreditRequest,
                                                                             messageEventArgs.Message.Chat.Id
                                                                                     .ToString(),
                                                                             CallbackTypes.CreditRequest,
                                                                             list));


                var selectButton = _Bot.SendTextMessageAsync(
                                                             messageEventArgs.Message.Chat.Id,
                                                             Emoji.Question + " " + String.Format(
                                                                                                  Resources
                                                                                                          .SendCreditRequest,
                                                                                                  phoneNumber),
                                                             false,
                                                             false,
                                                             0,
                                                             keyboardMarkup,
                                                             ParseMode.Markdown);
                Wait(selectButton);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
                SendErrorToUser(messageEventArgs.Message.Chat.Id);
            }
        }

        private static string FormatPhoneNumber(String contactPhoneNumber)
        {
            var phoneNumber = contactPhoneNumber.IndexOf("+", StringComparison.Ordinal) ==
                              0
                ? contactPhoneNumber
                : "+" + contactPhoneNumber;
            return phoneNumber;
        }

        private void SendMoneyWings(long chatId, string phone)
        {
            try
            {
                using (var client = new CashInVirtualTerminalServerClient())
                {
                    var now = DateTime.Now;
                    var creditRequest = new CreditRequest
                    {
                        Phone = phone,
                        CommandResult = 0,
                        Sign = TERMINAL_ID + now.Ticks.ToString(CultureInfo.InvariantCulture),
                        SystemTime = now,
                        TerminalId = TERMINAL_ID,
                        Ticks = now.Ticks
                    };
                    var standardResult = client.CreditRequest(creditRequest);

                    if (standardResult.ResultCodes == ResultCodes.Ok)
                    {
                        OracleDb.SaveTelegramCreditRequest(chatId);
                        SendSimpleResponse(chatId, Emoji.Ok + " " + Resources.RequestSaved);
                    }
                    else
                    {
                        SendErrorToUser(chatId);
                    }

                    client.Close();
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
                SendErrorToUser(chatId);
            }
        }

        public void Dispose()
        {
            _Running = false;

            if (_Bot != null)
            {
                _Bot.StopReceiving();
            }

            if (_SendThread != null)
            {
                _SendThread.Join(1000);
            }
        }

        private void SendCreditBriefcaseCallback(long chatId, string clientId, string birthDate, string creditNumber)
        {
            try
            {
                var reg = new Regex("^\\d{6}$");

                if (!reg.IsMatch(clientId))
                {
                    throw new Exception("ClientId invalid");
                }

                var birthdate = DateTime.ParseExact(birthDate, "dd.MM.yyyy", _Culture);

                var response = GetClientBirtday(clientId, birthdate);

                if (response.ResultCodes == ResultCodes.Ok &&
                    response.Birthday != DateTime.MinValue &&
                    response.Birthday == birthdate)
                {
                    if (response.Infos.Length == 0)
                    {
                        Log.Warn("response is 0");
                        SendSimpleResponse(chatId, Emoji.Shrugging + " " + Resources.CantFindYou);
                        return;
                    }

                    var creditInfo = response.Infos.First(ext => ext.CreditNumber == creditNumber);
                    if (creditInfo == null)
                    {
                        Log.Warn("Can't find this credit");
                        SendSimpleResponse(chatId, Emoji.Shrugging + " " + Resources.CantFindYou);
                        return;
                    }

                    var keyboardInline = new InlineKeyboardButton[1][];
                    var keyboardButtons = new InlineKeyboardButton[1];
                    keyboardButtons[0] = new InlineKeyboardButton
                    {
                        Text = Resources.Pay,
                        Url = "https://www.e-bankofbaku.com/OnlineCreditPayment/account_info?crd_num=" + creditNumber
                    };

                    keyboardInline[0] = keyboardButtons;

                    var keyboardMarkup =
                            new InlineKeyboardMarkup(keyboardInline);
                    var selectButton = _Bot.SendTextMessageAsync(
                                                                 chatId,
                                                                 String.Format(
                                                                               Resources.CreditInfo,
                                                                               creditInfo.CreditName,
                                                                               creditInfo.CreditNumber,
                                                                               creditInfo.BeginDate.ToString(
                                                                                                             "dd.MM.yyyy"),
                                                                               creditInfo.CreditAmount.ToString("F0"),
                                                                               creditInfo.Currency,
                                                                               creditInfo.AmountLeft.ToString("F0"),
                                                                               creditInfo.AmountLate.ToString("F0")),
                                                                 false,
                                                                 false,
                                                                 0,
                                                                 keyboardMarkup,
                                                                 ParseMode.Markdown);
                    Wait(selectButton);
                }
                else
                {
                    SendErrorToUser(chatId);
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
                SendErrorToUser(chatId);
            }
        }

        private void SendCreditBriefcase(MessageEventArgs messageEventArgs)
        {
            try
            {
                var chatId = messageEventArgs.Message.Chat.Id;

                lock (_ClientInfos)
                {
                    if (_ClientInfos.ContainsKey(chatId))
                    {
                        if (String.IsNullOrEmpty(_ClientInfos[chatId].ClientId))
                        {
                            AddLastCommand(chatId, Emoji.Briefcase);
                            var reg = new Regex("^\\d{6}$");

                            if (!reg.IsMatch(messageEventArgs.Message.Text))
                            {
                                SendSimpleResponse(chatId, Emoji.Failed + " " + Resources.InvalidClientId);
                                SendSimpleResponse(chatId, Resources.EnterClientId);
                            }
                            else
                            {
                                _ClientInfos[chatId].ClientId = messageEventArgs.Message.Text;
                                SendSimpleResponse(chatId, Resources.EnterBirthdate);
                            }
                        }
                        else
                        {
                            DateTime birthdate;
                            try
                            {
                                birthdate = DateTime.ParseExact(messageEventArgs.Message.Text, "dd.MM.yyyy", _Culture);
                            }
                            catch (Exception e)
                            {
                                Log.Warn(e.Message);
                                SendSimpleResponse(chatId, Emoji.Failed + " " + Resources.InvalidBirthdate);
                                SendSimpleResponse(chatId, Resources.EnterBirthdate);
                                return;
                            }

                            var clientId = _ClientInfos[chatId].ClientId;
                            _ClientInfos.Remove(chatId);
                            SendCreditList(clientId, birthdate, chatId);
                        }
                    }
                    else
                    {
                        var credits = OracleDb.GetSavedCreditTelegram(messageEventArgs.Message.Chat.Id);
                        if (credits.Count == 0)
                        {
                            SendSimpleResponse(chatId, Emoji.Shrugging + " " + Resources.DontHaveCredits);
                            SendSimpleResponse(chatId, Resources.EnterClientId);
                            AddLastCommand(chatId, Emoji.Briefcase);

                            if (_ClientInfos.ContainsKey(chatId))
                            {
                                _ClientInfos.Remove(chatId);
                            }
                            _ClientInfos.Add(chatId, new Structures.ClientInfo());
                        }
                        else
                        {
                            SendCreditList(credits[0].ClientId, credits[0].BirthDate, chatId);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
                SendErrorToUser(messageEventArgs.Message.Chat.Id);
            }
        }

        private void SendCreditList(string clientId, DateTime birthdate, long chatId)
        {
            var response = GetClientBirtday(clientId, birthdate);

            if (response.ResultCodes == ResultCodes.Ok &&
                response.Birthday != DateTime.MinValue &&
                response.Birthday == birthdate)
            {
                if (response.Infos.Length == 0)
                {
                    SendSimpleResponse(chatId, Emoji.Shrugging + " " + Resources.CantFindYou);
                }

                var list = response.Infos.ToDictionary(
                                                       item => item.CreditName + " " +
                                                               item.CreditAmount + " " +
                                                               item.Currency,
                                                       item => clientId + ";" + birthdate.ToString("dd.MM.yyyy") + ";" + item.CreditNumber);
                OracleDb.SaveTelegramCredit(chatId, clientId, birthdate);
                var keyboardMarkup =
                        new InlineKeyboardMarkup(
                                                 Keyboards.GetInlineKeyboard(
                                                                             CallbackTypes.CreditInfo,
                                                                             chatId.ToString(),
                                                                             list));

                var selectButton = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             Emoji.Briefcase + " " + Resources.CreditList,
                                                             false,
                                                             false,
                                                             0,
                                                             keyboardMarkup,
                                                             ParseMode.Markdown);
                Wait(selectButton);
            }
            else
            {
                Log.Debug(
                          "clientId: {0}, birthdate: {1}, chatId: {2}, response.Birthday: {3}",
                          clientId,
                          birthdate,
                          chatId,
                          response.Birthday);
                //SendErrorToUser(chatId);
                SendSimpleResponse(chatId, Emoji.Shrugging + " " + Resources.CantFindYou);
            }
        }

        private static BirthdayResponse GetClientBirtday(string clientId, DateTime birthDate)
        {
            var response = new BirthdayResponse { ResultCodes = ResultCodes.SystemError };

            try
            {
                var cacheKey = string.Format(
                                             MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums
                                                                                   .GetCashClientInfo],
                                             clientId,
                                             birthDate.ToString(CultureInfo.InvariantCulture));

                if (MemoryCacheUtil.Contains(cacheKey))
                {
                    response = MemoryCacheUtil.Get<BirthdayResponse>(cacheKey);
                    if (response.ResultCodes == ResultCodes.Ok)
                    {
                        return response;
                    }
                }

                using (var client = new CashInVirtualTerminalServerClient())
                {
                    var now = DateTime.Now;
                    var request = new GetClientInfoRequest
                    {
                        ClientCode = clientId,
                        PaymentOperationType = 11,
                        CommandResult = 0,
                        Sign = TERMINAL_ID + now.Ticks.ToString(CultureInfo.InvariantCulture),
                        TerminalId = TERMINAL_ID,
                        SystemTime = now,
                        Ticks = now.Ticks
                    };

                    // 11 - CreditPaymentByClientCode

                    response = client.GetClientBirtday(request);
                    client.Close();
                }

                MemoryCacheUtil.Add(response, cacheKey, 5);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            return response;
        }

        private void SendCreditCardRequest(
            long chatId,
            String phoneNumber = "",
            IReadOnlyCollection<PhotoSize> photo = null,
            Document document = null)
        {
            try
            {
                var regExp = new Regex(@"^[a-z\s]+$", RegexOptions.IgnoreCase);
                if (!_CardRequests.ContainsKey(chatId))
                {
                    _CardRequests.Add(chatId, new ChatBotCreditCardRequest(CreditCardStep.Phone));
                }

                AddLastCommand(chatId, Emoji.CreditCard);

                switch (_CardRequests[chatId].CreditCardStep)
                {
                    case CreditCardStep.Phone:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(chatId, String.Empty, false);
                        }
                        else
                        {
                            _CardRequests[chatId].Phone = phoneNumber;
                            CreditCardRequestStepUp(chatId);
                        }
                        break;

                    case CreditCardStep.Sex:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            var list = new Dictionary<string, string>
                            {
                                {Resources.SexMale, ((int) PersonSex.Male).ToString()},
                                {Resources.SexFemale, ((int) PersonSex.Female).ToString()}
                            };
                            CreditCardKeyboardRequest(chatId, Resources.EnterSex, CallbackTypes.Sex, list);
                        }
                        else
                        {
                            _CardRequests[chatId].Sex =
                                    (Containers.Enums.PersonSex) Convert.ToInt32(phoneNumber);
                            CreditCardRequestStepUp(chatId);
                        }
                        break;

                    case CreditCardStep.FirstName:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(chatId, Resources.EnterFirstname);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 2)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].FirstName = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.LastName:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(chatId, Resources.EnterLastname);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 2)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].LastName = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;
                    case CreditCardStep.Patronomyc:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(chatId, Resources.EnterPatronymicName);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 2)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].Surname = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.CardName:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(chatId, Resources.EnterNameOnCard);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 2 ||
                                !regExp.IsMatch(phoneNumber))
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].CardName = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.CardLastName:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(chatId, Resources.EnterLastNameOnCard);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 2 ||
                                !regExp.IsMatch(phoneNumber))
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].CardLastName = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.SecretWord:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(chatId, Resources.EnterSecretWord);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 3 ||
                                !regExp.IsMatch(phoneNumber))
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].Code = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.BirthDate:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(chatId, Resources.EnterBirthdate);
                        }
                        else
                        {
                            DateTime? birthDate = null;

                            try
                            {
                                birthDate = DateTime.ParseExact(phoneNumber, "dd.MM.yyyy", _Culture);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e, e.Message);
                            }

                            if (birthDate == null)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].BirthDate = (DateTime)birthDate;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.PassportBack:
                    case CreditCardStep.PassportFront:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    _CardRequests[chatId].CreditCardStep == CreditCardStep.PassportFront
                                                        ? Resources.EnterPassportFront
                                                        : Resources.EnterPassportBack);
                        }
                        else if (photo != null && photo.Count > 0)
                        {
                            var file = GetFile(photo.LastOrDefault());
                            if (file == null)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else if (file.FileSize > MAX_SIZE * 1024)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   String.Format(Resources.InvalidFilesize, MAX_SIZE));
                            }
                            else
                            {
                                var buffer = ReadFully(file);
                                if (buffer == null ||
                                    buffer.Length == 0)
                                {
                                    SendSimpleResponse(
                                                       chatId,
                                                       Resources.InvalidData);
                                }
                                else
                                {
                                    if (_CardRequests[chatId].CreditCardStep == CreditCardStep.PassportFront)
                                    {
                                        _CardRequests[chatId].PassportFront = Convert.ToBase64String(buffer);
                                    }
                                    else
                                    {
                                        _CardRequests[chatId].PassportBack = Convert.ToBase64String(buffer);
                                    }
                                    CreditCardRequestStepUp(chatId);
                                }
                            }
                        }
                        else if (document != null && _AcceptMimeTypes.Any(t => t == document.MimeType))
                        {
                            var file = GetFile(document);
                            if (file == null)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else if (file.FileSize > MAX_SIZE * 1024)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   String.Format(Resources.InvalidFilesize, MAX_SIZE));
                            }
                            else
                            {
                                var buffer = ReadFully(file);
                                if (buffer == null ||
                                    buffer.Length == 0)
                                {
                                    SendSimpleResponse(
                                                       chatId,
                                                       Resources.InvalidData);
                                }
                                else
                                {
                                    if (_CardRequests[chatId].CreditCardStep == CreditCardStep.PassportFront)
                                    {
                                        _CardRequests[chatId].PassportFront = Convert.ToBase64String(buffer);
                                    }
                                    else
                                    {
                                        _CardRequests[chatId].PassportBack = Convert.ToBase64String(buffer);
                                    }
                                    CreditCardRequestStepUp(chatId);
                                }
                            }
                        }
                        else
                        {
                            SendSimpleResponse(
                                               chatId,
                                               Resources.InvalidData);                            
                        }
                        break;

                    case CreditCardStep.PassportNumber:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    Resources.EnterPassportNumber);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 8)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].PassportNumber = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;
                    case CreditCardStep.PassportOrgan:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    Resources.EnterPassportOrgan);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 2)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].PassportOrgan = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.PassportDate:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    Resources.EnterPassportDate);
                        }
                        else
                        {
                            DateTime? date = null;

                            try
                            {
                                date = DateTime.ParseExact(phoneNumber, "dd.MM.yyyy", _Culture);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e, e.Message);
                            }

                            if (date == null)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].PassportDate = (DateTime)date;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.RegistrationAddress:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    Resources.EnterRegistrationAddress);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 2)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].RegistrationAddress = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.LivingAddress:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    Resources.EnterLivingAdress);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 2)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].LivingAddress = phoneNumber.Trim().ToLowerInvariant() == "same"
                                    ? _CardRequests[chatId].RegistrationAddress
                                    : phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;
                    case CreditCardStep.HomePhone:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    Resources.EnterHomePhone);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 7)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].HomePhone = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.WorkPhone:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    Resources.EnterWorkPhone);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                phoneNumber.Length < 7)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].WorkPhone = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.Email:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            CreditCardSimpleRequest(
                                                    chatId,
                                                    Resources.EnterEmail);
                        }
                        else
                        {
                            var regEmail = new Regex(
                                                     @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
                                                     RegexOptions.IgnoreCase);
                            if (String.IsNullOrEmpty(phoneNumber) ||
                                !regEmail.IsMatch(phoneNumber))
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].Email = phoneNumber;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.CreditCardType:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            var rawList = GetBobSiteCardAndBranchList();
                            var list = rawList.BobSiteCardList.ToDictionary(
                                                                            card => card.CardTitle,
                                                                            card => card.CardId);
                            CreditCardKeyboardRequest(
                                                      chatId,
                                                      Resources.EnterCreditCardType,
                                                      CallbackTypes.CardType,
                                                      list);
                        }
                        else
                        {
                            var period = 0;
                            try
                            {
                                period = Convert.ToInt32(phoneNumber);
                            }
                            catch (Exception exp)
                            {
                                Log.Error(exp, exp.Message);
                            }

                            if (period == 0)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].CreditCardType = period;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.Currency:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            var list = new Dictionary<string, string>
                            {
                                {"AZN", "AZN"},
                                {"USD", "USD"},
                                {"EUR", "EUR"}
                            };
                            CreditCardKeyboardRequest(
                                                      chatId,
                                                      Resources.EnterCurrency,
                                                      CallbackTypes.Currency,
                                                      list);
                        }
                        else
                        {
                            _CardRequests[chatId].Currency = phoneNumber;
                            CreditCardRequestStepUp(chatId);
                        }
                        break;

                    case CreditCardStep.Period:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            var list = new Dictionary<string, string>
                            {
                                {Resources.OneYear, "12"},
                                {Resources.TwoYear, "24"},
                                {Resources.ThreeYear, "36"}
                            };

                            CreditCardKeyboardRequest(
                                                      chatId,
                                                      Resources.EnterPeriod,
                                                      CallbackTypes.CardPeriod,
                                                      list);
                        }
                        else
                        {
                            var period = 0;
                            try
                            {
                                period = Convert.ToInt32(phoneNumber);
                            }
                            catch (Exception exp)
                            {
                                Log.Error(exp, exp.Message);
                            }

                            if (period == 0)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].Period = period;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.OrderType:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            var list = new Dictionary<string, string>
                            {
                                {Resources.OrderNormal, "normal"},
                                {Resources.OrderFast, "fast"}
                            };

                            CreditCardKeyboardRequest(
                                                      chatId,
                                                      Resources.EnterOrderType,
                                                      CallbackTypes.OrderType,
                                                      list);
                        }
                        else
                        {
                            var period = String.Empty;

                            if (phoneNumber == "normal")
                            {
                                period = Resources.OrderNormal;
                            }
                            else if (phoneNumber == "fast")
                            {
                                period = Resources.OrderFast;
                            }

                            if (String.IsNullOrEmpty(period))
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].OrderType = period;
                                CreditCardRequestStepUp(chatId);
                            }
                        }
                        break;

                    case CreditCardStep.BranchId:
                        if (_CardRequests[chatId].PrevCreditCardStep != _CardRequests[chatId].CreditCardStep)
                        {
                            var rawList = GetBobSiteCardAndBranchList();
                            var list = rawList.BobSiteBranchList.ToDictionary(
                                                                              card => card.BranchNameEn,
                                                                              card => card.BranchId);

                            CreditCardKeyboardRequest(
                                                      chatId,
                                                      Resources.EnterBranch,
                                                      CallbackTypes.BranchId,
                                                      list);
                        }
                        else
                        {
                            var period = -1;
                            try
                            {
                                period = Convert.ToInt32(phoneNumber);
                            }
                            catch (Exception exp)
                            {
                                Log.Error(exp, exp.Message);
                            }

                            if (period == -1)
                            {
                                SendSimpleResponse(
                                                   chatId,
                                                   Resources.InvalidData);
                            }
                            else
                            {
                                _CardRequests[chatId].BranchId = period;
                                SendSimpleResponse(chatId, Emoji.HourGlass + " " + Resources.WaitAMoment);
                                SendPlasticCardInfo(chatId);
                                SendSimpleResponse(
                                                   chatId,
                                                   Emoji.Ok + " " + Resources.CardOrdered);
                                SendHelpMessage(chatId);
                            }
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                _CardRequests.Remove(chatId);
                Log.Error(e, e.Message);
                throw;
            }
        }

        private void CreditCardSimpleRequest(long chatId, string sendValue, bool simple = true)
        {
            if (!simple)
            {
                SendSimpleResponse(
                                   chatId,
                                   String.Format(
                                                 Resources.PlasticCardRequestStep,
                                                 (int)_CardRequests[chatId].CreditCardStep + 1,
                                                 CREDIT_CARD_MAX_STEPS));
                SendPhoneRequest(chatId);
            }
            else
            {
                SendSimpleResponse(
                                   chatId,
                                   String.Format(
                                                 Resources.PlasticCardRequestStep,
                                                 (int)_CardRequests[chatId].CreditCardStep + 1,
                                                 CREDIT_CARD_MAX_STEPS) + "\n" +
                                   sendValue);
            }

            _CardRequests[chatId].PrevCreditCardStep =
                    (CreditCardStep)((int)_CardRequests[chatId].PrevCreditCardStep + 1);
        }

        private void CreditCardKeyboardRequest(
            long chatId,
            string answerString,
            string callbackType,
            Dictionary<string, string> stringArray)
        {
            var keyboardMarkup =
                    new InlineKeyboardMarkup(
                                             Keyboards.GetInlineKeyboard(
                                                                         callbackType,
                                                                         chatId.ToString(),
                                                                         stringArray));

            var selectButton = _Bot.SendTextMessageAsync(
                                                         chatId,
                                                         String.Format(
                                                                       Resources.PlasticCardRequestStep,
                                                                       (int)_CardRequests[chatId].CreditCardStep + 1,
                                                                       CREDIT_CARD_MAX_STEPS) + "\n" +
                                                         Emoji.CreditCard + " " + answerString,
                                                         false,
                                                         false,
                                                         0,
                                                         keyboardMarkup,
                                                         ParseMode.Markdown);
            _CardRequests[chatId].PrevCreditCardStep =
                    (CreditCardStep)((int)_CardRequests[chatId].PrevCreditCardStep + 1);
            Wait(selectButton);
        }

        private void CreditCardRequestStepUp(long chatId)
        {
            _CardRequests[chatId].PrevCreditCardStep = _CardRequests[chatId].CreditCardStep;
            _CardRequests[chatId].CreditCardStep =
                    (CreditCardStep)((int)_CardRequests[chatId].CreditCardStep + 1);
            SendCreditCardRequest(chatId);
        }

        private void SendPlasticCardInfo(long chatId)
        {
            try
            {
                _CardRequests[chatId].PrevCreditCardStep = _CardRequests[chatId].CreditCardStep;

                using (var client = new InternetBankingMobileServiceClient())
                {
                    var info = _CardRequests[chatId];
                    Log.Info("Trying to send\n{0}", info);

                    try
                    {
                        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) +
                                   Path.DirectorySeparatorChar + "photos" + Path.DirectorySeparatorChar;

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        using (var file =
                                System.IO.File.Create(path + chatId.ToString("D20") + "_front.png"))
                        {
                            var buffer = Convert.FromBase64String(info.PassportFront);
                            file.Write(buffer, 0, buffer.Length);
                            file.Flush();
                            file.Close();
                        }

                        using (var file = System.IO.File.Create(path + chatId.ToString("D20") + "_back.png"))
                        {
                            var buffer = Convert.FromBase64String(info.PassportBack);
                            file.Write(buffer, 0, buffer.Length);
                            file.Flush();
                            file.Close();
                        }
                    }
                    catch (Exception exp)
                    {
                        Log.Error(exp, exp.Message);
                    }

                    _CardRequests.Remove(chatId);
                    RemoveLastCommand(chatId);
                    var request = info.ToBobSiteCardOrderRequest();

                    var response = client.OrderPlasticCard(request);
                    if (response.Code != InternetBankingMobileServiceReference.ResultCode.Success)
                    {
                        throw new Exception(response.Message);
                    }

                    Log.Info("Send ok");
                    client.Close();
                }

                OracleDb.UpdateTelegramPlasticCardRequest(chatId);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private File GetFile(File rawFile)
        {
            try
            {
                var result = _Bot.GetFileAsync(rawFile.FileId);

                while (!result.IsCompleted &&
                       !result.IsFaulted &&
                       !result.IsCanceled)
                {
                    Thread.Sleep(10);
                }

                return result.Result;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            return null;
        }

        private static byte[] ReadFully(File input)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    input.FileStream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return null;
        }
    }

}
