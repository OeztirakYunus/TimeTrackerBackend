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
        internal DbSet<Vacation> Vacations { get; set; }
        internal DbSet<NotificationOfIllness> NotificationOfIllness { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Company -> Employees
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Employees)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Employee -> WorkMonths
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.WorkMonths)
                .WithOne(wm => wm.Employee)
                .HasForeignKey(wm => wm.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // WorkMonth -> WorkDays
            modelBuilder.Entity<WorkMonth>()
                .HasMany(wm => wm.WorkDays)
                .WithOne(wd => wd.WorkMonth)
                .HasForeignKey(wd => wd.WorkMonthId)
                .OnDelete(DeleteBehavior.Cascade);

            // WorkDay -> Stamps
            modelBuilder.Entity<WorkDay>()
                .HasMany(wd => wd.Stamps)
                .WithOne(s => s.WorkDay)
                .HasForeignKey(s => s.WorkDayId)
                .OnDelete(DeleteBehavior.Cascade);

            // Stamp -> WorkDay
            modelBuilder.Entity<Stamp>()
                .HasOne(s => s.WorkDay)
                .WithMany(wd => wd.Stamps)
                .HasForeignKey(s => s.WorkDayId)
                .OnDelete(DeleteBehavior.Cascade);

            // Employee -> Notifications of Illness
            modelBuilder.Entity<NotificationOfIllness>()
                .HasOne(noi => noi.Employee)
                .WithMany()
                .HasForeignKey(noi => noi.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Employee -> Vacations
            modelBuilder.Entity<Vacation>()
                .HasOne(v => v.Employee)
                .WithMany()
                .HasForeignKey(v => v.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Identity Tables
            modelBuilder.Entity<Employee>(b =>
            {
                b.HasIndex(u => u.Email).IsUnique();
                b.HasIndex(u => u.UserName).IsUnique();
            });

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("SQLServerConnection");
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
