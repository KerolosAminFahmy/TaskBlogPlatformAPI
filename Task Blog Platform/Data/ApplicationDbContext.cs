using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task_Blog_Platform.Model;

namespace Task_Blog_Platform.Data
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options ) :base( options ) 
        {
            
        }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }  
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Follow>().HasKey(m => new { m.FollowingId,m.FollowerId });

          
          
            modelBuilder.Entity<Blog>().HasMany(u => u.Comments)
            .WithOne(b => b.Blog)
            .HasForeignKey(b => b.BlogId);
        }
    }
}
