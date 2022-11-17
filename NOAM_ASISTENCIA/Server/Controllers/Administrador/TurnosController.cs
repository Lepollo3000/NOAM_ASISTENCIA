using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;
using NOAM_ASISTENCIA.Server.Models.Utils.ControllerFiltering;
using NOAM_ASISTENCIA.Shared.Models;
using NOAM_ASISTENCIA.Shared.Utils;
using Syncfusion.Blazor.Data;

namespace NOAM_ASISTENCIA.Server.Controllers.Administrador
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurnosController : ControllerBase
    {
        private readonly ApplicationDbContext _dbcontext;

        public TurnosController(ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        // GET: api/Turnos
        [HttpGet]
        public async Task<IActionResult> GetTurnos()
        {
            try
            {
                IQueryCollection queryString = Request.Query;

                if (queryString == null)
                    return NoContent();

                IQueryable<Turno> dataSource = _dbcontext.Turnos.Where(d => d.Id != 1).AsQueryable();

                int countAll = dataSource.Count();

                StringValues sSkip, sTake, sFilter, sSort;
                int skip = (queryString.TryGetValue("$skip", out sSkip)) ? Convert.ToInt32(sSkip[0]) : 0;
                int top = (queryString.TryGetValue("$top", out sTake)) ? Convert.ToInt32(sTake[0]) : countAll;
                string filter = (queryString.TryGetValue("$filter", out sFilter)) ? sFilter[0] : null!;    //filter query
                string sort = (queryString.TryGetValue("$orderby", out sSort)) ? sSort[0] : null!;         //sort query

                List<DynamicLinqExpression.Filter> listFilter =
                    ParsingFilterFormula.PrepareFilter(filter);

                // PROCESO DE FILTRADO
                if (listFilter.Count() > 0)
                {
                    Expression<Func<Turno, bool>> deleg = DynamicLinqExpression.ExpressionBuilder
                        .GetExpressionFilter<Turno>(listFilter);

                    dataSource = dataSource.Where(deleg);
                }

                // PROCESO DE SORTEO
                if (sort != null)
                {
                    string s = DynamicLinqExpression.GetSortString(sort);

                    if (s == null)
                        return NoContent();
                    else if (s.Length > 0)
                        dataSource = (IQueryable<Turno>)dataSource.OrderBy(sort);
                }

                // SE HACE EL QUERY PARA INSTANCIAR LA INFORMACION
                IEnumerable<Turno> response = await dataSource.ToListAsync();

                // SE OBTIENE EL CONTEO DE LOS REGISTROS AGRUPADOS TOTALES Y SE FILTRA EL PAGINADO
                int countFiltered = response.Count();
                response = response.Skip(skip).Take(top);

                if (queryString.Keys.Contains("$inlinecount"))
                    return Ok(new SyncfusionApiResponse() { Items = response!, Count = countFiltered });
                else
                    return Ok(response);
            }
            catch (Exception)
            {
                return NoContent();
            }
        }

        // GET: api/Turnos/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTurno(int id)
        {
            var turno = await _dbcontext.Turnos.FindAsync(id);

            if (turno == null)
            {
                return NotFound();
            }

            TurnoDTO response = new TurnoDTO()
            {
                Id = turno.Id,
                Descripcion = turno.Descripcion,
                DescripcionCorta = turno.DescripcionCorta
            };

            return Ok(response);
        }

        // PUT: api/Turnos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut()]
        public async Task<IActionResult> PutTurno(Turno turno)
        {
            _dbcontext.Entry(turno).State = EntityState.Modified;

            try
            {
                await _dbcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TurnoExists(turno.Id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(500, "Error interno del servidor. Intente de nuevo más tarde o contacte a un administrador");
                }
            }

            return Ok();
        }

        // POST: api/Turnos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Turno>> PostTurno(Turno turno)
        {
            _dbcontext.Turnos.Add(turno);
            await _dbcontext.SaveChangesAsync();

            return CreatedAtAction("GetTurno", new { id = turno.Id }, turno);
        }

        /*// DELETE: api/Turnos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTurno(int id)
        {
            var turno = await _dbcontext.Turnos.FindAsync(id);
            if (turno == null)
            {
                return NotFound();
            }

            _dbcontext.Turnos.Remove(turno);
            await _dbcontext.SaveChangesAsync();

            return NoContent();
        }*/

        private bool TurnoExists(int id)
        {
            return _dbcontext.Turnos.Any(e => e.Id == id);
        }
    }
}
