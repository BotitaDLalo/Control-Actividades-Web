using ControlActividades.Dtos.Migracion;
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
        private const string HASH_RESET = "knOYa8/oV1qcfGtF5bd3eaeFzTLleVaXClllXpbLo89wbbclItc5z6glVnGfW76f";
        private readonly string _connectionString;

        public BulkIdentityMigrator()
        {
            _connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"]
                .ConnectionString;
        }

        private bool YaHayUsuarios()
        {
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM AspNetUsers", cn))
            {
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
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

        //Generar ids válidos.
        public void MigrarUsuarios(List<UsuarioMigracionDto> usuarios)
        {
            /*Migración sólo una vez
            if (YaHayUsuarios())
                throw new Exception("La migración ya fue ejecutada anteriormente.");
            */
            //Validar lista vacía
            if (usuarios == null || usuarios.Count == 0)
                throw new Exception("No hay usuarios para migrar");

            int totalUsuarios = usuarios.Count;

            var tablaUsuarios = CrearTablaAspNetUsers();

            var duplicados = usuarios
                .GroupBy(x => x.Correo)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            //Valida duplicados en el archivo
            if (duplicados.Any())
                throw new Exception("Correos duplicados en archivo JSON");

            var correosExistentes = new HashSet<string>();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT Email FROM AspNetUsers", cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        correosExistentes.Add(rd.GetString(0));
            }

            var repetidosEnBd = usuarios
                .Where(u => correosExistentes.Contains(u.Correo))
                .Select(u => u.Correo)
                .ToList();

            //Valida duplicados en la base de datos
            if (repetidosEnBd.Any())
                throw new Exception("Existen correos ya registrados en la base de datos");
            var inicio = DateTime.Now;
            foreach (var dto in usuarios)
            {
                var userId = Guid.NewGuid().ToString();

                tablaUsuarios.Rows.Add(
                    userId,
                    dto.Correo,
                    dto.Correo,
                    true,
                    HASH_RESET,
                    Guid.NewGuid().ToString(),
                    false,
                    false,
                    false,
                    0
                );
            }
            var fin = DateTime.Now;

            System.Diagnostics.Debug.WriteLine(
                $"(Usuarios migrados: {totalUsuarios} en {(fin-inicio).TotalSeconds:N2} segundos"
            );
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