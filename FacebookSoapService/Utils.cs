using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using Containers;
using Containers.Requests;
using Db;
using FacebookSoapService.InternetBankingMobileServiceReference;
using FacebookSoapService.ServiceHelper;
using FacebookSoapService.VirtualCashInServiceReference;
using NLog;
using BobSiteCardAndBranchListResult = FacebookSoapService.InternetBankingMobileServiceReference.BobSiteCardAndBranchListResult;
using BobSiteCardOrderRequest = FacebookSoapService.InternetBankingMobileServiceReference.BobSiteCardOrderRequest;
using FeedbackMessage = FacebookSoapService.ServiceHelper.FeedbackMessage;
using PersonSex = Containers.Enums.PersonSex;
using ResultCode = FacebookSoapService.InternetBankingMobileServiceReference.ResultCode;
using StandardRequest = Containers.Requests.StandardRequest;

namespace FacebookSoapService
{
    static class Utils
    {
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming

        private const int TERMINAL_ID = 496;

        // Default service for 1 client

        private const String USERNAME = "FbService";
        private const String PASSWORD = "uKkJvha6ANe2BcyO261f";

        public static BobSiteCardAndBranchListResult
                GetBobSiteCardAndBranchList()
        {
            BobSiteCardAndBranchListResult result;
            var cacheKey = string.Format(
                                         MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums
                                                                               .GetBobSiteCardAndBranchList]);

            if (MemoryCacheUtil.Contains(cacheKey))
            {
                result =
                        MemoryCacheUtil
                                .Get<BobSiteCardAndBranchListResult>(cacheKey);
                return result;
            }

            using (var client = new InternetBankingMobileServiceClient())
            {
                var response = client.GetCardsAndBranches();
                if (response.Code != ResultCode.Success)
                {
                    throw new Exception(response.Message);
                }

                result = response;
                client.Close();
            }

            MemoryCacheUtil.Add(result, cacheKey, 240);

            return result;
        }

        public static CompositeList GetLocationObjectList()
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
                if (response.Code != ServiceHelper.ResultCode.Success)
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

        public static BirthdayResponse GetClientBirtday(string clientId, DateTime birthDate)
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

        public static string GetFlag(string rateCurrency)
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

        public static string ObtainIp()
        {
            try
            {
                var context = OperationContext.Current;
                var prop = context.IncomingMessageProperties;
                var endpoint =
                        prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                if (endpoint != null)
                {
                    return endpoint.Address;
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return String.Empty;
        }

        public static void Auth(StandardRequest request)
        {
            if (request.Username == USERNAME &&
                request.Password == PASSWORD)
            {
                return;
            }

            throw new InvalidCredentialException();
        }

        public static string FormatPhoneNumber(String contactPhoneNumber)
        {
            var phoneNumber = contactPhoneNumber.IndexOf("+", StringComparison.Ordinal) ==
                              0
                ? contactPhoneNumber
                : "+" + contactPhoneNumber;
            return phoneNumber;
        }

        public static bool CheckPhoneNumber(String phoneNumber)
        {
            Regex r = new Regex(@"^\+\d{5,32}$");
            return r.IsMatch(phoneNumber);
        }

        public static bool SendCreditRequest(String chatId, String phoneNumber)
        {
            try
            {
                using (var client = new CashInVirtualTerminalServerClient())
                {
                    var now = DateTime.Now;
                    var creditRequest = new CreditRequest
                    {
                        Phone = phoneNumber,
                        CommandResult = 0,
                        Sign = TERMINAL_ID + now.Ticks.ToString(CultureInfo.InvariantCulture),
                        SystemTime = now,
                        TerminalId = TERMINAL_ID,
                        Ticks = now.Ticks
                    };
                    var standardResult = client.CreditRequest(creditRequest);

                    if (standardResult.ResultCodes == ResultCodes.Ok)
                    {
                        OracleDb.SaveFacebookCreditRequest(chatId);
                        return true;
                    }

                    client.Close();
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }

        private static BobSiteCardOrderRequest ToBobSiteCardOrderRequest(CreditCardRequest request)
        {
            var newRequest = new BobSiteCardOrderRequest
            {
                BirthDate = request.BirthDate,
                BranchId = request.BranchId,
                CardId = request.CreditCardType,
                CardName = request.CardName,
                CardSurname = request.CardLastName,
                Currency = request.Currency,
                Email = request.Email,
                HomePhone = request.HomePhone,
                LivingAddress = request.LivingAddress,
                MobilePhone = request.Phone,
                Name = request.FirstName,
                Code = request.Code,
                Surname = request.LastName,
                WorkPhone = request.WorkPhone,
                PassportOrgan = request.PassportOrgan,
                PassportDate = request.PassportDate,
                PassportBack = request.PassportBack,
                Period = request.Period,
                OrderType = request.OrderType,
                PassportNumber = request.PassportNumber,
                PassportFront = request.PassportFront,
                PatronymicName = request.Surname,
                RegAddress = request.RegistrationAddress,
                Sex = request.Sex == PersonSex.Male
                    ? InternetBankingMobileServiceReference.PersonSex.Male
                    : InternetBankingMobileServiceReference.PersonSex.Female
            };

            return newRequest;
        }

        public static bool SendPlasticCardInfo(CreditCardRequest info)
        {
            try
            {
                Log.Info("Trying to send\n{0}", info);

                using (var client = new InternetBankingMobileServiceClient())
                {
                    try
                    {
                        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) +
                                   Path.DirectorySeparatorChar + "photos" + Path.DirectorySeparatorChar;

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var ulongChatId = UInt64.Parse(info.ChatId).ToString("D20");

                        using (var file =
                                File.Create(path + ulongChatId + "_front.png"))
                        {
                            var buffer = Convert.FromBase64String(info.PassportFront);
                            file.Write(buffer, 0, buffer.Length);
                            file.Flush();
                            file.Close();
                        }

                        using (var file = File.Create(path + ulongChatId + "_back.png"))
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

                    var request = ToBobSiteCardOrderRequest(info);

                    var response = client.OrderPlasticCard(request);
                    if (response.Code != ResultCode.Success)
                    {
                        throw new Exception(response.Message);
                    }

                    Log.Info("Send ok");
                    client.Close();
                }

                OracleDb.UpdateFacebookPlasticCardRequest(info.ChatId);

                return true;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }

        public static List<IdTitlePair> ListFeedbackCategories()
        {
            var values = new List<IdTitlePair>();
            var cacheKey = string.Format(
                                         MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums
                                                                               .FeedbackCategories]);
            if (MemoryCacheUtil.Contains(cacheKey))
            {
                values = MemoryCacheUtil.Get<List<IdTitlePair>>(cacheKey);
                if (values.Count > 0)
                {
                    return values;
                }
            }

            using (var client = new TelegramBotHelperServiceClient())
            {
                var list = client.GetFeedbackCategories();

                foreach (var value in list)
                {
                    if (value.id == "1")
                    {
                        continue;
                    }
                    values.Add(new IdTitlePair(value.id, value.value));
                }
                client.Close();
            }

            MemoryCacheUtil.Add(values, cacheKey, 300);

            return values;
        }

        public static bool SendFeedback(FeedbackRequest request)
        {
            try
            {
                using (var client = new TelegramBotHelperServiceClient())
                {
                    if (String.IsNullOrEmpty(request.Type))
                    {
                        Log.Warn("Something wrong in feedback");
                        return false;
                    }

                    var list = client.GetFeedbackCategories();
                    var selectedType = list.First(v => v.id == request.Type);
                    var message = new FeedbackMessage
                    {
                        Subject = "Telegram Bot",
                        Message = request.Text
                    };
                    OracleDb.UpdateFacebookFeedback(request.ChatId);
                    var result = client.RegisterUserFeedback(request.Phone, message, selectedType);

                    if (result.Code != ServiceHelper.ResultCode.Success)
                    {
                        Log.Warn("Not success! " + result.Code + "\t" + result.Message);
                        return false;
                    }
                    client.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }
    }
}
