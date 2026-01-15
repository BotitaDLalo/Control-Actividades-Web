namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFksGrupoyMateria_ToNotificaciones : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.tbNotificaciones", "MateriaId");
            CreateIndex("dbo.tbNotificaciones", "GrupoId");
            AddForeignKey("dbo.tbNotificaciones", "GrupoId", "dbo.tbGrupos", "GrupoId");
            AddForeignKey("dbo.tbNotificaciones", "MateriaId", "dbo.tbMaterias", "MateriaId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tbNotificaciones", "MateriaId", "dbo.tbMaterias");
            DropForeignKey("dbo.tbNotificaciones", "GrupoId", "dbo.tbGrupos");
            DropIndex("dbo.tbNotificaciones", new[] { "GrupoId" });
            DropIndex("dbo.tbNotificaciones", new[] { "MateriaId" });
        }
    }
}
