namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Modifica_Nombre_cTipoNotificacion : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.cTipoNotificacions", newName: "cTipoNotificacion");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.cTipoNotificacion", newName: "cTipoNotificacions");
        }
    }
}
