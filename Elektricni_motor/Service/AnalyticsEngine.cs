using System;
using Common;
using System.Configuration;
using System.Globalization;

namespace Service
{
    // publisher
    public class AnalyticsEngine
    {
        // transfer dogadjaji
        public event EventHandler<TransferEventArgs> OnTransferStarted;
        public event EventHandler<SampleEventArgs> OnSampleReceived;
        public event EventHandler<TransferEventArgs> OnTransferCompleted;
        public event EventHandler<WarningEventArgs> OnWarningRaised;

        // analiticki dogadjaji
        public event EventHandler<SpikeEventArgs> VoltageSpikeQ;
        public event EventHandler<SpikeEventArgs> VoltageSpikeD;
        public event EventHandler<SpikeEventArgs> SpeedSpike;
        public event EventHandler<WarningEventArgs> OutOfBandWarning;

        // pragovi iz app.config
        private readonly double _uqThreshold;
        private readonly double _udThreshold;
        private readonly double _speedThreshold;
        private readonly double _deviation;

        // stanje za delta i running mean
        private MotorSample _prev;
        private double _speedSum;
        private int _speedCount;

        public AnalyticsEngine()
        {
            _uqThreshold = ReadDouble("Uq_threshold", 0.5);
            _udThreshold = ReadDouble("Ud_threshold", 0.5);
            _speedThreshold = ReadDouble("Speed_threshold", 0.3);
            _deviation = ReadDouble("DeviationPercent", 25) / 100.0;
        }

        public void RaiseTransferStarted(string sessionId) => OnTransferStarted?.Invoke(this, new TransferEventArgs($"prenos u toku... (sesija {sessionId})"));

        public void RaiseTransferCompleted(int total) => OnTransferCompleted?.Invoke(this, new TransferEventArgs($"zavrsen prenos ({total} uzoraka)"));

        // glavna obrada jednog uzorka
        public void Process(MotorSample s, int count)
        {
            OnSampleReceived?.Invoke(this, new SampleEventArgs(s, count));

            // running mean za brzinu motora
            _speedSum += s.Motor_Speed;
            _speedCount++;
            double mean = _speedSum / _speedCount;

            // delta provjere samo ako postoji prethodni uzorak
            if (_prev != null)
            {
                CheckVoltageQ(s);
                CheckVoltageD(s);
                CheckSpeed(s);
            }

            CheckOutOfBand(s, mean);
            _prev = s;
        }

        // delta uq
        private void CheckVoltageQ(MotorSample s)
        {
            double d = s.U_q - _prev.U_q;
            
            if (Math.Abs(d) > _uqThreshold)
                VoltageSpikeQ?.Invoke(this, new SpikeEventArgs("U_q", d, _uqThreshold, Dir(d), s.RowIndex));
        }

        // delta ud
        private void CheckVoltageD(MotorSample s)
        {
            double d = s.U_d - _prev.U_d;
            if (Math.Abs(d) > _udThreshold)
                VoltageSpikeD?.Invoke(this, new SpikeEventArgs(
                    "U_d", d, _udThreshold, Dir(d), s.RowIndex));
        }

        // delta speed
        private void CheckSpeed(MotorSample s)
        {
            double d = s.Motor_Speed - _prev.Motor_Speed;
            
            if (Math.Abs(d) > _speedThreshold)
                SpeedSpike?.Invoke(this, new SpikeEventArgs("Motor_Speed", d, _speedThreshold, Dir(d), s.RowIndex));
        }

        // odstupanje
        private void CheckOutOfBand(MotorSample s, double mean)
        {
            double low = (1 - _deviation) * mean;
            double high = (1 + _deviation) * mean;

            if (s.Motor_Speed < low)
                OutOfBandWarning?.Invoke(this, new WarningEventArgs($"Speed {s.Motor_Speed:F3} < {low:F3} (red {s.RowIndex})", "ispod ocekivane vrijednosti"));
            
            else if (s.Motor_Speed > high)
                OutOfBandWarning?.Invoke(this, new WarningEventArgs($"Speed {s.Motor_Speed:F3} > {high:F3} (red {s.RowIndex})", "iznad ocekivane vrijednosti"));
        }

        private string Dir(double d) => d > 0 ? "iznad ocekivanog" : "ispod ocekivanog";

        private double ReadDouble(string key, double fallback)
        {
            var v = ConfigurationManager.AppSettings[key];
            return double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out double r) ? r : fallback;
        }
    }
}
