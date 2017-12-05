using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Containers.Enums;

namespace Containers.Results
{
    [Serializable, XmlRoot("StandardResult")]
    [DataContract(Name = "StandardResult", Namespace = "urn:BankOfBaku")]
    public class StandardResult
    {
        private ResultCodes _Code;
        private String _Description;

        [XmlElement(ElementName = "ResultCodes")]
        [DataMember(Name = "ResultCodes")]
        public ResultCodes Code
        {
            get
            {
                return _Code;
            }
            set
            {
                _Code = value;
                _Description = EnumEx.GetDescription(_Code);
            }
        }

        [XmlElement(ElementName = "Description")]
        [DataMember(Name = "Description")]
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        public StandardResult()
        {
            Code = ResultCodes.UnknownError;
        }

        public StandardResult(ResultCodes code)
        {
            Code = code;
        }

        public override string ToString()
        {
            return string.Format("Code: {0}, Description: {1}", _Code, _Description);
        }
    }
}
