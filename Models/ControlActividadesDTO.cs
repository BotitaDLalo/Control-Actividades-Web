using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Models
{
    public class ObtenerGruposPorUsuarioRequest
    {
        public string Role { get; set; }

        public int ca_usuarioId { get; set; }

        public int st_usuarioId { get; set; }

        public string View { get; set; }
    }

    public class ObtenerGrupoCreadoCARequest
    {
        public int ca_usuarioId { get; set; }
        public int st_usuarioId { get; set; }
    }
    
    public class ObtenerMateriasCARequest
    {
        public int ca_usuarioId { get; set; }
        public int st_usuarioId { get; set; }
        public string Role { get; set; }

        public string View { get; set; }
    }

    public class ObtenerMateriaDetallesRequest
    {
        public int MateriaId { get; set; }

        public int GrupoId { get; set; }
        public string View { get; set; }
        public int st_usuarioId { get; set; }

        public string Role { get; set; }
    }


    public class RegistrarAlumnoRequest
    {
        public List<int> AlumnosId { get; set; }

        public int? MateriaId { get; set; }

        public int? GrupoId { get; set; }
    }


    public class ObtenerMateriasPorGrupoRequest
    {
        public string Role { get; set; }

        public int st_usuarioId { get; set; }

        public int ca_usuarioId { get; set; }

        public int GrupoId { get; set; }
    }


    public class TieneClasesRequest
    {
        public string Role { get; set; }

        public int st_usuarioId { get; set; }

        public int ca_usuarioId { get; set; }
    }


    // Actividades
    /// Petición usada para obtener actividades por materia.
    /// Se agregó la propiedad <see cref="EsDocente"/> para compatibilidad
    /// con el servicio ST (ActividadesSTService) que espera ese campo.
    public class ObtenerActividadesPorMateriaRequest
    {
        /// Indica si el solicitante tiene rol de docente/administrador.
        /// Usado por ActividadesSTService para filtrar la respuesta.
        public bool EsDocente { get; set; }

        public string Role { get; set; }

        public int MateriaId { get; set; }
        public string View { get; set; }
    }
}