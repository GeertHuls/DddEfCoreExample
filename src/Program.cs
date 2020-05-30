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
            InitDatabase();

            string disenrollmentResult = Execute(x => x.DisenrollStudent(1, 2));
            string favoriteCourseCheckResult = Execute(x => x.CheckStudentFavoriteCourse(1, 2));
            string enrollmentResult = Execute(x => x.EnrollStudent(1, 2, Grade.A));
            string registerResult = Execute(x => x.RegisterStudent(
                "Carl", "Carlson", 1, "carl@gmail.com", 2, Grade.B));
            string editResult = Execute(x => x.EditPersonalInfo(
                2, "Carl 2", "Carlson2", 1, "carl@gmail.com", 1));
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

        private static void InitDatabase()
        {
            var connectionString = GetConnectionString();

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
