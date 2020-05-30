using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace DddEfCoreExample
{
    public sealed class SchoolContext : DbContext
    {
        private static readonly Type[] EnumerationTypes = { typeof(Course), typeof(Suffix) };
        
        private readonly EventDispatcher _eventDispatcher;

        private readonly string _connectionString;
        private readonly bool _useConsoleLogger;

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }

        public SchoolContext(string connectionString, bool useConsoleLogger,
            EventDispatcher eventDispatcher)
        {
            _connectionString = connectionString;
            _useConsoleLogger = useConsoleLogger;
            _eventDispatcher = eventDispatcher;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)
                    .AddConsole();
            });

            optionsBuilder
                .UseSqlServer(_connectionString);

            if (_useConsoleLogger)
            {
                optionsBuilder
                    .UseLoggerFactory(loggerFactory)
                    .EnableSensitiveDataLogging()
                    .UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(x =>
            {
                x.ToTable("Student").HasKey(k => k.Id);
                x.Property(p => p.Id).HasColumnName("StudentID");
                x.Property(p => p.Email)
                    .HasConversion(p => p.Value, p => Email.Create(p).Value);
                // This is an owned entity wich is treated as an external table by ef and
                // by consequence the generated sql query will join the student table to itself.
                x.OwnsOne(p => p.Name, p =>
                {
                    p.Property<long?>("NameSuffixID").HasColumnName("NameSuffixID");
                    p.Property(pp => pp.First).HasColumnName("FirstName");
                    p.Property(pp => pp.Last).HasColumnName("LastName");
                    p.HasOne(pp => pp.Suffix).WithMany().HasForeignKey("NameSuffixID").IsRequired(false);
                });
                x.HasOne(p => p.FavoriteCourse).WithMany();
                x.HasMany(p => p.Enrollments).WithOne(p => p.Student)
                    .OnDelete(DeleteBehavior.Cascade)
                    // one-to-many mapping sides:
                    // one side = principal (= student)
                    // many side = dependent (= enrollment)
                    .Metadata.PrincipalToDependent.SetPropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<Suffix>(x =>
            {
                x.ToTable("Suffix").HasKey(p => p.Id);
                x.Property(p => p.Id).HasColumnName("SuffixID");
                x.Property(p => p.Name);
            });

            modelBuilder.Entity<Course>(x =>
            {
                x.ToTable("Course").HasKey(k => k.Id);
                x.Property(p => p.Id).HasColumnName("CourseID");
                x.Property(p => p.Name)
                    // This will prevent that the entity will be updated
                    // while in detached mode on another entity.
                    // .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore)

                    // Instead use the unchanged tracker enumeration
                    // in the SaveChanges override below:
                    ;
            });

            modelBuilder.Entity<Enrollment>(x =>
            {
                x.ToTable("Enrollment").HasKey(k => k.Id);
                x.Property(p => p.Id).HasColumnName("EnrollmentID");
                x.HasOne(p => p.Student).WithMany(p => p.Enrollments);
                x.HasOne(p => p.Course).WithMany();
                x.Property(p => p.Grade);
            });
        }

        // This will also prevent that the entity will be updated
        // while in detached mode on another entity, is similar behavior
        // as metadata SetAfterSaveBehavior.
        public override int SaveChanges()
        {
            IEnumerable<EntityEntry> enumerationEntries = ChangeTracker.Entries()
                .Where(x => EnumerationTypes.Contains(x.Entity.GetType()));

            foreach (EntityEntry enumerationEntry in enumerationEntries)
            {
                enumerationEntry.State = EntityState.Unchanged;
            }

            List<Entity> entities = ChangeTracker
                .Entries()
                .Where(x => x.Entity is Entity)
                .Select(x => (Entity) x.Entity)
                .ToList();

            var result = base.SaveChanges();

            foreach (Entity entity in entities)
            {
                _eventDispatcher.Dispatch(entity.DomainEvents);
                entity.ClearDomainEvents();
            }

            return result;
        }
    }
}