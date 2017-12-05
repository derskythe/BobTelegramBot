namespace BobTelegramBot.Structures
{
    class Feedback
    {
        public string Phone { get; set; }
        public string Type { get; set; }

        public Feedback(string text, string type)
        {
            Phone = text;
            Type = type;
        }

        public override string ToString()
        {
            return string.Format("Phone: {0}, Type: {1}", Phone, Type);
        }
    }
}
