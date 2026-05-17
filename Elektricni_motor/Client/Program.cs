using System;
using System.IO;
using System.Configuration;
using System.ServiceModel;
using Common;

namespace Client
{
    class Program
    {
        static void Main()
        {
            string csvPath = ConfigurationManager.AppSettings["csvPath"];
            string rejectsLog = ConfigurationManager.AppSettings["rejectsLog"];
            int maxRows = int.Parse(ConfigurationManager.AppSettings["maxRows"]);

            var factory = new ChannelFactory<IMotorMonitoringService>("MotorMonitoringEndpoint");
            IMotorMonitoringService proxy = factory.CreateChannel();

            try
            {
                // using garantuje dispose u slucaju izuzetka
                using (var reader = new CsvReader(csvPath, rejectsLog))
                {
                    Console.WriteLine($"Citanje CSV fajla: {reader.Path}");
                    var samples = reader.ReadSamples(maxRows);
                    Console.WriteLine($"Ucitano validnih: {samples.Count}, Odbaceno: {reader.RejectedCount}");

                    var meta = new SessionMeta
                    {
                        SessionId = Guid.NewGuid().ToString(),
                        StartTime = DateTime.Now,
                        ColumnsHeader = "U_q,U_d,Motor_Speed,Profile_Id,Ambient,Torque",
                        ExpectedRowCount = samples.Count
                    };

                    // start
                    var startAck = proxy.StartSession(meta);
                    Console.WriteLine($"StartSession -> {startAck.Ack} | {startAck.Status} | {startAck.Message}");

                    int accepted = 0, rejected = 0;

                    // sekvencijalno slanje po jedan red
                    for (int i = 0; i < samples.Count; i++)
                    {
                        try
                        {
                            var resp = proxy.PushSample(samples[i]);
                            Console.WriteLine($"[{i + 1}/{samples.Count}] {resp.Ack} | {resp.Status} | {resp.Message}");
                            accepted++;
                        }
                        catch (FaultException<ValidationFault> vex)
                        {
                            Console.WriteLine($"  ValidationFault: {vex.Detail.Field} - {vex.Detail.Reason}");
                            rejected++;
                        }
                        catch (FaultException<DataFormatFault> dex)
                        {
                            Console.WriteLine($"  DataFormatFault: {dex.Detail.Detail}");

                        }
                    }

                    // end
                    var endAck = proxy.EndSession();
                    Console.WriteLine($"EndSession   -> {endAck.Ack} | {endAck.Status} | {endAck.Message}");
                    Console.WriteLine($"\nUkupno poslato: {samples.Count}, Prihvaceno: {accepted}, Odbaceno: {rejected}");
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Fajl nije pronađen: {ex.FileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska: {ex.Message}");
            }
            finally
            {
                // Uredno zatvaranje kanala
                if (proxy is ICommunicationObject co)
                {
                    try { co.Close(); }
                    catch { co.Abort(); }
                }
            }

            Console.WriteLine("\nPritisni Enter za izlaz...");
            Console.ReadLine();
        }
    }
}
