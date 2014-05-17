using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAPIExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Here is the contents of public.crt (without headers starting with "------")
            // You may also store the file directly on disk and read it here instead of
            // hardcoding the key, but it makes possible hacker's life easier
            var cert =
                "MIIBcTCB3aADAgECAgEAMAsGCSqGSIb3DQEBBTAAMCIYDzIwMTQwNTE3MjE0NzQwWhgPMjAxNTA1MTcyMTQ3NDBaMAAwgZ0wCwYJKoZIhvcNAQEBA4GNADCBiQKBgQCtgUWctaH+oKbhwbiJoHyVFIHejy/5RZaVyAM3VN0dq/TvHLT4gDZ2UQsTY2fvxO1RPprGL6387qh5HPr4AWCHEcPlxKUWU2UpUZm/YRlPUIpzBuQnPY+EwC30vzRdruvcFUrPJy+lKScIL7bEz6s4eWEOf+AXvCVKFVmrDMqvWwIDAQABMAsGCSqGSIb3DQEBBQOBgQA4si2dwQNMdqpqgE9N9Z95Zf6Aimm9ohHicwuOKEFD8PPZqyTyvwfvbUaVpiMhgsFg5tf3+78yQTf6B4GdBfghS2TNeZFuxDYhW9dvs1qxeTI0wIa26c2f/R/wJ8HBa4FHgYV/xUNuTw+JUlk6krxa1msWgbBVvNgEHrX9eQNJJA==";

            try
            {
                var authCore = new AutoMH.Auth.AuthCore("http://cryptoapi.localhost", cert);

                authCore.Init();
                var validationResult = authCore.ValidateLicense("MY-SECRET-LICENSE-KEY");

                if (validationResult)
                {
                    Console.WriteLine("License successfully validated");
                }
                else
                {
                    Console.WriteLine("Your license is no longer valid");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR: {0}", exception.Message);
            }

            Console.ReadLine();
        }
    }
}
