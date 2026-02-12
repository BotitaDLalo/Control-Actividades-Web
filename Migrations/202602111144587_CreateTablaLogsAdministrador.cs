namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTablaLogsAdministrador : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tbAuditoria",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    AdminId = c.String(),
                    AdminEmail = c.String(),
                    UsuarioImpersonadoId = c.String(),
                    UsuarioImpersonadoEmail = c.String(),
                    Accion = c.String(),
                    Controlador = c.String(),
                    Descripcion = c.String(),
                    DateUtc = c.DateTime(nullable: false),
                    DireccionIp = c.String(),
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.tbAuditoria");
        }
    }
}
