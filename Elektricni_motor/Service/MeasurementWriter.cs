using System;
using System.Globalization;
using System.IO;
using Common;

namespace Service
{
    // upisuje validne uzorke u measurements.csv, odbacene u rejects.csv
    public class MeasurementWriter : IDisposable
    {
        private StreamWriter _measurements;
        private StreamWriter _rejects;
        private bool _disposed;

        public string MeasurementsPath { get; }
        public string RejectsPath { get; }

        public MeasurementWriter(string folder, string sessionId)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            MeasurementsPath = Path.Combine(folder, $"measurements_{sessionId}.csv");
            RejectsPath = Path.Combine(folder, $"rejects_{sessionId}.csv");

            // Sekvencijalno nadovezivanje redova
            _measurements = new StreamWriter(new FileStream(MeasurementsPath, FileMode.Create, FileAccess.Write));
            _rejects = new StreamWriter(new FileStream(RejectsPath, FileMode.Create, FileAccess.Write));

            // zaglavlja
            _measurements.WriteLine("RowIndex,U_q,U_d,Motor_Speed,Profile_Id,Ambient,Torque");
            _rejects.WriteLine("RowIndex,Reason");
        }

        // nadovezi validan red
        public void WriteSample(MotorSample s)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MeasurementWriter));
            var c = CultureInfo.InvariantCulture;
            _measurements.WriteLine(string.Join(",", s.RowIndex, s.U_q.ToString(c), s.U_d.ToString(c), s.Motor_Speed.ToString(c), s.Profile_Id, s.Ambient.ToString(c), s.Torque.ToString(c)));
        }

        // nadovezi odbaceno mjerenje
        public void WriteReject(int rowIndex, string reason)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MeasurementWriter));
            _rejects.WriteLine($"{rowIndex},{reason}");
        }

        // Dispose
        ~MeasurementWriter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _measurements?.Flush();
                _measurements?.Dispose();
                _rejects?.Flush();
                _rejects?.Dispose();
            }
            _measurements = null;
            _rejects = null;
            _disposed = true;
        }
    }
}
