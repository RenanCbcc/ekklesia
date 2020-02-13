﻿using Caelum.Stella.CSharp.Vault;
using ekklesia.Models.EventModel;
using ekklesia.Models.MemberModel;
using ekklesia.Models.ReportModel;
using ekklesia.Models.TransactionModel;
using Microsoft.EntityFrameworkCore;


namespace ekklesia
{

    public class ApplicationContext : DbContext
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Occasion> Occasions { get; set; }
        public DbSet<Report> Reports { get; set; }

        public ApplicationContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SundaySchool>();
            modelBuilder.Entity<Reunion>();
            modelBuilder.Entity<Cult>();

            
            modelBuilder
                .Entity<OccasionMember>()
                .HasKey(mm => new { mm.MemberId, mm.OccasionId });

            modelBuilder
                .Entity<Transaction>()
                .Property(t => t.Value)
                .HasConversion(v => (decimal)v, v => new Money(v));

            modelBuilder
                .Entity<Report>()
                .Property(r => r.PreviousMonth)
                .HasConversion(v => (decimal)v, v => new Money(v));

            modelBuilder
                .Entity<Report>()
                .Property(r => r.Income)
                .HasConversion(v => (decimal)v, v => new Money(v));

            modelBuilder
                .Entity<Report>()
                .Property(r => r.Expense)
                .HasConversion(v => (decimal)v, v => new Money(v));

            modelBuilder
                .Entity<Report>()
                .Property(r => r.Tenth)
                .HasConversion(v => (decimal)v, v => new Money(v));

            modelBuilder
                .Entity<Report>()
                .Property(r => r.Balance)
                .HasConversion(v => (decimal)v, v => new Money(v));
        }
    }
}
