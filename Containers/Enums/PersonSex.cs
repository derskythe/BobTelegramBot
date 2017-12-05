using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Containers.Enums
{
    [Serializable]
    [DataContract(Name = "PersonSex")]
    public enum PersonSex
    {
        [EnumMember(Value = "0")]
        Male,
        [EnumMember(Value = "1")]
        Female
    }
}
