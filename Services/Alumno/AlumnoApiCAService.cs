using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using ControlActividades.Exceptions;
using ControlActividades.Interfaces.Alumnos;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using static NPOI.HSSF.Util.HSSFColor;

namespace ControlActividades.Services.Alumno
{
    public class AlumnoApiCAService : IAlumnoApiService, IDisposable
    {
        #region Propiedades
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private ApplicationUserManager _userManager;
        private bool _disposed = false;

        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? (_db = new ApplicationDbContext());
            }
            private set
            {
                _db = value;
            }
        }

        public FuncionalidadesGenerales Fg
        {
            get
            {
                return _fg ?? (_fg = new FuncionalidadesGenerales());
            }
            set
            {
                _fg = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }
                _disposed = true;
            }
        }
        #endregion

        private async Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnos(List<int> lsAlumnosId)
        {
            try
            {
                var lsAlumnos = new List<EmailVerificadoAlumno>();
                foreach (var id in lsAlumnosId)
                {
                    var alumnoDatos = Db.tbAlumnos.Where(a => a.AlumnoId == id).FirstOrDefault();
                    if (alumnoDatos != null)
                    {
                        var userName = await UserManager.FindByIdAsync(alumnoDatos.UserId);

                        var alumno = new EmailVerificadoAlumno()
                        {
                            // 🎯 LÍNEA AÑADIDA: Asignar el ID del alumno al DTO de respuesta
                            AlumnoId = alumnoDatos.AlumnoId,
                            // O si tu DTO usa la propiedad 'Id': Id = alumnoDatos.AlumnoId, 

                            Email = userName?.Email ?? "",
                            UserName = userName?.UserName ?? "",
                            Nombre = alumnoDatos.Nombre,
                            ApellidoPaterno = alumnoDatos.ApellidoPaterno,
                            ApellidoMaterno = alumnoDatos.ApellidoMaterno,
                        };

                        lsAlumnos.Add(alumno);
                    }
                }
                return lsAlumnos;
            }
            catch (Exception)
            {
                return new List<EmailVerificadoAlumno>();
            }
        }


        public async Task<List<RegistrarEnvioActividadRes>> RegistrarEnvioActividadAlumnoConEnlaces(HttpRequest httpRequest, int actividadId, int alumnoId, int tipoEntrega, string fechaEntrega, string respuestaRaw, string enlacesJson)
        {
            var lsFiles = httpRequest.Files;

            #region Validaciones entrega
            var actividad = Db.tbActividades.FirstOrDefault(a => a.ActividadId == actividadId);

            var limiteEntrega = actividad.LimiteEntregasPorAlumno;

            var tieneLimiteEntregas = actividad.TieneLimiteEntregas;

            var entregasAlumno = Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId).ToList();
            if (tieneLimiteEntregas && entregasAlumno.Count > 0)
            {
                var totalEntregasPorAlumno = entregasAlumno.Count;
                if (totalEntregasPorAlumno > limiteEntrega)
                {
                    //return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    //{
                    //    Mensaje = "Limite de entregas",
                    //    Codigo = "LIMITE_ENTREGA_ACTIVIDAD_ALUMNO",
                    //    Detalles = "Has llegado a tu limite de entregas asignado por el docente."
                    //});
                    throw new EntregaAlumnoException("Has llegado a tu limite de entregas asignado por el docente.");
                }
            }



            #endregion



            #region PROCESAR RESPUESTA: detectar si viene JSON stringifyado
            string textoRespuesta = respuestaRaw;
            List<string> enlacesValidos = new List<string>();
            List<object> archivosExternos = new List<object>();
            try
            {
                if (!string.IsNullOrEmpty(respuestaRaw) && respuestaRaw.TrimStart().StartsWith("{"))
                {
                    var respuestaObj = JsonConvert.DeserializeObject<dynamic>(respuestaRaw);
                    textoRespuesta = respuestaObj.texto ?? respuestaObj.Respuesta ?? "";

                    // Procesar enlaces del JSON interno
                    if (respuestaObj.enlaces != null)
                    {
                        var enlacesTemp = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(respuestaObj.enlaces));
                        foreach (var enlace in enlacesTemp)
                        {
                            if (Fg._validarURL(enlace))
                                enlacesValidos.Add(enlace);
                        }
                    }

                    // Procesar archivos del JSON interno (URLs de archivos subidos)
                    if (respuestaObj.archivos != null)
                    {
                        var archivosTemp = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(respuestaObj.archivos));
                        foreach (var archivo in archivosTemp)
                        {
                            if (archivo != null)
                                archivosExternos.Add(archivo);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                textoRespuesta = respuestaRaw;
            }
            #endregion

            #region 2. VALIDAR PARÁMETROS
            if (actividadId <= 0 || alumnoId <= 0)
            {
                //return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //{
                //    Mensaje = "Faltan parámetros obligatorios",
                //    Codigo = "PARAMETROS_INVALIDOS",
                //    Detalles = $"ActividadId: {actividadId}, AlumnoId: {alumnoId}"
                //});
                throw new EntregaAlumnoException($"ActividadId: {actividadId}, AlumnoId: {alumnoId}");
            }

            DateTime fechaEntregaParsed;
            try
            {
                fechaEntregaParsed = DateTime.Parse(fechaEntrega);
            }
            catch
            {
                //return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //{
                //    Mensaje = "Formato de fecha inválido",
                //    Codigo = "FECHA_INVALIDA",
                //    Detalles = $"Recibido: {fechaEntrega}"
                //});
                throw new EntregaAlumnoException("Formato de fecha inválido.");
            }
            #endregion

            #region 3. AGREGAR ENLACES ADICIONALES (si vienen en el campo separado)
            try
            {
                var enlaces = JsonConvert.DeserializeObject<List<string>>(enlacesJson) ?? new List<string>();
                foreach (var enlace in enlaces)
                {
                    if (Fg._validarURL(enlace))
                    {
                        if (!enlacesValidos.Contains(enlace))
                        {
                            enlacesValidos.Add(enlace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Error parseando enlaces JSON: {enlacesJson} - {ex.Message}");
            }
            #endregion



            #region 4. VERIFICAR SI YA EXISTE ENTREGA

            //tbEntregaActividadAlumno entregaActiva = entregasAlumno.FirstOrDefault(a => a.Estatus);

            //var entregaExistente = await Db.tbEntregaActividadAlumno
            //    .FirstOrDefaultAsync(e => e.EntregaActividadAlumnoId == entregaActiva.EntregaActividadAlumnoId);



            tbEntregaActividadAlumno entregaActividad;
            int entregaActividadAlumnoId = 0;
            var fechaLimite = actividad.FechaLimite;
            var permiteEntregaTardia = actividad.PermitirEntregasTarde;

            if (fechaEntregaParsed > fechaLimite && !actividad.PermitirEntregasTarde)
            {
                //return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //{
                //    Mensaje = "Fecha limite de entrega.",
                //    Codigo = "FECHA_LIMITE_ENTREGA_ACTIVIDAD_ALUMNO",
                //    Detalles = $"La fecha de entrega es el {fechaEntrega:dd/MM/yyyy} a las {fechaEntrega:HH:mm}"
                //});
                throw new EntregaAlumnoException($"La fecha de entrega es el {fechaEntrega:dd/MM/yyyy} a las {fechaEntrega:HH:mm}");
            }

            //if (entregaExistente != null)
            //{
            //    int entregaIdExistente = entregaExistente.EntregaActividadAlumnoId;


            //    entregaActividad = new tbEntregaActividadAlumno()
            //    {
            //        ActividadId = actividadId,
            //        AlumnoId = alumnoId,
            //        FechaEntrega = fechaEntregaParsed,
            //        EstadoEntregaId = 1,
            //        Estatus = true
            //    };


            //    if (entregaActiva.FechaEntrega > fechaLimite)
            //    {
            //        entregaActiva.EntregaTardia = true;
            //    }



            //    Db.tbEntregaActividadAlumno.Add(entregaActividad);
            //    await Db.SaveChangesAsync();

            //    entregaActividadAlumnoId = entregaActividad.EntregaActividadAlumnoId;
            //}
            //else
            //{
            //    // ✅ NUEVA ENTREGA - CREAR
            //    entregaActividad = new tbEntregaActividadAlumno()
            //    {
            //        ActividadId = actividadId,
            //        AlumnoId = alumnoId,
            //        FechaEntrega = fechaEntregaParsed,
            //        EstadoEntregaId = 1
            //    };


            //    if (entregaActiva.FechaEntrega > fechaLimite)
            //    {
            //        entregaActiva.EntregaTardia = true;
            //    }

            //    Db.tbEntregaActividadAlumno.Add(entregaActividad);
            //    await Db.SaveChangesAsync();

            //    entregaActividadAlumnoId = entregaActividad.EntregaActividadAlumnoId;
            //}

            // ✅ NUEVA ENTREGA - CREAR
            entregaActividad = new tbEntregaActividadAlumno()
            {
                ActividadId = actividadId,
                AlumnoId = alumnoId,
                FechaEntrega = fechaEntregaParsed,
                EstadoEntregaId = 1,
                EntregaTardia = (fechaEntregaParsed > fechaLimite),
                Estatus = true
            };


            Db.tbEntregaActividadAlumno.Add(entregaActividad);
            await Db.SaveChangesAsync();

            entregaActividadAlumnoId = entregaActividad.EntregaActividadAlumnoId;
            #endregion


            #region 5. PROCESAR ARCHIVOS
            var archivosMetadata = new List<object>();
            var files = httpRequest.Files;
            var uploadRoot = HttpContext.Current.Server.MapPath("~/Uploads/Entregas/");
            var destFolder = Path.Combine(uploadRoot, actividadId.ToString(), alumnoId.ToString());

            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            var extensionesPermitidas = new[]
            {
                    ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                    ".jpg", ".jpeg", ".png", ".gif", ".txt", ".zip", ".rar", ".7z",
                    ".odt", ".ods", ".odp", ".rtf"
                };

            const long maxPorArchivo = 50 * 1024 * 1024;
            const long maxTotal = 200 * 1024 * 1024;
            long tamanoTotal = 0;

            Console.WriteLine($"[LOG] Procesando {files.Count} archivo(s)");

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file == null || file.ContentLength == 0)
                {
                    Console.WriteLine($"[LOG] Archivo {i} vacío, se omite");
                    continue;
                }

                var extension = Path.GetExtension(file.FileName).ToLower();

                // Validar extensión
                if (!extensionesPermitidas.Contains(extension))
                {
                    //Console.WriteLine($"[ERROR] Extensión no permitida: {extension}");
                    //return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    //{
                    //    Mensaje = $"Extensión no permitida: {extension}",
                    //    Codigo = "ARCHIVO_NO_PERMITIDO",
                    //    Detalles = $"Extensiones válidas: {string.Join(", ", extensionesPermitidas)}"
                    //});
                    throw new EntregaAlumnoException($"Extensión no permitida: {extension}");
                }

                // Validar tamaño individual
                if (file.ContentLength > maxPorArchivo)
                {
                    //Console.WriteLine($"[ERROR] Archivo demasiado grande: {file.FileName}");
                    //return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    //{
                    //    Mensaje = "Archivo excede 50MB",
                    //    Codigo = "ARCHIVO_MUY_GRANDE",
                    //    Detalles = $"Archivo: {file.FileName} ({file.ContentLength / (1024 * 1024)}MB)"
                    //});
                    throw new EntregaAlumnoException("Archivo excede 50MB");
                }

                tamanoTotal += file.ContentLength;

                // Validar tamaño total
                if (tamanoTotal > maxTotal)
                {
                    //Console.WriteLine($"[ERROR] Tamaño total excedido");
                    //return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    //{
                    //    Mensaje = "Tamaño total excede 200MB",
                    //    Codigo = "ESPACIO_INSUFICIENTE",
                    //    Detalles = $"Total actual: {tamanoTotal / (1024 * 1024)}MB"
                    //});
                    throw new EntregaAlumnoException("Tamaño total excede 200MB");
                }

                // Guardar archivo
                var safeName = Path.GetFileName(file.FileName);
                var destPath = Path.Combine(destFolder, safeName);

                if (File.Exists(destPath))
                {
                    var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    safeName = $"{ts}_{safeName}";
                    destPath = Path.Combine(destFolder, safeName);
                }

                file.SaveAs(destPath);
                var ruta = $"/Uploads/Entregas/{actividadId}/{alumnoId}/{safeName}";

                archivosMetadata.Add(new
                {
                    nombre = file.FileName,
                    nombreGuardado = safeName,
                    size = file.ContentLength,
                    ruta = ruta,
                    fechaGuardado = DateTime.Now
                });

                Console.WriteLine($"[LOG] Archivo guardado: {ruta}");
            }
            #endregion


            #region 6. DETERMINAR TIPO DE ENTREGA
            int tipoEntregaDeterminado = Fg._determinarTipoEntrega(textoRespuesta, enlacesValidos, archivosMetadata);

            // Combinar archivos subidos directamente con archivos del JSON (URLs)
            var todosArchivos = new List<object>();
            todosArchivos.AddRange(archivosMetadata);
            todosArchivos.AddRange(archivosExternos);

            // 7. CREAR ENTREGABLE CON TODO ESTRUCTURADO
            var contenidoEstructurado = new
            {
                texto = textoRespuesta,
                enlaces = enlacesValidos,
                archivos = todosArchivos,
                fechaEntrega = DateTime.Now,
                totalArchivos = todosArchivos.Count,
                totalEnlaces = enlacesValidos.Count
            };

            var entregable = new tbEntregables()
            {
                EntregaActividadAlumnoId = entregaActividadAlumnoId,
                TipoEntregaId = 1,
                Contenido = JsonConvert.SerializeObject(contenidoEstructurado),
                Calificacion = null
            };

            Db.tbEntregables.Add(entregable);
            await Db.SaveChangesAsync();

            Console.WriteLine($"[LOG] Entregable creado: {entregable.EntregableId}");

            // 8. LIMPIAR CACHÉ
            Db.ChangeTracker.Entries()
                .Where(e => e.Entity is tbEntregables || e.Entity is tbEntregaActividadAlumno)
                .ToList()
                .ForEach(e => e.State = System.Data.Entity.EntityState.Detached);

            // 9. RETORNAR RESPUESTA
            var datosAlumnoActividad = await Db.tbEntregaActividadAlumno
                .FirstOrDefaultAsync(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId);

            var lsDatosEntregables = Db.tbEntregables
                .Where(a => a.EntregaActividadAlumnoId == datosAlumnoActividad.EntregaActividadAlumnoId)
                .ToList();

            var lsEnvios = new List<RegistrarEnvioActividadRes>();

            foreach (var datoEntregable in lsDatosEntregables)
            {
                lsEnvios.Add(new RegistrarEnvioActividadRes
                {
                    AlumnoId = alumnoId,
                    EntregaActividadAlumnoId = datoEntregable.EntregaActividadAlumnoId,
                    EntregableId = datoEntregable.EntregableId,
                    ActividadId = datosAlumnoActividad.ActividadId,
                    FechaEntrega = datosAlumnoActividad.FechaEntrega,
                    Contenido = datoEntregable.Contenido,
                    Calificacion = datoEntregable.Calificacion ?? 0,
                    EstadoEntregaId = datosAlumnoActividad.EstadoEntregaId,
                    TipoEntrega = tipoEntregaDeterminado
                });
            }
            #endregion

            return lsEnvios;
        }

        public async Task<List<EnvioActividadAlumnoResponse>> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            List<EnvioActividadAlumnoResponse> lsEnvios = new List<EnvioActividadAlumnoResponse>();

            var datosAlumnoActividad = await Db.tbEntregaActividadAlumno.FirstOrDefaultAsync(a => a.ActividadId == ActividadId && a.AlumnoId == AlumnoId && a.Estatus);

            if (datosAlumnoActividad != null)
            {
                var entregaActividadId = datosAlumnoActividad.EntregaActividadAlumnoId;

                var fechaEntrega = datosAlumnoActividad?.FechaEntrega;


                //List<EnvioRes> lsEnvios = new List<EnvioRes>();

                var lsEntregas = Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == entregaActividadId).ToList();
                if (lsEntregas.Count > 0)
                {
                    foreach (var entrega in lsEntregas)
                    {
                        EnvioActividadAlumnoResponse envio = new EnvioActividadAlumnoResponse()
                        {
                            AlumnoId = datosAlumnoActividad.AlumnoId,
                            EntregaActividadAlumnoId = datosAlumnoActividad.EntregaActividadAlumnoId,
                            EntregableId = entrega.EntregableId,
                            ActividadId = datosAlumnoActividad.ActividadId,
                            FechaEntrega = datosAlumnoActividad.FechaEntrega,
                            Contenido = entrega.Contenido,
                            //Calificacion = entrega.Calificacion ?? 0,
                            //FechaCalificado = entrega.FechaCalificado,
                            EstadoEntregaId = datosAlumnoActividad.EstadoEntregaId
                        };

                        lsEnvios.Add(envio);
                    }
                }
            }

            return lsEnvios;
        }

        public async Task CancelarEnvioActividad(int alumnoId, int actividadId)
        {
            var alumnoActividadEliminar = Db.tbEntregaActividadAlumno.FirstOrDefault(a => a.AlumnoId == alumnoId && a.ActividadId == actividadId && a.EstadoEntregaId == 1 && a.Estatus) ?? throw new Exception();

            var entregables = Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == alumnoActividadEliminar.EntregaActividadAlumnoId).ToList();

            //foreach (var entrega in entregables)
            //{
            //    if (entrega.Calificacion.HasValue)
            //    {
            //        //return BadRequest("No puedes cancelar esta entrega pues ya esta calificada");
            //        throw new Exception();
            //    }
            //}

            if (alumnoActividadEliminar.Calificacion > 0 && alumnoActividadEliminar.FechaCalificado != null)
            {
                throw new Exception();
            }


            //foreach (var entrega in entregables)
            //{
            //    Db.tbEntregables.Remove(entrega);
            //}
            Db.tbEntregables.RemoveRange(entregables);
            await Db.SaveChangesAsync();


            alumnoActividadEliminar.Estatus = false;
            Db.Entry(alumnoActividadEliminar).State = EntityState.Modified;

            await Db.SaveChangesAsync();
        }

        public Task AlumnoGrupoCodigo(int alumnoId, string codigoAcceso)
        {
            throw new NotImplementedException();
        }

        public Task AlumnoMateriaCodigo(int alumnoId, string codigoAcceso)
        {
            throw new NotImplementedException();
        }

        public async Task<UnirseAClaseMRespuesta> UnirseAClase(int alumnoId, string codigoAcceso)
        {

            // 3. Buscar grupo con comparación case-insensitive
            var grupo = await Db.tbGrupos
                .FirstOrDefaultAsync(g => g.CodigoAcceso.ToUpper() == codigoAcceso);

            if (grupo != null)
            {
                // 4. Validar que el docente existe
                var docente = await Db.tbDocentes
                    .FirstOrDefaultAsync(d => d.DocenteId == grupo.DocenteId);

                if (docente == null)
                {
                    //return Content(HttpStatusCode.NotFound, new
                    //{
                    //    mensaje = "Docente no encontrado. El grupo no tiene un docente asociado válido."
                    //});
                    throw new AlumnosException("Docente no encontrado. El grupo no tiene un docente asociado válido.", "");
                }

                // 5. ✅ VALIDAR si el alumno YA ESTÁ registrado en este grupo
                var alumnoYaEnGrupo = await Db.tbAlumnosGrupos
                    .AnyAsync(ag => ag.AlumnoId == alumnoId && ag.GrupoId == grupo.GrupoId);

                if (alumnoYaEnGrupo)
                {
                    // El alumno ya está registrado en este grupo
                    //return Content(HttpStatusCode.Conflict, new
                    //{
                    //    mensaje = $"Ya estás registrado en el grupo '{grupo.NombreGrupo}'. No puedes unirte nuevamente.",
                    //    grupoId = grupo.GrupoId,
                    //    nombreGrupo = grupo.NombreGrupo,
                    //    esGrupo = true
                    //});

                    throw new AlumnosException($"Ya estás registrado en el grupo '{grupo.NombreGrupo}'. No puedes unirte nuevamente.", "");
                }

                // 6. Obtener materias del grupo
                var lsMateriasId = await Db.tbGruposMaterias
                    .Where(gm => gm.GrupoId == grupo.GrupoId)
                    .Select(gm => gm.MateriaId)
                    .ToListAsync();

                var lsMaterias = await Db.tbMaterias
                    .Where(m => lsMateriasId.Contains(m.MateriaId))
                    .Select(m => new MateriaRes
                    {
                        MateriaId = m.MateriaId,
                        NombreMateria = m.NombreMateria,
                        Descripcion = m.Descripcion,
                        Actividades = Db.tbActividades
                            .Where(a => a.MateriaId == m.MateriaId)
                            .Select(a => new ActividadRes
                            {
                                ActividadId = a.ActividadId,
                                NombreActividad = a.NombreActividad,
                                Descripcion = a.Descripcion,
                                FechaCreacion = a.FechaCreacion,
                                FechaLimite = a.FechaLimite,
                                //Puntaje = a.Puntaje
                            })
                            .ToList()
                    })
                    .ToListAsync();

                // 7. Crear respuesta del grupo
                var grupoRes = new GrupoRes()
                {
                    GrupoId = grupo.GrupoId,
                    NombreGrupo = grupo.NombreGrupo,
                    Descripcion = grupo.Descripcion,
                    CodigoAcceso = grupo.CodigoAcceso,
                    // 🔧 CORREGIDO: Asignar color por defecto si es null para evitar errores de serialización
                    CodigoColor = string.IsNullOrEmpty(grupo.CodigoColor) ? "#2196F3" : grupo.CodigoColor,
                    Materias = lsMaterias
                };

                // 8. Crear relación alumno-grupo
                var nuevaRelacion = new tbAlumnosGrupos
                {
                    AlumnoId = alumnoId,
                    GrupoId = grupo.GrupoId
                };

                Db.tbAlumnosGrupos.Add(nuevaRelacion);
                await Db.SaveChangesAsync();

                // 9. Retornar respuesta exitosa
                var respuesta = new UnirseAClaseMRespuesta()
                {
                    Grupo = grupoRes,
                    EsGrupo = true
                };

                return respuesta;
            }

            // 10. Si no es grupo, buscar materia con comparación case-insensitive
            var materia = await Db.tbMaterias
                .FirstOrDefaultAsync(m => m.CodigoAcceso.ToUpper() == codigoAcceso);

            if (materia != null)
            {
                // 11. Validar que el docente existe
                var docente = await Db.tbDocentes
                    .FirstOrDefaultAsync(d => d.DocenteId == materia.DocenteId);

                if (docente == null)
                {
                    //return Content(HttpStatusCode.NotFound, new
                    //{
                    //    mensaje = "Docente no encontrado. La materia no tiene un docente asociado válido."
                    //});

                    throw new AlumnosException("Docente no encontrado. La materia no tiene un docente asociado válido.", "");
                }

                // 12. ✅ VALIDAR si el alumno YA ESTÁ registrado en esta materia
                var alumnoYaEnMateria = await Db.tbAlumnosMaterias
                    .AnyAsync(am => am.AlumnoId == alumnoId && am.MateriaId == materia.MateriaId);

                if (alumnoYaEnMateria)
                {
                    // El alumno ya está registrado en esta materia
                    //return Content(HttpStatusCode.Conflict, new
                    //{
                    //    mensaje = $"Ya estás registrado en la materia '{materia.NombreMateria}'. No puedes unirte nuevamente.",
                    //    materiaId = materia.MateriaId,
                    //    nombreMateria = materia.NombreMateria,
                    //    esGrupo = false
                    //});

                    throw new AlumnosException($"Ya estás registrado en la materia '{materia.NombreMateria}'. No puedes unirte nuevamente.", "");
                }

                // 13. Crear respuesta de la materia
                var materiaRes = new MateriaRes()
                {
                    MateriaId = materia.MateriaId,
                    NombreMateria = materia.NombreMateria,
                    Descripcion = materia.Descripcion,
                    Actividades = await Db.tbActividades
                        .Where(a => a.MateriaId == materia.MateriaId)
                        .Select(a => new ActividadRes
                        {
                            ActividadId = a.ActividadId,
                            NombreActividad = a.NombreActividad,
                            Descripcion = a.Descripcion,
                            FechaCreacion = a.FechaCreacion,
                            FechaLimite = a.FechaLimite,
                            //Puntaje = a.Puntaje
                        })
                        .ToListAsync()
                };

                // 14. Crear relación alumno-materia
                var nuevaRelacion = new tbAlumnosMaterias
                {
                    AlumnoId = alumnoId,
                    MateriaId = materia.MateriaId
                };

                Db.tbAlumnosMaterias.Add(nuevaRelacion);
                await Db.SaveChangesAsync();

                // 15. Retornar respuesta exitosa
                var respuesta = new UnirseAClaseMRespuesta()
                {
                    Materia = materiaRes,
                    EsGrupo = false
                };

                return respuesta;
            }

            throw new AlumnosException("Código de acceso inválido o inexistente. Verifica que el código sea correcto.", "");
        }

        public async Task<RegistrarAlumnoGrupoMateriaDocenteRes> RegistrarAlumnoGrupoMateriaDocente(List<string> lsEmails, int grupoId, int materiaId)
        {
            bool alumnoRegistradoGrupo = false;
            bool alumnoRegistradoMateria = false;
            int docenteId = -1;
            List<int> lsAlumnosId = new List<int>();

            foreach (var email in lsEmails)
            {
                var user = await UserManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var alumnoId = await Db.tbAlumnos.Where(a => a.UserId == user.Id).Select(a => a.AlumnoId).FirstOrDefaultAsync();

                    lsAlumnosId.Add(alumnoId);
                }
            }

            //int grupoId = alumnoGMRegistro.GrupoId;
            //int materiaId = alumnoGMRegistro.MateriaId;


            if (grupoId != 0)
            {
                docenteId = await Db.tbGrupos.Where(a => a.GrupoId == grupoId).Select(a => a.DocenteId).FirstOrDefaultAsync();

                var lsMateriasGrupo = Db.tbGruposMaterias.Where(a => a.GrupoId == grupoId).Select(a => a.MateriaId).ToList();

                foreach (var aluId in lsAlumnosId)
                {
                    bool alumnoYaRegistrado = Db.tbAlumnosGrupos.Any(a => a.GrupoId == grupoId && a.AlumnoId == aluId);
                    if (!alumnoYaRegistrado)
                    {
                        tbAlumnosGrupos alumnosGrupos = new tbAlumnosGrupos()
                        {
                            AlumnoId = aluId,
                            GrupoId = grupoId
                        };

                        Db.tbAlumnosGrupos.Add(alumnosGrupos);
                    }
                    else
                    {
                        //Content(HttpStatusCode.BadRequest, new { mensaje = "El alumno ya esta registrado" });
                        throw new AlumnosException("El alumno ya esta registrado en el grupo.", "");
                    }


                    #region Registramos el alumno en todas las materias del grupo tambien
                    List<tbAlumnosMaterias> lsMateriasAlumno = lsMateriasGrupo.Select(a => new tbAlumnosMaterias
                    {
                        AlumnoId = aluId,
                        MateriaId = a
                    }).ToList();

                    Db.tbAlumnosMaterias.AddRange(lsMateriasAlumno);
                    #endregion
                }


                await Db.SaveChangesAsync();
                alumnoRegistradoGrupo = true;
                //var lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);
            }

            if (materiaId != 0)
            {
                docenteId = await Db.tbMaterias.Where(a => a.MateriaId == materiaId).Select(a => a.DocenteId).FirstOrDefaultAsync();
                foreach (var aluId in lsAlumnosId)
                {
                    bool alumnoYaRegistrado = Db.tbAlumnosMaterias.Any(a => a.MateriaId == materiaId && a.AlumnoId == aluId);
                    if (!alumnoYaRegistrado)
                    {
                        tbAlumnosMaterias alumnosMaterias = new tbAlumnosMaterias()
                        {
                            AlumnoId = aluId,
                            MateriaId = materiaId
                        };
                        Db.tbAlumnosMaterias.Add(alumnosMaterias);
                    }
                    else
                    {
                        throw new AlumnosException("El alumno ya esta registrado en el grupo.", "");
                    }
                }
                await Db.SaveChangesAsync();
                alumnoRegistradoMateria = true;
            }



            var lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);

            var res = new RegistrarAlumnoGrupoMateriaDocenteRes()
            {
                AlumnoRegistradoGrupo = alumnoRegistradoGrupo,
                AlumnoRegistradoMateria = alumnoRegistradoMateria,
                Alumnos = lsAlumnos
            };

            return res;
        }

        public async Task EliminarAlumnoDeMateria(int materiaId, int alumnoId)
        {
            // Buscar la relación alumno-materia
            var relacionAEliminar = await Db.tbAlumnosMaterias
                .FirstOrDefaultAsync(am => am.MateriaId == materiaId && am.AlumnoId == alumnoId);

            if (relacionAEliminar == null)
            {
                //return Content(HttpStatusCode.NotFound, new ErrorResponse
                //{
                //    Mensaje = "El alumno no está inscrito en esta materia.",
                //    Codigo = AlumnoErrorCodes.ALUMNO_NO_ENCONTRADO,
                //    Detalles = $"No se encontró una inscripción del alumno {alumnoId} en la materia {materiaId}."
                //});
                throw new AlumnosException("El alumno no está inscrito en esta materia.", $"");

            }

            // Eliminar la inscripción
            Db.tbAlumnosMaterias.Remove(relacionAEliminar);
            await Db.SaveChangesAsync();

            // Limpiar caché
            Db.ChangeTracker.Entries()
                .Where(e => e.Entity is tbAlumnosMaterias)
                .ToList()
                .ForEach(e => e.State = System.Data.Entity.EntityState.Detached);
        }

        public async Task EliminarAlumnoDeGrupo(int grupoId, int alumnoId)
        {
            // Buscar la relación alumno-grupo
            var relacionAEliminar = await Db.tbAlumnosGrupos
                .FirstOrDefaultAsync(ag => ag.GrupoId == grupoId && ag.AlumnoId == alumnoId);

            if (relacionAEliminar == null)
            {
                //return Content(HttpStatusCode.NotFound, new ErrorResponse
                //{
                //    Mensaje = "El alumno no está inscrito en este grupo.",
                //    Codigo = AlumnoErrorCodes.ALUMNO_NO_ENCONTRADO,
                //    Detalles = $"No se encontró una inscripción del alumno {alumnoId} en el grupo {grupoId}."
                //});
                throw new AlumnosException("El alumno no está inscrito en este grupo.", $"No se encontró una inscripción del alumno {alumnoId} en el grupo {grupoId}.");
            }

            // Eliminar la inscripción

            var lsMateriasGrupo = Db.tbGruposMaterias.Where(a => a.GrupoId == grupoId).Select(a => a.MateriaId).ToList();

            var lsMateriasAlumno = Db.tbAlumnosMaterias.Where(a => lsMateriasGrupo.Contains(a.MateriaId) && a.AlumnoId == alumnoId).ToList();

            var lsMateriasAlumnoId = lsMateriasAlumno.Select(a => a.MateriaId).ToList();

            var lsActividadesPorMateria = Db.tbActividades.Where(a => lsMateriasAlumnoId.Contains(a.MateriaId)).Select(a => a.ActividadId).ToList();

            var alumnoTieneEntregas = Db.tbEntregaActividadAlumno.Where(a => lsActividadesPorMateria.Contains(a.ActividadId) && a.AlumnoId == alumnoId).Any();

            if (alumnoTieneEntregas)
            {
                throw new AlumnosException("El alumno ya ha realizado una entrega.", "");
            }

            Db.tbAlumnosGrupos.Remove(relacionAEliminar);
            Db.tbAlumnosMaterias.RemoveRange(lsMateriasAlumno);

            await Db.SaveChangesAsync();


            // Limpiar caché
            Db.ChangeTracker.Entries()
                .Where(e => e.Entity is tbAlumnosGrupos)
                .ToList()
                .ForEach(e => e.State = System.Data.Entity.EntityState.Detached);
        }

        public async Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnosGrupo(int grupoId)
        {
            List<int> lsAlumnosId = await Db.tbAlumnosGrupos.Where(a => a.GrupoId == grupoId).Select(a => a.AlumnoId).ToListAsync();

            List<EmailVerificadoAlumno> lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);

            return lsAlumnos;
        }

        public async Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnosMateria(int grupoId, int materiaId)
        {
            // =========================
            // CASO 1: CON GRUPO
            // =========================
            if (grupoId > 0 && materiaId > 0)
            {
                var alumnosGrupo = await Db.tbAlumnosGrupos
                    .Where(g => g.GrupoId == grupoId)
                    .Select(g => g.AlumnoId)
                    .ToListAsync();

                var alumnos = await ObtenerListaAlumnos(alumnosGrupo);

                return alumnos;
            }

            // =========================
            // CASO 2: SIN GRUPO (MATERIA)
            // =========================
            var alumnosMateria = await Db.tbAlumnosMaterias
                .Where(am => am.MateriaId == materiaId)
                .Select(am => new
                {
                    am.AlumnoMateriaId,
                    am.AlumnoId
                })
                .ToListAsync();


            var alumnosIds = alumnosMateria
                .Select(a => a.AlumnoId)
                .ToList();

            var alumnosInfo = await ObtenerListaAlumnos(alumnosIds);

            return alumnosInfo;

            // 🔑 UNIÓN CORRECTA
            //var resultado = alumnosInfo.Select((a, index) => new
            //{
            //    AlumnoMateriaId = alumnosMateria[index].AlumnoMateriaId,
            //    AlumnoId = alumnosMateria[index].AlumnoId,

            //    UserName = a.UserName,
            //    Email = a.Email,
            //    Nombre = a.Nombre,
            //    ApellidoPaterno = a.ApellidoPaterno,
            //    ApellidoMaterno = a.ApellidoMaterno,
            //    GrupoId = (int?)null
            //});
        }
    }
}