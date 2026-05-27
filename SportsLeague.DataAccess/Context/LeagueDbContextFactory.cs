using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SportsLeague.DataAccess.Context
{
    public class LeagueDbContextFactory : IDesignTimeDbContextFactory<LeagueDbContext>
    {
        public LeagueDbContext CreateDbContext(string[] args)
        {
            // Buscar la configuración desde el proyecto de API
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SportsLeague.API"))
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new LeagueDbContext(optionsBuilder.Options);
        }
    }
}
