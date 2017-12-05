using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers.Requests
{
    [Serializable, XmlRoot("CreditListRequest")]
    [DataContract(Name = "CreditListRequest", Namespace = "urn:BankOfBaku")]
    public class CreditListRequest : StandardRequest
    {
        [XmlElement]
        [DataMember]
        public String UserId { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime BirthDate { get; set; }

        public CreditListRequest()
        {
        }

        public CreditListRequest(string username, string password, string chatId)
            : base(username, password, chatId)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, UserId: {1}, BirthDate: {2}", base.ToString(), UserId, BirthDate);
        }
    }
}
