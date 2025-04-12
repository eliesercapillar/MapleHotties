using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System;
using api.Models;

namespace api.Data
{
    public class PlayerDbContext : DbContext
    {
        public PlayerDbContext(DbContextOptions<PlayerDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
    }
}
