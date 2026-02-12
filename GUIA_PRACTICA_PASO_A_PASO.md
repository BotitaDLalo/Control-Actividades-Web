# 🎓 GUÍA PRÁCTICA - Paso a Paso para Implementar en Flutter

## 📋 Tabla de Contenidos
1. [Instalación de dependencias](#1-instalación-de-dependencias)
2. [Crear servicio](#2-crear-servicio)
3. [Crear widget](#3-crear-widget)
4. [Integrar en pantalla](#4-integrar-en-pantalla)
5. [Pruebas completas](#5-pruebas-completas)

---

## 1️⃣ Instalación de Dependencias

### Paso 1: Actualizar pubspec.yaml

```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0
  file_picker: ^6.1.1
  url_launcher: ^6.2.4
  path: ^1.8.3

dev_dependencies:
  flutter_test:
    sdk: flutter
```

### Paso 2: Instalar dependencias

```bash
flutter pub get
```

---

## 2️⃣ Crear Servicio

### Paso 1: Crear archivo `lib/services/actividad_service.dart`

```dart
import 'package:http/http.dart' as http;
import 'package:file_picker/file_picker.dart';
import 'dart:convert';

class ActividadService {
  // ⚠️ CAMBIAR: Tu URL real
  static const String baseUrl = 'http://192.168.0.9:5000';

  /// Envía entrega con texto, archivos y enlaces
  static Future<EntregaResponse> enviarEntregaConArchivos({
    required int actividadId,
    required int alumnoId,
    required String respuesta,
    required List<PlatformFile> archivos,
    required List<String> enlaces,
  }) async {
    try {
      // Construir URL
      final url = Uri.parse(
        '$baseUrl/api/Alumnos/RegistrarEnvioActividadAlumnoConArchivos',
      );

      // Crear request multipart
      final request = http.MultipartRequest('POST', url);

      // Agregar campos
      request.fields['ActividadId'] = actividadId.toString();
      request.fields['AlumnoId'] = alumnoId.toString();
      request.fields['FechaEntrega'] = DateTime.now().toIso8601String();
      request.fields['TipoEntregaId'] = '1';

      // Agregar respuesta con enlaces como JSON
      request.fields['Respuesta'] = jsonEncode({
        'respuesta': respuesta,
        'enlaces': enlaces,
      });

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

      // Enviar request
      print('[INFO] Enviando entrega...');
      final streamedResponse = await request.send().timeout(
        const Duration(minutes: 5),
        onTimeout: () {
          throw Exception('Timeout: La solicitud tardó demasiado');
        },
      );

      final response = await http.Response.fromStream(streamedResponse);

      print('[INFO] Status: ${response.statusCode}');
      print('[INFO] Response: ${response.body}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return EntregaResponse.fromJson(data);
      } else {
        final data = jsonDecode(response.body);
        throw Exception(
          data['mensaje'] ?? 'Error al enviar la entrega',
        );
      }
    } catch (e) {
      print('[ERROR] $e');
      rethrow;
    }
  }

  /// Obtener entregas anteriores
  static Future<List<EntregaData>> obtenerEntregas(
    int actividadId,
    int alumnoId,
  ) async {
    try {
      final url = Uri.parse(
        '$baseUrl/api/Alumnos/ObtenerEnviosActividadesAlumno'
        '?ActividadId=$actividadId&AlumnoId=$alumnoId',
      );

      final response = await http.get(url);

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as List;
        return data
            .map((item) => EntregaData.fromJson(item))
            .toList();
      } else {
        throw Exception('Error al obtener entregas');
      }
    } catch (e) {
      print('[ERROR] $e');
      rethrow;
    }
  }
}

// Modelos de respuesta
class EntregaResponse {
  final String mensaje;
  final String codigo;
  final List<dynamic> datos;

  EntregaResponse({
    required this.mensaje,
    required this.codigo,
    required this.datos,
  });

  factory EntregaResponse.fromJson(Map<String, dynamic> json) {
    return EntregaResponse(
      mensaje: json['mensaje'] ?? '',
      codigo: json['codigo'] ?? '',
      datos: json['datos'] ?? [],
    );
  }
}

class EntregaData {
  final int alumnoId;
  final int entregaActividadAlumnoId;
  final int entregableId;
  final int actividadId;
  final DateTime fechaEntrega;
  final String contenido;
  final double calificacion;
  final int estadoEntregaId;

  EntregaData({
    required this.alumnoId,
    required this.entregaActividadAlumnoId,
    required this.entregableId,
    required this.actividadId,
    required this.fechaEntrega,
    required this.contenido,
    required this.calificacion,
    required this.estadoEntregaId,
  });

  factory EntregaData.fromJson(Map<String, dynamic> json) {
    return EntregaData(
      alumnoId: json['alumnoId'] ?? 0,
      entregaActividadAlumnoId: json['entregaActividadAlumnoId'] ?? 0,
      entregableId: json['entregableId'] ?? 0,
      actividadId: json['actividadId'] ?? 0,
      fechaEntrega: DateTime.parse(json['fechaEntrega'] ?? DateTime.now().toIso8601String()),
      contenido: json['contenido'] ?? '',
      calificacion: (json['calificacion'] ?? 0).toDouble(),
      estadoEntregaId: json['estadoEntregaId'] ?? 0,
    );
  }
}
```

---

## 3️⃣ Crear Widget

### Paso 1: Crear archivo `lib/widgets/respuesta_widget.dart`

```dart
import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:url_launcher/url_launcher.dart';
import '../services/actividad_service.dart';

class RespuestaWidget extends StatefulWidget {
  final int actividadId;
  final int alumnoId;
  final VoidCallback? onSuccess;

  const RespuestaWidget({
    Key? key,
    required this.actividadId,
    required this.alumnoId,
    this.onSuccess,
  }) : super(key: key);

  @override
  State<RespuestaWidget> createState() => _RespuestaWidgetState();
}

class _RespuestaWidgetState extends State<RespuestaWidget> {
  final _respuestaController = TextEditingController();
  final _enlaceController = TextEditingController();
  final List<PlatformFile> _archivos = [];
  final List<String> _enlaces = [];
  bool _enviando = false;

  @override
  void dispose() {
    _respuestaController.dispose();
    _enlaceController.dispose();
    super.dispose();
  }

  Future<void> _seleccionarArchivos() async {
    try {
      final resultado = await FilePicker.platform.pickFiles(
        type: FileType.custom,
        allowedExtensions: [
          'pdf', 'doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx',
          'jpg', 'jpeg', 'png', 'gif', 'txt', 'zip', 'rar',
        ],
        allowMultiple: true,
      );

      if (resultado != null) {
        setState(() => _archivos.addAll(resultado.files));
        _mostrarSnackBar(
          '✅ ${resultado.files.length} archivo(s) agregado(s)',
          Colors.green,
        );
      }
    } catch (e) {
      _mostrarSnackBar('❌ Error al seleccionar: $e', Colors.red);
    }
  }

  void _agregarEnlace() {
    final enlace = _enlaceController.text.trim();
    if (enlace.isEmpty) {
      _mostrarSnackBar('❌ Ingresa una URL', Colors.red);
      return;
    }

    if (!enlace.startsWith('http://') && !enlace.startsWith('https://')) {
      _mostrarSnackBar('❌ URL debe empezar con http:// o https://', Colors.red);
      return;
    }

    setState(() {
      _enlaces.add(enlace);
      _enlaceController.clear();
    });

    _mostrarSnackBar('✅ Enlace agregado', Colors.green);
  }

  void _eliminarArchivo(int index) {
    setState(() => _archivos.removeAt(index));
    _mostrarSnackBar('Archivo eliminado', Colors.orange);
  }

  void _eliminarEnlace(int index) {
    setState(() => _enlaces.removeAt(index));
    _mostrarSnackBar('Enlace eliminado', Colors.orange);
  }

  Future<void> _abrirEnlace(String url) async {
    if (await canLaunchUrl(Uri.parse(url))) {
      await launchUrl(Uri.parse(url));
    }
  }

  Future<void> _enviar() async {
    if (_respuestaController.text.isEmpty &&
        _archivos.isEmpty &&
        _enlaces.isEmpty) {
      _mostrarSnackBar(
        '❌ Escribe una respuesta, agrega archivos o enlaces',
        Colors.red,
      );
      return;
    }

    setState(() => _enviando = true);

    try {
      final respuesta = await ActividadService.enviarEntregaConArchivos(
        actividadId: widget.actividadId,
        alumnoId: widget.alumnoId,
        respuesta: _respuestaController.text,
        archivos: _archivos,
        enlaces: _enlaces,
      );

      _mostrarSnackBar('✅ ${respuesta.mensaje}', Colors.green);
      _limpiar();
      widget.onSuccess?.call();
    } catch (e) {
      _mostrarSnackBar('❌ Error: $e', Colors.red);
    } finally {
      setState(() => _enviando = false);
    }
  }

  void _limpiar() {
    _respuestaController.clear();
    _enlaceController.clear();
    setState(() {
      _archivos.clear();
      _enlaces.clear();
    });
  }

  void _mostrarSnackBar(String mensaje, Color color) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(mensaje),
        backgroundColor: color,
        duration: const Duration(seconds: 3),
      ),
    );
  }

  String _formatearTamano(int bytes) {
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(2)} KB';
    return '${(bytes / (1024 * 1024)).toStringAsFixed(2)} MB';
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Título
            const Text(
              'Enviar Respuesta',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),

            // Respuesta de texto
            TextField(
              controller: _respuestaController,
              maxLines: 5,
              decoration: InputDecoration(
                hintText: 'Escribe tu respuesta...',
                labelText: 'Respuesta',
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
            ),
            const SizedBox(height: 16),

            // Archivos
            const Text(
              '📎 Archivos',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            ElevatedButton.icon(
              onPressed: _archivos.length >= 10 ? null : _seleccionarArchivos,
              icon: const Icon(Icons.attach_file),
              label: const Text('Seleccionar Archivos (máx 10)'),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.orange,
              ),
            ),
            if (_archivos.isNotEmpty) ...[
              const SizedBox(height: 12),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.orange),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: ListView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: _archivos.length,
                  itemBuilder: (context, index) {
                    final archivo = _archivos[index];
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
            const SizedBox(height: 16),

            // Enlaces
            const Text(
              '🔗 Enlaces',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.bold,
              ),
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
                padding: const EdgeInsets.all(12),
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
                            icon: const Icon(Icons.open_in_new),
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
            const SizedBox(height: 24),

            // Botones
            Row(
              children: [
                Expanded(
                  child: ElevatedButton.icon(
                    onPressed: _enviando ? null : _limpiar,
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
                    onPressed: _enviando ? null : _enviar,
                    icon: _enviando
                        ? const SizedBox(
                            width: 20,
                            height: 20,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Icon(Icons.send),
                    label: Text(_enviando ? 'Enviando...' : 'Enviar'),
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
    );
  }
}
```

---

## 4️⃣ Integrar en Pantalla

### Paso 1: Usar widget en tu pantalla

```dart
import 'package:flutter/material.dart';
import 'widgets/respuesta_widget.dart';

class ActividadDetalleScreen extends StatefulWidget {
  final int actividadId;
  final int alumnoId;
  final String nombreActividad;

  const ActividadDetalleScreen({
    Key? key,
    required this.actividadId,
    required this.alumnoId,
    required this.nombreActividad,
  }) : super(key: key);

  @override
  State<ActividadDetalleScreen> createState() => _ActividadDetalleScreenState();
}

class _ActividadDetalleScreenState extends State<ActividadDetalleScreen> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.nombreActividad),
      ),
      body: RespuestaWidget(
        actividadId: widget.actividadId,
        alumnoId: widget.alumnoId,
        onSuccess: () {
          // Actualizar pantalla si es necesario
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('✅ Entrega enviada correctamente'),
              duration: Duration(seconds: 3),
            ),
          );
        },
      ),
    );
  }
}
```

---

## 5️⃣ Pruebas Completas

### Test 1: Respuesta solo texto
```dart
void testRespuestaTexto() async {
  final respuesta = await ActividadService.enviarEntregaConArchivos(
    actividadId: 5,
    alumnoId: 123,
    respuesta: 'Mi respuesta',
    archivos: [],
    enlaces: [],
  );
  print('✅ ${respuesta.mensaje}');
}
```

### Test 2: Respuesta con archivos
```dart
void testConArchivos() async {
  // Simular archivos
  final archivo = PlatformFile(
    name: 'documento.pdf',
    size: 1024 * 100, // 100KB
    path: '/ruta/al/archivo',
  );

  final respuesta = await ActividadService.enviarEntregaConArchivos(
    actividadId: 5,
    alumnoId: 123,
    respuesta: 'Mi respuesta',
    archivos: [archivo],
    enlaces: ['https://ejemplo.com'],
  );
  print('✅ ${respuesta.mensaje}');
}
```

---

## 📚 Estructura Final del Proyecto

```
lib/
├── services/
│   └── actividad_service.dart (nuevo)
├── widgets/
│   └── respuesta_widget.dart (nuevo)
├── screens/
│   └── actividad_detalle_screen.dart (modificado)
└── main.dart
```

---

## ✅ Checklist de Implementación

- [ ] Agregué dependencias a `pubspec.yaml`
- [ ] Ejecuté `flutter pub get`
- [ ] Creé `lib/services/actividad_service.dart`
- [ ] Creé `lib/widgets/respuesta_widget.dart`
- [ ] Actualicé mi pantalla para usar el widget
- [ ] Cambié la URL del servidor (baseUrl)
- [ ] Probé con texto solo
- [ ] Probé con archivos
- [ ] Probé con enlaces
- [ ] Probé que aparezcan errores correctamente
- [ ] Probé que los archivos se guarden en servidor

---

## 🚀 Listo para usar

Una vez completes estos pasos, tu app Flutter tendrá soporte completo para:

✅ Enviar respuestas de texto  
✅ Adjuntar archivos (múltiples)  
✅ Agregar enlaces clickeables  
✅ Validar antes de enviar  
✅ Ver errores descriptivos  
✅ Mostrar confirmación al usuario  

**¡A programar!** 💻

