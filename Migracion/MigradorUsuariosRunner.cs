//using ControlActividades.Dtos.Migracion;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//POSIBLE USO FUTURO
//namespace ControlActividades.Migracion
//{
//    /// <summary>
//    /// Orquestador de migración de usuarios.
//    /// 
//    /// - Lee un archivo JSON con usuarios del sistema A
//    /// - Divide la información en lotes pequeños
//    /// - Envía cada lote al endpoint de migración del sistema B
//    /// - Evita timeouts de IIS
//    /// 
//    /// ESTA CLASE SE EJECUTA SOLO UNA VEZ.
//    /// </summary>
//    public class MigradorUsuariosRunner
//    {
//        private const int BatchSize = 500;
//        private const string Endpoint = "https://localhost:44344/api/migracion/usuarios";

//        public async Task EjecutarAsync(string rutaJson)
//        {
//            Console.WriteLine("- INICIANDO MIGRACIÓN DE USUARIOS -");

//            if (!File.Exists(rutaJson))
//                throw new FileNotFoundException("No se encontró el archivo JSON", rutaJson);

//            var json = File.ReadAllText(rutaJson);
//            var dto = JsonSerializer.Deserialize<MigracionUsuariosDto>(json);

//            if (dto?.Usuarios == null || dto.Usuarios.Count == 0)
//                throw new Exception("El archivo no contiene usuarios");

//            Console.WriteLine($"Usuarios totales en archivo: {dto.Usuarios.Count}");

//            using (var http = new HttpClient())
//            {
//                http.Timeout = TimeSpan.FromMinutes(12);
            

//                int total = dto.Usuarios.Count;
//                int procesados = 0;
//                int lote = 1;

//                for(int i=0; i<dto.Usuarios.Count; i+= BatchSize)
//                {
//                    var batch = dto.Usuarios.Skip(i).Take(BatchSize).ToList();
//                    Console.WriteLine($"Enviando lote {lote} ({batch.Count} usuarios)");

//                    var payload = new MigracionUsuariosDto
//                    {
//                        Usuarios = batch
//                    };

//                    var content = new StringContent(
//                        JsonSerializer.Serialize(payload),
//                        Encoding.UTF8,
//                        "application/json"
//                    );

//                    var response = await http.PostAsync(Endpoint, content);

//                    if (!response.IsSuccessStatusCode)
//                    {
//                        var error = await response.Content.ReadAsStringAsync();
//                        throw new Exception($"Error en lote {lote}: {response.StatusCode} - {error}");
//                    }

//                    var respuesta = await response.Content.ReadAsStringAsync();
//                    Console.WriteLine($"Lote {lote} procesado correctamente");

//                    procesados += batch.Count;
//                    Console.WriteLine($"Progreso: {procesados} / {total}");

//                    lote++;
//                }

//            }

//            Console.WriteLine("- MIGRACIÓN FINALIZADA -");
//        }
//    }
//}
