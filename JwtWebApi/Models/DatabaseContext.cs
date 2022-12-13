using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtWebApi.Models
{
    //If sometimes if your migrations are not adding then double check the connection string in appsettings.json and 
    //also check that all the NuGet packages are of same version.

    /// <summary>
    /// Everytime you change the Database Name or Model Definition or the Connection String, you need to add new 
    /// migration
    /// </summary>
    public partial class DatabaseContext : IdentityDbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Employee>? Employees { get; set; }
    }
}
