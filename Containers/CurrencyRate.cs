using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("CurrencyRate")]
    [DataContract(Name = "CurrencyRate", Namespace = "urn:BankOfBaku")]
    public class CurrencyRate : ICloneable
    {
        [XmlElement]
        [DataMember]
        public string Currency { get; set; }

        [XmlElement]
        [DataMember]
        public string Flag { get; set; }

        [XmlElement]
        [DataMember]
        public decimal BuyRate { get; set; }

        [XmlElement]
        [DataMember]
        public decimal SellRate { get; set; }

        public CurrencyRate()
        {
        }

        public CurrencyRate(string currency, string flag, decimal buyRate, decimal sellRate)
        {
            Currency = currency;
            Flag = flag;
            BuyRate = buyRate;
            SellRate = sellRate;
        }

        public object Clone()
        {
            return new CurrencyRate(Currency, Flag, BuyRate, SellRate);
        }

        public override string ToString()
        {
            return string.Format(
                                 "Currency: {0}, Flag: {1}, BuyRate: {2}, SellRate: {3}",
                                 Currency,
                                 Flag,
                                 BuyRate,
                                 SellRate);
        }
    }
}
