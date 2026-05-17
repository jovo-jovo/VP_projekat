using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Common;

namespace Client
{
    // otvara csv fajl, parsira invariant cultureom, loguje odbacene redove
    public class CsvReader : IDisposable
    {
        private StreamReader _reader;
        private StreamWriter _rejectsLog;
        private bool _disposed;
        private readonly string _path;
        private int _rejectedCount;

        public string Path => _path;
        public int RejectedCount => _rejectedCount;

        public CsvReader(string csvPath, string rejectsLogPath)
        {
            _path = csvPath ?? throw new ArgumentNullException(nameof(csvPath));

            if (!File.Exists(_path))
                throw new FileNotFoundException("CSV fajl ne postoji.", _path);

            _reader = new StreamReader(_path);
            _rejectsLog = new StreamWriter(rejectsLogPath, append: false);
            _rejectsLog.WriteLine($"# Rejects log - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _rejectsLog.WriteLine("# format: linija | razlog | sadrzaj");
        }

        // ucitava prvih maks validnih redova, ostalo ide u reject
        public List<MotorSample> ReadSamples(int maxRows)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CsvReader));

            var samples = new List<MotorSample>();

            string header = _reader.ReadLine();
            if (string.IsNullOrWhiteSpace(header))
                throw new InvalidDataException("CSV je prazan ili nema zaglavlje.");

            var columnMap = ParseHeader(header);

            int lineNo = 1;
            string line;
            while ((line = _reader.ReadLine()) != null && samples.Count < maxRows)
            {
                lineNo++;
                if (TryParseSample(line, columnMap, lineNo, out var sample))
                    samples.Add(sample);
            }

            return samples;
        }

        private Dictionary<string, int> ParseHeader(string header)
        {
            var parts = header.Split(',');
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parts.Length; i++)
                map[parts[i].Trim()] = i;
            return map;
        }

        private bool TryParseSample(string line, Dictionary<string, int> cols, int lineNo, out MotorSample sample)
        {
            sample = null;
            try
            {
                var parts = line.Split(',');
                sample = new MotorSample
                {
                    RowIndex = lineNo,
                    U_q = GetDouble(parts, cols, "u_q"),
                    U_d = GetDouble(parts, cols, "u_d"),
                    Motor_Speed = GetDouble(parts, cols, "motor_speed"),
                    Profile_Id = (int)GetDouble(parts, cols, "profile_id"),
                    Ambient = GetDouble(parts, cols, "ambient"),
                    Torque = GetDouble(parts, cols, "torque")
                };
                return true;
            }
            catch (Exception ex)
            {
                _rejectedCount++;
                _rejectsLog.WriteLine($"{lineNo} | {ex.Message} | {line}");
                return false;
            }
        }

        private double GetDouble(string[] parts, Dictionary<string, int> cols, string name)
        {
            if (!cols.TryGetValue(name, out int idx))
                throw new InvalidDataException($"Nedostaje kolona '{name}'.");
            if (idx >= parts.Length)
                throw new InvalidDataException($"Nedostaje vrednost u koloni '{name}'.");

            // tacka odvaja decimale
            return double.Parse(parts[idx], CultureInfo.InvariantCulture);
        }

        // disposable zbog streamova tj resursa
        ~CsvReader()
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
                // flush resursa
                _reader?.Dispose();
                _rejectsLog?.Flush();
                _rejectsLog?.Dispose();
            }

            _reader = null;
            _rejectsLog = null;
            _disposed = true;
        }
    }
}
