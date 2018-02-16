using System;
using BobTelegramBot.InternetBankingMobileServiceReference;
using Containers.Requests;
using PersonSex = Containers.Enums.PersonSex;

namespace BobTelegramBot.Structures
{
    class ChatBotCreditCardRequest : CreditCardRequest
    {      
        public CreditCardStep CreditCardStep { get; set; }

        public CreditCardStep PrevCreditCardStep { get; set; }

        public ChatBotCreditCardRequest()
        {
        }

        public ChatBotCreditCardRequest(CreditCardStep creditCardStep)
        {
            PrevCreditCardStep = CreditCardStep.None;
            CreditCardStep = creditCardStep;
        }

        public ChatBotCreditCardRequest(
            CreditCardStep creditCardStep,
            string phone,
            PersonSex sex,
            string firstName,
            string lastName,
            string surname,
            DateTime birthDate,
            string passportFront,
            string passportBack,
            string passportNumber,
            string passportOrgan,
            DateTime passportDate,
            string registrationAddress,
            string livingAddress,
            string homePhone,
            string workPhone,
            string email,
            int creditCardType,
            string currency,
            int period,
            string orderType,
            int branchId)
        {
            CreditCardStep = creditCardStep;
            Phone = phone;
            Sex = sex;
            FirstName = firstName;
            LastName = lastName;
            Surname = surname;
            BirthDate = birthDate;
            PassportFront = passportFront;
            PassportBack = passportBack;
            PassportNumber = passportNumber;
            PassportOrgan = passportOrgan;
            PassportDate = passportDate;
            RegistrationAddress = registrationAddress;
            LivingAddress = livingAddress;
            HomePhone = homePhone;
            WorkPhone = workPhone;
            Email = email;
            CreditCardType = creditCardType;
            Currency = currency;
            Period = period;
            OrderType = orderType;
            BranchId = branchId;
        }

        public BobSiteCardOrderRequest ToBobSiteCardOrderRequest()
        {
            var request = new BobSiteCardOrderRequest
            {
                BirthDate = BirthDate,
                BranchId = BranchId,
                CardId = CreditCardType,
                CardName = CardName,
                CardSurname = CardLastName,
                Currency = Currency,
                Email = Email,
                HomePhone = HomePhone,
                LivingAddress = LivingAddress,
                MobilePhone = Phone,
                Name = FirstName,
                Code = Code,
                Surname = LastName,
                WorkPhone = WorkPhone,
                PassportOrgan = PassportOrgan,
                PassportDate = PassportDate,
                PassportBack = PassportBack,
                Period = Period,
                OrderType = OrderType,
                PassportNumber = PassportNumber,
                PassportFront = PassportFront,
                PatronymicName = Surname,
                RegAddress = RegistrationAddress,
                Sex = Sex == PersonSex.Male
                    ? InternetBankingMobileServiceReference.PersonSex.Male
                    : InternetBankingMobileServiceReference.PersonSex.Female
            };

            return request;
        }

        public override string ToString()
        {
            return string.Format(
                                 "{0}, CreditCardStep: {1}, PrevCreditCardStep: {2}",
                                 base.ToString(),
                                 CreditCardStep,
                                 PrevCreditCardStep);
        }
    }
}
