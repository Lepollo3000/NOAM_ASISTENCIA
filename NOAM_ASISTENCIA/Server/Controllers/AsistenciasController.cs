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
using Syncfusion.Blazor.Grids;

namespace NOAM_ASISTENCIA.Server.Controllers
{
    [Authorize(Roles = "Intendente, Gerente")]
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
        [HttpGet("{requestingUsername}")]
        public async Task<IActionResult> GetAsistencia(string requestingUsername, string? requestedUsername = null!)
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

                // OBTENER LOS DATOS DEL USUARIO QUE HACE LA SOLICITUD
                ApplicationUser requestingUser = await _userManager.FindByNameAsync(requestingUsername);
                Guid userID = requestingUser.Id;
                IEnumerable<string> userRole = await _userManager.GetRolesAsync(requestingUser);

                // DATOS DE FILTRADO PRINCIPAL
                DateTime? minDate = null!;
                DateTime? maxDate = null!;

                bool esIntendente = false;

                List<DynamicLinqExpression.Filter> listFilter =
                    ParsingFilterFormula.PrepareFilter(filter!);

                //Actualizacion de tabla final de filtro
                if (listFilter.Count() > 0)
                {
                    /*Expression<Func<Asistencia, bool>> deleg = DynamicLinqExpression.ExpressionBuilder
                        .GetExpressionFilter<Asistencia>(listFilter);*/

                    foreach (var item in listFilter)
                    {
                        if (item.PropertyName.Contains("Fecha"))
                        {
                            // FILTRAR EXACTAMENTE LA FECHA INDICADA
                            if (item.Operation == DynamicLinqExpression.Op.Equals)
                            {
                                string[] dateString = item.Value.ToString()!.Split(" ", 3);
                                DateTime date = DateTime.Parse(dateString[1]);

                                dataSource = dataSource.Where(d => d.FechaEntrada.Date == date);
                            }
                            // FILTRAR FECHAS MENORES A LA FECHA DADA
                            else if (item.Operation == DynamicLinqExpression.Op.LessThanOrEqual)
                            {
                                string[] dateString = item.Value.ToString()!.Split(" ", 3);
                                DateTime date = DateTime.Parse(dateString[1]);

                                // ASIGNAR EL VALOR A MAX DATE
                                maxDate = date;

                                dataSource = dataSource.Where(d => d.FechaEntrada.Date <= date);
                            }
                            // FILTRAR FECHAS MAYORES A LA FECHA DADA
                            else if (item.Operation == DynamicLinqExpression.Op.GreaterThanOrEqual)
                            {
                                string[] dateString = item.Value.ToString()!.Split(" ", 3);
                                DateTime date = DateTime.Parse(dateString[1]);

                                // ASIGNAR EL VALOR A MIN DATE
                                minDate = date;

                                dataSource = dataSource.Where(d => d.FechaEntrada.Date >= date);
                            }
                        }
                    }
                }

                /*//Proceso de sorteo
                if (sort != null)
                {
                    string s = DynamicLinqExpression.GetSortString(sort);

                    if (s == null)
                        return NoContent();
                    else if (s.Length > 0)
                        dataSource = (IQueryable<Asistencia>)dataSource.OrderBy(sort);
                }*/

                // SI EL USUARIO QUE HIZO LA PETICION ES INTENDENTE
                if (userRole.Contains("Intendente"))
                {
                    // SE ENLISTAN SOLO LAS ASISTENCIAS DEL USUARIO
                    dataSource = dataSource.Where(ds => ds.IdUsuario == requestingUser.Id).AsQueryable();

                    esIntendente = true;
                }

                // SE HACE EL QUERY PARA INSTANCIAR LA INFORMACION
                IEnumerable<Asistencia> listedDataSource = dataSource.ToList();

                int countFiltered = listedDataSource.Count();

                IEnumerable<object?> response = null!;

                // OBTENER LOS DATOS DEL USUARIO QUE SE ESTA VERIFICANDO
                ApplicationUser requestedUser = null!;

                if (requestedUsername != null)
                    requestedUser = await _userManager.FindByNameAsync(requestedUsername);

                // SI ES UN INTENDENTE EL QUE HACE LA SOLICITUD
                if (esIntendente)
                {
                    if (minDate != null && maxDate != null)
                    {
                        // SE AGRUPAN LOS REGISTROS CON RESPECTO A LA FECHA PARA MANEJAR TODO POR DIA
                        // Y SE FILTRA CON EL USUARIO DADO POR LA SOLICITUD, ADEMAS DEL RANGO DE FECHAS
                        IEnumerable<Asistencia> groupedData = listedDataSource
                            //.Where(b => b.FechaSalida != null)
                            .Where(a => a.FechaEntrada >= minDate)
                            .Where(a => a.FechaEntrada <= maxDate)
                            //.GroupBy(a => a.FechaEntrada.Date)
                            .ToList();

                        countFiltered = groupedData.Count();

                        // QUEDAN LOS REGISTROS POR DIA EN UN DETERMINADO RANGO DE TIEMPO
                        response = groupedData
                                //.Select(a => a
                                .Select(c =>
                                    new ReporteAsistenciaGeneralUsuarioDTO()
                                    {
                                        Username = c.IdUsuarioNavigation.UserName,
                                        UsuarioNombre = c.IdUsuarioNavigation.Nombre,
                                        UsuarioApellido = c.IdUsuarioNavigation.Apellido,
                                        Sucursal = c.IdSucursalNavigation.Descripcion,
                                        Fecha = c.FechaEntrada,
                                        FechaSalida = c.FechaSalida,
                                        HorasLaboradas = c.FechaSalida != null
                                            ? (c.FechaSalida - c.FechaEntrada)!.Value.TotalHours
                                            : 0
                                    }
                            //).FirstOrDefault()
                            ).Where(a => a != null).ToList();
                    }
                    else
                    {
                        minDate = DateTime.Today;
                        maxDate = DateTime.Today.AddDays(1).AddSeconds(-1);

                        // SE AGRUPAN LOS REGISTROS CON RESPECTO AL USUARIO
                        IEnumerable<Asistencia> groupedData = listedDataSource
                            //.Where(b => b.FechaSalida != null)
                            .Where(a => a.FechaEntrada >= minDate)
                            .Where(a => a.FechaEntrada <= maxDate)
                            //.GroupBy(a => a.IdUsuario)
                            .ToList();

                        countFiltered = groupedData.Count();

                        /*groupedData = groupedData
                            .Select(a => a.Where(b => b.FechaSalida != null));*/

                        // QUEDAN LOS REGISTROS POR USUARIO EN UN DETERMINADO RANGO DE DIAS
                        response = groupedData
                                //.Select(a => a
                                .Select(c =>
                                    new ReporteAsistenciaGeneralUsuarioDTO()
                                    {
                                        Username = c.IdUsuarioNavigation.UserName,
                                        UsuarioNombre = c.IdUsuarioNavigation.Nombre,
                                        UsuarioApellido = c.IdUsuarioNavigation.Apellido,
                                        Sucursal = c.IdSucursalNavigation.Descripcion,
                                        Fecha = c.FechaEntrada,
                                        FechaSalida = c.FechaSalida,
                                        HorasLaboradas = c.FechaSalida != null
                                            ? (c.FechaSalida - c.FechaEntrada)!.Value.TotalHours
                                            : 0
                                    }
                            //).FirstOrDefault()
                            ).Where(a => a != null).ToList();
                    }
                }
                // SI NO SE ESTA PIDIENDO UN USUARIO EN ESPECIFICO
                else if (requestedUser == null)
                {
                    IEnumerable<Asistencia> listedFinalData = listedDataSource
                        .OrderByDescending(a => a.FechaEntrada).ToList();

                    if (minDate != null && maxDate != null)
                    {
                        // SE AGRUPAN LOS REGISTROS CON RESPECTO AL USUARIO Y SE FILTRA POR RANGO DE DIAS
                        IEnumerable<IGrouping<Guid, Asistencia>> groupedData = listedDataSource
                            .Where(b => b.FechaSalida != null)
                            .Where(a => a.FechaEntrada >= minDate)
                            .Where(a => a.FechaEntrada <= maxDate)
                            .GroupBy(a => a.IdUsuario).ToList();

                        countFiltered = groupedData.Count();

                        // QUEDAN LOS REGISTROS POR USUARIO EN UN DETERMINADO RANGO DE DIAS
                        response = groupedData
                            .Select(a => a
                                .Select(c =>
                                    new ReporteAsistenciaGeneralDTO()
                                    {
                                        Username = c.IdUsuarioNavigation.UserName,
                                        UsuarioNombre = c.IdUsuarioNavigation.Nombre,
                                        UsuarioApellido = c.IdUsuarioNavigation.Apellido,
                                        Fecha = minDate.Value.Date,
                                        HorasLaboradas = a.Sum(c => (c.FechaSalida - c.FechaEntrada)!.Value.TotalHours)
                                    }
                                ).FirstOrDefault()
                            ).Where(a => a != null).ToList();
                    }
                    else
                    {

                        // OBTENER DIAS DE ESTE MES POR PREDETERMINADO
                        DateTime currentDate = DateTime.Now;
                        minDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                        maxDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));

                        // SE AGRUPAN LOS REGISTROS CON RESPECTO AL USUARIO
                        IEnumerable<IGrouping<Guid, Asistencia>> groupedData = listedDataSource
                            .Where(b => b.FechaSalida != null)
                            .Where(a => a.FechaEntrada >= minDate)
                            .Where(a => a.FechaEntrada <= maxDate)
                            .GroupBy(a => a.IdUsuario).ToList();

                        countFiltered = groupedData.Count();

                        /*groupedData = groupedData
                            .Select(a => a.Where(b => b.FechaSalida != null));

                        // QUEDAN LOS REGISTROS POR USUARIO EN UN DETERMINADO RANGO DE DIAS*/
                        response = groupedData
                            .Select(a => a
                                .Select(c =>
                                    new ReporteAsistenciaGeneralDTO()
                                    {
                                        Username = c.IdUsuarioNavigation.UserName,
                                        UsuarioNombre = c.IdUsuarioNavigation.Nombre,
                                        UsuarioApellido = c.IdUsuarioNavigation.Apellido,
                                        Fecha = c.FechaEntrada.Date,
                                        HorasLaboradas = a.Sum(c => (c.FechaSalida - c.FechaEntrada)!.Value.TotalHours)
                                    }
                                ).FirstOrDefault()
                            ).Where(a => a != null).ToList();
                    }
                }
                // SI SI SE ESTA PIDIENDO UN USUARIO EN ESPECIFICO
                else
                {
                    if (minDate != null && maxDate != null)
                    {
                        // SE AGRUPAN LOS REGISTROS CON RESPECTO A LA FECHA PARA MANEJAR TODO POR DIA
                        // Y SE FILTRA CON EL USUARIO DADO POR LA SOLICITUD, ADEMAS DEL RANGO DE FECHAS
                        IEnumerable<Asistencia> groupedData = listedDataSource
                            //.Where(b => b.FechaSalida != null)
                            .Where(a => a.IdUsuario == requestedUser.Id)
                            .Where(a => a.FechaEntrada >= minDate)
                            .Where(a => a.FechaEntrada <= maxDate)
                            //.GroupBy(a => a.FechaEntrada.Date)
                            .ToList();

                        countFiltered = groupedData.Count();

                        // QUEDAN LOS REGISTROS POR DIA EN UN DETERMINADO RANGO DE TIEMPO
                        response = groupedData
                                //.Select(a => a
                                .Select(c =>
                                    new ReporteAsistenciaGeneralUsuarioDTO()
                                    {
                                        Username = c.IdUsuarioNavigation.UserName,
                                        UsuarioNombre = c.IdUsuarioNavigation.Nombre,
                                        UsuarioApellido = c.IdUsuarioNavigation.Apellido,
                                        Sucursal = c.IdSucursalNavigation.Descripcion,
                                        Fecha = c.FechaEntrada,
                                        FechaSalida = c.FechaSalida,
                                        HorasLaboradas = c.FechaSalida != null
                                            ? (c.FechaSalida - c.FechaEntrada)!.Value.TotalHours
                                            : 0
                                    }
                            //).FirstOrDefault()
                            ).Where(a => a != null).ToList();
                    }
                    else
                    {
                        // OBTENER DIAS DE ESTE MES POR PREDETERMINADO
                        DateTime currentDate = DateTime.Now;
                        minDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                        maxDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));

                        // SE AGRUPAN LOS REGISTROS CON RESPECTO A LA FECHA PARA MANEJAR TODO POR DIA
                        // Y SE FILTRA CON EL USUARIO DADO POR LA SOLICITUD
                        IEnumerable<Asistencia> groupedData = listedDataSource
                            //.Where(b => b.FechaSalida != null)
                            .Where(a => a.IdUsuario == requestedUser.Id)
                            .Where(a => a.FechaEntrada >= minDate)
                            .Where(a => a.FechaEntrada <= maxDate)
                            //.GroupBy(a => a.FechaEntrada.Date)
                            .ToList();

                        countFiltered = groupedData.Count();

                        // QUEDAN LOS REGISTROS POR USUARIO EN UN DETERMINADO RANGO DE DIAS
                        response = groupedData
                                //.Select(a => a
                                .Select(c =>
                                    new ReporteAsistenciaGeneralUsuarioDTO()
                                    {
                                        Username = c.IdUsuarioNavigation.UserName,
                                        UsuarioNombre = c.IdUsuarioNavigation.Nombre,
                                        UsuarioApellido = c.IdUsuarioNavigation.Apellido,
                                        Sucursal = c.IdSucursalNavigation.Descripcion,
                                        Fecha = c.FechaEntrada,
                                        FechaSalida = c.FechaSalida,
                                        HorasLaboradas = c.FechaSalida != null
                                            ? (c.FechaSalida - c.FechaEntrada)!.Value.TotalHours
                                            : 0
                                    }
                            //).FirstOrDefault()
                            ).Where(a => a != null).ToList();
                    }
                }

                // SE OBTIENE EL CONTEO DE LOS REGISTROS AGRUPADOS TOTALES Y SE FILTRA EL PAGINADO
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

        /*// GET: api/Asistencias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Asistencia>> GetAsistencium(Guid id)
        {
            var asistencium = await _dbcontext.Asistencia.FindAsync(id);

            if (asistencium == null)
            {
                return NotFound();
            }

            return asistencium;
        }*/

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

                return Ok(response);
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
