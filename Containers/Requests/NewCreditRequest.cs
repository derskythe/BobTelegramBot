using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers.Requests
{
    [Serializable, XmlRoot("NewCreditRequest")]
    [DataContract(Name = "NewCreditRequest", Namespace = "urn:BankOfBaku")]
    public class NewCreditRequest : StandardRequest
    {
        [XmlElement]
        [DataMember]
        public string Phone { get; set; }

        public NewCreditRequest()
        {
        }

        public NewCreditRequest(string username, string password, string chatId)
            : base(username, password, chatId)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, Phone: {1}", base.ToString(), Phone);
        }
    }
}
