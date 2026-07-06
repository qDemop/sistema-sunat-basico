# Especificaciones de Disposicion y Shell de Escritorio

## Principio

El shell adopta la claridad, deference al contenido y jerarquia estable de Apple HIG, adaptadas a una aplicacion empresarial de Windows. La estructura debe permanecer familiar durante jornadas largas y no competir con datos financieros.

## Jerarquia del Espacio de Trabajo

```text
Ventana principal
|-- Barra superior: identidad, periodo/contexto y sesion
|-- Barra lateral persistente: modulos autorizados
|-- Encabezado del espacio de trabajo
|   |-- Migas de pan
|   |-- Titulo, periodo, estado y accion principal
|-- Banda de busqueda y filtros
|-- Contenido principal: lista, formulario, proceso o reporte
|-- Resumen persistente: conteo, seleccion, totales o estado
|-- Panel contextual opcional: detalle, historial o validaciones
```

La busqueda global transversal permanece `IMPLEMENTATION PENDING` y no se muestra en el shell activo. Las busquedas locales de cada lista o modulo siguen definidas en su banda de filtros.

## Barra Lateral Persistente

- Measurements are canonical in `design-system.md`: 240 px expanded and 64 px collapsed.
- Se ubica a la izquierda y permanece visible en la ventana principal.
- Orden canonico: Inicio, Empleados, Planillas, Contabilidad, SUNAT, Reportes, Administracion.
- Solo muestra modulos autorizados por el rol.
- El modulo activo usa icono, texto y estado seleccionado; el color nunca es la unica senal.
- No contiene acciones de tarea como Guardar, Calcular o Exportar.
- En modo expandido muestra icono y etiqueta; en modo contraido conserva iconos, nombre accesible y ayuda contextual.

## Colapso de la Barra Lateral

- El usuario puede expandirla o contraerla en cualquier momento.
- La preferencia se conserva por usuario.
- En ventanas estrechas se contrae automaticamente, sin ocultar el acceso a modulos.
- La expansion no debe cambiar el orden ni reiniciar el espacio de trabajo.
- El foco regresa al mismo destino despues del cambio.

## Migas de Pan

- Aparecen desde el segundo nivel de profundidad.
- Formato: `Modulo > Seccion > Objeto > Vista`.
- Cada nivel navegable conserva filtros y seleccion al regresar.
- No reemplazan el titulo de pantalla ni la navegacion Atras.
- Ejemplos: `Planillas > Periodos > 2026-05 > Resultados`; `SUNAT > Libro de ventas > 2026-05 > Version 2`.

## Encabezado del Espacio de Trabajo

Debe mostrar, en este orden:

1. Migas de pan.
2. Titulo de pantalla y objeto, si corresponde.
3. Periodo, alcance o estado vigente.
4. Una accion visualmente dominante para el estado actual.
5. Acciones secundarias agrupadas y acciones riesgosas separadas.

## Distribucion Responsiva

Typography, spacing, density, target sizes, focus, and contrast follow `design-system.md`; this document controls only region behavior.

| Ancho disponible | Comportamiento |
|---|---|
| 1600 px o mas | Barra lateral expandida; contenido y panel contextual simultaneos cuando aporten valor. |
| 1366-1599 px | Barra lateral expandida o contraida por preferencia; panel contextual bajo demanda. |
| 1280-1365 px | Barra lateral contraida por defecto; acciones secundarias agrupadas; sin perder accion principal ni totales. |

La altura minima de referencia es 720 px. Encabezado, filtros y resumen no deben consumir el espacio necesario para revisar al menos ocho filas de una grilla operativa.

## Ventana y Restauracion

- La primera ejecucion abre una ventana centrada y maximizada cuando la resolucion lo permita.
- En sesiones posteriores se restauran tamano, posicion, estado maximizado, estado de barra lateral y ultimo modulo autorizado.
- En configuraciones con varios monitores, una posicion que ya no existe vuelve al monitor principal sin quedar fuera de pantalla.
- Se restaura una vista segura de lista o reporte, no una confirmacion, operacion en curso ni accion destructiva.
- Un formulario con cambios no guardados no se restaura despues de cerrar la aplicacion o vencer la sesion. Antes de navegar o cerrar dentro de una sesion activa, el sistema exige Guardar, Descartar or Continuar editando. Local unsaved draft recovery is outside the current scope.
- Si el rol cambio, se abre Inicio y se descarta cualquier destino ya no autorizado.
- La restauracion persistente de filtros y vistas guardadas es `IMPLEMENTATION PENDING`; el estado de consulta actual solo vive durante la sesion.

## Historial de Navegacion

- Atras y Adelante recorren vistas, no repiten Calcular, Finalizar, Generar, Contabilizar o Exportar.
- Regresar a una lista conserva busqueda, filtros, orden, agrupacion, pagina y fila seleccionada.
- Cerrar un panel contextual devuelve el foco al registro que lo abrio.

## Ventanas Secundarias

- La experiencia principal usa una sola ventana de trabajo.
- Los dialogos se reservan para decisiones breves y bloqueantes.
- Detalles extensos, formularios y vistas previas permanecen dentro del espacio de trabajo para conservar contexto.
- Vista previa de impresion es la unica experiencia que abre una ventana secundaria no modal. All forms, record details, and process previews remain inside the primary workspace.
