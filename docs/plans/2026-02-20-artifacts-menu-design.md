# Design: ArtifactsMenu — Mesa de Artefactos

**Fecha:** 2026-02-20
**Estado:** Aprobado

## Contexto

Los artefactos estaban integrados como sub-panel dentro de `BonfireMenu`. El desarrollador quiere que sean un menú independiente accesible en cualquier momento via hotkey (K), con dos tabs: Equipar y Craftear (C-2).

## Estructura de escena

```
ArtifactsMenu (CanvasLayer)           ← siempre en Main.tscn (UI node), Visible=false
  Container (Control, fullscreen)
    Panel (~420×420, centrado)
      VBoxContainer
        TitleLabel "Mesa de Artefactos"
        HSeparator
        TabsRow (HBoxContainer)
          EquipTabButton  "Equipar"
          CraftTabButton  "Craftear"
        HSeparator
        EquipPanel (VBoxContainer)
          SlotsLabel
          ArtifactsListContainer (VBoxContainer) ← filas dinámicas
        CraftPanel (VBoxContainer, hidden por defecto)
          CraftListContainer (VBoxContainer) ← filas dinámicas
```

## Comportamiento

- `K` (`artifacts_menu` input action) hace toggle del menú.
- No abre si `GameManager.IsMenuOpen` es true (otro menú abierto).
- Al abrir: tab Equipar activo por defecto, llama `RefreshEquip()`.
- Tabs: click en "Equipar" / "Craftear" muestra su panel y oculta el otro, refresh correspondiente.
- Al cerrar: `IsMenuOpen = false`.

## Tab Equipar

Misma lógica que el `ArtifactsPanel` eliminado de BonfireMenu:
- `SlotsLabel`: "Slots: X/Y"
- Filas dinámicas: `"Nombre (Xsl) +20%HP +15%DMG"` + botón "Equipar" / "Desequipar"
- Botón disabled si no hay slots libres y no está equipado
- Al equipar/desequipar: `ApplyAllMultipliers()` + `SaveMeta()` + `RefreshEquip()`

## Tab Craftear (C-2)

- `ArtifactData` gana `[Export] public int IsotopeCost { get; set; }`
- Los tres `.tres` existentes se actualizan con su coste en isótopos
- Filas dinámicas: `"Nombre — X isótopos"` + botón "Craftear"
- Botón disabled si: ya owned, o `EconomyManager.Isotopes < IsotopeCost`
- Al craftear: `EconomyManager.TrySpend(cost)` → `ArtifactManager.AddOwned(artifact)` → `SaveMeta()` → `RefreshCraft()`
- Lista hardcoded de los tres `.tres` (no discovery automático — YAGNI)

## Cambios en archivos existentes

### `BonfireMenu.tscn` / `BonfireMenu.cs`
- Eliminar `ArtifactsButton`, `ArtifactsPanel` y todo el código de artefactos.
- Vuelve al menú simple de resonancias + elevación.

### `Main.tscn`
- Añadir instancia de `ArtifactsMenu.tscn` bajo el nodo `UI`.

### `GameManager.cs`
- El seed temporal de artefactos se mantiene (sigue siendo necesario para testeo).
- Cuando C-2 esté implementado, el seed se elimina.

### `ArtifactData.cs`
- Añadir `[Export] public int IsotopeCost { get; set; } = 0`

### `project.godot`
- Añadir input action `artifacts_menu` mapeada a tecla K.

## Archivos nuevos

- `Scenes/UI/ArtifactsMenu/ArtifactsMenu.tscn`
- `Scenes/UI/ArtifactsMenu/ArtifactsMenu.cs`

## Costes de crafteo (propuesta)

| Artefacto        | Slots | IsotopeCost |
|------------------|-------|-------------|
| Escudo de Grafito | 1    | 30          |
| Lente de Foco    | 1     | 30          |
| Nucleo Denso     | 2     | 60          |
