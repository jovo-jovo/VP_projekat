using System;
using System.ServiceModel;
using System.Configuration;
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

        private MeasurementWriter _writer;
        private AnalyticsEngine _analytics;
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

            // kreiranje fajlova, analitike i pretplata loggera
            string folder = ConfigurationManager.AppSettings["storagePath"] ?? "Measurements";
            _writer = new MeasurementWriter(folder, meta.SessionId);
            _analytics = new AnalyticsEngine();
            new EventLogger().Subscribe(_analytics);

            _analytics.RaiseTransferStarted(meta.SessionId);
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
                throw new FaultException<ValidationFault>(new ValidationFault { Field = "Session", Reason = "Sesija nije pokrenuta." });

            // validacija, odbacena mjerenja idu u rejects.csv
            try
            {
                ValidateSample(sample);
            }
            catch (FaultException<ValidationFault> ex)
            {
                _writer.WriteReject(sample?.RowIndex ?? -1, ex.Detail.Reason);
                throw;
            }
            catch (FaultException<DataFormatFault> ex)
            {
                _writer.WriteReject(sample?.RowIndex ?? -1, ex.Detail.Detail);
                throw;
            }

            _received++;
            _writer.WriteSample(sample);
            _analytics.Process(sample, _received);

            // upis u csv fajl
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
            _analytics?.RaiseTransferCompleted(_received);
            _writer?.Dispose();   // zatvori fajlove (dispose pattern)

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
                throw new FaultException<DataFormatFault>(new DataFormatFault { Detail = "Uzorak je null." });

            if (double.IsNaN(s.U_q) || double.IsInfinity(s.U_q) ||
                double.IsNaN(s.U_d) || double.IsInfinity(s.U_d) ||
                double.IsNaN(s.Motor_Speed) || double.IsInfinity(s.Motor_Speed))
                throw new FaultException<DataFormatFault>(new DataFormatFault { Detail = "NaN/Infinity vrednost u U_q/U_d/Motor_Speed." });

            if (s.Motor_Speed <= 0)
                throw new FaultException<ValidationFault>(new ValidationFault { Field = "Motor_Speed", Reason = "Mora biti > 0." });

            if (s.Profile_Id < 0)
                throw new FaultException<ValidationFault>(new ValidationFault { Field = "Profile_Id", Reason = "Mora biti >= 0." });
        }
    }
}
