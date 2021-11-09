using System;
using System.Threading.Tasks;
using System.Linq;

namespace MidcoUsageChecker.Example
{
    static class Program
    {
        static string username = "<your midco.com username>";
        static string password = "<password>";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Checking Midco usage...");

            var client = new MidcoWebClient();

            if (await client.LogIn(username, password))
            {
                var usage = await client.LoadHourlyUsage();

                Console.WriteLine("Hourly downloading usage today (MB):");
                for(int x = 0; x < usage.Downstream.Count; x++)
                {
                    Console.WriteLine($"{x} - {usage.Downstream[x].ToString("##,###.00")}");
                }


                var monthlyUsage = await client.LoadHourlyUsage();
                var monthlyUp = monthlyUsage.Upstream.Sum();
                var monthlyDown = monthlyUsage.Downstream.Sum();

                Console.WriteLine("Monthly (GB):");
                Console.WriteLine($"Up: {monthlyUp.ToString("##,###.00")}");
                Console.WriteLine($"Down: {monthlyDown.ToString("##,###.00")}");
            }
        }
    }
}
