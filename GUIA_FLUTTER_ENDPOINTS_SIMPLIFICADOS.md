# 🚀 GUÍA FLUTTER - Endpoints Simplificados

## 📍 Endpoints Disponibles

### 1. Eliminar de Materia
```
POST http://tu-servidor/api/Alumnos/EliminarAlumnoMateria
```

**Body (JSON):**
```json
{
  "MateriaId": 5,
  "AlumnoId": 123
}
```

**Respuesta Exitosa (200 OK):**
```json
{
  "mensaje": "El alumno ha sido desinscrito de la materia correctamente.",
  "codigo": "EXITO",
  "datos": {
    "alumnoId": 123,
    "materiaId": 5
  }
}
```

**Error - No existe (404):**
```json
{
  "mensaje": "El alumno no está inscrito en esta materia.",
  "codigo": "ALUMNO_NO_ENCONTRADO",
  "detalles": "No se encontró una inscripción del alumno 123 en la materia 5."
}
```

---

### 2. Eliminar de Grupo
```
POST http://tu-servidor/api/Alumnos/EliminarAlumnoGrupo
```

**Body (JSON):**
```json
{
  "GrupoId": 3,
  "AlumnoId": 123
}
```

**Respuesta Exitosa (200 OK):**
```json
{
  "mensaje": "El alumno ha sido desinscrito del grupo correctamente.",
  "codigo": "EXITO",
  "datos": {
    "alumnoId": 123,
    "grupoId": 3
  }
}
```

---

### 3. Eliminar de Grupo (Alternativa)
```
POST http://tu-servidor/api/Alumnos/EliminarAlumnoDelGrupo
```

**Idéntico al endpoint 2** (mismo comportamiento, ruta diferente)

---

## 💻 Código Flutter Completo

### Servicio de Eliminación

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class AlumnoService {
  static const String baseUrl = 'http://192.168.0.9:5000'; // Cambiar por tu servidor

  /// Eliminar alumno de una materia
  /// Parámetros:
  ///   - materiaId: ID de la materia
  ///   - alumnoId: ID del alumno
  /// Retorna: true si fue exitoso
  static Future<bool> eliminarAlumnoDeMateria({
    required int materiaId,
    required int alumnoId,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/api/Alumnos/EliminarAlumnoMateria'),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
        body: jsonEncode({
          'MateriaId': materiaId,
          'AlumnoId': alumnoId,
        }),
      );

      print('[DEBUG] Eliminar Materia - Status: ${response.statusCode}');
      print('[DEBUG] Response: ${response.body}');

      if (response.statusCode == 200) {
        print('✅ Alumno $alumnoId eliminado de materia $materiaId');
        return true;
      } else if (response.statusCode == 404) {
        print('⚠️ La relación no existe');
        return false;
      } else {
        print('❌ Error ${response.statusCode}: ${response.body}');
        return false;
      }
    } catch (e) {
      print('❌ Exception: $e');
      return false;
    }
  }

  /// Eliminar alumno de un grupo
  /// Parámetros:
  ///   - grupoId: ID del grupo
  ///   - alumnoId: ID del alumno
  /// Retorna: true si fue exitoso
  static Future<bool> eliminarAlumnoDelGrupo({
    required int grupoId,
    required int alumnoId,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/api/Alumnos/EliminarAlumnoGrupo'),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
        body: jsonEncode({
          'GrupoId': grupoId,
          'AlumnoId': alumnoId,
        }),
      );

      print('[DEBUG] Eliminar Grupo - Status: ${response.statusCode}');
      print('[DEBUG] Response: ${response.body}');

      if (response.statusCode == 200) {
        print('✅ Alumno $alumnoId eliminado del grupo $grupoId');
        return true;
      } else if (response.statusCode == 404) {
        print('⚠️ La relación no existe');
        return false;
      } else {
        print('❌ Error ${response.statusCode}: ${response.body}');
        return false;
      }
    } catch (e) {
      print('❌ Exception: $e');
      return false;
    }
  }
}
```

---

## 🎨 UI Component - Botón de Eliminación

```dart
import 'package:flutter/material.dart';

class EliminarAlumnoButton extends StatefulWidget {
  final int materiaId;
  final int alumnoId;
  final String alumnoNombre;
  final Function() onSuccess; // Callback cuando se elimina exitosamente

  const EliminarAlumnoButton({
    required this.materiaId,
    required this.alumnoId,
    required this.alumnoNombre,
    required this.onSuccess,
  });

  @override
  State<EliminarAlumnoButton> createState() => _EliminarAlumnoButtonState();
}

class _EliminarAlumnoButtonState extends State<EliminarAlumnoButton> {
  bool _isLoading = false;

  void _confirmarEliminar() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('Confirmar eliminación'),
        content: Text(
          '¿Eliminar a ${widget.alumnoNombre} de esta materia?\n\n'
          'Esta acción no se puede deshacer.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              _eliminar();
            },
            child: Text('Eliminar', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
  }

  Future<void> _eliminar() async {
    setState(() => _isLoading = true);

    try {
      final success = await AlumnoService.eliminarAlumnoDeMateria(
        materiaId: widget.materiaId,
        alumnoId: widget.alumnoId,
      );

      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Alumno eliminado exitosamente'),
            backgroundColor: Colors.green,
            duration: Duration(seconds: 2),
          ),
        );
        
        // Llamar callback para actualizar la lista
        widget.onSuccess();
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('No se pudo eliminar al alumno'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } finally {
      setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return ElevatedButton.icon(
      onPressed: _isLoading ? null : _confirmarEliminar,
      icon: _isLoading ? SizedBox(
        width: 20,
        height: 20,
        child: CircularProgressIndicator(strokeWidth: 2),
      ) : Icon(Icons.delete),
      label: Text(_isLoading ? 'Eliminando...' : 'Eliminar'),
      style: ElevatedButton.styleFrom(
        backgroundColor: Colors.red,
        foregroundColor: Colors.white,
      ),
    );
  }
}
```

---

## 📋 Uso en ListView

```dart
ListView.builder(
  itemCount: alumnos.length,
  itemBuilder: (context, index) {
    final alumno = alumnos[index];
    
    return ListTile(
      title: Text(alumno.nombre),
      subtitle: Text(alumno.email),
      trailing: EliminarAlumnoButton(
        materiaId: materiaId,
        alumnoId: alumno.alumnoId,
        alumnoNombre: alumno.nombre,
        onSuccess: () {
          // Actualizar lista de alumnos
          setState(() {
            alumnos.removeAt(index);
          });
        },
      ),
    );
  },
)
```

---

## 🔄 Provider State Management

```dart
class AlumnoProvider extends ChangeNotifier {
  List<Alumno> alumnos = [];
  bool isLoading = false;

  Future<void> cargarAlumnos(int materiaId) async {
    isLoading = true;
    notifyListeners();
    
    try {
      // Fetch alumnos...
      alumnos = await AlumnoService.obtenerAlumnos(materiaId);
    } finally {
      isLoading = false;
      notifyListeners();
    }
  }

  Future<void> eliminarAlumno(int materiaId, int alumnoId) async {
    try {
      final success = await AlumnoService.eliminarAlumnoDeMateria(
        materiaId: materiaId,
        alumnoId: alumnoId,
      );

      if (success) {
        // Eliminar de la lista local
        alumnos.removeWhere((a) => a.alumnoId == alumnoId);
        notifyListeners();
      }
    } catch (e) {
      print('Error: $e');
    }
  }
}
```

---

## ✅ Checklist Flutter

- [ ] Cambié baseUrl a mi servidor
- [ ] Estoy enviando MateriaId y AlumnoId
- [ ] Verifico el status code (200, 404, etc)
- [ ] Refresco la lista después de eliminar
- [ ] Muestro mensajes de error al usuario
- [ ] Tengo confirmación antes de eliminar

---

## 🧪 Prueba Manual con cURL

```bash
# Eliminar de materia
curl -X POST http://localhost:5000/api/Alumnos/EliminarAlumnoMateria \
  -H "Content-Type: application/json" \
  -d '{"MateriaId": 5, "AlumnoId": 123}'

# Eliminar de grupo
curl -X POST http://localhost:5000/api/Alumnos/EliminarAlumnoGrupo \
  -H "Content-Type: application/json" \
  -d '{"GrupoId": 3, "AlumnoId": 123}'
```

---

## 📊 Respuestas HTTP

| Status | Significado | Acción Flutter |
|--------|------------|----------------|
| 200 | Eliminado ✅ | Actualizar lista |
| 400 | Datos inválidos ❌ | Mostrar error |
| 404 | No existe ❌ | Mostrar "no encontrado" |
| 500 | Error servidor ❌ | Mostrar error genérico |

