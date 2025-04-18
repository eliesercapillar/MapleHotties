using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System;
using MapleTinder.Shared.Models.Entities;

namespace MapleTinder.Shared.Data
{
    public class MapleTinderDbContext : DbContext
    {
        public MapleTinderDbContext(DbContextOptions<MapleTinderDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Character> Characters { get; set; }
    }
}
