using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace DddEfCoreExample
{
    class Program
    {
        static void Main(string[] args)
        {

            var connectionString = GetConnectionString();

            Console.WriteLine("Hello World! " + connectionString);
            Console.ReadLine();
        }

        private static string GetConnectionString()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            return configuration["ConnectionString"];
        }
    }
}
