using BlazorControlCenter.Services;
using Microsoft.EntityFrameworkCore;

namespace BlazorControlCenter.Data
{
    public class SensorDataContext : DbContext
    {
        public SensorDataContext(DbContextOptions<SensorDataContext> options)
            : base(options)
        {
        }

        public DbSet<Scd4xDataPoint> DataPoints { get; set; }
    }
}
