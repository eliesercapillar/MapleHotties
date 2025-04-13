using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System;
using api.Models.Entities;

namespace api.Data
{
    public class MapleTinderDbContext : DbContext
    {
        public MapleTinderDbContext(DbContextOptions<MapleTinderDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Character> Characters { get; set; }
    }
}
