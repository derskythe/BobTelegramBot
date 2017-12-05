using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers.Results
{
    [Serializable, XmlRoot("CreditListResult")]
    [DataContract(Name = "CreditListResult", Namespace = "urn:BankOfBaku")]
    public class CreditListResult: StandardResult
    {
        [XmlElement]
        [DataMember]
        public List<CreditRecord> List { get; set; }

        public CreditListResult()
        {
        }

        public CreditListResult(ResultCodes code) : base(code)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, List: {1}", base.ToString(), EnumEx.GetStringFromArray(List));
        }
    }
}
