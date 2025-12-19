namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTipoToNotificaciones : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tbNotificaciones", "Tipo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tbNotificaciones", "Tipo");
        }
    }
}
