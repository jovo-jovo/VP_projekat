using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public enum AckType
    {
        [EnumMember] ACK,
        [EnumMember] NACK
    }

    [DataContract]
    public enum SessionStatus
    {
        [EnumMember] NOT_STARTED,
        [EnumMember] IN_PROGRESS,
        [EnumMember] COMPLETED
    }

    // odgovor servisa na svaku operaciju
    [DataContract]
    public class AckResponse
    {
        [DataMember] public AckType Ack { get; set; }
        [DataMember] public SessionStatus Status { get; set; }
        [DataMember] public string Message { get; set; }
    }
}
