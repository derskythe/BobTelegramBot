using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers.Results
{
    [Serializable, XmlRoot("ChatBotUserResult")]
    [DataContract(Name = "ChatBotUserResult", Namespace = "urn:BankOfBaku")]
    public class ChatBotUserResult : StandardResult
    {
        [XmlElement]
        [DataMember]
        public BotUser User { get; set; }

        public ChatBotUserResult()
        {
        }

        public ChatBotUserResult(ResultCodes code) : base(code)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, User: {1}", base.ToString(), User);
        }
    }
}
