# ✅ GUÍA DE CORRECCIÓN FLUTTER - ENVÍO DE IDs CORRECTO

## 🎯 Problema Identificado en Flutter

Flutter está enviando:
```json
{
  "alumnoId": 123
}
```

Pero el backend espera:
```json
{
  "alumnoMateriaId": 42
}
```
O alternativamente:
```json
{
  "MateriaId": 5,
  "AlumnoId": 123
}
```

---

## ✅ SOLUCIÓN: Qué Enviar desde Flutter

### 1. Para Eliminar de Materia
**Ruta:** `POST /api/Alumnos/EliminarAlumnoMateria`

**Opción A - Usa AlumnoMateriaId (RECOMENDADO) ⭐**
```dart
// Desde tu lista de alumnos, encuentra alumnoMateriaId
final alumnoMateriaId = 42;  // <- Este es el ID de la relación

await http.post(
  Uri.parse('http://192.168.0.9:5000/api/Alumnos/EliminarAlumnoMateria'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'AlumnoMateriaId': alumnoMateriaId,  // ← Envía ESTE
  }),
);
```

**Opción B - Usa MateriaId + AlumnoId**
```dart
await http.post(
  Uri.parse('http://192.168.0.9:5000/api/Alumnos/EliminarAlumnoMateria'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'MateriaId': 5,
    'AlumnoId': 123,
  }),
);
```

---

### 2. Para Eliminar de Grupo
**Ruta:** `POST /api/Alumnos/EliminarAlumnoGrupo`

**Opción A - Usa AlumnoGrupoId (RECOMENDADO) ⭐**
```dart
final alumnoGrupoId = 28;  // <- Este es el ID de la relación

await http.post(
  Uri.parse('http://192.168.0.9:5000/api/Alumnos/EliminarAlumnoGrupo'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'AlumnoGrupoId': alumnoGrupoId,  // ← Envía ESTE
  }),
);
```

**Opción B - Usa GrupoId + AlumnoId**
```dart
await http.post(
  Uri.parse('http://192.168.0.9:5000/api/Alumnos/EliminarAlumnoGrupo'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'GrupoId': 3,
    'AlumnoId': 123,
  }),
);
```

---

### 3. Para Eliminar de Grupo (Ruta Alternativa)
**Ruta:** `POST /api/Alumnos/EliminarAlumnoDelGrupo`

Mismo formato que la opción 2.

---

## 📋 Estructura de Datos en Flutter

Tu modelo debería ser así:

```dart
class EstudianteEnMateriaOGrupo {
  final int alumnoId;              // ← ID del alumno
  final int alumnoMateriaId;       // ← ID de la RELACIÓN alumno-materia
  final int alumnoGrupoId;         // ← ID de la RELACIÓN alumno-grupo
  final int materiaId;             // ← ID de la materia
  final int grupoId;               // ← ID del grupo
  final String nombre;
  final String email;
  
  // En el proveedor de eliminación, ENVÍA esto:
  // Para materias: alumnoMateriaId (no alumnoId!)
  // Para grupos: alumnoGrupoId (no alumnoId!)
}
```

---

## 🔧 Código Recomendado para Flutter

```dart
// En tu servicio/proveedor:

// ELIMINAR DE MATERIA
Future<void> eliminarAlumnoDeMateria(int alumnoMateriaId) async {
  if (alumnoMateriaId == 0) {
    throw Exception('alumnoMateriaId no puede ser 0');
  }

  final response = await http.post(
    Uri.parse('$baseUrl/api/Alumnos/EliminarAlumnoMateria'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode({'AlumnoMateriaId': alumnoMateriaId}),
  );

  if (response.statusCode == 200) {
    print('✅ Alumno eliminado de materia');
  } else if (response.statusCode == 409) {
    final error = jsonDecode(response.body);
    throw Exception(error['detalles']);
  } else {
    throw Exception('Error ${response.statusCode}');
  }
}

// ELIMINAR DE GRUPO
Future<void> eliminarAlumnoDelGrupo(int alumnoGrupoId) async {
  if (alumnoGrupoId == 0) {
    throw Exception('alumnoGrupoId no puede ser 0');
  }

  final response = await http.post(
    Uri.parse('$baseUrl/api/Alumnos/EliminarAlumnoGrupo'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode({'AlumnoGrupoId': alumnoGrupoId}),
  );

  if (response.statusCode == 200) {
    print('✅ Alumno eliminado de grupo');
  } else if (response.statusCode == 409) {
    final error = jsonDecode(response.body);
    throw Exception(error['detalles']);
  } else {
    throw Exception('Error ${response.statusCode}');
  }
}
```

---

## ⚠️ IMPORTANTE - Debugging en Flutter

Antes de enviar, verifica que los IDs sean correctos:

```dart
print('[DEBUG ELIMINACIÓN]');
print('alumnoMateriaId: $alumnoMateriaId');
print('alumnoGrupoId: $alumnoGrupoId');
print('alumnoId: $alumnoId');
print('---------------------------');
```

✅ Debe mostrarse:
```
alumnoMateriaId: 42      ← NO CERO
alumnoGrupoId: 28        ← NO CERO
alumnoId: 123
```

❌ Si ves:
```
alumnoMateriaId: 0       ← PROBLEMA
alumnoGrupoId: 0         ← PROBLEMA
```

Entonces necesitas revisar cómo estás obteniendo estos IDs desde tu base de datos local.

---

## 🎯 Resumen Rápido

| Acción | Envía | Backend Recibe |
|--------|-------|-----------------|
| Eliminar de materia | `{"AlumnoMateriaId": 42}` | Lee de BD y obtiene materiaId y alumnoId |
| Eliminar de grupo | `{"AlumnoGrupoId": 28}` | Lee de BD y obtiene grupoId y alumnoId |
| Alternativa materia | `{"MateriaId": 5, "AlumnoId": 123}` | Busca la relación directamente |
| Alternativa grupo | `{"GrupoId": 3, "AlumnoId": 123}` | Busca la relación directamente |

---

## ✅ Backend Aceptará

El backend ahora acepta AMBOS formatos automáticamente:
- ✅ Si envías ID de la relación (alumnoMateriaId/alumnoGrupoId), lo busca en BD
- ✅ Si envías IDs individuales (materiaId/alumnoId), los usa directamente

**La prioridad es:** Usa IDs de relación > IDs individuales
