using System;

namespace BobTelegramBot.Structures
{
    class ClientInfo
    {
        public string ClientId { get; set; }
        public DateTime BirthDate { get; set; }

        public ClientInfo()
        {
            BirthDate = DateTime.MinValue;
        }

        public ClientInfo(string clientId, DateTime birthDate)
        {
            ClientId = clientId;
            BirthDate = birthDate;
        }

        public override string ToString()
        {
            return string.Format("ClientId: {0}, BirthDate: {1}", ClientId, BirthDate);
        }
    }
}
