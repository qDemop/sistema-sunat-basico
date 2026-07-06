# Atajos de Teclado y Flujos de Productividad

## Principios

Los atajos aceleran tareas frecuentes sin ser obligatorios para aprender el sistema. Respetan convenciones de escritorio, son consistentes entre modulos y nunca ejecutan una accion irreversible sin revision explicita.

## Atajos Globales

| Atajo | Accion | Regla |
|---|---|---|
| `Ctrl+K` | Sin asignar | Global search is `IMPLEMENTATION PENDING`; do not intercept this shortcut. |
| `Ctrl+F` | Buscar en la pantalla actual | Conserva filtros activos. |
| `Ctrl+N` | Nuevo registro | Disponible solo en contextos de alta. |
| `Ctrl+S` | Guardar | No omite validaciones. |
| `Ctrl+Mayus+S` | Guardar y crear otro | Solo en comprobantes y otros flujos repetitivos aprobados. |
| `F5` | Actualizar | Conserva busqueda, filtros, orden y seleccion cuando siga vigente. |
| `Ctrl+Mayus+E` | Exportar vista actual | Abre resumen de alcance antes de exportar. |
| `Ctrl+P` | Imprimir o vista previa | Solo cuando exista salida imprimible. |
| `Alt+Izquierda` | Atras | No repite procesos. |
| `Alt+Derecha` | Adelante | Recupera la vista siguiente. |
| `F6` | Cambiar region de foco | Barra lateral, encabezado, filtros, contenido y resumen. |
| `Esc` | Cerrar contexto o cancelar | Advierte si hay cambios no guardados. |

## Navegacion por Modulos

| Atajo | Destino |
|---|---|
| `Ctrl+1` | Inicio. |
| `Ctrl+2` | Empleados. |
| `Ctrl+3` | Planillas. |
| `Ctrl+4` | Contabilidad. |
| `Ctrl+5` | SUNAT. |
| `Ctrl+6` | Reportes. |
| `Ctrl+7` | Administracion. |

Si el rol no tiene acceso, el atajo no navega ni revela contenido restringido.

## Grillas

| Atajo | Accion |
|---|---|
| Flechas | Mover foco entre filas o celdas navegables. |
| `Inicio` / `Fin` | Primera o ultima columna visible. |
| `Ctrl+Inicio` / `Ctrl+Fin` | Primer o ultimo registro de la pagina actual. |
| `Espacio` | Seleccionar fila cuando la grilla admite seleccion. |
| `Mayus+Espacio` | Extender seleccion desde la fila ancla hasta la fila enfocada en la pagina actual. |
| `Ctrl+A` | Seleccionar la pagina actual cuando la accion masiva lo admite. All filtered results require the explicit banner action. |
| `Enter` | Abrir detalle del registro enfocado. |
| `F2` | Editar celda solo en grillas de captura autorizadas. |
| `Ctrl+C` | Copiar valores visibles con formato legible. |
| `Ctrl+Mayus+C` | Copiar valores con encabezados. |
| `Alt+Mayus+F` | Abrir filtros avanzados. |
| `Alt+Mayus+V` | Sin asignar; vistas guardadas `IMPLEMENTATION PENDING`. |

## Formularios

- `Tab` y `Mayus+Tab` siguen el orden del proceso de negocio.
- `Enter` confirma una seleccion o avanza cuando no existe riesgo de guardar accidentalmente.
- `Alt+Abajo` abre opciones disponibles en campos de seleccion.
- El foco va al primer error despues de un intento de guardado fallido.
- Guardar y crear otro conserva solo contexto seguro: periodo, fecha de trabajo, movimiento o tipo cuando el usuario lo haya elegido. Nunca conserva documento, tercero, importes ni datos bancarios.

## Flujo Repetitivo de Comprobantes

1. `Ctrl+N` abre Nuevo comprobante con periodo de trabajo visible.
2. El orden de foco sigue: movimiento, tipo, serie, numero, fecha, documento, tercero, operacion e importes.
3. Los calculos derivados se anuncian sin mover el foco.
4. `Ctrl+Mayus+S` guarda y crea otro, conservando solo periodo, fecha y opciones expresamente seguras.
5. Un duplicado ofrece Abrir comprobante existente o Corregir identidad.

## Flujo Masivo de Horas Extra

1. Se fija un periodo antes de editar filas.
2. La grilla permite buscar empleado, ingresar primeras dos horas y horas adicionales, y copiar valores permitidos.
3. Los errores permanecen asociados a cada fila.
4. Guardar informa filas correctas, filas rechazadas y si hubo guardado parcial.

## Flujo de Planilla

1. Seleccionar periodo y revisar preparacion.
2. Calcular es la accion dominante cuando no existen resultados vigentes.
3. Revisar diferencias y detalle por empleado.
4. Finalizar se vuelve dominante solo cuando el borrador es valido.
5. La finalizacion requiere revision de periodo, empleados y totales; no tiene atajo directo.

## Flujo SUNAT

1. Seleccionar periodo y tipo de libro.
2. Revisar elegibles, exclusiones y version existente.
3. Generar una nueva version mediante accion explicita, sin atajo directo.
4. Exportar muestra libro, version, filtros y formato antes de iniciar.

## Descubribilidad

- Los menus y ayudas contextuales muestran el atajo junto al nombre de la accion.
- Ningun flujo basico depende de memorizar atajos.
- Los atajos se deshabilitan cuando la accion no aplica y explican el motivo mediante ayuda accesible.
