using System.Data.Entity;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Threading.Tasks;
using ControlActividades.Models.db;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControlActividades.Models
{
    // Para agregar datos de perfil del usuario, agregue más propiedades a su clase ApplicationUser. Visite https://go.microsoft.com/fwlink/?LinkID=317594 para obtener más información.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Tenga en cuenta que authenticationType debe coincidir con el valor definido en CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Agregar reclamaciones de usuario personalizadas aquí
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<tbUsuariosFcmTokens> tbUsuariosFcmTokens { get; set; }
        public DbSet<tbAlumnos> tbAlumnos { get; set; }
        public DbSet<tbDocentes> tbDocentes { get; set; }
        public DbSet<tbAlumnosGrupos> tbAlumnosGrupos { get; set; }
        public DbSet<tbAlumnosMaterias> tbAlumnosMaterias { get; set; }
        //public DbSet<tbAlumnosActividades> tbAlumnosActividades { get; set; }
        //public DbSet<tbEntregableAlumno> tbEntregablesAlumno { get; set; }
        public DbSet<tbGrupos> tbGrupos { get; set; }
        public DbSet<tbGruposMaterias> tbGruposMaterias { get; set; }
        public DbSet<tbMaterias> tbMaterias { get; set; }
        public DbSet<tbActividades> tbActividades { get; set; }
        //public DbSet<tbCalificaciones> tbCalificaciones { get; set; }
        //public DbSet<cTiposActividades> cTiposActividades { get; set; }
        public DbSet<tbAvisos> tbAvisos { get; set; }
        public DbSet<tbEventosAgenda> tbEventosAgenda { get; set; }
        public DbSet<tbEventosGrupos> tbEventosGrupos { get; set; }
        public DbSet<tbEventosMaterias> tbEventosMaterias { get; set; }
        public DbSet<tbNotificaciones> tbNotificaciones { get; set; }



        public DbSet<tbEntregaActividadAlumno> tbEntregaActividadAlumno { get; set; }
        public DbSet<tbEntregables> tbEntregables { get; set; }
        public DbSet<cTipoEntrega> cTipoEntrega { get; set; }
        public DbSet<cTipoNotificacion> cTipoNotificacion { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region UsuarioFcmTokens
            modelBuilder.Entity<tbUsuariosFcmTokens>()
                .HasRequired(a => a.IdentityUser)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .WillCascadeOnDelete(false);
            #endregion

            #region AlumnosGrupos
            modelBuilder.Entity<tbAlumnosGrupos>()
                .HasRequired(a => a.Alumnos)
                .WithMany(a => a.AlumnosGrupos)
                .HasForeignKey(a => a.AlumnoId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbAlumnosGrupos>()
                .HasRequired(a => a.Grupos)
                .WithMany(a => a.AlumnosGrupos)
                .HasForeignKey(a => a.GrupoId)
                .WillCascadeOnDelete(false);
            #endregion

            #region Alumnos Materias
            modelBuilder.Entity<tbAlumnosMaterias>()
                .HasRequired(a => a.Alumnos)
                .WithMany(a => a.AlumnosMaterias)
                .HasForeignKey(a => a.AlumnoId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbAlumnosMaterias>()
                .HasRequired(a => a.Materias)
                .WithMany(a => a.AlumnosMaterias)
                .HasForeignKey(a => a.MateriaId)
                .WillCascadeOnDelete(false);
            #endregion

            #region Alumnos Actividades
            //modelBuilder.Entity<tbAlumnosActividades>()
            //    .HasRequired(a => a.Alumnos)
            //    .WithMany(a => a.AlumnosActividades)
            //    .HasForeignKey(a => a.AlumnoId)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<tbAlumnosActividades>()
            //    .HasRequired(a => a.Actividades)
            //    .WithMany(a => a.AlumnosActividades)
            //    .HasForeignKey(a => a.ActividadId)
            //    .WillCascadeOnDelete(false);


            //modelBuilder.Entity<tbAlumnosActividades>()
            //    .HasOptional(a => a.EntregablesAlumno)
            //    .WithRequired(e => e.AlumnosActividades);
            #endregion

            #region Grupos
            modelBuilder.Entity<tbGrupos>()
                .HasRequired(a => a.Docentes)
                .WithMany(a => a.Grupos)
                .HasForeignKey(a => a.DocenteId)
                .WillCascadeOnDelete(false);
            #endregion

            #region Grupos Materias
            modelBuilder.Entity<tbGruposMaterias>()
                .HasRequired(a => a.Grupos)
                .WithMany(a => a.GruposMaterias)
                .HasForeignKey(a => a.GrupoId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbGruposMaterias>()
                .HasRequired(a => a.Materias)
                .WithMany(a => a.GruposMaterias)
                .HasForeignKey(a => a.MateriaId)
                .WillCascadeOnDelete(false);
            #endregion

            #region Materias
            modelBuilder.Entity<tbMaterias>()
                .HasRequired(a => a.Docentes)
                .WithMany(a => a.Materias)
                .HasForeignKey(a => a.DocenteId)
                .WillCascadeOnDelete(false);
            #endregion

            #region Actividades
            //modelBuilder.Entity<tbActividades>()
            //    .HasRequired(a => a.TiposActividades)
            //    .WithMany(a => a.Actividades)
            //    .HasForeignKey(a => a.TipoActividadId)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbEntregaActividadAlumno>()
                .HasRequired(a => a.tbActividades)
                .WithMany(a => a.tbEntregaActividadAlumno)
                .HasForeignKey(a => a.ActividadId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbActividades>()
                .HasRequired(a => a.Materias)
                .WithMany(a => a.Actividades)
                .HasForeignKey(a => a.MateriaId)
                .WillCascadeOnDelete(false);
            #endregion

            #region Calificaciones
            //modelBuilder.Entity<tbCalificaciones>()
            //    .HasRequired(a => a.EntregablesAlumno)
            //    .WithMany(a => a.Calificaciones)
            //    .HasForeignKey(a => a.EntregaId)
            //    .WillCascadeOnDelete(false);
            #endregion

            #region Avisos
            modelBuilder.Entity<tbAvisos>()
                .HasRequired(a => a.Docentes)
                .WithMany(a => a.Avisos)
                .HasForeignKey(a => a.DocenteId)
                .WillCascadeOnDelete(true); // equivalente a Cascade
            #endregion

            #region Eventos agenda
            modelBuilder.Entity<tbEventosAgenda>()
                .HasRequired(a => a.Docentes)
                .WithMany(a => a.EventosAgendas)
                .HasForeignKey(a => a.DocenteId)
                .WillCascadeOnDelete(true);
            #endregion

            #region Eventos grupos
            modelBuilder.Entity<tbEventosGrupos>()
                .HasRequired(a => a.Grupos)
                .WithMany(a => a.EventosGrupos)
                .HasForeignKey(a => a.GrupoId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<tbEventosGrupos>()
                .HasRequired(a => a.EventosAgenda)
                .WithMany(a => a.EventosGrupos)
                .HasForeignKey(a => a.FechaId)
                .WillCascadeOnDelete(true);
            #endregion

            #region Eventos materias
            modelBuilder.Entity<tbEventosMaterias>()
                .HasRequired(a => a.Materias)
                .WithMany(a => a.EventosMaterias)
                .HasForeignKey(a => a.MateriaId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<tbEventosMaterias>()
                .HasRequired(a => a.EventosAgenda)
                .WithMany(a => a.EventosMaterias)
                .HasForeignKey(a => a.FechaId)
                .WillCascadeOnDelete(true);
            #endregion

            #region Notificaciones
            modelBuilder.Entity<tbNotificaciones>()
                .HasRequired(a => a.IdentityUser)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .WillCascadeOnDelete(false);
            #endregion



            #region tbEntregaActividadAlumno
            modelBuilder.Entity<tbEntregaActividadAlumno>()
                .HasRequired(a => a.tbAlumnos)
                .WithMany(a => a.tbEntregaActividadAlumno)
                .HasForeignKey(a => a.AlumnoId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbEntregaActividadAlumno>()
                .HasRequired(a => a.cEstadoEntrega)
                .WithMany(a => a.tbEntregaActividadAlumno)
                .HasForeignKey(a => a.EstadoEntregaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbEntregaActividadAlumno>()
                .HasIndex(e => new { e.AlumnoId, e.ActividadId })
                .IsUnique();
            #endregion


            #region tbEntregables
            modelBuilder.Entity<tbEntregables>()
                .HasRequired(a => a.tbEntregaActividadAlumno)
                .WithMany(a => a.tbEntregables)
                .HasForeignKey(a => a.EntregaActividadAlumnoId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbEntregables>()
                .HasRequired(a => a.cTipoEntrega)
                .WithMany(a => a.tbEntregables)
                .HasForeignKey(a => a.TipoEntregaId)
                .WillCascadeOnDelete(false);
            #endregion


            #region TipoNotificacion
            modelBuilder.Entity<tbNotificaciones>()
                .HasRequired(a=> a.cTipoNotificacion)
                .WithMany(a=>a.tbNotificaciones)
                .HasForeignKey(a=>a.TipoId)
                .WillCascadeOnDelete(false);
            #endregion

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}