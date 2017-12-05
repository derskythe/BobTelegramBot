using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers.Requests
{
    [Serializable, XmlRoot("StandardRequest")]
    [DataContract(Name = "StandardRequest", Namespace = "urn:BankOfBaku")]
    public class StandardRequest
    {
        [XmlElement]
        [DataMember]
        public string Username { get; set; }

        [XmlElement]
        [DataMember]
        public string Password { get; set; }

        [XmlElement]
        [DataMember]
        public String ChatId { get; set; }

        public StandardRequest()
        {
        }

        public StandardRequest(string username, string password, String chatId)
        {
            Username = username;
            Password = password;
            ChatId = chatId;
        }

        public override string ToString()
        {
            return string.Format("Username: {0}, Password: {1}, ChatId: {2}", Username, Password, ChatId);
        }
    }
}
