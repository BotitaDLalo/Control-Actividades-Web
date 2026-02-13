namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.cEstadoEntregas",
                c => new
                    {
                        EstadoEntregaId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.EstadoEntregaId);
            
            CreateTable(
                "dbo.tbEntregableActividadAlumno",
                c => new
                    {
                        EntregaActividadAlumnoId = c.Int(nullable: false, identity: true),
                        ActividadId = c.Int(nullable: false),
                        AlumnoId = c.Int(nullable: false),
                        FechaEntrega = c.DateTime(nullable: false),
                        Estatus = c.Boolean(nullable: false),
                        EstadoEntregaId = c.Int(nullable: false),
                        FechaCalificado = c.DateTime(),
                        Calificacion = c.Decimal(nullable: false, precision: 18, scale: 2),
                        EntregaTardia = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EntregaActividadAlumnoId)
                .ForeignKey("dbo.cEstadoEntregas", t => t.EstadoEntregaId)
                .ForeignKey("dbo.tbActividades", t => t.ActividadId)
                .ForeignKey("dbo.tbAlumnos", t => t.AlumnoId)
                .Index(t => new { t.AlumnoId, t.ActividadId }, unique: true)
                .Index(t => t.EstadoEntregaId);
            
            CreateTable(
                "dbo.tbActividades",
                c => new
                    {
                        ActividadId = c.Int(nullable: false, identity: true),
                        NombreActividad = c.String(nullable: false),
                        Descripcion = c.String(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                        FechaLimite = c.DateTime(nullable: false),
                        Puntaje = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MateriaId = c.Int(nullable: false),
                        PermitirEntregasTarde = c.Boolean(nullable: false),
                        Enviado = c.Boolean(),
                        FechaProgramada = c.DateTime(),
                        LimiteEntregasPorAlumno = c.Int(nullable: false),
                        TieneLimiteEntregas = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ActividadId)
                .ForeignKey("dbo.tbMaterias", t => t.MateriaId)
                .Index(t => t.MateriaId);
            
            CreateTable(
                "dbo.tbMaterias",
                c => new
                    {
                        MateriaId = c.Int(nullable: false, identity: true),
                        NombreMateria = c.String(nullable: false),
                        Descripcion = c.String(),
                        CodigoColor = c.String(),
                        CodigoAcceso = c.String(),
                        DocenteId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MateriaId)
                .ForeignKey("dbo.tbDocentes", t => t.DocenteId)
                .Index(t => t.DocenteId);
            
            CreateTable(
                "dbo.tbAlumnosMaterias",
                c => new
                    {
                        AlumnoMateriaId = c.Int(nullable: false, identity: true),
                        AlumnoId = c.Int(nullable: false),
                        MateriaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AlumnoMateriaId)
                .ForeignKey("dbo.tbAlumnos", t => t.AlumnoId)
                .ForeignKey("dbo.tbMaterias", t => t.MateriaId)
                .Index(t => t.AlumnoId)
                .Index(t => t.MateriaId);
            
            CreateTable(
                "dbo.tbAlumnos",
                c => new
                    {
                        AlumnoId = c.Int(nullable: false, identity: true),
                        Matricula = c.String(),
                        ApellidoPaterno = c.String(nullable: false),
                        ApellidoMaterno = c.String(nullable: false),
                        Nombre = c.String(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ST_UsuarioId = c.Int(),
                    })
                .PrimaryKey(t => t.AlumnoId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.tbAlumnosGrupos",
                c => new
                    {
                        AlumnoGrupoId = c.Int(nullable: false, identity: true),
                        AlumnoId = c.Int(nullable: false),
                        GrupoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AlumnoGrupoId)
                .ForeignKey("dbo.tbAlumnos", t => t.AlumnoId)
                .ForeignKey("dbo.tbGrupos", t => t.GrupoId)
                .Index(t => t.AlumnoId)
                .Index(t => t.GrupoId);
            
            CreateTable(
                "dbo.tbGrupos",
                c => new
                    {
                        GrupoId = c.Int(nullable: false, identity: true),
                        NombreGrupo = c.String(nullable: false),
                        Descripcion = c.String(),
                        CodigoAcceso = c.String(),
                        CodigoColor = c.String(),
                        DocenteId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GrupoId)
                .ForeignKey("dbo.tbDocentes", t => t.DocenteId)
                .Index(t => t.DocenteId);
            
            CreateTable(
                "dbo.tbDocentes",
                c => new
                    {
                        DocenteId = c.Int(nullable: false, identity: true),
                        ApellidoPaterno = c.String(nullable: false),
                        ApellidoMaterno = c.String(nullable: false),
                        Nombre = c.String(nullable: false),
                        estaAutorizado = c.Boolean(),
                        seEnvioCorreo = c.Boolean(nullable: false),
                        FechaExpiracionCodigo = c.DateTime(),
                        CodigoAutorizacion = c.String(),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ST_UsuarioId = c.Int(),
                    })
                .PrimaryKey(t => t.DocenteId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.tbAvisos",
                c => new
                    {
                        AvisoId = c.Int(nullable: false, identity: true),
                        DocenteId = c.Int(nullable: false),
                        Titulo = c.String(nullable: false),
                        Descripcion = c.String(nullable: false),
                        GrupoId = c.Int(),
                        MateriaId = c.Int(),
                        FechaCreacion = c.DateTime(nullable: false),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFin = c.DateTime(nullable: false),
                        Enlaces = c.String(),
                        FrecuenciaDias = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AvisoId)
                .ForeignKey("dbo.tbDocentes", t => t.DocenteId, cascadeDelete: true)
                .Index(t => t.DocenteId);
            
            CreateTable(
                "dbo.tbEventosAgendas",
                c => new
                    {
                        EventoId = c.Int(nullable: false, identity: true),
                        DocenteId = c.Int(nullable: false),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFinal = c.DateTime(nullable: false),
                        Titulo = c.String(nullable: false),
                        Descripcion = c.String(nullable: false),
                        Color = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.EventoId)
                .ForeignKey("dbo.tbDocentes", t => t.DocenteId, cascadeDelete: true)
                .Index(t => t.DocenteId);
            
            CreateTable(
                "dbo.tbEventosGrupos",
                c => new
                    {
                        EventoGrupoId = c.Int(nullable: false, identity: true),
                        FechaId = c.Int(nullable: false),
                        GrupoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventoGrupoId)
                .ForeignKey("dbo.tbEventosAgendas", t => t.FechaId, cascadeDelete: true)
                .ForeignKey("dbo.tbGrupos", t => t.GrupoId, cascadeDelete: true)
                .Index(t => t.FechaId)
                .Index(t => t.GrupoId);
            
            CreateTable(
                "dbo.tbEventosMaterias",
                c => new
                    {
                        EventoMateriaId = c.Int(nullable: false, identity: true),
                        FechaId = c.Int(nullable: false),
                        MateriaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventoMateriaId)
                .ForeignKey("dbo.tbEventosAgendas", t => t.FechaId, cascadeDelete: true)
                .ForeignKey("dbo.tbMaterias", t => t.MateriaId, cascadeDelete: true)
                .Index(t => t.FechaId)
                .Index(t => t.MateriaId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.tbGruposMaterias",
                c => new
                    {
                        GrupoMateriasId = c.Int(nullable: false, identity: true),
                        GrupoId = c.Int(nullable: false),
                        MateriaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GrupoMateriasId)
                .ForeignKey("dbo.tbGrupos", t => t.GrupoId)
                .ForeignKey("dbo.tbMaterias", t => t.MateriaId)
                .Index(t => t.GrupoId)
                .Index(t => t.MateriaId);
            
            CreateTable(
                "dbo.tbEntregables",
                c => new
                    {
                        EntregableId = c.Int(nullable: false, identity: true),
                        EntregaActividadAlumnoId = c.Int(nullable: false),
                        TipoEntregaId = c.Int(nullable: false),
                        Contenido = c.String(),
                        Calificacion = c.Int(),
                        Comentario = c.String(),
                    })
                .PrimaryKey(t => t.EntregableId)
                .ForeignKey("dbo.cTipoEntregas", t => t.TipoEntregaId)
                .ForeignKey("dbo.tbEntregableActividadAlumno", t => t.EntregaActividadAlumnoId)
                .Index(t => t.EntregaActividadAlumnoId)
                .Index(t => t.TipoEntregaId);
            
            CreateTable(
                "dbo.cTipoEntregas",
                c => new
                    {
                        TipoActividadId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TipoActividadId);
            
            CreateTable(
                "dbo.cTipoNotificacion",
                c => new
                    {
                        TipoNotificacionId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TipoNotificacionId);
            
            CreateTable(
                "dbo.tbNotificaciones",
                c => new
                    {
                        NotificacionId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        MessageId = c.String(),
                        Title = c.String(),
                        Body = c.String(),
                        FechaRecibido = c.DateTime(nullable: false),
                        MateriaId = c.Int(),
                        GrupoId = c.Int(),
                        TipoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.NotificacionId)
                .ForeignKey("dbo.cTipoNotificacion", t => t.TipoId)
                .ForeignKey("dbo.tbGrupos", t => t.GrupoId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.tbMaterias", t => t.MateriaId)
                .Index(t => t.UserId)
                .Index(t => t.MateriaId)
                .Index(t => t.GrupoId)
                .Index(t => t.TipoId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.tbAuditoria",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdminId = c.String(),
                        AdminEmail = c.String(),
                        UsuarioImpersonadoId = c.String(),
                        UsuarioImpersonadoEmail = c.String(),
                        Accion = c.String(),
                        Controlador = c.String(),
                        Descripcion = c.String(),
                        DateUtc = c.DateTime(nullable: false),
                        DireccionIp = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.tbUsuariosFcmTokens",
                c => new
                    {
                        TokenId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Token = c.String(),
                    })
                .PrimaryKey(t => t.TokenId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tbUsuariosFcmTokens", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.tbNotificaciones", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbNotificaciones", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tbNotificaciones", "GrupoId", "dbo.tbGrupos");
            DropForeignKey("dbo.tbNotificaciones", "TipoId", "dbo.cTipoNotificacion");
            DropForeignKey("dbo.tbEntregables", "EntregaActividadAlumnoId", "dbo.tbEntregableActividadAlumno");
            DropForeignKey("dbo.tbEntregables", "TipoEntregaId", "dbo.cTipoEntregas");
            DropForeignKey("dbo.tbEntregableActividadAlumno", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbEntregableActividadAlumno", "ActividadId", "dbo.tbActividades");
            DropForeignKey("dbo.tbActividades", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbMaterias", "DocenteId", "dbo.tbDocentes");
            DropForeignKey("dbo.tbAlumnosMaterias", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbAlumnosMaterias", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbAlumnos", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tbAlumnosGrupos", "GrupoId", "dbo.tbGrupos");
            DropForeignKey("dbo.tbGruposMaterias", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbGruposMaterias", "GrupoId", "dbo.tbGrupos");
            DropForeignKey("dbo.tbGrupos", "DocenteId", "dbo.tbDocentes");
            DropForeignKey("dbo.tbDocentes", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tbEventosMaterias", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbEventosMaterias", "FechaId", "dbo.tbEventosAgendas");
            DropForeignKey("dbo.tbEventosGrupos", "GrupoId", "dbo.tbGrupos");
            DropForeignKey("dbo.tbEventosGrupos", "FechaId", "dbo.tbEventosAgendas");
            DropForeignKey("dbo.tbEventosAgendas", "DocenteId", "dbo.tbDocentes");
            DropForeignKey("dbo.tbAvisos", "DocenteId", "dbo.tbDocentes");
            DropForeignKey("dbo.tbAlumnosGrupos", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbEntregableActividadAlumno", "EstadoEntregaId", "dbo.cEstadoEntregas");
            DropIndex("dbo.tbUsuariosFcmTokens", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.tbNotificaciones", new[] { "TipoId" });
            DropIndex("dbo.tbNotificaciones", new[] { "GrupoId" });
            DropIndex("dbo.tbNotificaciones", new[] { "MateriaId" });
            DropIndex("dbo.tbNotificaciones", new[] { "UserId" });
            DropIndex("dbo.tbEntregables", new[] { "TipoEntregaId" });
            DropIndex("dbo.tbEntregables", new[] { "EntregaActividadAlumnoId" });
            DropIndex("dbo.tbGruposMaterias", new[] { "MateriaId" });
            DropIndex("dbo.tbGruposMaterias", new[] { "GrupoId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.tbEventosMaterias", new[] { "MateriaId" });
            DropIndex("dbo.tbEventosMaterias", new[] { "FechaId" });
            DropIndex("dbo.tbEventosGrupos", new[] { "GrupoId" });
            DropIndex("dbo.tbEventosGrupos", new[] { "FechaId" });
            DropIndex("dbo.tbEventosAgendas", new[] { "DocenteId" });
            DropIndex("dbo.tbAvisos", new[] { "DocenteId" });
            DropIndex("dbo.tbDocentes", new[] { "UserId" });
            DropIndex("dbo.tbGrupos", new[] { "DocenteId" });
            DropIndex("dbo.tbAlumnosGrupos", new[] { "GrupoId" });
            DropIndex("dbo.tbAlumnosGrupos", new[] { "AlumnoId" });
            DropIndex("dbo.tbAlumnos", new[] { "UserId" });
            DropIndex("dbo.tbAlumnosMaterias", new[] { "MateriaId" });
            DropIndex("dbo.tbAlumnosMaterias", new[] { "AlumnoId" });
            DropIndex("dbo.tbMaterias", new[] { "DocenteId" });
            DropIndex("dbo.tbActividades", new[] { "MateriaId" });
            DropIndex("dbo.tbEntregableActividadAlumno", new[] { "EstadoEntregaId" });
            DropIndex("dbo.tbEntregableActividadAlumno", new[] { "AlumnoId", "ActividadId" });
            DropTable("dbo.tbUsuariosFcmTokens");
            DropTable("dbo.tbAuditoria");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.tbNotificaciones");
            DropTable("dbo.cTipoNotificacion");
            DropTable("dbo.cTipoEntregas");
            DropTable("dbo.tbEntregables");
            DropTable("dbo.tbGruposMaterias");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.tbEventosMaterias");
            DropTable("dbo.tbEventosGrupos");
            DropTable("dbo.tbEventosAgendas");
            DropTable("dbo.tbAvisos");
            DropTable("dbo.tbDocentes");
            DropTable("dbo.tbGrupos");
            DropTable("dbo.tbAlumnosGrupos");
            DropTable("dbo.tbAlumnos");
            DropTable("dbo.tbAlumnosMaterias");
            DropTable("dbo.tbMaterias");
            DropTable("dbo.tbActividades");
            DropTable("dbo.tbEntregableActividadAlumno");
            DropTable("dbo.cEstadoEntregas");
        }
    }
}
