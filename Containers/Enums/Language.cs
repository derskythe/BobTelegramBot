using System;
using System.Runtime.Serialization;

namespace Containers.Enums
{
    [Serializable]
    [DataContract(Name = "Language")]
    public enum Language
    {
        [EnumMember]
        En = 0,
        [EnumMember]
        Az = 1,
        [EnumMember]
        Ru = 2
    }
}
