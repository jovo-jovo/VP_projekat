using System;
using System.ServiceModel;

namespace Service
{
    class Program
    {
        static void Main()
        {
            using (var host = new ServiceHost(typeof(MotorMonitoringService)))
            {
                try
                {
                    host.Open();
                    Console.WriteLine("PMSM Monitoring servis pokrenut.");
                    Console.WriteLine("Endpoints:");
                    foreach (var ep in host.Description.Endpoints)
                        Console.WriteLine($"  - {ep.Address}");
                    Console.WriteLine("\nPritisni Enter za zaustavljanje...");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greska u radu servisa: {ex.Message}");
                    host.Abort();
                }
            }
        }
    }
}
