namespace BobTelegramBot.Structures
{
    class CurrencyCalculator
    {
        public string From { get; set; }
        public string To { get; set; }

        public CurrencyCalculator(string fromValue, string to)
        {
            From = fromValue;
            To = to;
        }

        public override string ToString()
        {
            return string.Format("From: {0}, To: {1}", From, To);
        }
    }
}
