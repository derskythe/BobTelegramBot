using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("MultilanguageString")]
    [DataContract(Name = "MultilanguageString", Namespace = "urn:BankOfBaku")]
    public class MultiLanguageString
    {
        [XmlElement(ElementName = "Az")]
        [DataMember(Name = "Az")]
        public string Az { get; set; }

        [XmlElement(ElementName = "En")]
        [DataMember(Name = "En")]
        public string En { get; set; }

        [XmlElement(ElementName = "Ru")]
        [DataMember(Name = "Ru")]
        public string Ru { get; set; }

        public MultiLanguageString()
        {
            Az = String.Empty;
            En = String.Empty;
            Ru = String.Empty;
        }

        public MultiLanguageString(string az, string en, string ru)
        {
            Az = az;
            En = en;
            Ru = ru;
        }

        public override string ToString()
        {
            return string.Format("Az: {0}, En: {1}, Ru: {2}", Az, En, Ru);
        }
    }
}
