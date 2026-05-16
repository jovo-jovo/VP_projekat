using System.Runtime.Serialization;

namespace Common
{
    // greska kada format podatka nije ispravan
    [DataContract]
    public class DataFormatFault
    {
        [DataMember] public string Detail { get; set; }
    }
}
