using System;
using System.Runtime.Serialization;

namespace Containers.Enums
{
    [Serializable]
    [DataContract(Name = "BotUserType")]
    public enum BotUserType
    {
        [EnumMember(Value = "0")]
        Telegram = 0,
        [EnumMember(Value = "1")]
        Facebook = 1
    }
}
