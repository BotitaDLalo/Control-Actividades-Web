namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Refactorizacion_Dos : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.tbEntregaActividadAlumnoes", newName: "tbEntregableActividadAlumno");
            AddColumn("dbo.tbEntregables", "FechaCalificado", c => c.DateTime());
            AddColumn("dbo.tbEntregableActividadAlumno", "FechaCalificado", c => c.DateTime());
            AddColumn("dbo.tbActividades", "Enviado", c => c.Boolean());
            AddColumn("dbo.tbActividades", "FechaProgramada", c => c.DateTime());
            DropColumn("dbo.tbEntregableActividadAlumno", "FecheCalificado");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tbEntregableActividadAlumno", "FecheCalificado", c => c.DateTime());
            DropColumn("dbo.tbActividades", "FechaProgramada");
            DropColumn("dbo.tbActividades", "Enviado");
            DropColumn("dbo.tbEntregableActividadAlumno", "FechaCalificado");
            DropColumn("dbo.tbEntregables", "FechaCalificado");
            RenameTable(name: "dbo.tbEntregableActividadAlumno", newName: "tbEntregaActividadAlumnoes");
        }
    }
}
