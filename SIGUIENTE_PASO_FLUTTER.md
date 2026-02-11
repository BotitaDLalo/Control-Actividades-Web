# 🔄 SIGUIENTE PASO: ACTUALIZAR FLUTTER

## ¿Qué hacer ahora en Flutter?

Ya que el backend está adaptado, necesitas hacer estos cambios para que Flutter use el nuevo endpoint.

---

## 1️⃣ Cambio en activity_data_source_impl.dart

### Cambiar la URL del endpoint

**ANTES (línea ~119):**
```dart
final uri = '${baseUrl.replaceAll('/api/', '')}/Alumnos/RegistrarEnvioActividadAlumno';
```

**DESPUÉS:**
```dart
final uri = '${baseUrl.replaceAll('/api/', '')}/Alumnos/RegistrarEnvioActividadAlumnoConEnlaces';
```

---

## 2️⃣ Adaptación para Multipart (Cuando tengas archivos)

En el futuro, cuando agregues soporte para archivos, cambiarás a:

```dart
// FUTURO: Cuando implementes archivos
final formData = FormData();
formData.fields.addAll([
  MapEntry('ActividadId', activityId.toString()),
  MapEntry('AlumnoId', id.toString()),
  MapEntry('Respuesta', answer),
  MapEntry('Enlaces', jsonEncode(links)),  // Lista de enlaces
  MapEntry('FechaEntrega', dateNow.toString()),
  MapEntry('TipoEntregaId', '1'),
]);

// Agregar archivos si existen
if (files.isNotEmpty) {
  for (var file in files) {
    formData.files.add(
      MapEntry(
        'files',
        await MultipartFile.fromFile(file.path),
      ),
    );
  }
}

final response = await dio.post(uri, data: formData);
```

---

## 3️⃣ Parsear la Respuesta Actualizada

La respuesta ahora incluye estructura JSON en el campo `Contenido`. Para extraerla:

```dart
// En activity_state_notifier.dart o donde proceses la respuesta

import 'dart:convert';

Future<void> sendSubmissionWithLinks(
  int activityId, 
  String answer, 
  List<String> links
) async {
  try {
    final response = await dataSource.submitActivityResponse(
      activityId: activityId,
      answer: answer,
      links: links,
    );
    
    // La respuesta es un List<dynamic>
    if (response.isNotEmpty) {
      // Procesar cada entrega
      for (var entrega in response) {
        // El contenido ahora es JSON
        final contenido = jsonDecode(entrega['Contenido']);
        
        // Acceder a partes específicas
        final texto = contenido['texto'];
        final enlaces = List<String>.from(contenido['enlaces']);
        final archivos = contenido['archivos']; // Lista de metadata
        final tipoEntrega = entrega['TipoEntrega']; // 1-4
        
        print('Entrega enviada:');
        print('  - Texto: $texto');
        print('  - Enlaces: ${enlaces.length}');
        print('  - Archivos: ${archivos.length}');
      }
    }
  } catch (e) {
    print('Error al enviar: $e');
  }
}
```

---

## 4️⃣ Entender los Tipos de Entrega

El backend ahora determina automáticamente el tipo:

| Tipo | Valor | Descripción |
|------|-------|-------------|
| **Texto** | 1 | Solo respuesta de texto |
| **Enlace** | 2 | Solo enlaces |
| **Archivo** | 3 | Solo archivos |
| **Mixto** | 4 | Texto + enlaces + archivos |

```dart
// Ejemplo: Ver qué tipo de entrega es
final tipoEntrega = response[0]['TipoEntrega'];

switch(tipoEntrega) {
  case 1:
    print('📝 Entrega de texto');
    break;
  case 2:
    print('🔗 Entrega con enlaces');
    break;
  case 3:
    print('📎 Entrega con archivos');
    break;
  case 4:
    print('🎯 Entrega completa (texto + enlaces + archivos)');
    break;
}
```

---

## 5️⃣ Validaciones en Flutter

El backend ahora valida, pero puedes agregar en Flutter también:

```dart
// En tu widget o notifier
bool _validarDatos(String respuesta, List<String> enlaces, List<File> archivos) {
  // Al menos uno de estos debe estar presente
  if (respuesta.isEmpty && enlaces.isEmpty && archivos.isEmpty) {
    return false; // Error: entrega vacía
  }
  
  // Validar que los enlaces sean válidos
  for (var enlace in enlaces) {
    if (!enlace.startsWith('http://') && !enlace.startsWith('https://')) {
      return false; // Error: URL inválida
    }
  }
  
  // Validar tamaño total de archivos
  long totalSize = 0;
  for (var file in archivos) {
    totalSize += file.lengthSync();
  }
  
  if (totalSize > 200 * 1024 * 1024) {
    return false; // Error: Total > 200MB
  }
  
  return true;
}
```

---

## 6️⃣ Actualizar Models si es necesario

Si tienes un modelo `Submission` o similar, puedes actualizarlo:

```dart
class Submission {
  final int actividadId;
  final int alumnoId;
  final String respuesta;
  final List<String> enlaces;        // ✅ Nuevo
  final List<SubmissionFile> archivos; // ✅ Nuevo (futuro)
  final DateTime fechaEntrega;
  final int tipoEntregaId;
  
  Submission({
    required this.actividadId,
    required this.alumnoId,
    required this.respuesta,
    required this.enlaces,
    this.archivos = const [],
    required this.fechaEntrega,
    this.tipoEntregaId = 1,
  });
}

class SubmissionFile {
  final String nombre;
  final String nombreGuardado;
  final int size;
  final String ruta;
  final DateTime fechaGuardado;
  
  SubmissionFile({
    required this.nombre,
    required this.nombreGuardado,
    required this.size,
    required this.ruta,
    required this.fechaGuardado,
  });
  
  factory SubmissionFile.fromJson(Map<String, dynamic> json) {
    return SubmissionFile(
      nombre: json['nombre'],
      nombreGuardado: json['nombreGuardado'],
      size: json['size'],
      ruta: json['ruta'],
      fechaGuardado: DateTime.parse(json['fechaGuardado']),
    );
  }
}
```

---

## 7️⃣ Checklist de Cambios

- [ ] Cambiar URL del endpoint en `activity_data_source_impl.dart`
- [ ] Compilar y verificar sin errores
- [ ] Probar envío de respuesta de texto
- [ ] Probar envío con enlaces (cuando lo implementes)
- [ ] Verificar que la respuesta se parsea correctamente
- [ ] Ver logs en backend para confirmar que se procesa

---

## 🧪 Test: Validar que funciona

### Desde Flutter:

```dart
// Enviar una respuesta simple
await notifier.sendSubmissionWithLinks(
  activityId: 5,
  answer: 'Mi respuesta de prueba',
  links: [],  // Sin enlaces por ahora
);
```

### Esperado en Backend:

```
[LOG] Registrando entrega - ActividadId: 5, AlumnoId: 3
[LOG] Entrega creada con ID: 42
[LOG] Procesando 0 archivo(s)
[LOG] Entregable creado: 15
```

### Respuesta esperada en Flutter:

```json
{
  "mensaje": "Entrega registrada correctamente (0 archivo(s), 0 enlace(s))",
  "codigo": "EXITO",
  "datos": [
    {
      "AlumnoId": 3,
      "EntregaActividadAlumnoId": 42,
      "EntregableId": 15,
      "ActividadId": 5,
      "Contenido": "{\"texto\":\"Mi respuesta de prueba\",\"enlaces\":[],\"archivos\":[],\"fechaEntrega\":\"2026-01-28T14:30:45.123\",\"totalArchivos\":0,\"totalEnlaces\":0}",
      "TipoEntrega": 1
    }
  ]
}
```

---

## 📝 Notas Importantes

1. **El cambio es mínimo** - Solo cambiar la URL del endpoint
2. **No hay breaking changes** - Todo el flujo sigue igual
3. **Las validaciones ahora son del backend** - Flutter puede ser más simple
4. **Para archivos futuro** - Implementar multipart cuando sea necesario
5. **El JSON en `Contenido` es flexible** - Fácil de extender

---

## 🚀 Próximo Paso

Una vez hayas cambiado la URL en Flutter:

```dart
// Cambio simple
final uri = '${baseUrl.replaceAll('/api/', '')}/Alumnos/RegistrarEnvioActividadAlumnoConEnlaces';
```

1. Compila Flutter
2. Prueba envío de respuesta simple
3. Verifica en backend los logs
4. Cuando esté listo, implementa soporte para archivos/enlaces

---

**¿Necesitas ayuda con alguno de estos cambios?** Avísame cuál es tu siguiente paso.

