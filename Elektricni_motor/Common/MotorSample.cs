using System.Runtime.Serialization;

namespace Common
{
    // jedan red iz csva
    [DataContract]
    public class MotorSample
    {
        [DataMember] public double U_q { get; set; }
        [DataMember] public double U_d { get; set; }
        [DataMember] public double Motor_Speed { get; set; }
        [DataMember] public int Profile_Id { get; set; }
        [DataMember] public double Ambient { get; set; }
        [DataMember] public double Torque { get; set; }

        // redni broj u csvu za logovanje
        [DataMember] public int RowIndex { get; set; }
    }
}
