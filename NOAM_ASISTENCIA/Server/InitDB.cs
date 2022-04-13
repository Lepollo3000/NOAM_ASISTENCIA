using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;
//using NOAM_ASISTENCIA.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace NOAM_ASISTENCIA.Server
{
    public class InitDB
    {
        public static bool TryToMigrate(/*FswDevContext dbcontext,*/ ApplicationDbContext identitycontext)
        {
            try
            {
                identitycontext.Database.Migrate();
                //dbcontext.Database.Migrate();
            }
            catch (Exception)
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
            string strAdministradorFirstName = "Usuario";
            string strAdministradorLastName = "Administrador";

            string strGerenteEmail = "usuario.gerente@gmail.com";
            string strGerentePassword = "Pa55w.rd";
            string strGerenteFirstName = "Usuario";
            string strGerenteLastName = "Gerente";

            string strCajeroEmail = "usuario.cajero@gmail.com";
            string strCajeroPassword = "Pa55w.rd";
            string strCajeroFirstName = "Usuario";
            string strCajeroLastName = "Cajero";

            if (!(tryCreateRoleIfNotExist(roleManager, strAdministradorRole) &&
                tryCreateRoleIfNotExist(roleManager, strGerenteRole) &&
                tryCreateRoleIfNotExist(roleManager, strCajeroRole)))
                return false;

            if (!(tryCreateUserIfNotExistsAndAddRole(userManager, strAdministradorEmail, strAdministradorPassword, strAdministradorRole, strAdministradorFirstName, strAdministradorLastName) &&
                tryCreateUserIfNotExistsAndAddRole(userManager, strGerenteEmail, strGerentePassword, strGerenteRole, strGerenteFirstName, strGerenteLastName) &&
                tryCreateUserIfNotExistsAndAddRole(userManager, strCajeroEmail, strCajeroPassword, strCajeroRole, strCajeroFirstName, strCajeroLastName)))
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

        private static bool tryCreateUserIfNotExistsAndAddRole(UserManager<ApplicationUser> userManager, string strEmail, string strPassword, string strRole, string strFirstName, string strLastName)
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
                        Id = Guid.NewGuid().ToString(),
                        UserName = strEmail,
                        Email = strEmail,
                        EmailConfirmed = true,
                        FirstName = strFirstName,
                        LastName = strLastName,
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
        /*
        public static bool TrySeedDefaultData(FswDevContext dbcontext)
        {
            try
            {
                CreateCategoriaIfNotExists(dbcontext, 1, null, "Comidas");
                CreateCategoriaIfNotExists(dbcontext, 2, null, "Bebidas");
                CreateCategoriaIfNotExists(dbcontext, 3, null, "Snacks");
                CreateCategoriaIfNotExists(dbcontext, 4, 2, "Refrescos");

                CreateContactoTipoIfNotExists(dbcontext, 1, "Compra");

                CreateInventarioStatusIfNotExists(dbcontext, 1, "En proceso");
                CreateInventarioStatusIfNotExists(dbcontext, 2, "Concretada");
                CreateInventarioStatusIfNotExists(dbcontext, 3, "Cancelada");

                CreateTipoMonedumIfNotExists(dbcontext, 1, "MXN");
                CreateTipoMonedumIfNotExists(dbcontext, 2, "USD");

                CreateTipoPagoIfNotExists(dbcontext, 1, "Débito");
                CreateTipoPagoIfNotExists(dbcontext, 1, "Tarjeta");

                CreateUnidadMedidumIfNotExists(dbcontext, 1, "pza.");
                CreateUnidadMedidumIfNotExists(dbcontext, 2, "ml.");
                CreateUnidadMedidumIfNotExists(dbcontext, 3, "L.");
                CreateUnidadMedidumIfNotExists(dbcontext, 4, "g.");
                CreateUnidadMedidumIfNotExists(dbcontext, 5, "kg.");

                CreateVentaStatusIfNotExists(dbcontext, 1, "En proceso");
                CreateVentaStatusIfNotExists(dbcontext, 2, "Pago en Proceso");
                CreateVentaStatusIfNotExists(dbcontext, 3, "Pago Concretado");
                CreateVentaStatusIfNotExists(dbcontext, 4, "Cancelada");

                CreateCajaIfNotExists(dbcontext, 1, "Caja 1");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        
        private static Categorium CreateCategoriaIfNotExists(FswDevContext dbcontext, int Id, int? padre, string descripcion)
        {
            var obj = dbcontext.Categoria.Where(x => x.Id == Id);

            if (!obj.Any())
            {
                Categorium o = new Categorium()
                {
                    Descripcion = descripcion,
                    Padre = padre,
                    Status = true
                };

                dbcontext.Categoria.Add(o);
                dbcontext.SaveChanges();

                return o;
            }

            return null;
        }
        */
    }
}