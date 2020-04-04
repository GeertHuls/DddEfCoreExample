using System;
using System.IO;
using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;

namespace DddEfCoreExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = GetConnectionString();
            
            InitDatabase(connectionString);

            using (var context = new SchoolContext(connectionString, true))
            {
                Student student = context.Students.Find(1L);
            }
        }

        private static void InitDatabase(string connectionString)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();        

            if (result.Successful)
            {
                Console.WriteLine("Database migration was successful!");
            }
            else
            {
                Console.WriteLine("Database migration failed.");
            }
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
