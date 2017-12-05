using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("CreditRecord")]
    [DataContract(Name = "CreditRecord", Namespace = "urn:BankOfBaku")]
    public class CreditRecord
    {
        [XmlElement]
        [DataMember]
        public string CreditName { get; set; }

        [XmlElement]
        [DataMember]
        public string CreditNumber { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime BeginDate { get; set; }

        [XmlElement]
        [DataMember]
        public decimal CreditAmount { get; set; }

        [XmlElement]
        [DataMember]
        public string Currency { get; set; }

        [XmlElement]
        [DataMember]
        public decimal AmountLeft { get; set; }

        [XmlElement]
        [DataMember]
        public decimal AmountLate { get; set; }

        public CreditRecord()
        {
        }

        public CreditRecord(
            string creditName,
            string creditNumber,
            DateTime beginDate,
            decimal creditAmount,
            string currency,
            decimal amountLeft,
            decimal amountLate)
        {
            CreditName = creditName;
            CreditNumber = creditNumber;
            BeginDate = beginDate;
            CreditAmount = creditAmount;
            Currency = currency;
            AmountLeft = amountLeft;
            AmountLate = amountLate;
        }

        public override string ToString()
        {
            return string.Format(
                                 "CreditName: {0}, CreditNumber: {1}, BeginDate: {2}, CreditAmount: {3}, Currency: {4}, AmountLeft: {5}, AmountLate: {6}",
                                 CreditName,
                                 CreditNumber,
                                 BeginDate,
                                 CreditAmount,
                                 Currency,
                                 AmountLeft,
                                 AmountLate);
        }
    }
}
