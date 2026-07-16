# Especificaciones de Disposicion y Shell de Escritorio

## Principio

El shell adopta la claridad, deference al contenido y jerarquia estable de Apple HIG, adaptadas a una aplicacion empresarial de Windows. La estructura debe permanecer familiar durante jornadas largas y no competir con datos financieros.

El shell usa los tokens semanticos definidos en `design-system.md` para modo claro y oscuro. La direccion visual y sus limites de adaptacion a Windows se rigen por 'Canonical Visual Direction and Current Baseline' de ux-vision.md. Las reglas especificas del chrome de ventana se definen en 'Direccion de Chrome Personalizado' de este documento.

La tipografia debe ser legible, jerarquica y compatible con Windows; la fuente concreta se rige por los tokens de design-system.md.

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

## Superficies y Tema

- Shell, barra lateral, espacio de trabajo, grillas, formularios, paneles y dialogos usan tokens semanticos de tema.
- Los formularios y controles no deben usar colores hardcodeados; deben recibir valores desde la paleta activa.
- La profundidad visual se logra con capas de superficie, bordes y separacion espacial sutil.
- Evitar sombras pesadas, gradientes decorativos o fondos de alto contraste que compitan con datos financieros.
- Los estados de seleccion, foco, error, advertencia, exito y deshabilitado deben ser distinguibles en modo claro y oscuro.

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

## Direccion de Chrome Personalizado

El chrome personalizado esta aprobado como direccion futura del producto, pero no se implementa en esta fase. Si se prototipa, preserva el comportamiento esperado y las convenciones funcionales de Windows y usa una unica implementacion reutilizable del shell; no se permite que cada Form cree su propia barra de titulo o controles de ventana.

La adopcion final queda condicionada a aprobar la siguiente lista completa. Una mejora solo visual que reduzca funcionalidad o accesibilidad es inaceptable. Si el prototipo no preserva algun punto de forma confiable, el chrome nativo de Windows sigue siendo obligatorio hasta corregirlo.

Los entornos objetivo son Windows 10 22H2 y Windows 11 23H2 o posterior, en equipos fisicos o maquinas virtuales soportadas que usen el shell y administrador de ventanas estandar de Windows. Para Snap y Snap Layouts, `cuando sea tecnicamente posible` significa que la capacidad existe en el entorno objetivo y en la configuracion probada. Si no existe, el registro de validacion debe identificar version de Windows, compilacion, hardware/entorno, configuracion y evidencia tecnica reproducible. No se adopta chrome personalizado si Snap deja de funcionar en un entorno objetivo ordinario donde Windows lo ofrece.

| Paridad requerida | Criterio de aceptacion |
|---|---|
| Arrastre | Arrastrar una zona no interactiva de la barra de titulo mueve la ventana; arrastrar un boton, menu o campo de esa barra activa solo ese control. |
| Minimizar, maximizar/restaurar y cerrar | Cada comando cambia el estado esperado de la ventana; restaurar recupera el ultimo tamano no maximizado y cerrar solicita guardar cambios cuando corresponde. |
| Doble clic en la barra de titulo | Dos clics en una zona no interactiva alternan entre maximizada y restaurada; dos clics sobre controles no cambian el estado. |
| Cambio de tamano | El cursor y el arrastre redimensionan desde los cuatro bordes y cuatro esquinas cuando la ventana esta restaurada; no funcionan cuando esta maximizada. |
| Menu del sistema y `Alt+Espacio` | `Alt+Espacio` abre el menu del sistema y sus comandos disponibles se ejecutan con teclado; `Esc` lo cierra sin cambiar la ventana. |
| Windows Snap y Snap Layouts | En cada entorno objetivo que los ofrezca, `Win+Flecha` ajusta la ventana y los disparadores disponibles de Snap Layouts muestran destinos; el ajuste no recorta contenido esencial. |
| Teclado y lectores de pantalla | La prueba de teclado y lector de pantalla definida en `accessibility.md` pasa para cada comando de ventana. |
| DPI por monitor y varios monitores | Pasa la matriz de DPI por monitor siguiente, sin contenido esencial recortado, controles inaccesibles ni escala anterior despues de cada traslado. |
| Alto contraste y temas claro/oscuro | En alto contraste, tema claro y tema oscuro, foco, seleccion y comandos conservan nombre accesible, contraste y operacion. |
| Implementacion reutilizable | La inspeccion de la solucion identifica una unica implementacion de shell/chrome usada por todas las ventanas que la requieran. |

### Matriz de DPI por Monitor

Ejecutar cada fila en Windows 10 22H2 y Windows 11 23H2 o posterior. En cada traslado, probar ventana restaurada y maximizada; moverla de Monitor A a B y de B a A con arrastre y con `Win+Mayus+Flecha` cuando este disponible.

| Monitor A | Monitor B | Evidencia de aprobado |
|---:|---:|---|
| 100% | 150% | La ventana actualiza escala al entrar en cada monitor; titulo, controles, foco y contenido esencial siguen visibles. |
| 125% | 200% | Despues de ambos traslados, no quedan coordenadas fuera de pantalla ni texto, botones, dialogos o bordes de cambio de tamano recortados. |
| 150% | 100% | Al restaurar, maximizar y usar Snap en cada monitor cuando este disponible, `Alt+Espacio`, minimizar, maximizar/restaurar, cerrar y el foco de teclado siguen funcionando. |

## Historial de Navegacion

- Atras y Adelante recorren vistas, no repiten Calcular, Finalizar, Generar, Contabilizar o Exportar.
- Regresar a una lista conserva busqueda, filtros, orden, agrupacion, pagina y fila seleccionada.
- Cerrar un panel contextual devuelve el foco al registro que lo abrio.

## Ventanas Secundarias

- La experiencia principal usa una sola ventana de trabajo.
- Los dialogos se reservan para decisiones breves y bloqueantes.
- Detalles extensos, formularios y vistas previas permanecen dentro del espacio de trabajo para conservar contexto.
- Vista previa de impresion es la unica experiencia que abre una ventana secundaria no modal. All forms, record details, and process previews remain inside the primary workspace.
