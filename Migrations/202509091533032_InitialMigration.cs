namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.cTiposActividades",
                c => new
                    {
                        TipoActividadId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TipoActividadId);
            
            CreateTable(
                "dbo.tbActividades",
                c => new
                    {
                        ActividadId = c.Int(nullable: false, identity: true),
                        NombreActividad = c.String(nullable: false),
                        Descripcion = c.String(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                        FechaLimite = c.DateTime(nullable: false),
                        TipoActividadId = c.Int(nullable: false),
                        Puntaje = c.Int(nullable: false),
                        MateriaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ActividadId)
                .ForeignKey("dbo.tbMaterias", t => t.MateriaId)
                .ForeignKey("dbo.cTiposActividades", t => t.TipoActividadId)
                .Index(t => t.TipoActividadId)
                .Index(t => t.MateriaId);
            
            CreateTable(
                "dbo.tbAlumnosActividades",
                c => new
                    {
                        AlumnoActividadId = c.Int(nullable: false, identity: true),
                        ActividadId = c.Int(nullable: false),
                        AlumnoId = c.Int(nullable: false),
                        FechaEntrega = c.DateTime(nullable: false),
                        EstatusEntrega = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.AlumnoActividadId)
                .ForeignKey("dbo.tbActividades", t => t.ActividadId)
                .ForeignKey("dbo.tbAlumnos", t => t.AlumnoId)
                .Index(t => t.ActividadId)
                .Index(t => t.AlumnoId);
            
            CreateTable(
                "dbo.tbAlumnos",
                c => new
                    {
                        AlumnoId = c.Int(nullable: false, identity: true),
                        ApellidoPaterno = c.String(nullable: false),
                        ApellidoMaterno = c.String(nullable: false),
                        Nombre = c.String(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
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
                "dbo.tbEntregableAlumnoes",
                c => new
                    {
                        EntregaId = c.Int(nullable: false, identity: true),
                        AlumnoActividadId = c.Int(nullable: false),
                        Respuesta = c.String(),
                    })
                .PrimaryKey(t => t.EntregaId)
                .ForeignKey("dbo.tbAlumnosActividades", t => t.EntregaId)
                .Index(t => t.EntregaId);
            
            CreateTable(
                "dbo.tbCalificaciones",
                c => new
                    {
                        CalificacionId = c.Int(nullable: false, identity: true),
                        EntregaId = c.Int(nullable: false),
                        FechaCalificacionAsignada = c.DateTime(nullable: false),
                        Comentarios = c.String(),
                        Calificacion = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CalificacionId)
                .ForeignKey("dbo.tbEntregableAlumnoes", t => t.EntregaId)
                .Index(t => t.EntregaId);
            
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
                "dbo.tbNotificaciones",
                c => new
                    {
                        NotificacionId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        MessageId = c.String(),
                        Title = c.String(),
                        Body = c.String(),
                        FechaRecibido = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.NotificacionId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.tbUsuariosFcmTokens",
                c => new
                    {
                        TokenId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Token = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TokenId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tbUsuariosFcmTokens", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tbNotificaciones", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.tbActividades", "TipoActividadId", "dbo.cTiposActividades");
            DropForeignKey("dbo.tbActividades", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbEntregableAlumnoes", "EntregaId", "dbo.tbAlumnosActividades");
            DropForeignKey("dbo.tbCalificaciones", "EntregaId", "dbo.tbEntregableAlumnoes");
            DropForeignKey("dbo.tbAlumnosActividades", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbAlumnos", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tbAlumnosGrupos", "GrupoId", "dbo.tbGrupos");
            DropForeignKey("dbo.tbGrupos", "DocenteId", "dbo.tbDocentes");
            DropForeignKey("dbo.tbDocentes", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.tbEventosMaterias", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbGruposMaterias", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbGruposMaterias", "GrupoId", "dbo.tbGrupos");
            DropForeignKey("dbo.tbMaterias", "DocenteId", "dbo.tbDocentes");
            DropForeignKey("dbo.tbAlumnosMaterias", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbAlumnosMaterias", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbEventosMaterias", "FechaId", "dbo.tbEventosAgendas");
            DropForeignKey("dbo.tbEventosGrupos", "GrupoId", "dbo.tbGrupos");
            DropForeignKey("dbo.tbEventosGrupos", "FechaId", "dbo.tbEventosAgendas");
            DropForeignKey("dbo.tbEventosAgendas", "DocenteId", "dbo.tbDocentes");
            DropForeignKey("dbo.tbAvisos", "DocenteId", "dbo.tbDocentes");
            DropForeignKey("dbo.tbAlumnosGrupos", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbAlumnosActividades", "ActividadId", "dbo.tbActividades");
            DropIndex("dbo.tbUsuariosFcmTokens", new[] { "UserId" });
            DropIndex("dbo.tbNotificaciones", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.tbCalificaciones", new[] { "EntregaId" });
            DropIndex("dbo.tbEntregableAlumnoes", new[] { "EntregaId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.tbGruposMaterias", new[] { "MateriaId" });
            DropIndex("dbo.tbGruposMaterias", new[] { "GrupoId" });
            DropIndex("dbo.tbAlumnosMaterias", new[] { "MateriaId" });
            DropIndex("dbo.tbAlumnosMaterias", new[] { "AlumnoId" });
            DropIndex("dbo.tbMaterias", new[] { "DocenteId" });
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
            DropIndex("dbo.tbAlumnosActividades", new[] { "AlumnoId" });
            DropIndex("dbo.tbAlumnosActividades", new[] { "ActividadId" });
            DropIndex("dbo.tbActividades", new[] { "MateriaId" });
            DropIndex("dbo.tbActividades", new[] { "TipoActividadId" });
            DropTable("dbo.tbUsuariosFcmTokens");
            DropTable("dbo.tbNotificaciones");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.tbCalificaciones");
            DropTable("dbo.tbEntregableAlumnoes");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.tbGruposMaterias");
            DropTable("dbo.tbAlumnosMaterias");
            DropTable("dbo.tbMaterias");
            DropTable("dbo.tbEventosMaterias");
            DropTable("dbo.tbEventosGrupos");
            DropTable("dbo.tbEventosAgendas");
            DropTable("dbo.tbAvisos");
            DropTable("dbo.tbDocentes");
            DropTable("dbo.tbGrupos");
            DropTable("dbo.tbAlumnosGrupos");
            DropTable("dbo.tbAlumnos");
            DropTable("dbo.tbAlumnosActividades");
            DropTable("dbo.tbActividades");
            DropTable("dbo.cTiposActividades");
        }
    }
}
