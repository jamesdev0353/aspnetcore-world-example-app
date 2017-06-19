﻿using Microsoft.AspNetCore.Hosting;

namespace RealWorld
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://+:5000")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
