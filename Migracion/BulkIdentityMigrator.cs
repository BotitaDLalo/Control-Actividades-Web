using ControlActividades.Dtos.Migracion;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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
        
        /* Usar solo si no hay ningún usuario en la bdd
        private bool YaHayUsuarios()
        {
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM AspNetUsers", cn))
            {
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
        */

        // Tabla de usuarios
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

        // Tabla de alumnso
        private DataTable CrearTablaAlumnos()
        {
            var table = new DataTable();

            table.Columns.Add("ApellidoPaterno", typeof(string));
            table.Columns.Add("ApellidoMaterno", typeof(string));
            table.Columns.Add("Nombre", typeof(string));
            table.Columns.Add("UserId", typeof(string));

            var estatusCol = table.Columns.Add("Estatus", typeof(bool));
            estatusCol.AllowDBNull = true;
            
            table.Columns.Add("Matricula", typeof(string));

            return table;
        }

        //Tabla de docentes
        private DataTable CrearTablaDocentes()
        {
            var table = new DataTable();

            table.Columns.Add("ApellidoPaterno", typeof(string));
            table.Columns.Add("ApellidoMaterno", typeof(string));
            table.Columns.Add("Nombre", typeof(string));
            table.Columns.Add("estaAutorizado", typeof(bool));
            table.Columns.Add("seEnvioCorreo", typeof(bool));

            var fechaCol = table.Columns.Add("FechaExpiracionCodigo", typeof(DateTime));
            fechaCol.AllowDBNull = true;

            table.Columns.Add("CodigoAutorizacion", typeof(string));
            table.Columns.Add("UserId", typeof(string));

            return table;
        }

        // Relacionar usuarios con sus roles
        private DataTable CrearTablaUserRoles()
        {
            var table = new DataTable();

            table.Columns.Add("UserId", typeof(string));
            table.Columns.Add("RoleId", typeof(string));
            
            return table;
        }

        private Dictionary<string, string> ObtenerRoles()
        {
            var roles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT Id, Name FROM AspNetRoles", cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        roles[rd.GetString(1)] = rd.GetString(0);
                }
            }

            return roles;
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
            var tablaAlumnos = CrearTablaAlumnos();
            var tablaDocentes = CrearTablaDocentes();
            var tablaUserRoles = CrearTablaUserRoles();
            var roles = ObtenerRoles();

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
            
            foreach (var dto in usuarios)
            {
                var userId = Guid.NewGuid().ToString();
                if(!roles.ContainsKey(dto.Rol))
                    throw new Exception($"Rol inválido: {dto.Rol}");

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

                tablaUserRoles.Rows.Add(
                    userId,
                    roles[dto.Rol]
                );

                if (dto.Rol == "Alumno")
                {
                    tablaAlumnos.Rows.Add(
                        dto.ApellidoPaterno,
                        dto.ApellidoMaterno,
                        dto.Nombre,
                        userId,
                        DBNull.Value,
                        dto.Matricula
                    );
                }

                if(dto.Rol == "Docente")
                {
                    tablaDocentes.Rows.Add(
                        dto.ApellidoPaterno,
                        dto.ApellidoMaterno,
                        dto.Nombre,
                        false, //estaAutorizado
                        false, //seEnvioCorreo
                        DBNull.Value, //FechaExpiracionCodigo
                        DBNull.Value, //codigoAutorizacion
                        userId
                    );
                }

            }
            
            var sw = Stopwatch.StartNew();
            InsertarUsuariosBulk(tablaUsuarios);
            InsertarBulk(tablaAlumnos, "tbAlumnos");
            InsertarBulk(tablaDocentes, "tbDocentes");
            InsertarBulk(tablaUserRoles, "AspNetUserRoles");
            sw.Stop();

            var segundos = sw.Elapsed.TotalSeconds;
            var velocidad = totalUsuarios/ segundos;

            Debug.WriteLine(
                $"TIEMPO REAL DE INSERCIÓN: {segundos:N2} s");
           
            Debug.WriteLine(
                $"VELOCIDAD: {velocidad:N2} usuarios/s");

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

        // Método de migración por rol
        private void InsertarBulk(DataTable tabla, string nombreTabla)
        {
            if (tabla.Rows.Count == 0)
                return;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var bulk = new SqlBulkCopy(connection))
                {
                    bulk.DestinationTableName = nombreTabla;
                    bulk.BatchSize = 1000;
                    bulk.BulkCopyTimeout = 600;

                    foreach (DataColumn col in tabla.Columns)
                        bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                    bulk.WriteToServer(tabla);
                }
            }
        }
    }
}