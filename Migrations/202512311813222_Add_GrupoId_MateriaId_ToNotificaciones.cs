namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_GrupoId_MateriaId_ToNotificaciones : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tbNotificaciones", "MateriaId", c => c.Int());
            AddColumn("dbo.tbNotificaciones", "GrupoId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tbNotificaciones", "GrupoId");
            DropColumn("dbo.tbNotificaciones", "MateriaId");
        }
    }
}
