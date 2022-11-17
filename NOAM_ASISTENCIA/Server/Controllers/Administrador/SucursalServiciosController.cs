using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Administrador, Intendente")]
    [Route("api/[controller]")]
    [ApiController]
    public class SucursalServiciosController : ControllerBase
    {
        private readonly ApplicationDbContext _dbcontext;

        public SucursalServiciosController(ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        // GET: api/SucursalServicios
        [HttpGet]
        public async Task<IActionResult> GetSucursalServicios()
        {
            try
            {
                IQueryCollection queryString = Request.Query;

                if (queryString == null)
                    return NoContent();

                IQueryable<SucursalServicio> dataSource = _dbcontext.SucursalServicios.AsQueryable();

                int countAll = dataSource.Count();

                int skip = (queryString.TryGetValue("$skip", out StringValues sSkip)) ? Convert.ToInt32(sSkip[0]) : 0;
                int top = (queryString.TryGetValue("$top", out StringValues sTake)) ? Convert.ToInt32(sTake[0]) : countAll;
                string filter = (queryString.TryGetValue("$filter", out StringValues sFilter)) ? sFilter[0] : null!;    //filter query
                string sort = (queryString.TryGetValue("$orderby", out StringValues sSort)) ? sSort[0] : null!;         //sort query

                List<DynamicLinqExpression.Filter> listFilter =
                    ParsingFilterFormula.PrepareFilter(filter!);

                // PROCESO DE FILTRADO
                if (listFilter.Count() > 0)
                {
                    Expression<Func<SucursalServicio, bool>> deleg = DynamicLinqExpression.ExpressionBuilder
                        .GetExpressionFilter<SucursalServicio>(listFilter);

                    dataSource = dataSource.Where(deleg);
                }

                // PROCESO DE SORTEO
                if (sort != null)
                {
                    string s = DynamicLinqExpression.GetSortString(sort);

                    if (s == null)
                        return NoContent();
                    else if (s.Length > 0)
                        dataSource = (IQueryable<SucursalServicio>)dataSource.OrderBy(sort);
                }

                // SE HACE EL QUERY PARA INSTANCIAR LA INFORMACION
                IEnumerable<SucursalServicio> response = await dataSource.ToListAsync();

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

        // GET: api/SucursalServicios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SucursalServicio>> GetSucursalServicio(int id)
        {
            var sucursalServicio = await _dbcontext.SucursalServicios.FindAsync(id);
            var response = new ApiResponse<SucursalServicioDTO>();

            if (sucursalServicio == null)
            {
                var errors = new List<string>() { "Lo sentimos. La sucursal solicitada no se encontró o no existe." };

                response.Successful = false;
                response.ErrorMessages = errors;

                return NotFound(response);
            }
            else
            {
                response.Successful = true;
                response.Result = new SucursalServicioDTO()
                {
                    Id = sucursalServicio.Id,
                    Descripcion = sucursalServicio.Descripcion
                };
            }

            return Ok(response);
        }

        // PUT: api/SucursalServicios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSucursalServicio(int id, SucursalServicio sucursalServicio)
        {
            if (id != sucursalServicio.Id)
            {
                return BadRequest();
            }

            _dbcontext.Entry(sucursalServicio).State = EntityState.Modified;

            try
            {
                await _dbcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SucursalServicioExists(id))
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

        // POST: api/SucursalServicios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SucursalServicio>> PostSucursalServicio(SucursalServicio sucursalServicio)
        {
            _dbcontext.SucursalServicios.Add(sucursalServicio);
            await _dbcontext.SaveChangesAsync();

            return CreatedAtAction("GetSucursalServicio", new { id = sucursalServicio.Id }, sucursalServicio);
        }

        /*// DELETE: api/SucursalServicios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSucursalServicio(int id)
        {
            var sucursalServicio = await _dbcontext.SucursalServicios.FindAsync(id);
            if (sucursalServicio == null)
            {
                return NotFound();
            }

            _dbcontext.SucursalServicios.Remove(sucursalServicio);
            await _dbcontext.SaveChangesAsync();

            return NoContent();
        }*/

        private bool SucursalServicioExists(int id)
        {
            return _dbcontext.SucursalServicios.Any(e => e.Id == id);
        }
    }
}
