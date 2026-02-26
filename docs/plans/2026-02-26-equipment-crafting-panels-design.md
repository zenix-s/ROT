# EquipmentPanel + CraftingPanel — Diseño

**Fecha:** 2026-02-26

## Contexto

El `GlobalMenu` tiene cuatro tabs: ElevationPanel (0), SkillsPanel (1), CraftingPanel (2), EquipmentPanel (3).

Estado previo:
- `EquipmentPanel.tscn` rota — exports `ArtifactsListContainer` y `SlotsLabel` no wired → NullReference en runtime
- `CraftingPanel.tscn` funcional pero `MarginContainer` en offset layout en lugar de full anchors
- `ArtifactRow.Setup()` sin comportamiento propio — botón y lógica inyectados desde el padre (anti-pattern)
- `ArtifactsPanel.cs` dead code (legacy monolítico, GlobalMenu ya no lo usa)

## Principio de diseño

Los componentes Godot son autocontenidos. Conocen su propio estado, gestionan su UI y sus acciones internamente. Comunican hacia arriba exclusivamente mediante signals — el padre no inyecta callbacks ni lógica.

## Componentes

### ArtifactRow (solo EquipmentPanel)

Ubicación: `Scenes/UI/GlobalMenu/EquipmentPanel/Components/ArtifactRow/`

**Responsabilidades:**
- Conoce su `ArtifactType` (seteado antes de `_Ready()` via factory)
- En `_Ready()`: carga `ArtifactData`, setea nombre, texto de botón ("Equipar"/"Desequipar"), disabled state
- Botón conectado internamente → `OnActionPressed()` → `Equip()`/`Unequip()` → emite `ActionPerformed`

```csharp
[Signal] public delegate void ActionPerformedEventHandler();

public static ArtifactRow Create(PackedScene scene, ArtifactType type)
{
    var row = scene.Instantiate<ArtifactRow>();
    row._type = type;   // seteado antes de AddChild → antes de _Ready()
    return row;
}
```

Scene nodes: `NameLabel (Label)`, `ActionButton (Button)` — sin cambios respecto a la scene actual.

### CraftingRow (nuevo, solo CraftingPanel)

Ubicación: `Scenes/UI/GlobalMenu/CraftingPanel/Components/CraftingRow/`

**Responsabilidades:**
- Conoce su `ArtifactType`
- En `_Ready()`: nombre, StatusLabel ("ya obtenido" / "{cost} isótopos"), disabled si owned o sin fondos
- `OnActionPressed()`: `SpendIsotopes()` → `AddOwned()` → emite `ActionPerformed`

```csharp
[Signal] public delegate void ActionPerformedEventHandler();

public static CraftingRow Create(PackedScene scene, ArtifactType type) { ... }
```

Scene nodes: `NameLabel (Label)`, `StatusLabel (Label)`, `ActionButton (Button)`.

## Panels (padres)

El panel solo instancia, conecta la señal y hace AddChild. Los side-effects globales (SaveMeta, ApplyAllMultipliers) viven en el handler del panel porque el componente no debe conocer al Player ni a GameManager.

```csharp
// EquipmentPanel.Refresh()
var row = ArtifactRow.Create(ArtifactRowScene, type);
row.ActionPerformed += () =>
{
    player?.ApplyAllMultipliers();
    GameManager.Instance.SaveMeta();
    Refresh();
};
ArtifactsListContainer.AddChild(row); // _Ready() dispara aquí
```

```csharp
// CraftingPanel.Refresh()
var row = CraftingRow.Create(CraftingRowScene, type);
row.ActionPerformed += () =>
{
    GameManager.Instance.SaveMeta();
    Refresh();
};
CraftListContainer.AddChild(row);
```

## Scenes

### EquipmentPanel.tscn (reescrita)

```
Control (EquipmentPanel)
  node_paths: ArtifactsListContainer, SlotsLabel
  export: ArtifactRowScene (PackedScene)
  MarginContainer (anchors preset=15, full panel)
    VBoxContainer
      SlotsLabel (Label)
      ArtifactsListContainer (VBoxContainer) ← filas dinámicas
```

### CraftingPanel.tscn (arreglada)

- `MarginContainer` → anchors preset=15 (full panel)
- Export renombrado: `ArtifactRowScene` → `CraftingRowScene` apuntando a `CraftingRow.tscn`

## Limpieza

- Borrar `Scenes/UI/GlobalMenu/ArtifactsPanel.cs` y `.uid` (dead code)

## Verificación

Abrir GlobalMenu (tecla K):
- **Tab Crafting (2):** lista todos los artefactos con nombre, costo/estado; botón disabled si ya poseído o sin isótopos; craftear descuenta isótopos y refresca
- **Tab Equipment (3):** lista solo los poseídos; botón "Equipar"/"Desequipar" correcto; disabled si slots llenos y no está equipado; cambio aplica multiplicadores al player y guarda
