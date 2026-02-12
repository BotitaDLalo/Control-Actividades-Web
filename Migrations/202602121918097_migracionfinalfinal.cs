namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migracionfinalfinal : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.tbEntregaActividadAlumno", "Estatus", c => c.Boolean(nullable: false));
            //AddColumn("dbo.tbEntregaActividadAlumno", "Calificacion", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            //AddColumn("dbo.tbEntregaActividadAlumno", "EntregaTardia", c => c.Boolean(nullable: false));
            //AddColumn("dbo.tbActividades", "PermitirEntregasTarde", c => c.Boolean(nullable: false));
            //AddColumn("dbo.tbActividades", "LimiteEntregasPorAlumno", c => c.Int(nullable: false));
            //AddColumn("dbo.tbAvisos", "FechaInicio", c => c.DateTime(nullable: false));
            //AddColumn("dbo.tbAvisos", "FechaFin", c => c.DateTime(nullable: false));
            //AddColumn("dbo.tbAvisos", "Enlaces", c => c.String());
            //AddColumn("dbo.tbAvisos", "FrecuenciaDias", c => c.Int(nullable: false));
            AddColumn("dbo.tbAlumnos", "ST_UsuarioId", c => c.Int(nullable: true));
            AddColumn("dbo.tbDocentes", "ST_UsuarioId", c => c.Int(nullable: true));
            //AlterColumn("dbo.tbActividades", "Puntaje", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            //AlterColumn("dbo.tbActividades", "Puntaje", c => c.Int(nullable: false));
            //DropColumn("dbo.tbAvisos", "FrecuenciaDias");
            //DropColumn("dbo.tbAvisos", "Enlaces");
            //DropColumn("dbo.tbAvisos", "FechaFin");
            //DropColumn("dbo.tbAvisos", "FechaInicio");
            DropColumn("dbo.tbAlumnos", "ST_UsuarioId");
            DropColumn("dbo.tbDocentes", "ST_UsuarioId");
            //DropColumn("dbo.tbActividades", "LimiteEntregasPorAlumno");
            //DropColumn("dbo.tbActividades", "PermitirEntregasTarde");
            //DropColumn("dbo.tbEntregaActividadAlumno", "EntregaTardia");
            //DropColumn("dbo.tbEntregaActividadAlumno", "Calificacion");
            //DropColumn("dbo.tbEntregaActividadAlumno", "FechaCalificado");
            //DropColumn("dbo.tbEntregaActividadAlumno", "Estatus");
            //RenameTable(name: "dbo.tbAuditoria", newName: "tbAuditorias");
        }
    }
}
