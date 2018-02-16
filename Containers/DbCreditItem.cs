using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("DbCreditItem")]
    [DataContract(Name = "DbCreditItem", Namespace = "urn:BankOfBaku")]
    public class DbCreditItem
    {
        [XmlElement]
        [DataMember]
        public long Id { get; set; }

        [XmlElement]
        [DataMember]
        public string UserId { get; set; }

        [XmlElement]
        [DataMember]
        public string ClientId { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime BirthDate { get; set; }

        public DbCreditItem()
        {
        }

        public DbCreditItem(long id, string userId, string clientId, DateTime birthDate)
        {
            Id = id;
            UserId = userId;
            ClientId = clientId;
            BirthDate = birthDate;
        }

        public override string ToString()
        {
            return string.Format(
                                 "Id: {0}, UserId: {1}, ClientId: {2}, BirthDate: {3}",
                                 Id,
                                 UserId,
                                 ClientId,
                                 BirthDate);
        }
    }
}
