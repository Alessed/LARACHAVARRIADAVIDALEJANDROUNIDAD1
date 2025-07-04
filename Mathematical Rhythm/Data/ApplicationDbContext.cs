using MathematicalRhythm.Models;
using Microsoft.EntityFrameworkCore;

namespace MathematicalRhythm.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Recuperacion> Recuperaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura PK para Usuario
            modelBuilder.Entity<Usuario>()
                .HasKey(u => u.IdUsuario);

            
        }
    }
}