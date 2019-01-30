using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirewallLogViewer
{
    static class Program
    {
        static void Main(string [] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage command LogFilePath");
                Application.Exit();
            }

            var logLocation = args[0];
            var distinctIps = new HashSet<string>();
            var localIp = GetLocalIPAddress();
            var privateIps = new HashSet<string>() { "239.255.255.250", "192.168.1.1" };

            Console.WriteLine($"Local Ip : {localIp}, Log location : {logLocation}");

            using (var fs = File.Open(logLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }

                    var parts = line.Split(' ');
                    if (parts.Length < 7)
                    {
                        continue;
                    }

                    var srcIp = parts[4];
                    var destIp = parts[5];

                    if (srcIp != localIp && !privateIps.Contains(srcIp))
                    {
                        distinctIps.Add(srcIp);
                    }

                    if (destIp != localIp && !privateIps.Contains(srcIp))
                    {
                        distinctIps.Add(destIp);
                    }
                }
            }

            Console.WriteLine($"Lookup needed for {distinctIps.Count} IPs");

            foreach(var ip in distinctIps)
            {
                var details = GetGeoByIp(ip);
                Console.WriteLine(details.ToString());
            }

        }

        public static IpInfo GetGeoByIp(string ip)
        {
            IpInfo ipInfo = new IpInfo();
            try
            {
                string info = new WebClient().DownloadString("http://ipinfo.io/" + ip);
                ipInfo = JsonConvert.DeserializeObject<IpInfo>(info);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Err for ip : {ip}, {ex.Message}");
            }

            return ipInfo;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

    public class IpInfo
    {

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("loc")]
        public string Loc { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }

        public override string ToString()
        {
            return $"{Ip}({Hostname}/{Org}) : City : {City} Region : {Region} Loc : {Loc} Postal : {Postal} Country : {Country}";
        }
    }
}
