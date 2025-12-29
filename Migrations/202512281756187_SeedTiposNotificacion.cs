namespace ControlActividades.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedTiposNotificacion : DbMigration
    {
        public override void Up()
        {
            Sql(@"
                IF NOT EXISTS (SELECT 1 FROM cTipoNotificacion WHERE Nombre = 'ActividadCalificada')
                INSERT INTO cTipoNotificacion (Nombre) VALUES ('ActividadCalificada');

                IF NOT EXISTS (SELECT 1 FROM cTipoNotificacion WHERE Nombre = 'ActividadCreada')
                INSERT INTO cTipoNotificacion (Nombre) VALUES ('ActividadCreada');

                IF NOT EXISTS (SELECT 1 FROM cTipoNotificacion WHERE Nombre = 'ActividadEntregada')
                INSERT INTO cTipoNotificacion (Nombre) VALUES ('ActividadEntregada');

                IF NOT EXISTS (SELECT 1 FROM cTipoNotificacion WHERE Nombre = 'Aviso')
                INSERT INTO cTipoNotificacion (Nombre) VALUES ('Aviso');

                IF NOT EXISTS (SELECT 1 FROM cTipoNotificacion WHERE Nombre = 'Evento')
                INSERT INTO cTipoNotificacion (Nombre) VALUES ('Evento');

                IF NOT EXISTS (SELECT 1 FROM cTipoNotificacion WHERE Nombre = 'GrupoAsignado')
                INSERT INTO cTipoNotificacion (Nombre) VALUES ('GrupoAsignado');

                IF NOT EXISTS (SELECT 1 FROM cTipoNotificacion WHERE Nombre = 'MateriaAsignada')
                INSERT INTO cTipoNotificacion (Nombre) VALUES ('MateriaAsignada');
            ");
        }
        
        public override void Down()
        {
        }
    }
}
