//Funcion para editar nombre y descripcion de una materia.

async function editarMateria(){
    
}

async function eliminarMateria(MateriaId) {
  const confirmacion = await Swal.fire({
    title: "¿Estás seguro?",
    text: "No podrás recuperar esta materia después de eliminarla.",
    icon: "warning",
    showCancelButton: true,
    confirmButtonText: "Sí, eliminar",
    cancelButtonText: "Cancelar",
  });

  if (confirmacion.isConfirmed) {
    try {
      const response = await fetch(`/Materias/EliminarMateria/${MateriaId}`, {
        method: "DELETE",
      });

      const resultado = await response.json();

      if (response.ok) {
        await Swal.fire({
          position: "top-end",
          icon: "success",
          title: resultado.mensaje || "Eliminado.",
          showConfirmButton: false,
          timer: 2000,
        });
        // Se ejecuta funcion inicializar para actualizar vista completa
        if (typeof inicializar === "function") inicializar();
      } else {
        await Swal.fire({
          position: "top-end",
          icon: "error",
          title:
            resultado.mensaje || "No se pudo eliminar el grupo y sus materias.",
          showConfirmButton: false,
          timer: 2000,
        });
      }
    } catch (error) {
      await Swal.fire({
        position: "top-end",
        icon: "error",
        title: "Ocurrio un problema al eliminar la materia.",
        showConfirmButton: false,
        timer: 2000,
      });
    }
  }
}
