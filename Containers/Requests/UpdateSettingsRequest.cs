using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers.Requests
{
    [Serializable, XmlRoot("UpdateSettingsRequest")]
    [DataContract(Name = "UpdateSettingsRequest", Namespace = "urn:BankOfBaku")]
    public class UpdateSettingsRequest : StandardRequest
    {
        [XmlElement]
        [DataMember]
        public FacebookReceiveSettings Settings { get; set; }

        [XmlElement]
        [DataMember]
        public Language Lang { get; set; }

        public UpdateSettingsRequest()
        {
        }

        public UpdateSettingsRequest(string username, string password, string chatId) : base(username, password, chatId)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, Settings: {1}, Lang: {2}", base.ToString(), Settings, Lang);
        }
    }
}
