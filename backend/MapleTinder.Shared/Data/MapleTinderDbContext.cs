using System.Numerics;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MapleTinder.Shared.Models.Entities;

namespace MapleTinder.Shared.Data
{
    public class MapleTinderDbContext : IdentityDbContext<ApplicationUser>
    {
        public MapleTinderDbContext(DbContextOptions<MapleTinderDbContext> options) : base(options) { }

        public DbSet<Character> Characters { get; set; }

        public DbSet<UserHistory> UserHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "94afe2ef-c68e-4009-8d54-fc834f9c6083",
                    ConcurrencyStamp = null,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "e5807ebd-8836-4f0f-9bc8-80f8efb18b99",
                    ConcurrencyStamp = null,
                    Name = "User",
                    NormalizedName = "USER"
                }
            );
        }
    }
}
