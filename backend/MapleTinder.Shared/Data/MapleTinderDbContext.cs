using System.Numerics;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MapleTinder.Shared.Models.Entities;

namespace MapleTinder.Shared.Data
{
    public class MapleTinderDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public MapleTinderDbContext(DbContextOptions<MapleTinderDbContext> options) : base(options) { }

        public DbSet<Character> Characters { get; set; }

        public DbSet<UserHistory> UserHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // (Optional) rename Identity tables to match your naming conventions
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
        }
    }
}
