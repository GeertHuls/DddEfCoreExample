using System;
using System.IO;
using System.Reflection;
using DbUp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DddEfCoreExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = GetConnectionString();
            
            InitDatabase(connectionString);

            ILoggerFactory loggerFactory = CreateLoggerFactory();

            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder
                .UseSqlServer(connectionString)
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging();

            using (var context = new SchoolContext(optionsBuilder.Options))
            {
                Student student = context.Students.Find(1L);
            }
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)
                    .AddConsole();
            });
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
