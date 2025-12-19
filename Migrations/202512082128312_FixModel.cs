namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixModel : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tbAlumnos", "Estatus");
            DropColumn("dbo.tbAlumnosMaterias", "Estatus");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tbAlumnosMaterias", "Estatus", c => c.String());
            AddColumn("dbo.tbAlumnos", "Estatus", c => c.String());
        }
    }
}
