using System;
using System.ServiceModel;
using Common;

namespace Service
{
    // persession, svaki klijent ima individualnu instancu servisa
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class MotorMonitoringService : IMotorMonitoringService
    {
        private SessionMeta _meta;
        private int _received;
        private SessionStatus _status = SessionStatus.NOT_STARTED;

        public AckResponse StartSession(SessionMeta meta)
        {
            // validacija meta polja
            if (meta == null)
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Field = "meta", Reason = "Meta ne smije biti null." });

            if (string.IsNullOrWhiteSpace(meta.SessionId))
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Field = "SessionId", Reason = "Obavezno polje." });

            if (string.IsNullOrWhiteSpace(meta.ColumnsHeader))
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Field = "ColumnsHeader", Reason = "Obavezno polje." });

            _meta = meta;
            _received = 0;
            _status = SessionStatus.IN_PROGRESS;

            Console.WriteLine($"[Servis] StartSession: id={meta.SessionId}, Ocekivano={meta.ExpectedRowCount}");

            return new AckResponse
            {
                Ack = AckType.ACK,
                Status = _status,
                Message = $"Sesija {meta.SessionId} pokrenuta."
            };
        }

        public AckResponse PushSample(MotorSample sample)
        {
            // sesija mora biti aktivna
            if (_status != SessionStatus.IN_PROGRESS)
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Field = "Session", Reason = "Sesija nije pokrenuta." });

            ValidateSample(sample);

            _received++;
            Console.WriteLine($"[Servis] Sample #{_received} (red {sample.RowIndex}) primljen.");

            // upis u csv fajl za kt2
            return new AckResponse
            {
                Ack = AckType.ACK,
                Status = _status,
                Message = $"Primljen uzorak #{_received}."
            };
        }

        public AckResponse EndSession()
        {
            _status = SessionStatus.COMPLETED;
            Console.WriteLine($"[Servis] EndSession. Ukupno primljeno: {_received}");

            return new AckResponse
            {
                Ack = AckType.ACK,
                Status = _status,
                Message = $"Sesija zavrsena. Primljeno {_received} uzoraka."
            };
        }

        // provjera tipova i dozvoljenih opsega
        private void ValidateSample(MotorSample s)
        {
            if (s == null)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault { Detail = "Uzorak je null." });

            if (double.IsNaN(s.U_q) || double.IsInfinity(s.U_q) ||
                double.IsNaN(s.U_d) || double.IsInfinity(s.U_d) ||
                double.IsNaN(s.Motor_Speed) || double.IsInfinity(s.Motor_Speed))
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault { Detail = "NaN/Infinity vrednost u U_q/U_d/Motor_Speed." });

            if (s.Motor_Speed <= 0)
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Field = "Motor_Speed", Reason = "Mora biti > 0." });

            if (s.Profile_Id < 0)
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Field = "Profile_Id", Reason = "Mora biti >= 0." });
        }
    }
}
