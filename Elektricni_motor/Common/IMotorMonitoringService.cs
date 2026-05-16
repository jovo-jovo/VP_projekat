using System.ServiceModel;

namespace Common
{
    // 3 operacije servisa: start, push i end
    [ServiceContract]
    public interface IMotorMonitoringService
    {
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        AckResponse StartSession(SessionMeta meta);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        AckResponse PushSample(MotorSample sample);

        [OperationContract]
        AckResponse EndSession();
    }
}
