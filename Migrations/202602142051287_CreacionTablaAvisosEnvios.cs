namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreacionTablaAvisosEnvios : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tbAvisosEnvios",
                c => new
                    {
                        AvisoEnvioId = c.Int(nullable: false, identity: true),
                        AvisoId = c.Int(nullable: false),
                        FechaEnvio = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AvisoEnvioId)
                .ForeignKey("dbo.tbAvisos", t => t.AvisoId, cascadeDelete: true)
                .Index(t => t.AvisoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tbAvisosEnvios", "AvisoId", "dbo.tbAvisos");
            DropIndex("dbo.tbAvisosEnvios", new[] { "AvisoId" });
            DropTable("dbo.tbAvisosEnvios");
        }
    }
}
