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
        public DbSet<CharacterStats> CharacterStats { get; set; }
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

            builder.Entity<Character>(eb =>
            {
                eb.Property(c => c.Name).HasMaxLength(25);
                eb.Property(c => c.Job).HasMaxLength(25);
                eb.Property(c => c.World).HasMaxLength(25);
                eb.Property(c => c.ImageUrl).HasMaxLength(500);

                // For search filtering
                eb.HasIndex(c => c.Name);
                eb.HasIndex(c => c.World);
                eb.HasIndex(c => c.Job);
            });

            builder.Entity<UserHistory>(eb =>
            {
                eb.HasKey(uh => new { uh.UserId, uh.CharacterId }); // Composite PK

                eb.HasIndex(uh => uh.UserId);
                eb.HasIndex(uh => new { uh.UserId, uh.Status });
                eb.HasIndex(uh => new { uh.UserId, uh.SeenAt });
            });

            builder.Entity<CharacterStats>(eb =>
            {
                eb.HasKey(cs => cs.CharacterId);

                eb.HasOne(cs => cs.Character)
                  .WithOne(c => c.CharacterStats)
                  .HasForeignKey<CharacterStats>(cs => cs.CharacterId);

                eb.HasIndex(cs => new { cs.WeeklyLikes, cs.CharacterId });
                eb.HasIndex(cs => new { cs.WeeklyNopes, cs.CharacterId });
                eb.HasIndex(cs => new { cs.WeeklyFavourites, cs.CharacterId });

                eb.HasIndex(cs => new { cs.MonthlyLikes, cs.CharacterId });
                eb.HasIndex(cs => new { cs.MonthlyNopes, cs.CharacterId });
                eb.HasIndex(cs => new { cs.MonthlyFavourites, cs.CharacterId });

                eb.HasIndex(cs => new { cs.TotalNopes, cs.CharacterId });
                eb.HasIndex(cs => new { cs.TotalLikes, cs.CharacterId });
                eb.HasIndex(cs => new { cs.TotalFavourites, cs.CharacterId });
            });
        }
    }
}
