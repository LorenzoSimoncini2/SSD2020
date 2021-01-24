using Microsoft.EntityFrameworkCore;

namespace SsdWebApi.Models
{
    public class IndexContext : DbContext
    {
        public IndexContext(DbContextOptions<IndexContext> options) : base(options) 
        { 
            
        }
        public DbSet<Index> indici { get; set; }
    }
}