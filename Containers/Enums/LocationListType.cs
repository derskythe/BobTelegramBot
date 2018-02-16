using System;
using System.Runtime.Serialization;

namespace Containers.Enums
{
    [Serializable]
    [DataContract(Name = "LocationListType")]
    public enum LocationListType
    {
        [EnumMember(Value = "0")]
        Atm = 0,
        [EnumMember(Value = "1")]
        Bank = 1,
        [EnumMember(Value = "2")]
        CashIn = 2
    }
}
