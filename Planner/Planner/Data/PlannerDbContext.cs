using Microsoft.EntityFrameworkCore;
using Planner.Models;
using System;

namespace Planner.Data
{
    public class PlannerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }

        public PlannerDbContext(DbContextOptions<PlannerDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<TaskItem>()
                      .HasOne(t => t.User)
                      .WithMany(u => u.Tasks) 
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.User) 
                .WithMany(u => u.Sessions) 
                .HasForeignKey(s => s.UserId) 
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
