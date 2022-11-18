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
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbcontext;

        public UsersController(ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
        {
            try
            {
                IQueryCollection queryString = Request.Query;

                if (queryString == null)
                    return NoContent();

                IQueryable<ApplicationUser> dataSource = _dbcontext.Users
                    .Include(a => a.IdTurnoNavigation).AsQueryable();

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
                    Expression<Func<ApplicationUser, bool>> deleg = DynamicLinqExpression.ExpressionBuilder
                        .GetExpressionFilter<ApplicationUser>(listFilter);

                    dataSource = dataSource.Where(deleg);
                }

                // PROCESO DE SORTEO
                if (sort != null)
                {
                    string s = DynamicLinqExpression.GetSortString(sort);

                    if (s == null)
                        return NoContent();
                    else if (s.Length > 0)
                        dataSource = (IQueryable<ApplicationUser>)dataSource.OrderBy(sort);
                }

                // SE HACE EL QUERY PARA INSTANCIAR LA INFORMACION
                IEnumerable<ApplicationUser> listedDataSource = await dataSource.ToListAsync();

                IEnumerable<UserDTO> response = listedDataSource
                    .Select(appUser =>
                        new UserDTO()
                        {
                            Id = appUser.Id,
                            Username = appUser.UserName,
                            Nombre = appUser.Nombre,
                            Apellido = appUser.Apellido,
                            TurnoNombre = appUser.IdTurnoNavigation.DescripcionCorta,
                            Lockout = appUser.Lockout
                        }
                    ).ToList();

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

        /*// GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUser>> GetApplicationUser(Guid id)
        {
            var applicationUser = await _dbcontext.Users.FindAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return applicationUser;
        }*/

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutApplicationUser(UserDTO applicationUser)
        {
            ApplicationUser? user = await _dbcontext.Users.FindAsync(applicationUser.Id);

            if (user == null) return NotFound();
            else
            {
                user.Nombre = applicationUser.Nombre;
                user.Apellido = applicationUser.Apellido;
                user.Lockout = applicationUser.Lockout;

                _dbcontext.Entry(user).State = EntityState.Modified;

                try
                {
                    await _dbcontext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationUserExists(applicationUser.Id))
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
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ApplicationUser>> PostApplicationUser(ApplicationUser applicationUser)
        {
            _dbcontext.Users.Add(applicationUser);
            await _dbcontext.SaveChangesAsync();

            return CreatedAtAction("GetApplicationUser", new { id = applicationUser.Id }, applicationUser);
        }

        /*// DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplicationUser(Guid id)
        {
            var applicationUser = await _dbcontext.Users.FindAsync(id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            _dbcontext.Users.Remove(applicationUser);
            await _dbcontext.SaveChangesAsync();

            return NoContent();
        }*/

        private bool ApplicationUserExists(Guid id)
        {
            return _dbcontext.Users.Any(e => e.Id == id);
        }
    }
}
