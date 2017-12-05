using System;

namespace Containers
{
    public class TelegramMessage
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public String MessageText { get; set; }
        public bool IsSent { get; set; }
        public DateTime UpdateDate { get; set; }

        public TelegramMessage()
        {
        }

        public TelegramMessage(long id, long userId, String messageText, bool isSent, DateTime updateDate)
        {
            Id = id;
            UserId = userId;
            MessageText = messageText;
            IsSent = isSent;
            UpdateDate = updateDate;
        }

        public override string ToString()
        {
            return string.Format(
                                 "Id: {0}, UserId: {1}, MessageText: {2}, IsSent: {3}, UpdateDate: {4}",
                                 Id,
                                 UserId,
                                 MessageText,
                                 IsSent,
                                 UpdateDate);
        }
    }
}
