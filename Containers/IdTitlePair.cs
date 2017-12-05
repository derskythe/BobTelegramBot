using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("IdTitlePair")]
    [DataContract(Name = "IdTitlePair", Namespace = "urn:BankOfBaku")]
    public class IdTitlePair
    {
        [XmlElement]
        [DataMember]
        public string Id { get; set; }

        [XmlElement]
        [DataMember]
        public string Title { get; set; }

        public IdTitlePair()
        {
        }

        public IdTitlePair(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Title: {1}", Id, Title);
        }
    }
}
