using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services
{
    public class MatchLineupService : IMatchLineupService
    {
        private readonly IMatchLineupRepository _matchLineupRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly MatchValidationHelper _matchValidationHelper;
        private readonly ILogger<MatchLineupService> _logger;

        public MatchLineupService(
            IMatchLineupRepository matchLineupRepository,
            IMatchRepository matchRepository,
            IPlayerRepository playerRepository,
            MatchValidationHelper matchValidationHelper,
            ILogger<MatchLineupService> logger)
        {
            _matchLineupRepository = matchLineupRepository;
            _matchRepository = matchRepository;
            _playerRepository = playerRepository;
            _matchValidationHelper = matchValidationHelper;
            _logger = logger;
        }

        public async Task<MatchLineup> AddPlayerToLineupAsync(int matchId, MatchLineup lineup)
        {
            // V1: El partido debe existir
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Match with ID {MatchId} not found", matchId);
                throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");
            }

            // V6: El partido debe estar en estado Scheduled
            if (match.Status != MatchStatus.Scheduled)
            {
                _logger.LogWarning(
                    "Match {MatchId} is in status {Status}, only Scheduled allowed",
                    matchId, match.Status);
                throw new InvalidOperationException(
                    "Solo se pueden registrar alineaciones en partidos Scheduled");
            }

            // V2: El jugador debe existir
            var player = await _playerRepository.GetByIdAsync(lineup.PlayerId);
            if (player == null)
            {
                _logger.LogWarning("Player with ID {PlayerId} not found", lineup.PlayerId);
                throw new KeyNotFoundException($"No se encontró el jugador con ID {lineup.PlayerId}");
            }

            // V3: El jugador debe pertenecer al HomeTeam o AwayTeam del partido
            if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
            {
                _logger.LogWarning(
                    "Player {PlayerId} does not belong to either team in match {MatchId}",
                    lineup.PlayerId, matchId);
                throw new InvalidOperationException(
                    "El jugador no pertenece a ninguno de los equipos del partido");
            }

            // V4: El jugador no puede estar registrado dos veces en la misma alineación
            var existingLineup = await _matchLineupRepository
                .GetByMatchAndPlayerAsync(matchId, lineup.PlayerId);
            if (existingLineup != null)
            {
                _logger.LogWarning(
                    "Player {PlayerId} is already in the lineup for match {MatchId}",
                    lineup.PlayerId, matchId);
                throw new InvalidOperationException(
                    "El jugador ya está registrado en la alineación de este partido");
            }

            // V5: Máximo 11 titulares por equipo por partido
            if (lineup.IsStarter)
            {
                int teamId = player.TeamId;
                int starterCount = await _matchLineupRepository
                    .CountStartersByMatchAndTeamAsync(matchId, teamId);

                if (starterCount >= 11)
                {
                    _logger.LogWarning(
                        "Team {TeamId} already has 11 starters in match {MatchId}",
                        teamId, matchId);
                    throw new InvalidOperationException(
                        "El equipo ya tiene 11 titulares registrados en este partido");
                }
            }

            lineup.MatchId = matchId;

            _logger.LogInformation(
                "Adding player {PlayerId} to lineup for match {MatchId} as {Status} ({Position})",
                lineup.PlayerId, matchId,
                lineup.IsStarter ? "Starter" : "Substitute",
                lineup.Position);

            return await _matchLineupRepository.CreateAsync(lineup);
        }

        public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Match with ID {MatchId} not found", matchId);
                throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");
            }

            _logger.LogInformation("Retrieving lineup for match {MatchId}", matchId);
            return await _matchLineupRepository.GetByMatchAsync(matchId);
        }

        public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(int matchId, int teamId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Match with ID {MatchId} not found", matchId);
                throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");
            }

            _logger.LogInformation(
                "Retrieving lineup for match {MatchId} and team {TeamId}",
                matchId, teamId);
            return await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
        }

        public async Task DeleteLineupAsync(int id)
        {
            var exists = await _matchLineupRepository.ExistsAsync(id);
            if (!exists)
            {
                _logger.LogWarning("Lineup entry with ID {LineupId} not found", id);
                throw new KeyNotFoundException($"No se encontró la alineación con ID {id}");
            }

            _logger.LogInformation("Deleting lineup entry with ID: {LineupId}", id);
            await _matchLineupRepository.DeleteAsync(id);
        }
    }
}
