using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("LocationItem")]
    [DataContract(Name = "LocationItem", Namespace = "urn:BankOfBaku")]
    public class LocationItem
    {
        [XmlElement]
        [DataMember]
        public int Id { get; set; }

        [XmlElement]
        [DataMember]
        public MultiLanguageString Title { get; set; }

        [XmlElement]
        [DataMember]
        public MultiLanguageString Desc { get; set; }

        [XmlElement]
        [DataMember]
        public double Latitude { get; set; }

        [XmlElement]
        [DataMember]
        public double Longitude { get; set; }

        public LocationItem()
        {
        }

        public LocationItem(
            int id,
            MultiLanguageString title,
            MultiLanguageString desc,
            double latitude,
            double longitude)
        {
            Id = id;
            Title = title;
            Desc = desc;
            Latitude = latitude;
            Longitude = longitude;
        }        

        public override string ToString()
        {
            return string.Format(
                                 "Id: {0}, Title: {1}, Desc: {2}, Latitude: {3}, Longitude: {4}",
                                 Id,
                                 Title,
                                 Desc,
                                 Latitude,
                                 Longitude);
        }

        
    }
}
