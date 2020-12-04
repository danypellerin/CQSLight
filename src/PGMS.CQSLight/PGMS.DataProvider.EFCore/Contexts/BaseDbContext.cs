using Microsoft.EntityFrameworkCore;
using PGMS.Data.Services;

namespace PGMS.DataProvider.EFCore.Contexts
{
    public class BaseDbContext : DbContext, IDbContext
    {
        public BaseDbContext(DbContextOptions options)
            : base(options)
        { }

        public DbSet<DbSequenceHiLo> SequenceHiLo { get; set; }
    }

    public class DbSequenceHiLo
    {
        public long Id { get; set; }
        public string IdParameters { get; set; }
        public int IntVal { get; set; }
    }
}