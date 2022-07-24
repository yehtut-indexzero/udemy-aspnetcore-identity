using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace udemy_aspnetcore_identity.Data
{
    public class ApplicationDBContext:IdentityDbContext
    {
        public ApplicationDBContext()
        {

        }

        public ApplicationDBContext(DbContextOptions options):base(options)
        {

        }
    }
}
