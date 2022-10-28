using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;

namespace NOAM_ASISTENCIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenciasController : ControllerBase
    {
        private readonly ApplicationDbContext _dbcontext;

        public AsistenciasController(ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        // GET: api/Asistencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetAsistencia()
        {
            return await _dbcontext.Asistencia.ToListAsync();
        }

        // GET: api/Asistencias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Asistencia>> GetAsistencium(Guid id)
        {
            var asistencium = await _dbcontext.Asistencia.FindAsync(id);

            if (asistencium == null)
            {
                return NotFound();
            }

            return asistencium;
        }

        // PUT: api/Asistencias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsistencium(Guid id, Asistencia asistencium)
        {
            if (id != asistencium.IdUsuario)
            {
                return BadRequest();
            }

            _dbcontext.Entry(asistencium).State = EntityState.Modified;

            try
            {
                await _dbcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AsistenciumExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Asistencias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Asistencia>> PostAsistencium(Asistencia asistencium)
        {
            _dbcontext.Asistencia.Add(asistencium);

            try
            {
                await _dbcontext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (AsistenciumExists(asistencium.IdUsuario))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAsistencium", new { id = asistencium.IdUsuario }, asistencium);
        }

        // DELETE: api/Asistencias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsistencium(Guid id)
        {
            var asistencium = await _dbcontext.Asistencia.FindAsync(id);
            if (asistencium == null)
            {
                return NotFound();
            }

            _dbcontext.Asistencia.Remove(asistencium);
            await _dbcontext.SaveChangesAsync();

            return NoContent();
        }

        private bool AsistenciumExists(Guid id)
        {
            return _dbcontext.Asistencia.Any(e => e.IdUsuario == id);
        }
    }
}
