using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BoxFusion.API.BoxFusion.Domain.Entities
{
    public class BoxFusionDbContext : IdentityDbContext<ApplicationUser>
    {
        public BoxFusionDbContext(DbContextOptions<BoxFusionDbContext> options)
            : base(options) { }

        // აი, აქ ჩავამატოთ შენი ცხრილები!
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ფილტრები, რომ IsDeleted მუშაობდეს
            builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        }
    }
}