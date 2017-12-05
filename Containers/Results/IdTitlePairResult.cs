using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers.Results
{
    [Serializable, XmlRoot("IdTitlePairResult")]
    [DataContract(Name = "IdTitlePairResult", Namespace = "urn:BankOfBaku")]
    public class IdTitlePairResult : StandardResult
    {
        [XmlElement]
        [DataMember]
        public List<IdTitlePair> Pairs { get; set; }

        public IdTitlePairResult()
        {
        }

        public IdTitlePairResult(ResultCodes code)
            : base(code)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}, Pairs: {1}", base.ToString(), EnumEx.GetStringFromArray(Pairs));
        }
    }
}
