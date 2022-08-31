using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOAM_ASISTENCIA.Server.Models.Utils;

namespace NOAM_ASISTENCIA.Server
{
    public class InitDB
    {
        public static bool TryToMigrate(ApplicationDbContext dbcontext)
        {
            try
            {
                dbcontext.Database.Migrate();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static bool TryCreateDefaultUsersAndRoles(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string strAdministradorRole = "Administrador";
            string strGerenteRole = "Gerente";
            string strCajeroRole = "Cajero";

            string strAdministradorEmail = "usuario.administrador@gmail.com";
            string strAdministradorPassword = "Pa55w.rd";
            string strAdministradorNombre = "Usuario";
            string strAdministradorApellido = "Administrador";

            string strGerenteEmail = "usuario.gerente@gmail.com";
            string strGerentePassword = "Pa55w.rd";
            string strGerenteNombre = "Usuario";
            string strGerenteApellido = "Gerente";

            string strCajeroEmail = "usuario.cajero@gmail.com";
            string strCajeroPassword = "Pa55w.rd";
            string strCajeroNombre = "Usuario";
            string strCajeroApellido = "Cajero";

            if (!(tryCreateRoleIfNotExist(roleManager, strAdministradorRole) &&
                tryCreateRoleIfNotExist(roleManager, strGerenteRole) &&
                tryCreateRoleIfNotExist(roleManager, strCajeroRole)))
                return false;

            if (!(tryCreateUserIfNotExistsAndAddRole(userManager, strAdministradorEmail, strAdministradorPassword, strAdministradorRole, strAdministradorNombre, strAdministradorApellido) &&
                tryCreateUserIfNotExistsAndAddRole(userManager, strGerenteEmail, strGerentePassword, strGerenteRole, strGerenteNombre, strGerenteApellido) &&
                tryCreateUserIfNotExistsAndAddRole(userManager, strCajeroEmail, strCajeroPassword, strCajeroRole, strCajeroNombre, strCajeroApellido)))
                return false;

            return true;
        }

        private static bool tryCreateRoleIfNotExist(RoleManager<IdentityRole> roleManager, string strRole)
        {
            try
            {
#pragma warning disable CS8632 // La anotación para tipos de referencia que aceptan valores NULL solo debe usarse en el código dentro de un contexto de anotaciones "#nullable".
                IdentityRole? oRole = roleManager.FindByNameAsync(strRole).Result;
#pragma warning restore CS8632 // La anotación para tipos de referencia que aceptan valores NULL solo debe usarse en el código dentro de un contexto de anotaciones "#nullable".

                if (oRole == null)
                {
                    oRole = new IdentityRole();
                    oRole.Name = strRole;
                    oRole.Id = Guid.NewGuid().ToString();

                    roleManager.CreateAsync(oRole).Wait();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static bool tryCreateUserIfNotExistsAndAddRole(UserManager<ApplicationUser> userManager, string strEmail, string strPassword, string strRole, string strNombre, string strApellido)
        {
            try
            {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                ApplicationUser? oUser = userManager.FindByNameAsync(strEmail).Result;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                if (oUser == null)
                {
                    oUser = new ApplicationUser()
                    {
                        Id = Guid.NewGuid(),
                        UserName = strEmail,
                        Email = strEmail,
                        EmailConfirmed = true,
                        Nombre = strNombre,
                        Apellido = strApellido,
                        Lockout = false
                    };

                    userManager.CreateAsync(oUser, strPassword).Wait();
                }

                if (oUser != null)
                    userManager.AddToRoleAsync(oUser, strRole).Wait();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool TrySeedDefaultData(ApplicationDbContext dbcontext)
        {
            try
            {
                CreateTurnoIfNotExists(dbcontext, 1, "Lunes a viernes de 8:00 a 14:00", "L - V | M");
                CreateTurnoIfNotExists(dbcontext, 2, "Lunes a viernes de 14:00 a 22:00", "L - V | V");
                dbcontext.SaveChangesWithIdentityInsert<Turno>();

                CreateSucursalIfNotExists(dbcontext, 1, "3974 BOWLING MONTERREY");
                CreateSucursalIfNotExists(dbcontext, 2, "4010 SMART FIT PLAZA TITAN MTY");
                CreateSucursalIfNotExists(dbcontext, 3, "4011 SMART FIT MULTIPLAZA MTY");
                CreateSucursalIfNotExists(dbcontext, 4, "4012 SMART FIT PLAZA FIESTA MTY");
                CreateSucursalIfNotExists(dbcontext, 5, "4017 SMART FIT STA CATARINA MTY");
                dbcontext.SaveChangesWithIdentityInsert<SucursalServicio>();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static Turno? CreateTurnoIfNotExists(ApplicationDbContext dbcontext, int id, string descripcion, string descripcionCorta)
        {
            var obj = dbcontext.Turnos.Where(x => x.Id == id);

            if (!obj.Any())
            {
                Turno o = new Turno()
                {
                    Id = id,
                    Descripcion = descripcion,
                    DescripcionCorta = descripcionCorta
                };

                dbcontext.Turnos.Add(o);

                return o;
            }

            return null;
        }
        private static SucursalServicio? CreateSucursalIfNotExists(ApplicationDbContext dbcontext, int id, string descripcion)
        {
            var obj = dbcontext.SucursalServicios.Where(x => x.Id == id);

            if (!obj.Any())
            {
                SucursalServicio o = new SucursalServicio()
                {
                    Id = id,
                    Descripcion = descripcion
                };

                dbcontext.SucursalServicios.Add(o);

                return o;
            }

            return null;
        }
    }
}