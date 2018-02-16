using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.ServiceModel;
using Containers;
using Containers.Enums;
using Containers.Requests;
using Containers.Results;
using Db;
using FacebookSoapService.ServiceHelper;
using Geolocation;
using NLog;
using ResultCode = FacebookSoapService.ServiceHelper.ResultCode;
using ResultCodes = Containers.Enums.ResultCodes;
using StandardRequest = Containers.Requests.StandardRequest;

namespace FacebookSoapService
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = false,
        InstanceContextMode = InstanceContextMode.PerCall,
        UseSynchronizationContext = false,
        ConfigurationName = "FacebookSoapService.FacebookService")]
    public class FacebookService : IFacebookService
    {
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming


        [OperationBehavior(AutoDisposeParameters = true)]
        public CurrencyRateResult GetCurrencyRate(StandardRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new CurrencyRateResult();

            try
            {
                Utils.Auth(request);

                var cacheKey = string.Format(
                                             MemoryCacheUtil.KeyDictionary[MemoryCacheUtil.KeyEnums.CurrencyRate]);
                var list = new List<CurrencyRate>();

                if (MemoryCacheUtil.Contains(cacheKey))
                {
                    list = MemoryCacheUtil.Get<List<CurrencyRate>>(cacheKey);
                }

                if (list.Count <= 0)
                {
                    using (var client = new TelegramBotHelperServiceClient())
                    {
                        var response = client.GetCurrencyRates();
                        if (response.Code != ResultCode.Success)
                        {
                            throw new Exception(response.Message);
                        }

                        foreach (var rate in response.CurrencyRates)
                        {
                            var flag = Utils.GetFlag(rate.currency);
                            list.Add(
                                     new CurrencyRate(
                                                      rate.currency,
                                                      flag,
                                                      Convert.ToDecimal(rate.BuyRate),
                                                      Convert.ToDecimal(rate.SellRate)));
                        }

                        client.Close();
                    }

                    MemoryCacheUtil.Add(list, cacheKey, 10);
                }

                result.Rates = list;
                result.Code = ResultCodes.Ok;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public CreditListResult GetCreditList(CreditListRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new CreditListResult();

            try
            {
                Utils.Auth(request);

                if ((String.IsNullOrEmpty(request.UserId) ||
                     request.BirthDate <= DateTime.MinValue) &&
                    String.IsNullOrEmpty(request.ChatId))
                {
                    result.Code = ResultCodes.InvalidParameters;
                    throw new ArgumentException(result.Description);
                }

                var userId = request.UserId;
                var birthDate = request.BirthDate;

                if ((String.IsNullOrEmpty(request.UserId) ||
                     request.BirthDate <= DateTime.MinValue) &&
                    !String.IsNullOrEmpty(request.ChatId))
                {
                    var credits = OracleDb.GetSavedCreditFacebook(request.ChatId);
                    if (credits.Count == 0)
                    {
                        Log.Warn("Can't find this client by chat id");
                        result.Code = ResultCodes.NotFound;
                        throw new ArgumentException(result.Description);
                    }

                    userId = credits[0].ClientId;
                    birthDate = credits[0].BirthDate.Date;
                }

                var response = Utils.GetClientBirtday(userId, birthDate);
                Log.Info("ResultCodes: {0}, Birthday: {1}, Infos.Length: {2}", response.ResultCodes, response.Birthday, response.Infos != null ? response.Infos.Length.ToString() : "NULL");

                if (response.ResultCodes == VirtualCashInServiceReference.ResultCodes.Ok &&
                    response.Birthday != DateTime.MinValue &&
                    response.Birthday.Date == birthDate.Date &&
                    response.Infos != null &&
                    response.Infos.Length > 0)
                {
                    result.List = response.Infos.Select(
                                                        creditInfo => new CreditRecord(
                                                                                       creditInfo.CreditName,
                                                                                       creditInfo.CreditNumber,
                                                                                       creditInfo.BeginDate,
                                                                                       Convert.ToDecimal(
                                                                                                         creditInfo
                                                                                                                 .CreditAmount),
                                                                                       creditInfo.Currency,
                                                                                       Convert.ToDecimal(
                                                                                                         creditInfo
                                                                                                                 .AmountLeft),
                                                                                       Convert.ToDecimal(
                                                                                                         creditInfo
                                                                                                                 .AmountLate)))
                            .ToList();
                    OracleDb.SaveFacebookCredit(request.ChatId, userId, birthDate);
                    result.Code = ResultCodes.Ok;
                }
                else
                {
                    Log.Warn("Can't find this client by user id and birthdate");
                    result.Code = ResultCodes.NotFound;
                }
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public LocationListResult GetLocationList(LocationRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new LocationListResult();

            try
            {
                Utils.Auth(request);

                List<LocationItem> totalList;
                switch (request.Type)
                {
                    case LocationListType.Atm:
                        totalList = Utils.GetLocationObjectList().Atms;
                        break;

                    case LocationListType.Bank:
                        totalList = Utils.GetLocationObjectList().Branches;
                        break;

                    case LocationListType.CashIn:
                        totalList = Utils.GetLocationObjectList().CashIn;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (request.All)
                {
                    result.Location = totalList;
                }
                else
                {
                    var origin = new Coordinate
                    {
                        Longitude = request.Longitude,
                        Latitude = request.Latitude
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
                        throw new ArgumentOutOfRangeException();
                    }

                    result.Location = resultList;
                }

                result.Code = ResultCodes.Ok;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public StandardResult NewCredit(NewCreditRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new StandardResult();

            try
            {
                Utils.Auth(request);

                var user = OracleDb.GetFacebookUser(request.ChatId, true);
                if (user.LastCreditRequest >= DateTime.Today)
                {
                    Log.Warn("Too many requests");
                    result.Code = ResultCodes.TooManyRequests;
                    throw new ArgumentException(result.Description);
                }

                if (!Utils.CheckPhoneNumber(request.Phone))
                {
                    result.Code = ResultCodes.InvalidParameters;
                    throw new ArgumentException(result.Description);
                }

                var phoneNumber = Utils.FormatPhoneNumber(request.Phone);
                result.Code = Utils.SendCreditRequest(request.ChatId, phoneNumber)
                    ? ResultCodes.Ok
                    : ResultCodes.SystemError;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public StandardResult NewCard(CreditCardRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new StandardResult();

            try
            {
                Utils.Auth(request);
                request.Phone = Utils.FormatPhoneNumber(request.Phone);
                var user = OracleDb.GetFacebookUser(request.ChatId, true);
                if (user.PlasticCardCounts > 0)
                {
                    Log.Warn("Too many requests");
                    result.Code = ResultCodes.TooManyRequests;
                    throw new ArgumentException(result.Description);
                }

                if (!Utils.CheckPhoneNumber(request.Phone))
                {
                    result.Code = ResultCodes.InvalidParameters;
                    throw new ArgumentException(result.Description);
                }

                var phoneNumber = Utils.FormatPhoneNumber(request.Phone);
                result.Code = Utils.SendPlasticCardInfo(request)
                    ? ResultCodes.Ok
                    : ResultCodes.SystemError;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public IdTitlePairResult ListFeedbackCategories(StandardRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new IdTitlePairResult();

            try
            {
                Utils.Auth(request);
                result.Pairs = Utils.ListFeedbackCategories();
                result.Code = ResultCodes.Ok;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public StandardResult SendFeedback(FeedbackRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new StandardResult();

            try
            {
                Utils.Auth(request);
                request.Phone = Utils.FormatPhoneNumber(request.Phone);
                var user = OracleDb.GetFacebookUser(request.ChatId, true);

                if (user.FeedBackCount > 5)
                {
                    Log.Warn("Too many requests");
                    result.Code = ResultCodes.TooManyRequests;
                    throw new ArgumentException(result.Description);
                }

                if (!Utils.CheckPhoneNumber(request.Phone))
                {
                    result.Code = ResultCodes.InvalidParameters;
                    throw new ArgumentException(result.Description);
                }

                result.Code = Utils.SendFeedback(request)
                    ? ResultCodes.Ok
                    : ResultCodes.SystemError;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public IdTitlePairResult ListCardTypes(StandardRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new IdTitlePairResult();

            try
            {
                Utils.Auth(request);

                var rawList = Utils.GetBobSiteCardAndBranchList();
                result.Pairs = rawList.BobSiteCardList.Select(card => new IdTitlePair(card.CardId, card.CardTitle))
                        .ToList();

                result.Code = ResultCodes.Ok;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public IdTitlePairResult ListBranches(StandardRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new IdTitlePairResult();

            try
            {
                Utils.Auth(request);

                var rawList = Utils.GetBobSiteCardAndBranchList();
                result.Pairs = rawList.BobSiteBranchList
                        .Select(branch => new IdTitlePair(branch.BranchId, branch.BranchNameEn))
                        .ToList();

                result.Code = ResultCodes.Ok;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public ChatBotUserResult GetUser(StandardRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new ChatBotUserResult();

            try
            {
                Utils.Auth(request);

                var user = OracleDb.GetFacebookUser(request.ChatId, true);
                if (user == null)
                {
                    OracleDb.SaveFacebookUser(request.ChatId, String.Empty, String.Empty);
                    user = OracleDb.GetFacebookUser(request.ChatId, true);
                    result.User = user;
                    result.Code = ResultCodes.UserNotActivated;
                }
                else
                {
                    result.User = user;
                    result.Code = ResultCodes.Ok;
                }               
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public StandardResult UpdateUserSettings(UpdateSettingsRequest request)
        {
            Log.Info("Request: {0},{1}IP: {2}", request, Environment.NewLine, Utils.ObtainIp());
            var result = new StandardResult();

            try
            {
                Utils.Auth(request);
                string lang = String.Empty;                
                switch (request.Lang)
                {
                        case Language.Az:
                            lang = "az";
                            break;

                        case Language.En:
                            lang = "en";
                            break;

                        case Language.Ru:
                            lang = "ru";
                            break;
                }

                OracleDb.UpdateFacebookReceiveSettings(request.ChatId, (int) request.Settings, lang);
                result.Code = ResultCodes.Ok;
            }
            catch (ArgumentException)
            {
                Log.Warn(result.Description);
            }
            catch (InvalidCredentialException)
            {
                result.Code = ResultCodes.InvalidUsernameOrPassword;
                Log.Warn(result.Description + " " + request.Username);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return result;
        }
    }
}
