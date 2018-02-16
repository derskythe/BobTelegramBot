using System.ServiceModel;
using Containers.Requests;
using Containers.Results;

namespace FacebookSoapService
{
    [ServiceContract(Namespace = "http://fbbot.ebankofbaku.com/FacebookService",
         Name = "FacebookService"), XmlSerializerFormat]
    public interface IFacebookService
    {
        [OperationContract]
        CurrencyRateResult GetCurrencyRate(StandardRequest request);

        [OperationContract]
        CreditListResult GetCreditList(CreditListRequest request);

        [OperationContract]
        LocationListResult GetLocationList(LocationRequest request);

        [OperationContract]
        StandardResult NewCredit(NewCreditRequest request);

        [OperationContract]
        StandardResult NewCard(CreditCardRequest request);

        [OperationContract]
        IdTitlePairResult ListFeedbackCategories(StandardRequest request);

        [OperationContract]
        StandardResult SendFeedback(FeedbackRequest request);

        [OperationContract]
        IdTitlePairResult ListCardTypes(StandardRequest request);

        [OperationContract]
        IdTitlePairResult ListBranches(StandardRequest request);

        [OperationContract]
        ChatBotUserResult GetUser(StandardRequest request);

        [OperationContract]
        StandardResult UpdateUserSettings(UpdateSettingsRequest request);
    }
}