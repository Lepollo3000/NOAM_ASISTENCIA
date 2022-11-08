using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blazorise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;
using NOAM_ASISTENCIA.Server.Models.Utils.ControllerFiltering;
using NOAM_ASISTENCIA.Shared.Models;
using NOAM_ASISTENCIA.Shared.Utils;
using NOAM_ASISTENCIA.Shared.Utils.AsistenciaModels;
using Syncfusion.Blazor.Data;

namespace NOAM_ASISTENCIA.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenciasController : ControllerBase
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AsistenciasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _dbcontext = context;
            _userManager = userManager;
        }

        // GET: api/Asistencias
        public async Task<ActionResult<IEnumerable<AsistenciaViewModel>>> GetAsistencia()
        {
            try
            {
                IQueryCollection queryString = Request.Query;

                if (queryString == null)
                    return NoContent();

                IQueryable<Asistencia> dataSource = _dbcontext.Asistencia
                    .Include(a => a.IdSucursalNavigation)
                    .Include(a => a.IdUsuarioNavigation)
                    .AsQueryable();

                int countAll = dataSource.Count();

                StringValues sSkip, sTake, sFilter, sSort;
                int skip = (queryString.TryGetValue("$skip", out sSkip)) ? Convert.ToInt32(sSkip[0]) : 0;
                int top = (queryString.TryGetValue("$top", out sTake)) ? Convert.ToInt32(sTake[0]) : countAll;
                string filter = (queryString.TryGetValue("$filter", out sFilter)) ? sFilter[0] : null!;    //filter query
                string sort = (queryString.TryGetValue("$orderby", out sSort)) ? sSort[0] : null!;         //sort query

                ApplicationUser user = await _userManager.FindByNameAsync("intendente");
                Guid userID = user.Id;
                IEnumerable<string> userRole = await _userManager.GetRolesAsync(user);

                // SI EL USUARIO QUE HIZO LA PETICION ES INTENDENTE
                if (userRole.Contains("Intendente"))
                {
                    // SE ENLISTAN SOLO LAS ASISTENCIAS DEL USUARIO
                    dataSource = dataSource.Where(ds => ds.IdUsuario == user.Id).AsQueryable();
                }

                List<DynamicLinqExpression.Filter> listFilter =
                    ParsingFilterFormula.PrepareFilter(filter);

                //Actualizacion de tabla final de filtro
                if (listFilter.Count() > 0)
                {
                    Expression<Func<Asistencia, bool>> deleg = DynamicLinqExpression.ExpressionBuilder
                        .GetExpressionFilter<Asistencia>(listFilter);
                    dataSource = dataSource.Where(deleg);
                }

                //Proceso de sorteo
                if (sort != null)
                {
                    string s = DynamicLinqExpression.GetSortString(sort);

                    if (s == null)
                        return NoContent();
                    else if (s.Length > 0)
                        dataSource = (IQueryable<Asistencia>)dataSource.OrderBy(s);
                }

                int countFiltered = dataSource.Count();
                dataSource = dataSource.Skip(skip).Take(top);

                IEnumerable<AsistenciaViewModel> model = await dataSource
                    .Select(a =>
                        new AsistenciaViewModel()
                        {
                            NombreSucursal = a.IdSucursalNavigation.Descripcion,
                            Username = a.IdUsuarioNavigation.UserName,
                            NombreUsuario = a.IdUsuarioNavigation.Nombre,
                            ApellidoUsuario = a.IdUsuarioNavigation.Apellido,
                            FechaEntrada = a.FechaEntrada,
                            FechaSalida = a.FechaSalida
                        }
                    ).ToListAsync();

                if (queryString.Keys.Contains("$inlinecount"))
                    return Ok(new SyncfusionApiResponse() { Items = model, Count = countFiltered });
                else
                    return Ok(dataSource.ToListAsync());
            }
            catch (Exception)
            {
                return NoContent();
            }
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

        private void UpdateAsistencium(Asistencia asistencia, DateTime fechaSalida)
        {
            // ACTUALIZAR LA FECHA DE SALIDA
            asistencia.FechaSalida = fechaSalida;

            _dbcontext.Entry(asistencia).State = EntityState.Modified;
        }

        // POST: api/Asistencias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostAsistencium(RegistroAsistenciaRequest model)
        {
            // SE INICIALIZA EL OBJETO DE RESPUESTA DEL ENDPOINT
            var response = new ApiResponse<RegistroAsistenciaResult>();

            // SE OBTIENEN EL USUARIO Y LA SUCURSAL CON LOS QUE SE DESEA REGISTRAR ASISTENCIA
            // (SI ES QUE EXISTEN)
            ApplicationUser? user = await _userManager.FindByNameAsync(model.Username);
            SucursalServicio? sucursal = await _dbcontext.SucursalServicios.FindAsync(model.IdSucursal);

            // SI SI EXISTEN
            if (user != null && sucursal != null)
            {
                // SE OBTIENEN LAS ASISTENCIAS CON ENTRADAS DE ASISTENCIA YA MARCADAS EN EL DIA
                // (SI ES QUE EXISTEN)
                Asistencia? asistenciaExistente = await _dbcontext.Asistencia
                    .Where(a =>
                        a.IdUsuario == user.Id &&
                        a.IdSucursal == sucursal.Id &&
                        //a.FechaEntrada == hoy &&
                        //a.FechaEntrada <= DateTime.Today.AddDays(1) &&
                        a.FechaSalida == null
                    ).OrderByDescending(a => a.FechaEntrada).FirstOrDefaultAsync();

                // SI SI EXISTEN
                if (asistenciaExistente != null)
                {
                    if (asistenciaExistente.FechaEntrada.Date == DateTime.Today)
                    {
                        DateTime fechaSalida = DateTime.Now;

                        try
                        {
                            // ACTUALIZAR LA FECHA DE SALIDA
                            UpdateAsistencium(asistenciaExistente, fechaSalida);
                            // GUARDAR CAMBIOS
                            await _dbcontext.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            // ERROR
                            var errors = new List<string>() { "Lo sentimos. Error interno del servidor, inténtelo de nuevo más tarde." };

                            response.Successful = false;
                            response.Result = null;
                            response.ErrorMessages = errors;

                            return Conflict(response);
                        }

                        // EXITO
                        response.Successful = true;
                        response.Result = new RegistroAsistenciaResult()
                        {
                            Sucursal = sucursal.Descripcion,
                            Username = user.UserName,
                            Fecha = fechaSalida,
                            EsEntrada = false
                        };

                        return Ok(response);
                    }
                }

                var asistencia = new Asistencia()
                {
                    IdUsuario = user.Id,
                    IdSucursal = sucursal.Id,
                    FechaEntrada = DateTime.Now
                };

                _dbcontext.Asistencia.Add(asistencia);

                try
                {
                    await _dbcontext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (AsistenciaExists(asistencia.IdUsuario, asistencia.IdSucursal, asistencia.FechaEntrada))
                    {
                        var errors = new List<string>() { "Lo sentimos. Error interno del servidor, inténtelo de nuevo más tarde." };

                        response.Successful = false;
                        response.Result = null;
                        response.ErrorMessages = errors;

                        return Conflict(response);
                    }
                    else
                    {
                        var errors = new List<string>() { "Lo sentimos. Error interno del servidor, inténtelo de nuevo más tarde." };

                        response.Successful = false;
                        response.Result = null;
                        response.ErrorMessages = errors;

                        return StatusCode(500, response);
                    }
                }

                // EXITO
                response.Successful = true;
                response.Result = new RegistroAsistenciaResult()
                {
                    Sucursal = sucursal.Descripcion,
                    Username = user.UserName,
                    Fecha = asistencia.FechaEntrada,
                    EsEntrada = true
                };

                return CreatedAtAction("GetAsistencia", new { username = user.UserName }, response);
            }
            else
            {
                var errors = new List<string>() { "Información inválida. Verifique que la sucursal exista o tenga la sesión iniciada." };

                response.Successful = false;
                response.Result = null;
                response.ErrorMessages = errors;

                return BadRequest(response);
            }
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

        private bool AsistenciaExists(Guid idUsuario, int idSucursal, DateTime fechaEntrada)
        {
            return _dbcontext.Asistencia.Any(e =>
                e.IdUsuario == idUsuario &&
                e.IdSucursal == idSucursal &&
                e.FechaEntrada == fechaEntrada
            );
        }
    }
}
