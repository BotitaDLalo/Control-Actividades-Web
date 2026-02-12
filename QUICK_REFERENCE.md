# ⚡ QUICK REFERENCE - Cheat Sheet

## 🎯 Lo más importante en 1 página

### 📍 Endpoint

```
POST /api/Alumnos/RegistrarEnvioActividadAlumnoConArchivos
Content-Type: multipart/form-data
```

### 📋 Parámetros

```
ActividadId (int)       ✅ Requerido
AlumnoId (int)          ✅ Requerido
Respuesta (string)      ❌ Opcional
FechaEntrega (string)   ❌ Opcional (default: ahora)
TipoEntregaId (int)     ❌ Opcional (default: 1)
files (file[])          ❌ Opcional (múltiples)
```

### 📏 Límites

```
50 MB por archivo
200 MB total
16 extensiones permitidas
```

### ✅ Respuesta Exitosa

```json
{
  "mensaje": "Entrega registrada correctamente. X archivo(s) guardado(s).",
  "codigo": "EXITO",
  "datos": [...]
}
```

### ❌ Errores Comunes

```
400 - DATOS_INCOMPLETOS: Faltan ActividadId o AlumnoId
400 - ARCHIVO_NO_PERMITIDO: Extensión no permitida
400 - ARCHIVO_MUY_GRANDE: > 50MB
400 - ESPACIO_INSUFICIENTE: Total > 200MB
500 - ERROR_INTERNO: Error del servidor
```

---

## 💻 Flutter - Código Mínimo

### Importar
```dart
import 'package:http/http.dart' as http;
import 'package:file_picker/file_picker.dart';
import 'dart:convert';
```

### Enviar
```dart
final url = Uri.parse(
  'http://servidor/api/Alumnos/RegistrarEnvioActividadAlumnoConArchivos'
);
final request = http.MultipartRequest('POST', url);

request.fields['ActividadId'] = '5';
request.fields['AlumnoId'] = '123';
request.fields['Respuesta'] = 'Mi respuesta';

// Agregar archivo
request.files.add(await http.MultipartFile.fromPath('files', '/ruta/archivo.pdf'));

final response = await request.send();
final result = await http.Response.fromStream(response);

if (result.statusCode == 200) {
  print('✅ Enviado');
} else {
  print('❌ Error: ${result.body}');
}
```

---

## 🔧 C# - Código Backend

```csharp
[HttpPost]
[Route("RegistrarEnvioActividadAlumnoConArchivos")]
public async Task<IHttpActionResult> RegistrarEnvioActividadAlumnoConArchivos()
{
    var httpRequest = HttpContext.Current.Request;
    
    int actividadId = int.Parse(httpRequest.Form["ActividadId"]);
    int alumnoId = int.Parse(httpRequest.Form["AlumnoId"]);
    string respuesta = httpRequest.Form["Respuesta"] ?? "";
    
    var files = httpRequest.Files;
    
    // Procesar archivos...
    
    return Ok(new { mensaje = "Éxito", codigo = "EXITO" });
}
```

---

## 🎨 UI Flutter - Widget Mínimo

```dart
ElevatedButton(
  onPressed: _seleccionarArchivos,
  child: Text('Seleccionar Archivos'),
)

ElevatedButton(
  onPressed: _enviar,
  child: Text('Enviar'),
)
```

---

## 📊 Estructura Almacenada

```json
{
  "Respuesta": "texto",
  "Archivos": ["/ruta/archivo.pdf"],
  "TotalArchivos": 1,
  "TamanoTotal": "1.2 MB"
}
```

---

## ✅ Checklist Rápido

- [ ] URL de servidor configurada
- [ ] Dependencias instaladas
- [ ] Widget creado
- [ ] Servicio implementado
- [ ] Validaciones locales
- [ ] Prueba con archivo pequeño
- [ ] Prueba con múltiples archivos
- [ ] Verificar respuesta en servidor

---

## 🚀 Deploy

```bash
# Backend
1. Compilar: dotnet build
2. Deploy: copiar DLL a servidor

# Frontend
1. flutter pub get
2. flutter build apk (Android)
3. flutter build ios (iOS)
```

---

## 🆘 Troubleshooting

| Error | Solución |
|-------|----------|
| 400 - DATOS_INCOMPLETOS | Verificar ActividadId y AlumnoId |
| 400 - ARCHIVO_NO_PERMITIDO | Usar .pdf, .doc, .jpg, etc |
| 400 - ARCHIVO_MUY_GRANDE | Máximo 50MB |
| 500 - ERROR_INTERNO | Revisar logs del servidor |
| FileNotFound | Verificar carpeta Uploads existe |
| Timeout | Aumentar timeout (5 minutos) |

---

## 📚 Documentación Completa

- RESUMEN_FINAL_EJECUTIVO.md
- ENDPOINT_ARCHIVOS_COMPLETO.md
- GUIA_PRACTICA_PASO_A_PASO.md
- RESUMEN_IMPLEMENTACION.md

---

## 💡 Tips

✅ Siempre validar extensión en cliente  
✅ Mostrar progreso de carga  
✅ Permitir reintentos automáticos  
✅ Guardar borradores localmente  
✅ Usar caché para URL de servidor  

---

**¿Más información?** Ver 00_INDICE_DOCUMENTACION_COMPLETA.md

