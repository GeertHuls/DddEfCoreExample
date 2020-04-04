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
            string result2 = Execute(x => x.AddEnrollment(1, 2, Grade.A));
            string result = Execute(x => x.CheckStudentFavoriteCourse(1, 2));
        }

        private static string Execute(Func<StudentController, string> func)
        {
            string connectionString = GetConnectionString();

            using (var context = new SchoolContext(connectionString, true))
            {
                var controller = new StudentController(context);
                return func(controller);
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
