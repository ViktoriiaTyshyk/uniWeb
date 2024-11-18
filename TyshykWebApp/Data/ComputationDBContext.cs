using Microsoft.EntityFrameworkCore;
using TyshykWebApp.Models;

namespace TyshykWebApp.Data
{
    public class ComputationDBContext : DbContext
    {
        public ComputationDBContext(DbContextOptions<ComputationDBContext> options) : base(options) { }
        public DbSet<Computation> ComputationSet { get; set; }
    }
}
