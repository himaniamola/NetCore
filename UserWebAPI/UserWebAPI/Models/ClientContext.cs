using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserWebAPI
{
    public class ClientContext : DbContext
    {
        public ClientContext(DbContextOptions<ClientContext> options) : base(options)
        {

        }

        public DbSet<Client> Clients { get; set; }
    }
}
