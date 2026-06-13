using System;

namespace Service
{
    // subscriber
    public class EventLogger
    {
        public void Subscribe(AnalyticsEngine engine)
        {
            // transfer dogadjaji
            engine.OnTransferStarted += (s, e) => Console.WriteLine($"[TRANSFER] {e.Message}");
            engine.OnTransferCompleted += (s, e) => Console.WriteLine($"[TRANSFER] {e.Message}");
            engine.OnSampleReceived += (s, e) =>
                Console.WriteLine($"[SAMPLE] #{e.Count} (red {e.Sample.RowIndex}) primljen.");
            engine.OnWarningRaised += (s, e) => Console.WriteLine($"[WARNING] {e.Message}");

            // analiticki dogadjaji
            engine.VoltageSpikeQ += (s, e) =>
                Console.WriteLine($"[VoltageSpikeQ] dU_q={e.Delta:F3} > {e.Threshold} ({e.Direction}, red {e.RowIndex})");
            engine.VoltageSpikeD += (s, e) =>
                Console.WriteLine($"[VoltageSpikeD] dU_d={e.Delta:F3} > {e.Threshold} ({e.Direction}, red {e.RowIndex})");
            engine.SpeedSpike += (s, e) =>
                Console.WriteLine($"[SpeedSpike] dSpeed={e.Delta:F3} > {e.Threshold} ({e.Direction}, red {e.RowIndex})");
            engine.OutOfBandWarning += (s, e) =>
                Console.WriteLine($"[OutOfBand] {e.Message} ({e.Direction})");
        }
    }
}
