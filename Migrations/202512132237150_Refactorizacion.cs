namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Refactorizacion : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tbAlumnosActividades", "ActividadId", "dbo.tbActividades");
            DropForeignKey("dbo.tbAlumnosActividades", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbCalificaciones", "EntregaId", "dbo.tbEntregableAlumnoes");
            DropForeignKey("dbo.tbEntregableAlumnoes", "EntregaId", "dbo.tbAlumnosActividades");
            DropForeignKey("dbo.tbActividades", "TipoActividadId", "dbo.cTiposActividades");
            DropIndex("dbo.tbActividades", new[] { "TipoActividadId" });
            DropIndex("dbo.tbAlumnosActividades", new[] { "ActividadId" });
            DropIndex("dbo.tbAlumnosActividades", new[] { "AlumnoId" });
            DropIndex("dbo.tbEntregableAlumnoes", new[] { "EntregaId" });
            DropIndex("dbo.tbCalificaciones", new[] { "EntregaId" });
            CreateTable(
                "dbo.cTipoEntregas",
                c => new
                    {
                        TipoActividadId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TipoActividadId);
            
            CreateTable(
                "dbo.tbEntregables",
                c => new
                    {
                        EntregableId = c.Int(nullable: false, identity: true),
                        EntregaActividadAlumnoId = c.Int(nullable: false),
                        TipoEntregaId = c.Int(nullable: false),
                        Contenido = c.String(),
                        Calificacion = c.Int(),
                    })
                .PrimaryKey(t => t.EntregableId)
                .ForeignKey("dbo.cTipoEntregas", t => t.TipoEntregaId)
                .ForeignKey("dbo.tbEntregaActividadAlumnoes", t => t.EntregaActividadAlumnoId)
                .Index(t => t.EntregaActividadAlumnoId)
                .Index(t => t.TipoEntregaId);
            
            CreateTable(
                "dbo.tbEntregaActividadAlumnoes",
                c => new
                    {
                        EntregaActividadAlumnoId = c.Int(nullable: false, identity: true),
                        ActividadId = c.Int(nullable: false),
                        AlumnoId = c.Int(nullable: false),
                        FechaEntrega = c.DateTime(nullable: false),
                        EstadoEntregaId = c.Int(nullable: false),
                        FecheCalificado = c.DateTime(),
                    })
                .PrimaryKey(t => t.EntregaActividadAlumnoId)
                .ForeignKey("dbo.cEstadoEntregas", t => t.EstadoEntregaId)
                .ForeignKey("dbo.tbActividades", t => t.ActividadId)
                .ForeignKey("dbo.tbAlumnos", t => t.AlumnoId)
                .Index(t => new { t.AlumnoId, t.ActividadId }, unique: true)
                .Index(t => t.EstadoEntregaId);
            
            CreateTable(
                "dbo.cEstadoEntregas",
                c => new
                    {
                        EstadoEntregaId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.EstadoEntregaId);
            
            DropColumn("dbo.tbActividades", "TipoActividadId");
            DropTable("dbo.cTiposActividades");
            DropTable("dbo.tbAlumnosActividades");
            DropTable("dbo.tbEntregableAlumnoes");
            DropTable("dbo.tbCalificaciones");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.CalificacionId);
            
            CreateTable(
                "dbo.tbEntregableAlumnoes",
                c => new
                    {
                        EntregaId = c.Int(nullable: false, identity: true),
                        AlumnoActividadId = c.Int(nullable: false),
                        Respuesta = c.String(),
                    })
                .PrimaryKey(t => t.EntregaId);
            
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
                .PrimaryKey(t => t.AlumnoActividadId);
            
            CreateTable(
                "dbo.cTiposActividades",
                c => new
                    {
                        TipoActividadId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.TipoActividadId);
            
            AddColumn("dbo.tbActividades", "TipoActividadId", c => c.Int(nullable: false));
            DropForeignKey("dbo.tbEntregables", "EntregaActividadAlumnoId", "dbo.tbEntregaActividadAlumnoes");
            DropForeignKey("dbo.tbEntregaActividadAlumnoes", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbEntregaActividadAlumnoes", "ActividadId", "dbo.tbActividades");
            DropForeignKey("dbo.tbEntregaActividadAlumnoes", "EstadoEntregaId", "dbo.cEstadoEntregas");
            DropForeignKey("dbo.tbEntregables", "TipoEntregaId", "dbo.cTipoEntregas");
            DropIndex("dbo.tbEntregaActividadAlumnoes", new[] { "EstadoEntregaId" });
            DropIndex("dbo.tbEntregaActividadAlumnoes", new[] { "AlumnoId", "ActividadId" });
            DropIndex("dbo.tbEntregables", new[] { "TipoEntregaId" });
            DropIndex("dbo.tbEntregables", new[] { "EntregaActividadAlumnoId" });
            DropTable("dbo.cEstadoEntregas");
            DropTable("dbo.tbEntregaActividadAlumnoes");
            DropTable("dbo.tbEntregables");
            DropTable("dbo.cTipoEntregas");
            CreateIndex("dbo.tbCalificaciones", "EntregaId");
            CreateIndex("dbo.tbEntregableAlumnoes", "EntregaId");
            CreateIndex("dbo.tbAlumnosActividades", "AlumnoId");
            CreateIndex("dbo.tbAlumnosActividades", "ActividadId");
            CreateIndex("dbo.tbActividades", "TipoActividadId");
            AddForeignKey("dbo.tbActividades", "TipoActividadId", "dbo.cTiposActividades", "TipoActividadId");
            AddForeignKey("dbo.tbEntregableAlumnoes", "EntregaId", "dbo.tbAlumnosActividades", "AlumnoActividadId");
            AddForeignKey("dbo.tbCalificaciones", "EntregaId", "dbo.tbEntregableAlumnoes", "EntregaId");
            AddForeignKey("dbo.tbAlumnosActividades", "AlumnoId", "dbo.tbAlumnos", "AlumnoId");
            AddForeignKey("dbo.tbAlumnosActividades", "ActividadId", "dbo.tbActividades", "ActividadId");
        }
    }
}
