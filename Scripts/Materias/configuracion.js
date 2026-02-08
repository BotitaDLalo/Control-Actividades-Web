//Funcion para editar nombre y descripcion de una materia.
async function cargarMateriaEditar() {
    const params = new URLSearchParams(window.location.search);
    const materiaId = params.get("materiaId");

    if (!materiaId) return;

    try {
        const resp = await fetch(`/Materias/ObtenerMateriaEditar?materiaId=${materiaId}`);

        if (!resp.ok) {
            console.error("No se pudo obtener la materia");
            return;
        }

        const data = await resp.json();

        document.getElementById("configNombre").value = data.NombreMateria || "";
        document.getElementById("configDescripcion").value = data.Descripcion || "";

    } catch (error) {
        console.error("Error cargando materia:", error);
    }
}


async function guardarConfig() {
    const btn = document.getElementById("btnGuardarMateriaEditada");
    if (!btn) return;

    const materiaId = window.materiaIdGlobal;

    if (!materiaId) return;

    const nombre = document.getElementById("configNombre").value.trim();
    const descripcion = document.getElementById("configDescripcion").value.trim();

    if (!nombre) {
        Swal.fire({
            title: "El nombre no puede estar vacío",
            text: "Escribe un nombre para la materia",
            icon: "warning"
        });
        return;
    }

    btn.disabled = true;
    btn.innerText = "Guardando...";

    const bodyData = {
        NombreMateria: nombre,
        Descripcion: descripcion
    };

    try {
        const resp = await fetch(
            `/Materias/ActualizarMateria?materiaId=${materiaId}`,
            {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(bodyData),
            }
        );

        if (resp.ok) {
            const resJson = await resp.json();

            $("#materia-nombre").text(resJson.NombreMateria);

            document.getElementById("materiaNombre").textContent = resJson.NombreMateria;

            Swal.fire({
                position: "top-end",
                icon: "success",
                title: "Materia actualizada",
                showConfirmButton: false,
                timer: 1500,
            });
            cargarMateriaEditar();
            return true;
        } else {
            Swal.fire("Error", "No se pudo guardar", "error");
            return false;
        }
    } catch (e) {
        console.error(e);
        Swal.fire("Error", "No se pudo guardar", "error");
        return false;
    } finally {
        btn.disabled = false;
        btn.innerText = "Guardar";
    }
}

async function eliminarMateria() {
    const params = new URLSearchParams(window.location.search);

    const materiaId = params.get("materiaId");
    if (!materiaId) return;

    if (!confirm("¿Eliminar esta materia? Esta acción no se puede deshacer.")) return;

    try {
        const resp = await fetch(`/Materias/EliminarMateria?id=${materiaId}`, {
            method: "DELETE",
        });

        if (resp.ok) {
            Swal.fire(
                {
                    icon: 'success',
                    title: 'Materia eliminada',
                    showConfirmButton: false,
                    timer: 1500
                });
        
            window.location.href = '/Docente/Index';
        } else {
            const txt = await resp.text();
            Swal.fire('Error', txt || 'No se pudo eliminar la materia', 'error');
        }
    } catch (e) {
        console.error(e);
        Swal.fire("Error", "No se pudo eliminar", "error");
    }
}

//document.addEventListener("DOMContentLoaded", cargarMateriaEditar);
