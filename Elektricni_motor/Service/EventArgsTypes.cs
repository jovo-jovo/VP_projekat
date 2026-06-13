using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    // args kada je primljen jedan uzorak
    public class SampleEventArgs : EventArgs
    {
        public MotorSample Sample { get; }
        public int Count { get; }
        public SampleEventArgs(MotorSample sample, int count)
        {
            Sample = sample;
            Count = count;
        }
    }

    // args za naglu promjenu signala
    public class SpikeEventArgs : EventArgs
    {
        public string Signal { get; }
        public double Delta { get; }
        public double Threshold { get; }
        public string Direction { get; }
        public int RowIndex { get; }
        public SpikeEventArgs(string signal, double delta, double threshold, string direction, int rowIndex)
        {
            Signal = signal;
            Delta = delta;
            Threshold = threshold;
            Direction = direction;
            RowIndex = rowIndex;
        }
    }

    // args za upozorenja
    public class WarningEventArgs : EventArgs
    {
        public string Message { get; }
        public string Direction { get; }
        public WarningEventArgs(string message, string direction = "")
        {
            Message = message;
            Direction = direction;
        }
    }

    // args za status prenosa
    public class TransferEventArgs : EventArgs
    {
        public string Message { get; }
        public TransferEventArgs(string message)
        {
            Message = message;
        }
    }
}
