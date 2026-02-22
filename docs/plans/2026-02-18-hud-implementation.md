# HUD Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Crear HUD mínimo con HP bar, contador de isótopos y 3 indicadores de cooldown, conectado al Player vía Main.cs.

**Architecture:** HUD.tscn hijo del nodo `UI` (CanvasLayer) ya existente en Main.tscn. HUD.cs recibe referencias vía `Initialize(Player)` llamado desde Main.cs al instanciar el Player. Sin autoloads nuevos.

**Tech Stack:** Godot 4.6, C# (.NET 10.0), Godot Control nodes (ProgressBar, Label, HBoxContainer)

**Verificación:** `dotnet build` sin errores. Manual en Godot (F5): HP bar baja al recibir daño, contador de isótopos sube al recoger pickup, barras de cooldown se vacían al disparar y se rellenan.

---

## Task 1: Crear HUD.cs

**Files:**
- Create: `Scenes/UI/HUD/HUD.cs`

**Firmas importantes (verificadas en código):**
- `EntityStatsComponent.HealthChanged` → `delegate void HealthChangedEventHandler(int newHealth)` — solo pasa `newHealth`, el max se lee de `_statsComp.MaxHealth`
- `EconomyManager.IsotopesChanged` → `event System.Action<int>` — pasa el total nuevo
- `AttackManagerComponent.GetCooldownProgress(slot)` → `float` 0-1 (1 = cooldown lleno, 0 = listo). Para la barra: `1f - progress` = llena cuando listo

**Step 1: Crear el directorio y el archivo**

```bash
mkdir -p Scenes/UI/HUD
```

**Step 2: Escribir HUD.cs**

```csharp
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Economy;
using RotOfTime.Core.Entities.Components;
using RotOfTime.Scenes.Player;
using RotOfTime.Scenes.Player.Components;

namespace RotOfTime.Scenes.UI.HUD;

public partial class HUD : Control
{
    [Export] private ProgressBar _hpBar;
    [Export] private Label _isotopeLabel;
    [Export] private ProgressBar _basicIndicator;
    [Export] private ProgressBar _spell1Indicator;
    [Export] private ProgressBar _spell2Indicator;

    private EntityStatsComponent _statsComp;
    private EconomyManager _economy;
    private PlayerAttackManager _attackManager;

    public void Initialize(Player player)
    {
        _statsComp = player.EntityStatsComponent;
        _statsComp.HealthChanged += OnHealthChanged;
        OnHealthChanged(_statsComp.CurrentHealth);

        _economy = GameManager.Instance.EconomyManager;
        _economy.IsotopesChanged += OnIsotopesChanged;
        OnIsotopesChanged(_economy.Isotopes);

        _attackManager = player.AttackManager;

        Visible = true;
    }

    public void Teardown()
    {
        if (_statsComp != null) _statsComp.HealthChanged -= OnHealthChanged;
        if (_economy != null)   _economy.IsotopesChanged -= OnIsotopesChanged;
        _attackManager = null;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_attackManager == null) return;
        _basicIndicator.Value  = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.BasicAttack);
        _spell1Indicator.Value = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.Spell1);
        _spell2Indicator.Value = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.Spell2);
    }

    private void OnHealthChanged(int newHealth)
    {
        if (_statsComp == null) return;
        _hpBar.Value = (float)newHealth / _statsComp.MaxHealth;
    }

    private void OnIsotopesChanged(int amount)
    {
        _isotopeLabel.Text = $"{amount} isótopos";
    }
}
```

**Step 3: Build**

```bash
dotnet build
```

Expected: 0 errores.

---

## Task 2: Crear HUD.tscn

**Files:**
- Create: `Scenes/UI/HUD/HUD.tscn`

**Nota de layout:** Los indicadores de cooldown usan `min_value = 0`, `max_value = 1`, `value = 1` (lleno = listo). HP bar igual. El HUD empieza invisible (`visible = false`) — `Initialize()` lo pone visible.

**Step 1: Escribir HUD.tscn**

```
[gd_scene format=3 uid="uid://hud_scene"]

[ext_resource type="Script" path="res://Scenes/UI/HUD/HUD.cs" id="1_hud"]

[node name="HUD" type="Control" node_paths=PackedStringArray("_hpBar", "_isotopeLabel", "_basicIndicator", "_spell1Indicator", "_spell2Indicator")]
script = ExtResource("1_hud")
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
visible = false
_hpBar = NodePath("HPBar")
_isotopeLabel = NodePath("IsotopeLabel")
_basicIndicator = NodePath("AttackSlots/BasicAttackIndicator")
_spell1Indicator = NodePath("AttackSlots/Spell1Indicator")
_spell2Indicator = NodePath("AttackSlots/Spell2Indicator")

[node name="HPBar" type="ProgressBar" parent="."]
layout_mode = 1
anchors_preset = -1
anchor_left = 0.0
anchor_top = 0.0
anchor_right = 0.3
anchor_bottom = 0.0
offset_top = 10.0
offset_bottom = 30.0
min_value = 0.0
max_value = 1.0
value = 1.0

[node name="IsotopeLabel" type="Label" parent="."]
layout_mode = 1
anchors_preset = -1
anchor_left = 0.35
anchor_top = 0.0
anchor_right = 0.65
anchor_bottom = 0.0
offset_top = 10.0
offset_bottom = 30.0
text = "0 isótopos"

[node name="AttackSlots" type="HBoxContainer" parent="."]
layout_mode = 1
anchors_preset = -1
anchor_left = 0.0
anchor_top = 1.0
anchor_right = 0.5
anchor_bottom = 1.0
offset_top = -40.0
offset_bottom = -10.0
separation = 8

[node name="BasicAttackIndicator" type="ProgressBar" parent="AttackSlots"]
layout_mode = 2
custom_minimum_size = Vector2(60, 20)
min_value = 0.0
max_value = 1.0
value = 1.0

[node name="Spell1Indicator" type="ProgressBar" parent="AttackSlots"]
layout_mode = 2
custom_minimum_size = Vector2(60, 20)
min_value = 0.0
max_value = 1.0
value = 1.0

[node name="Spell2Indicator" type="ProgressBar" parent="AttackSlots"]
layout_mode = 2
custom_minimum_size = Vector2(60, 20)
min_value = 0.0
max_value = 1.0
value = 1.0
```

**Nota:** Godot auto-generará el `.uid` real al abrir el proyecto. El UID en el archivo es placeholder — no causa error de compilación, solo warning en el editor que se resuelve solo al guardar la escena.

**Step 2: Build**

```bash
dotnet build
```

Expected: 0 errores.

---

## Task 3: Añadir HUD a Main.tscn

**Files:**
- Modify: `Scenes/Main/Main.tscn`

El nodo `UI` (CanvasLayer) ya existe. Hay que:
1. Añadir `ext_resource` para HUD.tscn
2. Añadir `_hud` a `node_paths` del nodo Main
3. Añadir propiedad `_hud = NodePath(...)` al nodo Main
4. Añadir nodo HUD como instancia de HUD.tscn bajo UI

**Step 1: Leer Main.tscn actual**

Estado actual (verificado):
```
[gd_scene format=3 uid="uid://bx6kvvtoehxks"]
[ext_resource type="Script" uid="uid://bj8y5oo2tmkfh" path="res://Scenes/Main/Main.cs" id="1_p8rbg"]
[node name="Main" type="Node2D" unique_id=... node_paths=PackedStringArray("_camera", "_worldContainer")]
...
[node name="UI" type="CanvasLayer" parent="." ...]
```

**Step 2: Aplicar cambios**

Cambio 1 — añadir ext_resource para HUD.tscn justo después de la línea del script:
```
[ext_resource type="PackedScene" path="res://Scenes/UI/HUD/HUD.tscn" id="2_hud"]
```

Cambio 2 — actualizar `node_paths` del nodo Main para incluir `_hud`:
```
node_paths=PackedStringArray("_camera", "_worldContainer", "_hud")
```

Cambio 3 — añadir propiedad `_hud` al nodo Main (junto a `_camera` y `_worldContainer`):
```
_hud = NodePath("UI/HUD")
```

Cambio 4 — añadir nodo HUD como hijo de UI al final del archivo:
```
[node name="HUD" parent="UI" instance=ExtResource("2_hud")]
```

**Step 3: Build**

```bash
dotnet build
```

Expected: 0 errores.

---

## Task 4: Modificar Main.cs

**Files:**
- Modify: `Scenes/Main/Main.cs`

**Step 1: Añadir using y export**

Añadir using al bloque existente:
```csharp
using RotOfTime.Scenes.UI.HUD;
```

Añadir campo exportado junto a `_camera` y `_worldContainer`:
```csharp
[Export] private HUD _hud;
```

**Step 2: Añadir Initialize en OnSceneChangeRequested**

Al final de `OnSceneChangeRequested`, después de posicionar el player:
```csharp
_hud.Initialize(_player);
```

**Step 3: Añadir Teardown en OnMenuChangeRequested**

Al inicio de `OnMenuChangeRequested`, antes de destruir el player:
```csharp
_hud.Teardown();
```

**Step 4: Build**

```bash
dotnet build
```

Expected: 0 errores.

---

## Task 5: Verificación manual en Godot

**Step 1:** Abrir Godot, F5 para correr el juego.

**Step 2:** Iniciar partida desde el menú. Verificar:
- [ ] HUD visible con HP bar llena
- [ ] Label muestra "0 isótopos"
- [ ] 3 barras de cooldown visibles y llenas

**Step 3:** Disparar ataque básico (LMB). Verificar:
- [ ] BasicAttackIndicator baja y se rellena en ~0.4s

**Step 4:** Disparar Fireball (tecla 1). Verificar:
- [ ] Spell1Indicator baja y se rellena en ~2s

**Step 5:** Disparar IceShard (tecla 2). Verificar:
- [ ] Spell2Indicator baja y se rellena en ~1.2s

**Step 6:** Acercarse a un enemigo y recibir daño. Verificar:
- [ ] HP bar baja proporcionalmente

**Step 7:** Matar enemigo y recoger isótopo. Verificar:
- [ ] Label actualiza a "10 isótopos"

**Step 8:** Volver al menú principal. Verificar:
- [ ] HUD se oculta (no visible en menú)
