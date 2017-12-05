using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("CompositeList")]
    [DataContract(Name = "CompositeList", Namespace = "urn:BankOfBaku")]
    public class CompositeList
    {
        [XmlElement]
        [DataMember]
        public List<LocationItem> Branches { get; set; }

        [XmlElement]
        [DataMember]
        public List<LocationItem> Atms { get; set; }

        [XmlElement]
        [DataMember]
        public List<LocationItem> CashIn { get; set; }

        public CompositeList()
        {
        }

        public CompositeList(List<LocationItem> branches, List<LocationItem> atms, List<LocationItem> cashList)
        {
            Branches = branches;
            Atms = atms;
            CashIn = cashList;
        }
    }
}
