using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Materias;
using ControlActividades.Models;

namespace ControlActividades.Services.Materias
{
    public class MateriasSTService : IMateriasService
    {
        public Task<MateriaViewModel> ObtenerMateriaDetalles(int materiaId, int docenteId)
        {
            throw new NotImplementedException();
        }

        public List<MateriaViewModel> ObtenerMateriasSinGrupoPorUsuario(int usuarioId, string role)
        {
            throw new NotImplementedException();
        }

        public Task<ActividadRes> CrearActividadAsync(CrearActividad actividad)
        {
            throw new NotImplementedException();
        }
    }
}