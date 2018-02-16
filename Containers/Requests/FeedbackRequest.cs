using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers.Requests
{
    [Serializable, XmlRoot("FeedbackRequest")]
    [DataContract(Name = "FeedbackRequest", Namespace = "urn:BankOfBaku")]
    public class FeedbackRequest : StandardRequest
    {
        [XmlElement]
        [DataMember]
        public String Phone { get; set; }

        [XmlElement]
        [DataMember]
        public String Text { get; set; }

        [XmlElement]
        [DataMember]
        public String Type { get; set; }

        public FeedbackRequest()
        {
        }

        public FeedbackRequest(string username, string password, string chatId)
            : base(username, password, chatId)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, Phone: {1}, Text: {2}, Type: {3}", base.ToString(), Phone, Text, Type);
        }
    }
}
