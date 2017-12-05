using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("Branch")]
    [DataContract(Name = "Branch", Namespace = "urn:BankOfBaku")]
    public class Branch
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
        public decimal Latitude { get; set; }

        [XmlElement]
        [DataMember]
        public decimal Longitude { get; set; }

        [XmlElement]
        [DataMember]
        public bool Active { get; set; }    

        public Branch()
        {
        }

        public Branch(int id, MultiLanguageString title, MultiLanguageString desc, decimal latitude, decimal longitude, bool active)
        {
            Id = id;
            Title = title;
            Desc = desc;
            Latitude = latitude;
            Longitude = longitude;
            Active = active;
        }

        public override string ToString()
        {
            return string.Format(
                                 "Id: {0}, Title: {1}, Desc: {2}, Latitude: {3}, Longitude: {4}, Active: {5}",
                                 Id,
                                 Title,
                                 Desc,
                                 Latitude,
                                 Longitude,
                                 Active);
        }
    }
}
