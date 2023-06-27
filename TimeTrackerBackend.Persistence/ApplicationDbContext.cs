using TimeTrackerBackend.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace TimeTrackerBackend.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<Employee>
    {
        //Alle DbSets deklarieren. Format:
        //internal DbSet<Address> Addresses { get; set; }
        internal DbSet<Company> Companies { get; set; }
        internal DbSet<WorkMonth> WorkMonths { get; set; }
        internal DbSet<WorkDay> WorkDays { get; set; }
        internal DbSet<Stamp> Stamps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder
               .Entity<Company>()
               .HasMany(e => e.Employees)
               .WithOne(i => i.Company)
               .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder
               .Entity<Employee>()
               .HasMany(e => e.WorkMonths)
               .WithOne(i => i.Employee)
               .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder
               .Entity<Stamp>()
               .HasOne(e => e.WorkDay);

            modelBuilder
               .Entity<WorkDay>()
               .HasMany(e => e.Stamps)
               .WithOne(i => i.WorkDay)
               .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder
                .Entity<WorkMonth>()
                .HasMany(e => e.WorkDays)
                .WithOne(i => i.WorkMonth)
                .OnDelete(DeleteBehavior.ClientCascade);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                var configuration = builder.Build();
                Debug.Write(configuration.ToString());
                connectionString = configuration["ConnectionStrings:DefaultConnection"];
            }
            Console.WriteLine($"!!!!Connecting with {connectionString}");
            optionsBuilder
                .UseSqlServer(connectionString, builder =>{
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
            base.OnConfiguring(optionsBuilder);
        }
    }
}
