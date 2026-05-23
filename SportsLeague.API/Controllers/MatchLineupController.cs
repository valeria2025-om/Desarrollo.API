using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers
{
    [ApiController]
    [Route("api/match/{matchId}/lineup")]
    public class MatchLineupController : ControllerBase
    {
        private readonly IMatchLineupService _matchLineupService;
        private readonly IMapper _mapper;

        public MatchLineupController(
            IMatchLineupService matchLineupService,
            IMapper mapper)
        {
            _matchLineupService = matchLineupService;
            _mapper = mapper;
        }

        /// <summary>
        /// POST /api/match/{matchId}/lineup - Agregar un jugador a la alineación
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MatchLineupDto>> AddPlayer(int matchId, CreateMatchLineupDto dto)
        {
            try
            {
                var lineup = _mapper.Map<MatchLineup>(dto);
                var created = await _matchLineupService.AddPlayerToLineupAsync(matchId, lineup);
                var lineupWithDetails = await _matchLineupService.GetLineupByMatchAsync(matchId);
                var createdEntry = lineupWithDetails.FirstOrDefault(l => l.Id == created.Id);
                var responseDto = _mapper.Map<MatchLineupDto>(createdEntry);

                return CreatedAtAction(
                    nameof(GetLineup),
                    new { matchId },
                    responseDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/match/{matchId}/lineup - Obtener la alineación completa del partido
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchLineupDto>>> GetLineup(int matchId)
        {
            try
            {
                var lineup = await _matchLineupService.GetLineupByMatchAsync(matchId);
                return Ok(_mapper.Map<IEnumerable<MatchLineupDto>>(lineup));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/match/{matchId}/lineup/team/{teamId} - Obtener alineación de un equipo específico
        /// </summary>
        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<MatchLineupDto>>> GetLineupByTeam(int matchId, int teamId)
        {
            try
            {
                var lineup = await _matchLineupService.GetLineupByMatchAndTeamAsync(matchId, teamId);
                return Ok(_mapper.Map<IEnumerable<MatchLineupDto>>(lineup));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/match/{matchId}/lineup/{id} - Eliminar un jugador de la alineación
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLineup(int matchId, int id)
        {
            try
            {
                await _matchLineupService.DeleteLineupAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
