using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ContactPro.Models;

namespace ContactPro.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ContactPro.Models.Category>? Category { get; set; }
        public DbSet<ContactPro.Models.Contact>? Contact { get; set; }
    }
}