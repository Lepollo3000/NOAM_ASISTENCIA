using System;
using System.Collections.Generic;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using NOAM_ASISTENCIA.Server.Models;
using NOAM_ASISTENCIA.Server.Models.Utils;

namespace NOAM_ASISTENCIA.Server.Data
{
    public partial class ApplicationDbContext : CustomApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }

        public virtual DbSet<Asistencium> Asistencia { get; set; } = null!;
        public virtual DbSet<SucursalServicio> SucursalServicios { get; set; } = null!;
        public virtual DbSet<Turno> Turnos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Asistencium>(entity =>
            {
                entity.HasKey(e => new { e.IdUsuario, e.IdSucursal, e.FechaEntrada });

                entity.HasOne(d => d.IdSucursalNavigation)
                    .WithMany(p => p.Asistencia)
                    .HasForeignKey(d => d.IdSucursal)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Asistencia_SucursalServicio1");

                entity.HasOne(d => d.IdUsuarioNavigation)
                    .WithMany(p => p.Asistencias)
                    .HasForeignKey(d => d.IdUsuario)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Asistencia_Usuario");
            });

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(d => d.IdTurnoNavigation)
                    .WithMany(p => p.ApplicationUsers)
                    .HasForeignKey(d => d.IdTurno)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Usuario_Turno");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
