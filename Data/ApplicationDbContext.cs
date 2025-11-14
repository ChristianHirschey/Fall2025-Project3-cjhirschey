using Fall2025_Project3_cjhirschey.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fall2025_Project3_cjhirschey.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Movie> Movie { get; set; }
        public DbSet<Actor> Actor { get; set; }
        public DbSet<ActorMovie> ActorMovie { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // create unique index to prevent duplicate relationships
            builder.Entity<ActorMovie>()
                .HasIndex(am => new { am.ActorId, am.MovieId })
                .IsUnique();

            // delete all relationships when a movie is deleted
            builder.Entity<Movie>()
                .HasMany(m => m.ActorMovies)
                .WithOne(am => am.Movie)
                .HasForeignKey(am => am.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // delete all relationships when an actor is deleted
            builder.Entity<Actor>()
                .HasMany(a => a.ActorMovies)
                .WithOne(am => am.Actor)
                .HasForeignKey(am => am.ActorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
