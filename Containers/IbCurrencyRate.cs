using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("IbCurrencyRate")]
    [DataContract(Name = "IbCurrencyRate", Namespace = "urn:BankOfBaku")]
    public class IbCurrencyRate
    {
        [XmlElement]
        [DataMember]
        public String Currency { get; set; }

        [XmlElement]
        [DataMember]
        public decimal Rate { get; set; }

        [XmlElement]
        [DataMember]
        public decimal SellRate { get; set; }

        [XmlElement]
        [DataMember]
        public decimal BuyRate { get; set; }

        public IbCurrencyRate()
        {
        }

        public IbCurrencyRate(string currency, decimal rate, decimal sellRate, decimal buyRate)
        {
            Currency = currency;
            Rate = rate;
            SellRate = sellRate;
            BuyRate = buyRate;
        }

        public override string ToString()
        {
            return string.Format(
                                 "Currency: {0}, Rate: {1}, SellRate: {2}, BuyRate: {3}",
                                 Currency,
                                 Rate,
                                 SellRate,
                                 BuyRate);
        }
    }
}
