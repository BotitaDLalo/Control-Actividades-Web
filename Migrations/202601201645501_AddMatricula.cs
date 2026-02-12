namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMatricula : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tbEntregaActividadAlumno", "FechaCalificado", c => c.DateTime());
            AddColumn("dbo.tbAlumnos", "Matricula", c => c.String(nullable: false, defaultValue: ""));
            AddColumn("dbo.tbEntregables", "FechaCalificado", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tbEntregables", "FechaCalificado");
            DropColumn("dbo.tbAlumnos", "Matricula");
            DropColumn("dbo.tbEntregaActividadAlumno", "FechaCalificado");
        }
    }
}
