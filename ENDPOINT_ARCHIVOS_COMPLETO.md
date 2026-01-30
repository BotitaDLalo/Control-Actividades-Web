# 📚 Endpoint: RegistrarEnvioActividadAlumnoConArchivos()

## 🎯 Resumen
Nuevo endpoint para registrar entregas de actividades con soporte para **texto + archivos + enlaces**.

---

## 📍 Detalles del Endpoint

### URL
```
POST /api/Alumnos/RegistrarEnvioActividadAlumnoConArchivos
```

### Tipo de Request
```
multipart/form-data
```

### Parámetros

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| ActividadId | int | ✅ Sí | ID de la actividad |
| AlumnoId | int | ✅ Sí | ID del alumno |
| Respuesta | string | ❌ No | Respuesta de texto (puede ser JSON con enlaces) |
| FechaEntrega | string | ❌ No | Fecha ISO 8601 (default: ahora) |
| TipoEntregaId | int | ❌ No | Tipo de entrega (default: 1) |
| files | file[] | ❌ No | Archivos a subir (múltiples) |

### Límites
- **Máximo por archivo:** 50MB
- **Máximo total:** 200MB
- **Extensiones permitidas:** .pdf, .doc, .docx, .xls, .xlsx, .ppt, .pptx, .jpg, .jpeg, .png, .gif, .txt, .zip, .rar, .7z, .odt, .ods, .odp, .rtf

---

## ✅ Respuesta Exitosa (200 OK)

```json
{
  "mensaje": "Entrega registrada correctamente. 2 archivo(s) guardado(s).",
  "codigo": "EXITO",
  "datos": [
    {
      "alumnoId": 123,
      "entregaActividadAlumnoId": 45,
      "entregableId": 67,
      "actividadId": 5,
      "fechaEntrega": "2024-01-15T10:30:00",
      "contenido": "{\"Respuesta\":\"Mi respuesta\",\"Archivos\":[\"/Uploads/Entregas/5/123/documento.pdf\",\"/Uploads/Entregas/5/123/imagen.jpg\"],\"FechaGuardado\":\"2024-01-15T10:30:45.123\",\"TotalArchivos\":2,\"TamanoTotal\":\"3.45 MB\"}",
      "calificacion": 0,
      "estadoEntregaId": 1
    }
  ]
}
```

---

## ❌ Respuestas de Error

### Error 400 - Datos Incompletos
```json
{
  "mensaje": "Faltan datos obligatorios.",
  "codigo": "DATOS_INCOMPLETOS",
  "detalles": "ActividadId y AlumnoId deben ser mayores a 0. Recibido - ActividadId: 0, AlumnoId: 123"
}
```

### Error 400 - Archivo No Permitido
```json
{
  "mensaje": "Tipo de archivo no permitido.",
  "codigo": "ARCHIVO_NO_PERMITIDO",
  "detalles": "La extensión '.exe' no es permitida. Extensiones válidas: .pdf, .doc, .docx, ..."
}
```

### Error 400 - Archivo Muy Grande
```json
{
  "mensaje": "Archivo demasiado grande.",
  "codigo": "ARCHIVO_MUY_GRANDE",
  "detalles": "El archivo 'video.mp4' excede el límite de 50MB. Tamaño: 120MB"
}
```

### Error 500 - Error Interno
```json
{
  "mensaje": "Error al registrar la entrega con archivos.",
  "codigo": "ERROR_INTERNO",
  "detalles": "Mensaje de excepción"
}
```

---

## 🎯 Estructura de Datos Almacenada

El campo `Contenido` en `tbEntregables` guarda un JSON con esta estructura:

```json
{
  "Respuesta": "Mi respuesta de texto",
  "Archivos": [
    "/Uploads/Entregas/5/123/documento.pdf",
    "/Uploads/Entregas/5/123/imagen.jpg"
  ],
  "FechaGuardado": "2024-01-15T10:30:45.123",
  "TotalArchivos": 2,
  "TamanoTotal": "3.45 MB"
}
```

---

## 📱 Implementación en Flutter

### 1️⃣ Widget Principal (Mejorado)

```dart
import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:url_launcher/url_launcher.dart';

class RespuestaEntregableWidget extends StatefulWidget {
  final int actividadId;
  final int alumnoId;
  final Function(EntregaData) onSubmit;

  const RespuestaEntregableWidget({
    required this.actividadId,
    required this.alumnoId,
    required this.onSubmit,
  });

  @override
  State<RespuestaEntregableWidget> createState() =>
      _RespuestaEntregableWidgetState();
}

class _RespuestaEntregableWidgetState extends State<RespuestaEntregableWidget> {
  final TextEditingController _respuestaController = TextEditingController();
  final TextEditingController _enlaceController = TextEditingController();
  final List<PlatformFile> _archivosSeleccionados = [];
  final List<String> _enlaces = [];
  bool _isLoading = false;

  @override
  void dispose() {
    _respuestaController.dispose();
    _enlaceController.dispose();
    super.dispose();
  }

  Future<void> _seleccionarArchivos() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.custom,
        allowedExtensions: [
          'pdf', 'doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx',
          'jpg', 'jpeg', 'png', 'gif', 'txt', 'zip', 'rar', '7z',
          'odt', 'ods', 'odp', 'rtf'
        ],
        allowMultiple: true,
      );

      if (result != null) {
        final tamanoTotal = result.files.fold<int>(
          0,
          (sum, file) => sum + (file.size ?? 0),
        );

        const maxTamano = 200 * 1024 * 1024; // 200MB
        if (tamanoTotal > maxTamano) {
          _mostrarError(
            'Tamaño total excedido',
            'El tamaño total no debe exceder 200MB',
          );
          return;
        }

        setState(() {
          _archivosSeleccionados.addAll(result.files);
        });

        _mostrarExito(
          '${result.files.length} archivo(s) seleccionado(s)',
          'Tamaño: ${_formatearTamano(tamanoTotal)}',
        );
      }
    } catch (e) {
      _mostrarError('Error', 'No se pudieron seleccionar los archivos: $e');
    }
  }

  void _agregarEnlace() {
    final enlace = _enlaceController.text.trim();
    if (enlace.isEmpty) {
      _mostrarError('Enlace vacío', 'Por favor ingresa una URL válida');
      return;
    }

    if (!_validarURL(enlace)) {
      _mostrarError(
        'URL inválida',
        'La URL debe comenzar con http:// o https://',
      );
      return;
    }

    setState(() {
      _enlaces.add(enlace);
      _enlaceController.clear();
    });

    _mostrarExito('Enlace agregado', enlace);
  }

  bool _validarURL(String url) {
    try {
      Uri.parse(url);
      return url.startsWith('http://') || url.startsWith('https://');
    } catch (e) {
      return false;
    }
  }

  String _formatearTamano(int bytes) {
    const List<String> sizes = ['B', 'KB', 'MB', 'GB'];
    if (bytes == 0) return '0 B';
    int i = (bytes / 1024).floor();
    if (i == 0) return '$bytes B';
    return '${(bytes / (1024 * i)).toStringAsFixed(2)} ${sizes[i]}';
  }

  void _eliminarArchivo(int index) {
    setState(() {
      _archivosSeleccionados.removeAt(index);
    });
  }

  void _eliminarEnlace(int index) {
    setState(() {
      _enlaces.removeAt(index);
    });
  }

  void _abrirEnlace(String url) async {
    if (await canLaunchUrl(Uri.parse(url))) {
      await launchUrl(Uri.parse(url));
    } else {
      _mostrarError('Error', 'No se puede abrir el enlace');
    }
  }

  Future<void> _enviarRespuesta() async {
    if (_respuestaController.text.isEmpty &&
        _archivosSeleccionados.isEmpty &&
        _enlaces.isEmpty) {
      _mostrarError(
        'Respuesta vacía',
        'Por favor ingresa una respuesta, archivos o enlaces',
      );
      return;
    }

    setState(() => _isLoading = true);

    try {
      final entrega = EntregaData(
        actividadId: widget.actividadId,
        alumnoId: widget.alumnoId,
        respuesta: _respuestaController.text,
        archivos: _archivosSeleccionados,
        enlaces: _enlaces,
        fechaEntrega: DateTime.now().toIso8601String(),
        tipoEntregaId: 1,
      );

      await widget.onSubmit(entrega);
      _limpiarFormulario();

      _mostrarExito(
        'Entrega enviada',
        'Tu respuesta ha sido registrada correctamente',
      );
    } catch (e) {
      _mostrarError('Error al enviar', e.toString());
    } finally {
      setState(() => _isLoading = false);
    }
  }

  void _limpiarFormulario() {
    _respuestaController.clear();
    _enlaceController.clear();
    setState(() {
      _archivosSeleccionados.clear();
      _enlaces.clear();
    });
  }

  void _mostrarError(String titulo, String mensaje) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(titulo, style: const TextStyle(fontWeight: FontWeight.bold)),
            SizedBox(height: 4),
            Text(mensaje),
          ],
        ),
        backgroundColor: Colors.red,
        duration: const Duration(seconds: 4),
      ),
    );
  }

  void _mostrarExito(String titulo, String mensaje) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(titulo, style: const TextStyle(fontWeight: FontWeight.bold)),
            SizedBox(height: 4),
            Text(mensaje),
          ],
        ),
        backgroundColor: Colors.green,
        duration: const Duration(seconds: 3),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Card(
        margin: const EdgeInsets.all(16),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Enviar Respuesta',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 16),

              // Campo de texto
              TextField(
                controller: _respuestaController,
                maxLines: 5,
                decoration: InputDecoration(
                  hintText: 'Ingresa tu respuesta aquí...',
                  labelText: 'Respuesta',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  prefixIcon: const Icon(Icons.edit),
                ),
              ),
              const SizedBox(height: 16),

              // Sección de archivos
              _buildSeccionArchivos(),
              const SizedBox(height: 16),

              // Sección de enlaces
              _buildSeccionEnlaces(),
              const SizedBox(height: 24),

              // Botones
              Row(
                children: [
                  Expanded(
                    child: ElevatedButton.icon(
                      onPressed: _isLoading ? null : _limpiarFormulario,
                      icon: const Icon(Icons.clear),
                      label: const Text('Limpiar'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.grey,
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: ElevatedButton.icon(
                      onPressed: _isLoading ? null : _enviarRespuesta,
                      icon: _isLoading
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                              ),
                            )
                          : const Icon(Icons.send),
                      label: const Text('Enviar'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.blue,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSeccionArchivos() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          '📎 Archivos',
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
        ),
        const SizedBox(height: 8),
        ElevatedButton.icon(
          onPressed: _seleccionarArchivos,
          icon: const Icon(Icons.attach_file),
          label: const Text('Seleccionar Archivos'),
          style: ElevatedButton.styleFrom(
            backgroundColor: Colors.orange,
          ),
        ),
        if (_archivosSeleccionados.isNotEmpty) ...[
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              border: Border.all(color: Colors.orange),
              borderRadius: BorderRadius.circular(8),
            ),
            child: ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: _archivosSeleccionados.length,
              itemBuilder: (context, index) {
                final archivo = _archivosSeleccionados[index];
                return ListTile(
                  leading: const Icon(Icons.description, color: Colors.orange),
                  title: Text(archivo.name),
                  subtitle: Text(_formatearTamano(archivo.size ?? 0)),
                  trailing: IconButton(
                    icon: const Icon(Icons.delete, color: Colors.red),
                    onPressed: () => _eliminarArchivo(index),
                  ),
                );
              },
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildSeccionEnlaces() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          '🔗 Enlaces',
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(
              child: TextField(
                controller: _enlaceController,
                decoration: InputDecoration(
                  hintText: 'https://ejemplo.com',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  prefixIcon: const Icon(Icons.link),
                ),
              ),
            ),
            const SizedBox(width: 8),
            ElevatedButton.icon(
              onPressed: _agregarEnlace,
              icon: const Icon(Icons.add),
              label: const Text('Agregar'),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.green,
              ),
            ),
          ],
        ),
        if (_enlaces.isNotEmpty) ...[
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              border: Border.all(color: Colors.green),
              borderRadius: BorderRadius.circular(8),
            ),
            child: ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: _enlaces.length,
              itemBuilder: (context, index) {
                final enlace = _enlaces[index];
                return ListTile(
                  leading: const Icon(Icons.link, color: Colors.green),
                  title: Text(
                    enlace,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(color: Colors.blue),
                  ),
                  trailing: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      IconButton(
                        icon: const Icon(Icons.open_in_new, color: Colors.blue),
                        onPressed: () => _abrirEnlace(enlace),
                      ),
                      IconButton(
                        icon: const Icon(Icons.delete, color: Colors.red),
                        onPressed: () => _eliminarEnlace(index),
                      ),
                    ],
                  ),
                );
              },
            ),
          ),
        ],
      ],
    );
  }
}

// Modelo de datos
class EntregaData {
  final int actividadId;
  final int alumnoId;
  final String respuesta;
  final List<PlatformFile> archivos;
  final List<String> enlaces;
  final String fechaEntrega;
  final int tipoEntregaId;

  EntregaData({
    required this.actividadId,
    required this.alumnoId,
    required this.respuesta,
    required this.archivos,
    required this.enlaces,
    required this.fechaEntrega,
    required this.tipoEntregaId,
  });
}
```

### 2️⃣ Servicio

```dart
import 'package:http/http.dart' as http;
import 'package:file_picker/file_picker.dart';
import 'dart:convert';

class ActividadService {
  static const String baseUrl = 'https://tuapi.com';

  static Future<Map<String, dynamic>> enviarEntregaConArchivos({
    required int actividadId,
    required int alumnoId,
    required String respuesta,
    required List<PlatformFile> archivos,
    required List<String> enlaces,
    required String fechaEntrega,
    required int tipoEntregaId,
  }) async {
    try {
      final url = Uri.parse(
        '$baseUrl/api/Alumnos/RegistrarEnvioActividadAlumnoConArchivos',
      );

      final request = http.MultipartRequest('POST', url);

      request.fields['ActividadId'] = actividadId.toString();
      request.fields['AlumnoId'] = alumnoId.toString();
      request.fields['TipoEntregaId'] = tipoEntregaId.toString();
      request.fields['FechaEntrega'] = fechaEntrega;

      // Guardar respuesta + enlaces como JSON
      final contenido = {
        'respuesta': respuesta,
        'enlaces': enlaces,
      };
      request.fields['Respuesta'] = jsonEncode(contenido);

      // Agregar archivos
      for (var archivo in archivos) {
        if (archivo.bytes != null) {
          request.files.add(
            http.MultipartFile.fromBytes(
              'files',
              archivo.bytes!,
              filename: archivo.name,
            ),
          );
        } else if (archivo.path != null) {
          request.files.add(
            await http.MultipartFile.fromPath(
              'files',
              archivo.path!,
            ),
          );
        }
      }

      final streamedResponse = await request.send();
      final response = await http.Response.fromStream(streamedResponse);

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {'success': true, 'data': data};
      } else {
        final data = jsonDecode(response.body);
        return {
          'success': false,
          'error': data['mensaje'] ?? 'Error desconocido',
          'code': data['codigo'] ?? 'ERROR',
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Error de conexión: $e',
        'code': 'CONEXION_ERROR',
      };
    }
  }
}
```

### 3️⃣ Usar en Pantalla

```dart
class ActividadDetalleScreen extends StatelessWidget {
  final int actividadId;
  final int alumnoId;

  const ActividadDetalleScreen({
    required this.actividadId,
    required this.alumnoId,
  });

  Future<void> _handleEnviarEntrega(EntregaData entrega) async {
    final resultado = await ActividadService.enviarEntregaConArchivos(
      actividadId: entrega.actividadId,
      alumnoId: entrega.alumnoId,
      respuesta: entrega.respuesta,
      archivos: entrega.archivos,
      enlaces: entrega.enlaces,
      fechaEntrega: entrega.fechaEntrega,
      tipoEntregaId: entrega.tipoEntregaId,
    );

    if (resultado['success']) {
      print('✅ Entrega enviada exitosamente');
    } else {
      print('❌ Error: ${resultado['error']}');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Enviar Actividad')),
      body: RespuestaEntregableWidget(
        actividadId: actividadId,
        alumnoId: alumnoId,
        onSubmit: _handleEnviarEntrega,
      ),
    );
  }
}
```

### 4️⃣ pubspec.yaml

```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0
  file_picker: ^6.1.1
  url_launcher: ^6.2.4
  path: ^1.8.3
```

---

## 🔍 Cómo Recuperar Datos de la Entrega

Cuando obtienes el `Contenido` desde el servidor, viene como JSON:

```dart
// Parsear el contenido
final entrega = jsonDecode(response.data['Contenido']);
final respuesta = entrega['Respuesta'];
final archivos = List<String>.from(entrega['Archivos']);
final enlaces = List<String>.from(entrega['Enlaces'] ?? []);
final totalArchivos = entrega['TotalArchivos'];
```

---

## ✅ Flujo Completo

```
Usuario llena formulario
    ↓
Selecciona archivos (validación local)
    ↓
Agrega enlaces (validación URL)
    ↓
Click en "Enviar"
    ↓
Crea FormData con:
  - Parámetros (IDs, fecha)
  - Respuesta de texto
  - Lista de enlaces (JSON)
  - Archivos (multipart)
    ↓
Envía a servidor
    ↓
Backend valida:
  - IDs válidos
  - Extensiones permitidas
  - Tamaños dentro de límite
    ↓
Guarda archivos en disco
    ↓
Crea JSON con toda la información
    ↓
Almacena en BD
    ↓
Retorna respuesta con éxito
    ↓
Flutter muestra confirmación
```

---

## 📋 Resumen

✅ **Texto + Archivos + Enlaces** integrados  
✅ **Validaciones robustas** en backend  
✅ **Límites de tamaño** configurables  
✅ **Estructura JSON** para datos complejos  
✅ **Interfaz limpia** en Flutter  
✅ **Manejo de errores** detallado  

**¡Listo para producción!** 🚀

