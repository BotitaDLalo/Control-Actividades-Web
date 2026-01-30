# 🎯 Guía Flutter: Desvinculación de Alumnos - CORREGIDO ✅

## 🔴 Problema Identificado

Flutter estaba enviando:
```json
{
  "AlumnoMateriaId": 0
}
```

Pero el backend esperaba:
```json
{
  "MateriaId": 5,
  "AlumnoId": 123
}
```

## ✅ Solución Implementada

El backend ahora **acepta ambos formatos automáticamente**:
- Si recibe `AlumnoMateriaId`, lo busca en la BD y obtiene los IDs individuales
- Si recibe `MateriaId` + `AlumnoId`, los usa directamente

---

## 📱 Métodos de Desvinculación de Alumnos

### 1. Eliminar Alumno de Materia
**Ruta:** `POST /api/Alumnos/EliminarAlumnoMateria`

#### ⭐ Opción Recomendada: Usando AlumnoMateriaId
```json
{
  "AlumnoMateriaId": 42
}
```

**Código Flutter:**
```dart
Future<void> eliminarAlumnoDeMateria(int alumnoMateriaId) async {
  try {
    final response = await http.post(
      Uri.parse('http://192.168.0.9:5000/api/Alumnos/EliminarAlumnoMateria'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'AlumnoMateriaId': alumnoMateriaId}),
    );

    if (response.statusCode == 200) {
      print('✅ Alumno eliminado correctamente');
      // Recargar lista
    } else if (response.statusCode == 409) {
      final error = jsonDecode(response.body);
      print('⚠️ ${error['detalles']}');
    }
  } catch (e) {
    print('❌ Error: $e');
  }
}
```

#### Alternativa: Usando MateriaId + AlumnoId
```json
{
  "MateriaId": 5,
  "AlumnoId": 123
}
```

---

### 2. Eliminar Alumno de Grupo (Ruta 1)
**Ruta:** `POST /api/Alumnos/EliminarAlumnoGrupo`

#### ⭐ Opción Recomendada: Usando AlumnoGrupoId
```json
{
  "AlumnoGrupoId": 28
}
```

**Código Flutter:**
```dart
Future<void> eliminarAlumnoDelGrupo(int alumnoGrupoId) async {
  try {
    final response = await http.post(
      Uri.parse('http://192.168.0.9:5000/api/Alumnos/EliminarAlumnoGrupo'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'AlumnoGrupoId': alumnoGrupoId}),
    );

    if (response.statusCode == 200) {
      print('✅ Alumno eliminado del grupo');
    } else {
      print('❌ Error: ${response.statusCode}');
    }
  } catch (e) {
    print('❌ Error: $e');
  }
}
```

#### Alternativa: Usando GrupoId + AlumnoId
```json
{
  "GrupoId": 3,
  "AlumnoId": 123
}
```

---

### 3. Eliminar Alumno del Grupo (Ruta 2 - Alternativa)
**Ruta:** `POST /api/Alumnos/EliminarAlumnoDelGrupo`

Soporta los mismos formatos que la ruta anterior.

---

## 📊 Respuestas del API

### ✅ Éxito (200):
```json
{
  "mensaje": "El alumno ha sido desinscrito correctamente.",
  "codigo": "EXITO",
  "datos": {"alumnoId": 123, "materiaId": 5}
}
```

### ⚠️ Entregas Pendientes (409):
```json
{
  "mensaje": "No se puede desincribir porque tiene entregas.",
  "codigo": "ALUMNO_CON_ENTREGAS",
  "detalles": "El alumno tiene 2 entrega(s) entregada(s)..."
}
```

### ❌ Datos Inválidos (400):
```json
{
  "mensaje": "Los datos enviados son inválidos.",
  "codigo": "ERROR_INTERNO",
  "detalles": "Los IDs deben ser mayores a 0. AlumnoMateriaId: 0"
}
```

### ❌ No Encontrado (404):
```json
{
  "mensaje": "La inscripción no existe.",
  "codigo": "ALUMNO_NO_ENCONTRADO",
  "detalles": "No se encontró AlumnoMateriaId: 42"
}
```

---

## 🛠️ Checklist para Flutter

- ✅ Usa `AlumnoMateriaId` o `AlumnoGrupoId` en lugar de IDs individuales
- ✅ Content-Type debe ser `application/json`
- ✅ Body debe estar serializado con `jsonEncode()`
- ✅ Maneja status 409 para conflictos (entregas pendientes)
- ✅ Refresca la lista después de eliminar exitosamente

---

## 📝 Estructura de Datos Esperada en Flutter

En tu modelo de alumno probablemente tienes:

```dart
class AlumnoEnMateria {
  final int alumnoMateriaId;  // ← Usa ESTE valor
  final int alumnoId;
  final int materiaId;
  final String nombre;
  final String email;
}
```

---

## 🎯 Resumen Final

| Acción | Ruta | Body |
|--------|------|------|
| Eliminar de materia | `/EliminarAlumnoMateria` | `{"AlumnoMateriaId": 42}` |
| Eliminar de grupo 1 | `/EliminarAlumnoGrupo` | `{"AlumnoGrupoId": 28}` |
| Eliminar de grupo 2 | `/EliminarAlumnoDelGrupo` | `{"AlumnoGrupoId": 28}` |

**Todos funcionan con ambos formatos (ID de relación o IDs individuales).**

---

## ✅ Cambios en el Backend

Los métodos `EliminarAlumnoDeMateria` y `EliminarAlumnoDeGrupo` ahora:
- ✅ Aceptan `dynamic` en lugar de tipos específicos
- ✅ Soportan `AlumnoMateriaId` / `AlumnoGrupoId` 
- ✅ Buscan automáticamente en BD si reciben ID de relación
- ✅ Validan robustamente con try/catch
- ✅ Proporcionan mensajes de error detallados
