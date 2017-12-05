using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers.Requests
{
    [Serializable, XmlRoot("LocationRequest")]
    [DataContract(Name = "LocationRequest", Namespace = "urn:BankOfBaku")]
    public class LocationRequest : StandardRequest
    {
        [XmlElement]
        [DataMember]
        public double Longitude { get; set; }

        [XmlElement]
        [DataMember]
        public double Latitude { get; set; }

        [XmlElement]
        [DataMember]
        public bool All { get; set; }

        [XmlElement]
        [DataMember]
        public LocationListType Type { get; set; }

        public LocationRequest()
        {
        }

        public LocationRequest(string username, string password, string chatId)
            : base(username, password, chatId)
        {
        }

        public override string ToString()
        {
            return string.Format(
                                 "{0}, Longitude: {1}, Latitude: {2}, All: {3}, Type: {4}",
                                 base.ToString(),
                                 Longitude,
                                 Latitude,
                                 All,
                                 Type);
        }
    }
}
