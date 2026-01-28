# 🧪 PRUEBAS PARA VALIDAR LA SOLUCIÓN

## 📋 Plan de Pruebas

### Escenario: Eliminar un Alumno de una Materia

---

## ✅ Prueba 1: Verificar Eliminación Exitosa

### Setup
- Asegúrate de tener un alumno registrado en una materia
- Ejemplo: Alumno ID=5, Materia ID=3, AlumnoMateriaId=42

### Paso 1: Obtener lista de alumnos ANTES
```bash
POST http://localhost:5000/Materias/ObtenerAlumnosPorMateria
Content-Type: application/json

{
  "materiaId": 3
}
```

**Esperado:**
```json
{
  "alumnos": [
    {
      "alumnoMateriaId": 42,
      "alumnoId": 5,
      "nombre": "Juan",
      "apellidoPaterno": "Pérez",
      "apellidoMaterno": "Gómez",
      "email": "juan@email.com",
      "userName": "juan"
    },
    // ... otros alumnos
  ]
}
```

✅ **Confirma:** Ves al alumno en la lista

---

### Paso 2: Eliminar el alumno
```bash
POST http://localhost:5000/api/Alumnos/EliminarAlumnoMateria
Content-Type: application/json

{
  "AlumnoMateriaId": 42
}
```

**Esperado:**
```json
{
  "mensaje": "El alumno ha sido desinscrito de la materia correctamente.",
  "codigo": "EXITO",
  "datos": {
    "alumnoId": 5,
    "materiaId": 3
  }
}
```

✅ **Confirma:** Respuesta 200 OK

---

### Paso 3: Verificar que NO reaparece
```bash
POST http://localhost:5000/Materias/ObtenerAlumnosPorMateria
Content-Type: application/json

{
  "materiaId": 3
}
```

**Esperado:**
```json
{
  "alumnos": [
    // ❌ Juan NO debe estar aquí
  ]
}
```

✅ **Confirma:** El alumno desapareció de la lista (¡diferencia con el bug anterior!)

---

### Paso 4: Intentar eliminar de nuevo
```bash
POST http://localhost:5000/api/Alumnos/EliminarAlumnoMateria
Content-Type: application/json

{
  "AlumnoMateriaId": 42
}
```

**Esperado:**
```json
{
  "mensaje": "La inscripción alumno-materia no existe.",
  "codigo": "ALUMNO_NO_ENCONTRADO",
  "detalles": "No se encontró una inscripción con AlumnoMateriaId: 42"
}
```

✅ **Confirma:** Respuesta 404 Not Found (consistencia lógica)

---

## 🧪 Prueba 2: Eliminar de Grupo

### Setup
- Alumno ID=5, Grupo ID=2, AlumnoGrupoId=28

### Pasos Idénticos a Prueba 1, pero:
```bash
# Paso 1: Obtener alumnos del grupo
POST http://localhost:5000/Materias/ObtenerAlumnosPorGrupo
Content-Type: application/json

{
  "grupoId": 2
}
```

```bash
# Paso 2: Eliminar
POST http://localhost:5000/api/Alumnos/EliminarAlumnoGrupo
Content-Type: application/json

{
  "AlumnoGrupoId": 28
}
```

```bash
# Paso 3: Verificar NO reaparece
POST http://localhost:5000/Materias/ObtenerAlumnosPorGrupo
Content-Type: application/json

{
  "grupoId": 2
}
```

```bash
# Paso 4: Reintentar
POST http://localhost:5000/api/Alumnos/EliminarAlumnoGrupo
Content-Type: application/json

{
  "AlumnoGrupoId": 28
}
# Esperado: 404 Not Found
```

---

## 📊 Verificación en Base de Datos

### Opción 1: SQL Server Management Studio

```sql
-- Verificar que el alumno se eliminó realmente
SELECT * 
FROM tbAlumnosMaterias 
WHERE AlumnoMateriaId = 42 AND AlumnoId = 5 AND MateriaId = 3;

-- Resultado esperado: (0 rows affected)
```

```sql
-- Verificar que otros alumnos aún existen
SELECT * 
FROM tbAlumnosMaterias 
WHERE MateriaId = 3;

-- Resultado esperado: Otros alumnos listados (sin el 42)
```

### Opción 2: Visual Studio Data Connection

1. View → Server Explorer
2. Data Connections → [Tu BD]
3. Tables → dbo.tbAlumnosMaterias
4. Right-click → Show Table Data
5. Busca AlumnoMateriaId = 42 → No debe estar

---

## 🔍 Verificar Logs

### En Visual Studio Output Window

Después de cada eliminación, deberías ver:

```
[LOG] Alumno 5 desinscrito de materia 3. Contexto limpiado.
```

❌ **Si ves:**
```
[LOG] Alumno 5 desinscrito de materia 3
# (sin "Contexto limpiado")
```
→ El código viejo se ejecutó, no el nuevo

---

## 🚀 Prueba desde Flutter

### Flujo completo en la app

```dart
// 1. Cargar alumnos de materia
await cargarAlumnosDeMateria(3);
// Resultado: [Juan, María, ...]

// 2. Eliminar alumno
await eliminarAlumnoDeMateria(42);
// Respuesta: 200 OK

// 3. Recargar lista
await cargarAlumnosDeMateria(3);
// Resultado: [María, ...] ← ¡Juan desapareció!

// 4. Reintentar eliminar
await eliminarAlumnoDeMateria(42);
// Error: 404 Not Found ← Comportamiento esperado
```

---

## 📋 Checklist de Validación

### ✅ Verificaciones Obligatorias

- [ ] **Paso 1:** Alumno existe en lista de materia
- [ ] **Paso 2:** Eliminación retorna 200 OK
- [ ] **Paso 3:** Alumno NO aparece en lista después (★ Diferencia clave)
- [ ] **Paso 4:** Reintentar devuelve 404
- [ ] **BD:** Verifica directamente que el registro se eliminó
- [ ] **Logs:** Ves mensaje "Contexto limpiado"

### ✅ Pruebas Adicionales

- [ ] Prueba con múltiples alumnos (no afecta otros)
- [ ] Prueba con ambos endpoints (EliminarAlumnoMateria y EliminarAlumnoGrupo)
- [ ] Prueba desde Flutter después de compilar
- [ ] Prueba en diferentes máquinas si es aplicable

---

## 🐛 Troubleshooting

### Problema: Alumno aún reaparece después de eliminar

**Solución 1:** Verifica que la dll compilada se actualizó
```bash
# En VS, limpia y recompila
Build → Clean Solution
Build → Build Solution
```

**Solución 2:** Verifica que se redeployó la app
```bash
# Si usas IIS/servidor:
1. Detén la app
2. Reemplaza los archivos
3. Inicia la app
```

**Solución 3:** Verifica los logs
```
¿Ves "Contexto limpiado" en Output?
├─ SÍ → El código se ejecutó, pero hay otro problema
└─ NO → El código viejo aún se ejecuta
```

---

### Problema: Error 404 en primer intento

**Causa probable:** AlumnoMateriaId no existe

**Solución:**
1. Verifica que el alumnoMateriaId es correcto
2. Consulta la BD: `SELECT AlumnoMateriaId FROM tbAlumnosMaterias WHERE AlumnoId = 5 AND MateriaId = 3`
3. Usa el ID real en la prueba

---

### Problema: Error 500 "NullReferenceException"

**Causa probable:** Faltan usando statements

**Solución:** Verifica que al principio del archivo están todos los using:
```csharp
using System;
using System.Linq;           // ← Necesario para .Where()
using System.Collections.Generic;
// ... etc
```

---

## 📞 Reportar Resultados

Si algo falla, proporciona:

1. **Respuesta HTTP completa** (incluyendo headers y status code)
2. **Logs de Visual Studio** (Output window)
3. **Query de BD** confirmando si el registro existe o no
4. **Steps para reproducir** exactamente lo que hiciste

---

## ✨ Caso de Éxito Esperado

```
Antes del fix:
1. Eliminar ✅ → 200 OK
2. Recargar ✅ → Alumno aparece (❌ BUG)
3. Eliminar ✅ → 200 OK (❌ No debe pasar)

Después del fix:
1. Eliminar ✅ → 200 OK
2. Recargar ✅ → Alumno NO aparece (✅ CORRECTO)
3. Eliminar ❌ → 404 Not Found (✅ CORRECTO)
```

---

## 📝 Template para documentar resultados

```markdown
# Validación de Fix - [Fecha]

## Prueba 1: Eliminar Alumno de Materia
- [ ] Paso 1 (obtener antes): ✅ PASS / ❌ FAIL
- [ ] Paso 2 (eliminar): ✅ PASS / ❌ FAIL
- [ ] Paso 3 (verificar desaparición): ✅ PASS / ❌ FAIL
- [ ] Paso 4 (reintentar): ✅ PASS / ❌ FAIL

## Prueba 2: Eliminar Alumno de Grupo
- [ ] Similar a Prueba 1: ✅ PASS / ❌ FAIL

## Prueba 3: Verificación en BD
- [ ] Registro eliminado realmente: ✅ PASS / ❌ FAIL

## Conclusión
- ✅ El fix funciona correctamente
- ⚠️ Funcionamiento parcial (describe)
- ❌ Aún hay problemas (describe)

Notas: ...
```

