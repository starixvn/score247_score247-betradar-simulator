using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Demo.PushClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Listening Push");

            //ReadPushEvent().GetAwaiter().GetResult();

            var api = "https://localhost:44318/api/events/Subscribe";
            Subscribe(api).GetAwaiter().GetResult();

            Console.ReadLine();
        }

        public async static Task Subscribe(string api)
        {
            var client = new HttpClient();
            var stream = await client.GetStreamAsync(api);

            try
            {
                using (var reader = new StreamReader(stream))
                {
                    while (true)
                    {
                        var line = await reader.ReadLineAsync();

                        Console.WriteLine($"streaming {line}");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Stream ended");
            }
        }
    }
}
