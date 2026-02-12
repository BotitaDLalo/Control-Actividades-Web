namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModificacionActividades : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.cEstadoEntregas", newName: "cEstadoEntregas1");
            CreateTable(
                "dbo.cEstadoEntregas",
                c => new
                    {
                        EstadoEntregaId = c.Int(nullable: false),
                        Nombre = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.EstadoEntregaId);
            
            AddColumn("dbo.tbEntregables", "Comentario", c => c.String());
            AddColumn("dbo.tbEntregableActividadAlumno", "Estatus", c => c.Boolean(nullable: false));
            AddColumn("dbo.tbEntregableActividadAlumno", "Calificacion", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.tbEntregableActividadAlumno", "EntregaTardia", c => c.Boolean(nullable: false));
            AddColumn("dbo.tbActividades", "PermitirEntregasTarde", c => c.Boolean(nullable: false));
            AddColumn("dbo.tbActividades", "LimiteEntregasPorAlumno", c => c.Int(nullable: false));
            AddColumn("dbo.tbAlumnos", "Matricula", c => c.String());
            AddColumn("dbo.tbAlumnos", "ST_UsuarioId", c => c.Int());
            AddColumn("dbo.tbDocentes", "ST_UsuarioId", c => c.Int());
            AddColumn("dbo.tbAvisos", "FechaInicio", c => c.DateTime(nullable: false));
            AddColumn("dbo.tbAvisos", "FechaFin", c => c.DateTime(nullable: false));
            AddColumn("dbo.tbAvisos", "Enlaces", c => c.String());
            AddColumn("dbo.tbAvisos", "FrecuenciaDias", c => c.Int(nullable: false));
            AlterColumn("dbo.tbActividades", "Puntaje", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.tbUsuariosFcmTokens", "Token", c => c.String());
            DropColumn("dbo.tbEntregables", "FechaCalificado");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tbEntregables", "FechaCalificado", c => c.DateTime());
            DropForeignKey("dbo.tbEntregableActividadAlumno", "EstadoEntregaId", "dbo.cEstadoEntregas");
            AlterColumn("dbo.tbUsuariosFcmTokens", "Token", c => c.String(nullable: false));
            AlterColumn("dbo.tbActividades", "Puntaje", c => c.Int(nullable: false));
            DropColumn("dbo.tbAvisos", "FrecuenciaDias");
            DropColumn("dbo.tbAvisos", "Enlaces");
            DropColumn("dbo.tbAvisos", "FechaFin");
            DropColumn("dbo.tbAvisos", "FechaInicio");
            DropColumn("dbo.tbDocentes", "ST_UsuarioId");
            DropColumn("dbo.tbAlumnos", "ST_UsuarioId");
            DropColumn("dbo.tbAlumnos", "Matricula");
            DropColumn("dbo.tbActividades", "LimiteEntregasPorAlumno");
            DropColumn("dbo.tbActividades", "PermitirEntregasTarde");
            DropColumn("dbo.tbEntregableActividadAlumno", "EntregaTardia");
            DropColumn("dbo.tbEntregableActividadAlumno", "Calificacion");
            DropColumn("dbo.tbEntregableActividadAlumno", "Estatus");
            DropColumn("dbo.tbEntregables", "Comentario");
            DropTable("dbo.cEstadoEntregas");
            RenameTable(name: "dbo.cEstadoEntregas1", newName: "cEstadoEntregas");
        }
    }
}
