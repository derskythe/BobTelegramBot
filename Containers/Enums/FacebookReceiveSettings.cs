using System;
using System.Runtime.Serialization;

namespace Containers.Enums
{
    [Serializable]
    [DataContract(Name = "FacebookReceiveSettings")]
    public enum FacebookReceiveSettings
    {
        [EnumMember(Value = "0")]
        Bot = 0,
        [EnumMember(Value = "1")]
        Human = 1
    }
}
