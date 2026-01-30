# 🔧 SOLUCIÓN: Alumno Reaparece Después de Ser Eliminado

## 🐛 Problema Reportado

**Sintaxis:** "Al eliminar un alumno de una materia/grupo, sale que se eliminó correctamente, pero al volver a entrar a la materia/grupo, sale de nuevo el alumno ahí. Al querer borrarlo de nuevo sale error de que no existe la relación."

---

## 🔍 Causa Raíz

El problema es un **caché en Entity Framework (EF)** que no se limpia después de eliminar:

1. Se elimina el registro de la BD ✅
2. Se devuelve respuesta "Eliminado correctamente" ✅
3. **PERO** el contexto de EF mantiene en memoria la entidad eliminada
4. Las siguientes consultas encuentran el registro en caché (aunque esté eliminado en BD)
5. Cuando Flutter intenta eliminar de nuevo → Error "Relación no existe"

```
Flujo Problemático:
┌─────────────┐
│ Eliminar    │ → Db.Remove() → Db.SaveChanges() → OK respuesta
│             │       ↓
│             │   Contexto EF aún tiene la entidad en memoria
└─────────────┘

┌─────────────┐
│ Recargar    │ → Query busca alumnos → EF devuelve del CACHÉ
│             │       ↓
│             │   Alumno aparece de nuevo (aunque esté eliminado en BD)
└─────────────┘

┌─────────────┐
│ Eliminar    │ → Intenta eliminar pero la BD ya no tiene el registro
│ (2da vez)   │       ↓
│             │   ERROR "Relación no encontrada"
└─────────────┘
```

---

## ✅ SOLUCIÓN Implementada

Se añadió **limpieza agresiva del contexto de EF** después de cada eliminación:

```csharp
// 1. Desconectar la entidad eliminada
Db.Entry(relacionAEliminar).State = EntityState.Detached;

// 2. Eliminar de la BD
Db.tbAlumnosMaterias.Remove(relacionAEliminar);
await Db.SaveChangesAsync();

// 3. Limpiar TODO el caché del DbSet
Db.ChangeTracker.Entries()
    .Where(e => e.Entity is tbAlumnosMaterias)
    .ToList()
    .ForEach(e => e.State = EntityState.Detached);
```

### ¿Por qué funciona?

✅ **Detached state** → EF ya no devuelve el registro en futuras queries  
✅ **ChangeTracker limpio** → Fuerza que las consultas vayan a la BD, no al caché  
✅ **SaveChanges confirmado** → Garantiza que se persistió en la BD

---

## 📋 Métodos Actualizados

Todos estos métodos ahora limpian el contexto:

### 1️⃣ `EliminarAlumnoDeMateria()`
```csharp
[HttpPost]
[Route("EliminarAlumnoMateria")]
public async Task<IHttpActionResult> EliminarAlumnoDeMateria([FromBody] dynamic request)
// → Limpia: DbSet<tbAlumnosMaterias>
```

### 2️⃣ `EliminarAlumnoDeGrupo()`
```csharp
[HttpPost]
[Route("EliminarAlumnoGrupo")]
public async Task<IHttpActionResult> EliminarAlumnoDeGrupo([FromBody] dynamic request)
// → Limpia: DbSet<tbAlumnosGrupos>
```

### 3️⃣ `EliminarAlumnoGrupo()` (ruta alternativa)
```csharp
[HttpPost]
[Route("EliminarAlumnoDelGrupo")]
public async Task<IHttpActionResult> EliminarAlumnoGrupo([FromBody] dynamic request)
// → Limpia: DbSet<tbAlumnosGrupos>
```

---

## 🧪 Cómo Probar

### Paso 1: Eliminar alumno
```bash
POST /api/Alumnos/EliminarAlumnoMateria
{
  "AlumnoMateriaId": 42
}
# Respuesta: 200 OK
```

### Paso 2: Recargar lista de alumnos
```bash
POST /Materias/ObtenerAlumnosPorMateria
{
  "materiaId": 5
}
# ANTES (bug): Alumno aún aparecía ❌
# AHORA (fix): Alumno NO aparece ✅
```

### Paso 3: Intentar eliminar de nuevo
```bash
POST /api/Alumnos/EliminarAlumnoMateria
{
  "AlumnoMateriaId": 42
}
# ANTES (bug): Eliminaba de todas formas ❌
# AHORA (fix): Error 404 "La inscripción no existe" ✅
```

---

## 🎯 Impacto en Flutter

**Sin cambios en Flutter requeridos** ✅

- El frontend Flutter NO necesita cambios
- Ya enviaba los IDs correctamente (tras tus correcciones anteriores)
- El backend ahora devuelve datos consistentes

**Comportamiento esperado:**
1. Flutter elimina alumno → Recibe "Eliminado correctamente"
2. Flutter recarga lista → Alumno NO aparece
3. Flutter intenta eliminar de nuevo → Recibe error "No existe" (como debe ser)

---

## 📊 Registro de Cambios

### Antes
```
[LOG] Alumno 123 desinscrito de materia 5
# Contexto aún contiene la entidad
```

### Después
```
[LOG] Alumno 123 desinscrito de materia 5. Contexto limpiado.
# ChangeTracker desconectó todas las entidades tbAlumnosMaterias
```

---

## ⚠️ Notas Técnicas

**Entity Framework Context Management:**
- `EntityState.Detached` → Entidad no seguida por EF
- `ChangeTracker.Entries()` → Obtiene todas las entidades en caché
- `Where(e => e.Entity is tbAlumnosMaterias)` → Filtra solo el tipo relevante
- `ForEach()` → Asegura que SE EJECUTE la limpieza (no lazy evaluation)

**¿Por qué no se limpió antes?**
- El código original hacía `Remove()` y `SaveChanges()` pero NO desconectaba
- EF mantiene las entidades en `Added/Modified/Deleted` state hasta que se detach
- Las queries posteriores verifican primero el caché (ChangeTracker)

---

## 🚀 Deploy Checklist

- ✅ Backend compilado sin errores
- ✅ Métodos de eliminación actualizados (3 endpoints)
- ✅ Logging mejorado (incluye "Contexto limpiado")
- ✅ Sin cambios requeridos en Flutter
- ✅ BD intacta (no se hizo DELETE/ALTER)

---

## 📞 Si aún hay problemas

Si después de estos cambios el alumno aún reaparece:

1. **Verifica Flutter:**
   - ¿Está cacheando la lista de alumnos?
   - ¿Recarga correctamente después de eliminar?
   - Ejemplo: `setState(() => cargarAlumnos())` después de delete

2. **Verifica BD:**
   - Conecta a SQL Server directamente
   - Ejecuta: `SELECT * FROM tbAlumnosMaterias WHERE AlumnoId = 123`
   - ¿El registro realmente se eliminó?

3. **Verificar logs:**
   - Busca en Output window: `"Contexto limpiado"`
   - Si no aparece → El código no se ejecutó

4. **Nuclear option (último recurso):**
   ```csharp
   // Crear nuevo DbContext en cada operación
   using (var db = new ApplicationDbContext()) 
   {
       // Hacer operación
   } // DbContext se destruye → Sin caché
   ```

