using ControlActividades.Dtos.Migracion;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ControlActividades.Migracion
{
    //Migrador masivo de usuarios de Identity usando SqlBulkCopy
    public class BulkIdentityMigrator
    {
        private readonly string _connectionString;

        public BulkIdentityMigrator()
        {
            _connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"]
                .ConnectionString;
        }

        private DataTable CrearTablaAspNetUsers()
        {
            var table = new DataTable();

            table.Columns.Add("Id", typeof(string));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("EmailConfirmed", typeof(bool));
            table.Columns.Add("PasswordHash", typeof(string));
            table.Columns.Add("SecurityStamp", typeof(string));
            table.Columns.Add("PhoneNumberConfirmed", typeof(bool));
            table.Columns.Add("TwoFactorEnabled", typeof(bool));
            table.Columns.Add("LockoutEnabled", typeof(bool));
            table.Columns.Add("AccessFailedCount", typeof(int));

            return table;
        }

        //Generar ids válidos. Hash de contraseña
        public void MigrarUsuarios(List<UsuarioMigracionDto> usuarios)
        {
            if (usuarios == null || usuarios.Count == 0)
                throw new Exception("No hay usuarios para migrar");

            var tablaUsuarios = CrearTablaAspNetUsers();
            var hasher = new PasswordHasher();

            foreach (var dto in usuarios)
            {
                var userId = Guid.NewGuid().ToString();

                var passwordHash = hasher.HashPassword(dto.PasswordPlano);

                tablaUsuarios.Rows.Add(
                    userId,
                    dto.Correo,
                    dto.Correo,
                    false,
                    passwordHash,
                    Guid.NewGuid().ToString(),
                    false,
                    false,
                    false,
                    0
                );
            }

            InsertarUsuariosBulk(tablaUsuarios);
        }

        //Método de migración
        private void InsertarUsuariosBulk(DataTable tablaUsuarios)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = "AspNetUsers";
                    bulk.BatchSize = 1000;
                    bulk.BulkCopyTimeout = 600;


                    //Mapeo
                    bulk.ColumnMappings.Add("Id", "Id");
                    bulk.ColumnMappings.Add("UserName", "UserName");
                    bulk.ColumnMappings.Add("Email", "Email");
                    bulk.ColumnMappings.Add("EmailConfirmed", "EmailConfirmed");
                    bulk.ColumnMappings.Add("PasswordHash", "PasswordHash");
                    bulk.ColumnMappings.Add("SecurityStamp", "SecurityStamp");
                    bulk.ColumnMappings.Add("PhoneNumberConfirmed", "PhoneNumberConfirmed");
                    bulk.ColumnMappings.Add("TwoFactorEnabled", "TwoFactorEnabled");
                    bulk.ColumnMappings.Add("LockoutEnabled", "LockoutEnabled");
                    bulk.ColumnMappings.Add("AccessFailedCount", "AccessFailedCount");


                    bulk.WriteToServer(tablaUsuarios);
                }
            }
        }
    }
}