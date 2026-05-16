using System;
using System.Runtime.Serialization;

namespace Common
{
    // meta zaglavlje koje klijent salje na pocetku sesije
    [DataContract]
    public class SessionMeta
    {
        [DataMember] public string SessionId { get; set; }
        [DataMember] public DateTime StartTime { get; set; }

        // spisak kolona sampla
        [DataMember] public string ColumnsHeader { get; set; }

        [DataMember] public int ExpectedRowCount { get; set; }
    }
}
