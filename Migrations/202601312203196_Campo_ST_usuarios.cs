namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Campo_ST_usuarios : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.tbEntregableActividadAlumno", newName: "tbEntregaActividadAlumno");
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
                .ForeignKey("dbo.tbEntregableAlumnoes", t => t.EntregaId, cascadeDelete: true)
                .Index(t => t.EntregaId);
            
            CreateTable(
                "dbo.tbEntregableAlumnoes",
                c => new
                    {
                        EntregaId = c.Int(nullable: false, identity: true),
                        AlumnoActividadId = c.Int(nullable: false),
                        Respuesta = c.String(),
                    })
                .PrimaryKey(t => t.EntregaId)
                .ForeignKey("dbo.tbAlumnosActividades", t => t.AlumnoActividadId, cascadeDelete: true)
                .Index(t => t.AlumnoActividadId);
            
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
                .ForeignKey("dbo.tbActividades", t => t.ActividadId, cascadeDelete: true)
                .ForeignKey("dbo.tbAlumnos", t => t.AlumnoId, cascadeDelete: true)
                .Index(t => t.ActividadId)
                .Index(t => t.AlumnoId);
            
            AddColumn("dbo.tbEntregables", "Comentario", c => c.String());
            AddColumn("dbo.tbAlumnos", "ST_UsuarioId", c => c.Int());
            AddColumn("dbo.tbDocentes", "ST_UsuarioId", c => c.Int());
            DropColumn("dbo.tbEntregables", "FechaCalificado");
            DropColumn("dbo.tbEntregaActividadAlumno", "FechaCalificado");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tbEntregaActividadAlumno", "FechaCalificado", c => c.DateTime());
            AddColumn("dbo.tbEntregables", "FechaCalificado", c => c.DateTime());
            DropForeignKey("dbo.tbCalificaciones", "EntregaId", "dbo.tbEntregableAlumnoes");
            DropForeignKey("dbo.tbEntregableAlumnoes", "AlumnoActividadId", "dbo.tbAlumnosActividades");
            DropForeignKey("dbo.tbAlumnosActividades", "AlumnoId", "dbo.tbAlumnos");
            DropForeignKey("dbo.tbAlumnosActividades", "ActividadId", "dbo.tbActividades");
            DropIndex("dbo.tbAlumnosActividades", new[] { "AlumnoId" });
            DropIndex("dbo.tbAlumnosActividades", new[] { "ActividadId" });
            DropIndex("dbo.tbEntregableAlumnoes", new[] { "AlumnoActividadId" });
            DropIndex("dbo.tbCalificaciones", new[] { "EntregaId" });
            DropColumn("dbo.tbDocentes", "ST_UsuarioId");
            DropColumn("dbo.tbAlumnos", "ST_UsuarioId");
            DropColumn("dbo.tbEntregables", "Comentario");
            DropTable("dbo.tbAlumnosActividades");
            DropTable("dbo.tbEntregableAlumnoes");
            DropTable("dbo.tbCalificaciones");
            RenameTable(name: "dbo.tbEntregaActividadAlumno", newName: "tbEntregableActividadAlumno");
        }
    }
}
