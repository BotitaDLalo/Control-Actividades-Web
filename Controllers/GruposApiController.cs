﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;


namespace ControlActividades.Controllers
{
    public class GruposApiController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        public GruposApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
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

        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.Current.GetOwinContext().Get<RoleManager<IdentityRole>>();
            }
            private set
            {
                _roleManager = value;
            }
        }

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



        #region Docente
        private static string ObtenerClave()
        {
            int length = 8;

            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }

            return str_build.ToString();
        }

        public async Task<List<object>> ConsultaGruposMaterias(int docenteId)
        {
            try
            {
                var lsGrupos = Db.tbGrupos.Where(a => a.DocenteId == docenteId).ToList();

                var listaGruposMaterias = new List<object>();
                foreach (var grupo in lsGrupos)
                {
                    var lsMateriasId = await Db.tbGruposMaterias.Where(a => a.GrupoId == grupo.GrupoId).Select(a => a.MateriaId).ToListAsync();

                    var lsMaterias = await Db.tbMaterias.Where(a => lsMateriasId.Contains(a.MateriaId)).Select(m => new
                    {
                        m.MateriaId,
                        m.NombreMateria,
                        m.Descripcion,
                        //m.CodigoColor,
                        actividades = Db.tbActividades.Where(a => a.MateriaId == m.MateriaId).ToList()
                    }).ToListAsync();


                    listaGruposMaterias.Add(new
                    {
                        grupoId = grupo.GrupoId,
                        nombreGrupo = grupo.NombreGrupo,
                        descripcion = grupo.Descripcion,
                        codigoAcceso = grupo.CodigoAcceso,
                        codigoColor = grupo.CodigoColor,
                        materias = lsMaterias
                    });
                }

                return listaGruposMaterias;
            }
            catch (Exception)
            {
                return new List<object>();
            }
        }

        public async Task<List<object>> ConsultaGruposCreados()
        {
            try
            {
                var lsGrupos = await Db.tbGrupos.Select(a => new
                {
                    a.GrupoId,
                    a.NombreGrupo
                }).ToListAsync<object>();

                return lsGrupos;
            }
            catch (Exception)
            {
                return new List<object>();
            }
        }

        [HttpGet]
        [Route("api/Grupos/ObtenerGruposCreados")]
        public async Task<IHttpActionResult> ObtenerGruposCreados(int docenteId)
        {
            try
            {
                var lsGrupos = await Db.tbGrupos.Where(a => a.DocenteId == docenteId)
                    .Select(a => new
                    {
                        a.GrupoId,
                        a.NombreGrupo
                    }).ToListAsync();

                return Ok(lsGrupos);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest,new
                {
                    e.Message
                });
            }
        }

        [HttpPost]
        [Route("api/Grupos/ObtenerGruposMateriasDocente")]
        public async Task<IHttpActionResult> ObtenerGruposMateriasDocente(int docenteId)
        {
            try
            {
                var lsGrupos = await Db.tbGrupos.Where(a => a.DocenteId == docenteId).ToListAsync();


                var listaGruposMaterias = new List<object>();
                foreach (var grupo in lsGrupos)
                {
                    var lsMateriasId = await Db.tbGruposMaterias.Where(a => a.GrupoId == grupo.GrupoId).Select(a => a.MateriaId).ToListAsync();

                    var lsMaterias = await Db.tbMaterias.Where(a => lsMateriasId.Contains(a.MateriaId)).Select(m => new
                    {
                        m.MateriaId,
                        m.NombreMateria,
                        m.Descripcion,
                        actividades = Db.tbActividades.Where(a => a.MateriaId == m.MateriaId).ToList()
                    }).ToListAsync();


                    listaGruposMaterias.Add(new
                    {
                        grupoId = grupo.GrupoId,
                        nombreGrupo = grupo.NombreGrupo,
                        descripcion = grupo.Descripcion,
                        codigoAcceso = grupo.CodigoAcceso,
                        codigoColor = grupo.CodigoColor,
                        materias = lsMaterias
                    });
                }


                return Ok(listaGruposMaterias);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest,new
                {
                    e.Message
                });
            }
        }




        [HttpPost]
        [Route("api/Grupos/CrearGrupo")]
        public async Task<IHttpActionResult> CrearGrupo([FromBody] tbGrupos group)
        {
            try
            {
                group.CodigoAcceso = "AS4A65S";

                Db.tbGrupos.Add(group);
                await Db.SaveChangesAsync();
                return Ok(await Db.tbGrupos.ToListAsync());
            }
            catch (DbUpdateException ex)
            {
                // Captura la excepción interna para más detalles
                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content(HttpStatusCode.InternalServerError, $"Internal server error: {innerException}");
            }
        }

        [HttpPost]
        [Route("api/Grupos/CrearGrupoMaterias")]
        public async Task<IHttpActionResult> CrearGrupoMaterias([FromBody] GrupoMateriasRegistro group)
        {
            try
            {
                int docenteId = group.DocenteId;

                List<tbMaterias> lsMaterias = new List<tbMaterias>();
                foreach (var materia in group.Materias)
                {
                    string codigoAccesoMateria = ObtenerClave();
                    tbMaterias nuevaMateria = new tbMaterias()
                    {
                        DocenteId = docenteId,
                        NombreMateria = materia.NombreMateria,
                        Descripcion = materia.Descripcion,
                        CodigoAcceso = codigoAccesoMateria
                    };

                    lsMaterias.Add(nuevaMateria);
                }
                string codigoAccesoGrupo = ObtenerClave();
                tbGrupos nuevoGrupo = new tbGrupos()
                {
                    DocenteId = group.DocenteId,
                    NombreGrupo = group.NombreGrupo,
                    Descripcion = group.Descripcion,
                    //CodigoColor = group.CodigoColor,
                    CodigoAcceso = codigoAccesoGrupo
                };

                Db.tbGrupos.Add(nuevoGrupo);
                Db.tbMaterias.AddRange(lsMaterias);

                await Db.SaveChangesAsync();

                var nuevoGrupoId = nuevoGrupo.GrupoId;
                var lsMateriasId = lsMaterias.Select(a => a.MateriaId).ToList();
                List<tbGruposMaterias> vinculos = new List<tbGruposMaterias>();
                foreach (var materiaId in lsMateriasId)
                {
                    tbGruposMaterias vinculo = new tbGruposMaterias()
                    {
                        GrupoId = nuevoGrupoId,
                        MateriaId = materiaId
                    };
                    vinculos.Add(vinculo);
                }
                Db.tbGruposMaterias.AddRange(vinculos);

                await Db.SaveChangesAsync();

                var lsGruposMaterias = await ConsultaGruposMaterias(docenteId);

                return Ok(lsGruposMaterias);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, $"Error al crear el grupo y materias: {ex.Message}");
            }
        }


        [HttpPut]
        [Route("api/Grupos/ActualizarGrupo")]
        public async Task<IHttpActionResult> ActualizarGrupo([FromBody] tbGrupos updatedGroup)
        {
            try
            {
                int grupoId = updatedGroup.GrupoId;
                var dbGroup = await Db.tbGrupos.FindAsync(grupoId);
                if (dbGroup is null) return Content(HttpStatusCode.NotFound,"Grupo no encontrado");


                dbGroup.NombreGrupo = updatedGroup.NombreGrupo;
                dbGroup.Descripcion = updatedGroup.Descripcion;
                dbGroup.CodigoColor = updatedGroup.CodigoColor;
                //dbGroup.TipoUsuario = updatedGroup.TipoUsuario;

                await Db.SaveChangesAsync();

                var grupoActualizado = Db.tbGrupos.Where(a => a.GrupoId == grupoId).FirstOrDefault();
                return Ok(grupoActualizado);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteGroup(int id)
        {
            var dbGroup = await Db.tbGrupos.FindAsync(id);
            if (dbGroup is null) return Content(HttpStatusCode.NotFound,"Grupo no encontrado");

            Db.tbGrupos.Remove(dbGroup);
            await Db.SaveChangesAsync();
            return Ok(await Db.tbGrupos.ToListAsync());
        }
        #endregion


        #region Alumno
        [HttpGet]
        public async Task<IHttpActionResult> ObtenerGruposMateriasAlumno(int alumnoId)
        {
            try
            {
                var lsGruposAlumnosId = await Db.tbAlumnosGrupos.Where(a => a.AlumnoId == alumnoId).Select(a => a.GrupoId).ToListAsync();

                var lsGrupos = await Db.tbGrupos.Where(a => lsGruposAlumnosId.Contains(a.GrupoId)).ToListAsync();

                var listaGruposMaterias = new List<object>();
                foreach (var grupo in lsGrupos)
                {
                    var lsMateriasGrupoId = await Db.tbGruposMaterias.Where(a => a.GrupoId == grupo.GrupoId).Select(a => a.MateriaId).ToListAsync();


                    var lsMaterias = await Db.tbMaterias.Where(a => lsMateriasGrupoId.Contains(a.MateriaId)).Select(m => new
                    {
                        m.MateriaId,
                        m.NombreMateria,
                        m.Descripcion,
                        actividades = Db.tbActividades.Where(a => a.MateriaId == m.MateriaId).ToList()
                    }).ToListAsync();


                    listaGruposMaterias.Add(new
                    {
                        grupoId = grupo.GrupoId,
                        nombreGrupo = grupo.NombreGrupo,
                        descripcion = grupo.Descripcion,
                        //codigoAcceso = grupo.CodigoAcceso,
                        codigoColor = grupo.CodigoColor,
                        materias = lsMaterias
                    });
                }


                return Ok(listaGruposMaterias);

            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest,new
                {
                    e.Message
                });
            }
        }


        #endregion









        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }

                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
