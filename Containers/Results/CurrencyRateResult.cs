using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers.Results
{
    [Serializable, XmlRoot("CurrencyRateResult")]
    [DataContract(Name = "CurrencyRateResult", Namespace = "urn:BankOfBaku")]
    public class CurrencyRateResult : StandardResult
    {
        [XmlElement]
        [DataMember]
        public List<CurrencyRate> Rates { get; set; }

        public CurrencyRateResult()
        {
        }

        public CurrencyRateResult(ResultCodes code) : base(code)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, Rates: {1}", base.ToString(), EnumEx.GetStringFromArray(Rates));
        }
    }
}
