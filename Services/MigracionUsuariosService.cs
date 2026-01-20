using ControlActividades.Dtos.Migracion;
using ControlActividades.Models;
using ControlActividades.Models.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlActividades.Services
{
    public class MigracionUsuariosService : IMigracionUsuariosService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _context;

        public MigracionUsuariosService(
            ApplicationUserManager userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<MigracionResultado> MigrarUsuariosAsync(List<UsuarioMigracionDto> usuarios)
        {
            var resultado = new MigracionResultado
            {
                TotalRecibidos = usuarios.Count
            };

            const int batchSize = 500;

            for (int i = 0; i < usuarios.Count; i += batchSize)
            {
                var batch = usuarios.Skip(i).Take(batchSize).ToList();
                foreach (var dto in batch)
                {
                    try
                    {
                        // Validar duplicados
                        if (await _userManager.FindByEmailAsync(dto.Correo) != null)
                        {
                            resultado.Fallidos++;
                            resultado.Errores.Add($"Correo duplicado: {dto.Correo}");
                            continue;
                        }

                        // Crear usuario Identity
                        var user = new ApplicationUser
                        {
                            UserName = dto.Correo,
                            Email = dto.Correo,
                            EmailConfirmed = false
                        };

                        var createResult = await _userManager.CreateAsync(user, dto.PasswordPlano);

                        if (!createResult.Succeeded)
                        {
                            resultado.Fallidos++;
                            resultado.Errores.Add($"{dto.Correo}: {string.Join(", ", createResult.Errors)}");
                            continue;
                        }

                        // Asignar rol
                        await _userManager.AddToRoleAsync(user.Id, dto.Rol);

                        // Insertar en tabla correspondiente
                        InsertarDatosPorRol(dto, user.Id);

                        resultado.Insertados++;
                    }
                    catch (Exception ex)
                    {
                        resultado.Fallidos++;
                        resultado.Errores.Add($"{dto.Correo}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
            }

            return resultado;
        }

        private void InsertarDatosPorRol(UsuarioMigracionDto dto, string userId)
        {
            switch (dto.Rol)
            {
                case "Alumno":
                    _context.tbAlumnos.Add(new tbAlumnos
                    {
                        Nombre = dto.Nombre,
                        ApellidoPaterno = dto.ApellidoPaterno,
                        ApellidoMaterno = dto.ApellidoMaterno,
                        UserId = userId
                    });
                    break;

                case "Docente":
                    _context.tbDocentes.Add(new tbDocentes
                    {
                        Nombre = dto.Nombre,
                        ApellidoPaterno = dto.ApellidoPaterno,
                        ApellidoMaterno = dto.ApellidoMaterno,
                        UserId = userId,
                        estaAutorizado = false,
                        seEnvioCorreo = false
                    });
                    break;
            }
        }
    }
}
