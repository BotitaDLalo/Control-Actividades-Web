//Funcion para editar nombre y descripcion de una materia.

const btnGuardarNombre = document.getElementById("btnGuardarNombre");
const btnGuardarDescripcion = document.getElementById("btnGuardarDescripcion");

async function guardarConfig() {
  const params = new URLSearchParams(window.location.search);

  const materiaId = params.get("materiaId");
  if (!materiaId) return;

  const nombre = document.getElementById("configNombre").value.trim();
  const descripcion = document.getElementById("configDescripcion").value.trim();

  try {
    const resp = await fetch(
      `/Materias/ActualizarMateria?materiaId=${materiaId}`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          NombreMateria: nombre,
          Descripcion: descripcion,
        }),
      }
    );

    if (resp.ok) {
      const resJson = await resp.json();

      console.log(resJson.NombreMateria);
      $("#materia-nombre").text(resJson.NombreMateria);

      Swal.fire({
        position: "top-end",
        icon: "success",
        title: "Guardado",
        showConfirmButton: false,
        timer: 1500,
      });
      return true;
    } else {
      Swal.fire("Error", "No se pudo guardar", "error");
      return false;
    }
  } catch (e) {
    console.error(e);
    Swal.fire("Error", "No se pudo guardar", "error");
    return false;
  }
}

async function eliminarMateria() {
  const params = new URLSearchParams(window.location.search);

  const materiaId = params.get("materiaId");
  if (!materiaId) return;

  if (!confirm("¿Eliminar esta materia? Esta acción no se puede deshacer."))
    return;
  try {
    const resp = await fetch(`/Materias/EliminarMateria?id=${materiaId}`, {
      method: "DELETE",
    });

    const respJson = await resp.json();

    if (respJson.success) {
      // Swal.fire({
      //   icon: "success",
      //   title: "Materia eliminada",
      //   showConfirmButton: false,
      //   timer: 1500,
      // });
      window.location.href = "/Grupos/Index";
      // redirect to grupos overview
    } else {
      // const txt = await resp.text();
      const txt = respJson.mensaje;
      Swal.fire("Error", txt || "No se pudo eliminar la materia", "error");
    }
  } catch (e) {
    console.error(e);
    Swal.fire("Error", "No se pudo eliminar", "error");
  }
}
