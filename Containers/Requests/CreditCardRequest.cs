using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;
using NLog;

namespace Containers.Requests
{
    [Serializable, XmlRoot("CreditCardRequest")]
    [DataContract(Name = "CreditCardRequest", Namespace = "urn:BankOfBaku")]
    public class CreditCardRequest : StandardRequest
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        [XmlElement]
        [DataMember]
        public string Phone { get; set; }

        [XmlElement]
        [DataMember]
        public PersonSex Sex { get; set; }

        [XmlElement]
        [DataMember]
        public string FirstName { get; set; }

        [XmlElement]
        [DataMember]
        public string LastName { get; set; }

        [XmlElement]
        [DataMember]
        public string Surname { get; set; }

        [XmlElement]
        [DataMember]
        public string CardName { get; set; }

        [XmlElement]
        [DataMember]
        public string CardLastName { get; set; }

        [XmlElement]
        [DataMember]
        public string Code { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime BirthDate { get; set; }

        [XmlElement]
        [DataMember]
        public String PassportFront { get; set; }

        [XmlElement]
        [DataMember]
        public String PassportBack { get; set; }

        [XmlElement]
        [DataMember]
        public string PassportNumber { get; set; }

        [XmlElement]
        [DataMember]
        public string PassportOrgan { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime PassportDate { get; set; }

        [XmlElement]
        [DataMember]
        public string RegistrationAddress { get; set; }

        [XmlElement]
        [DataMember]
        public string LivingAddress { get; set; }

        [XmlElement]
        [DataMember]
        public string HomePhone { get; set; }

        [XmlElement]
        [DataMember]
        public string WorkPhone { get; set; }

        [XmlElement]
        [DataMember]
        public string Email { get; set; }

        [XmlElement]
        [DataMember]
        public int CreditCardType { get; set; }

        [XmlElement]
        [DataMember]
        public string Currency { get; set; }

        [XmlElement]
        [DataMember]
        public int Period { get; set; }

        [XmlElement]
        [DataMember]
        public string OrderType { get; set; }

        [XmlElement]
        [DataMember]
        public int BranchId { get; set; }

        public CreditCardRequest()
        {
        }

        public CreditCardRequest(string username, string password, string chatId) : base(username, password, chatId)
        {
        }

        private static int Base64ImageSizeKb(String imageBase64Encoded)
        {
            try
            {
                if (String.IsNullOrEmpty(imageBase64Encoded))
                {
                    return 0;
                }
                var size = Convert.FromBase64String(imageBase64Encoded);
                return size.Length;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Format(
                                 "CreditCardStep: {0}, Phone: {1}, Sex: {2}, FirstName: {3}, LastName: {4}, Patronomyc: {5}, BirthDate: {6}, PassportFront: {7}, PassportBack: {8}, PassportNumber: {9}, PassportOrgan: {10}, PassportDate: {11}, RegistrationAddress: {12}, LivingAddress: {13}, HomePhone: {14}, WorkPhone: {15}, Email: {16}, CreditCardType: {17}, Currency: {18}, Period: {19}, OrderType: {20}, BranchId: {21}",
                                 0,
                                 Phone,
                                 Sex,
                                 FirstName,
                                 LastName,
                                 Surname,
                                 BirthDate,
                                 Base64ImageSizeKb(PassportFront) + " bytes",
                                 Base64ImageSizeKb(PassportBack) + " bytes",
                                 PassportNumber,
                                 PassportOrgan,
                                 PassportDate,
                                 RegistrationAddress,
                                 LivingAddress,
                                 HomePhone,
                                 WorkPhone,
                                 Email,
                                 CreditCardType,
                                 Currency,
                                 Period,
                                 OrderType,
                                 BranchId);
        }
    }
}
