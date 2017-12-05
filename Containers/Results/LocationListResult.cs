using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers.Results
{
    [Serializable, XmlRoot("LocationListResult")]
    [DataContract(Name = "LocationListResult", Namespace = "urn:BankOfBaku")]
    public class LocationListResult : StandardResult
    {
        [XmlElement]
        [DataMember]
        public List<LocationItem> Location { get; set; }

        public LocationListResult()
        {
        }

        public LocationListResult(ResultCodes code) : base(code)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, Location: {1}", base.ToString(), EnumEx.GetStringFromArray(Location));
        }
    }
}
