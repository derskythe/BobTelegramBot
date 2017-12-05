using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers
{
    [Serializable, XmlRoot("BotUser")]
    [DataContract(Name = "BotUser", Namespace = "urn:BankOfBaku")]
    public class BotUser
    {
        [XmlElement]
        [DataMember]
        public string UserId { get; set; }

        [XmlElement]
        [DataMember]
        public string PhoneNumber { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime AddDate { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime UpdateDate { get; set; }

        [XmlElement]
        [DataMember]
        public int ReceiveSettings { get; set; }

        [XmlElement]
        [DataMember]
        public String Username { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime LastFeedback { get; set; }

        [XmlElement]
        [DataMember]
        public int FeedBackCount { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime LastCreditRequest { get; set; }

        [XmlElement]
        [DataMember]
        public int PlasticCardCounts { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime LastPlasticCard { get; set; }

        [XmlElement]
        [DataMember]
        public BotUserType UserType { get; set; }

        [XmlElement]
        [DataMember]
        public Language Language { get; set; }

        public BotUser()
        {
        }

        public BotUser(
            string userId,
            string phoneNumber,
            DateTime addDate,
            DateTime updateDate,
            int receiveSettings,
            string username,
            DateTime lastFeedback,
            int feedBackCount,
            DateTime lastCreditRequest,
            int plasticCardCounts,
            DateTime lastPlasticCard,
            BotUserType userType,
            Language lang)
        {
            UserId = userId;
            PhoneNumber = phoneNumber;
            AddDate = addDate;
            UpdateDate = updateDate;
            ReceiveSettings = receiveSettings;
            Username = username;
            LastFeedback = lastFeedback;
            FeedBackCount = feedBackCount;
            LastCreditRequest = lastCreditRequest;
            PlasticCardCounts = plasticCardCounts;
            LastPlasticCard = lastPlasticCard;
            UserType = userType;
            Language = lang;
        }

        public override string ToString()
        {
            return string.Format("UserId: {0}, PhoneNumber: {1}, AddDate: {2}, UpdateDate: {3}, ReceiveSettings: {4}, Username: {5}, LastFeedback: {6}, FeedBackCount: {7}, LastCreditRequest: {8}, PlasticCardCounts: {9}, LastPlasticCard: {10}, UserType: {11}, Language: {12}", UserId, PhoneNumber, AddDate, UpdateDate, ReceiveSettings, Username, LastFeedback, FeedBackCount, LastCreditRequest, PlasticCardCounts, LastPlasticCard, UserType, Language);
        }
    }
}
