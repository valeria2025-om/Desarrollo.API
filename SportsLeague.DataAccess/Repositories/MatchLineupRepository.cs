using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories
{
    public class MatchLineupRepository : GenericRepository<MatchLineup>, IMatchLineupRepository
    {
        public MatchLineupRepository(LeagueDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
        {
            return await _dbSet
                .Where(ml => ml.MatchId == matchId)
                .Include(ml => ml.Player)
                    .ThenInclude(p => p.Team)
                .OrderBy(ml => ml.Player.TeamId)
                .ThenByDescending(ml => ml.IsStarter)
                .ToListAsync();
        }

        public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId)
        {
            return await _dbSet
                .Where(ml => ml.MatchId == matchId && ml.Player.TeamId == teamId)
                .Include(ml => ml.Player)
                    .ThenInclude(p => p.Team)
                .OrderByDescending(ml => ml.IsStarter)
                .ToListAsync();
        }

        public async Task<MatchLineup?> GetByMatchAndPlayerAsync(int matchId, int playerId)
        {
            return await _dbSet
                .Where(ml => ml.MatchId == matchId && ml.PlayerId == playerId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountStartersByMatchAndTeamAsync(int matchId, int teamId)
        {
            return await _dbSet
                .Where(ml => ml.MatchId == matchId && ml.IsStarter == true)
                .Include(ml => ml.Player)
                .CountAsync(ml => ml.Player.TeamId == teamId);
        }
    }
}
