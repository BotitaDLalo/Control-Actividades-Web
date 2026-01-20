using ControlActividades.Dtos.Migracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlActividades.Services
{
    public interface IMigracionUsuariosService
    {
        Task<MigracionResultado> MigrarUsuariosAsync(List<UsuarioMigracionDto> usuarios);
    }
}
