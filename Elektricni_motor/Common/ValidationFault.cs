using System.Runtime.Serialization;

namespace Common
{
    // greska kada vrijednost ne odgovara trazenom formatu
    [DataContract]
    public class ValidationFault
    {
        [DataMember] public string Field { get; set; }
        [DataMember] public string Reason { get; set; }
    }
}
