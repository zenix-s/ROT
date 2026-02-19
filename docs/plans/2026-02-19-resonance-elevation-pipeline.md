# Resonance & Elevation Pipeline — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement collectible resonances and elevation items with bonfire activation UI.

**Architecture:** InventoryManager (Dictionary<string,int>) centralizes collectibles. ProgressionManager simplified to int counter. ResonanceTrigger + ElevationItem are Area2D pickups. BonfireMenu is a CanvasLayer in Main.tscn.

**Tech Stack:** Godot 4.6, C# .NET 10, System.Text.Json (already used by SaveManager)

**Design doc:** `docs/plans/2026-02-19-resonance-elevation-pipeline-design.md`

**Validation:** No test suite. Each task validates with `dotnet build` from project root. Manual Godot testing at Task 10.

---

### Task 1: Simplificar ProgressionManager + MetaData

**Files:**
- Modify: `Autoload/ProgressionManager.cs`
- Modify: `Core/GameData/MetaData.cs`
- Modify: `Autoload/GameManager.cs`

**Step 1: Reemplazar ProgressionManager.cs completo**

```csharp
using Godot;

namespace RotOfTime.Autoload;

/// <summary>
///     Tracks activated resonances and current elevation.
///     Plain C# class owned by GameManager (not a Godot Node).
/// </summary>
public class ProgressionManager
{
    public int CurrentElevation { get; set; } = 1;
    public int ActivatedResonances { get; private set; }

    public void ActivateResonance()
    {
        ActivatedResonances++;
        GD.Print($"ProgressionManager: Resonance activated (total: {ActivatedResonances})");
    }

    public void AdvanceElevation()
    {
        CurrentElevation++;
        GD.Print($"ProgressionManager: Advanced to Elevation {CurrentElevation}");
    }

    public float GetHealthMultiplier() => 1.0f + ActivatedResonances * 0.20f;
    public float GetDamageMultiplier() => 1.0f + ActivatedResonances * 0.10f;

    public void Load(int activatedResonances, int currentElevation)
    {
        ActivatedResonances = activatedResonances;
        CurrentElevation = currentElevation;
    }
}
```

**Step 2: Actualizar MetaData.cs**

Eliminar la línea:
```csharp
public List<string> UnlockedResonances { get; set; } = [];
```

Añadir:
```csharp
public int ActivatedResonances { get; set; }
public Dictionary<string, int> Inventory { get; set; } = new();
```

Añadir el using al inicio del archivo si no existe:
```csharp
using System.Collections.Generic;
```

**Step 3: Actualizar LoadMeta y SaveMeta en GameManager.cs**

En `LoadMeta()`, reemplazar las dos líneas de ProgressionManager:
```csharp
// Antes:
ProgressionManager.CurrentElevation = Meta.CurrentElevation;
ProgressionManager.LoadResonanceKeys(Meta.UnlockedResonances);

// Después:
ProgressionManager.Load(Meta.ActivatedResonances, Meta.CurrentElevation);
```

En `SaveMeta()`, reemplazar las dos líneas de ProgressionManager:
```csharp
// Antes:
Meta.CurrentElevation = ProgressionManager.CurrentElevation;
Meta.UnlockedResonances = ProgressionManager.GetResonanceKeys();

// Después:
Meta.CurrentElevation = ProgressionManager.CurrentElevation;
Meta.ActivatedResonances = ProgressionManager.ActivatedResonances;
```

También actualizar el GD.Print de debug en LoadMeta:
```csharp
// Antes:
GD.Print($"Progression: Elevation {ProgressionManager.CurrentElevation}, " +
         $"HP mult {ProgressionManager.GetHealthMultiplier():F2}x, " +
         $"DMG mult {ProgressionManager.GetDamageMultiplier():F2}x");

// Después (igual, sigue funcionando):
GD.Print($"Progression: Elevation {ProgressionManager.CurrentElevation}, " +
         $"Resonances {ProgressionManager.ActivatedResonances}, " +
         $"HP mult {ProgressionManager.GetHealthMultiplier():F2}x, " +
         $"DMG mult {ProgressionManager.GetDamageMultiplier():F2}x");
```

**Step 4: Build**

```bash
dotnet build
```

Expected: 0 errors. Si hay error "does not contain a definition for 'LoadResonanceKeys'" u otro método eliminado, buscar cualquier otro lugar que los llame y eliminar esas referencias.

**Step 5: Commit**

```bash
git add Autoload/ProgressionManager.cs Core/GameData/MetaData.cs Autoload/GameManager.cs
git commit -m "refactor: simplificar ProgressionManager a contador de resonancias"
```

---

### Task 2: Crear InventoryManager

**Files:**
- Create: `Autoload/InventoryManager.cs`
- Modify: `Autoload/GameManager.cs`

**Step 1: Crear Autoload/InventoryManager.cs**

```csharp
using System.Collections.Generic;
using Godot;

namespace RotOfTime.Autoload;

/// <summary>
///     Manages all collectible items as a Dictionary&lt;string, int&gt;.
///     Plain C# class owned by GameManager (not a Godot Node).
///
///     Known item IDs:
///       "resonance"   — collected resonance (fungible, activatable at bonfire)
///       "elevation_N" — elevation item dropped by boss N
/// </summary>
public class InventoryManager
{
    private Dictionary<string, int> _items = new();

    public void AddItem(string id, int amount = 1)
    {
        _items.TryGetValue(id, out int current);
        _items[id] = current + amount;
        GD.Print($"InventoryManager: +{amount}x '{id}' (total: {_items[id]})");
    }

    public bool RemoveItem(string id, int amount = 1)
    {
        if (!HasItem(id, amount)) return false;
        _items[id] -= amount;
        if (_items[id] <= 0) _items.Remove(id);
        GD.Print($"InventoryManager: -{amount}x '{id}'");
        return true;
    }

    public bool HasItem(string id, int amount = 1)
    {
        return _items.TryGetValue(id, out int count) && count >= amount;
    }

    public int GetQuantity(string id)
    {
        return _items.TryGetValue(id, out int count) ? count : 0;
    }

    public Dictionary<string, int> GetAllItems() => new(_items);

    public void Load(Dictionary<string, int> items)
    {
        _items = items != null ? new Dictionary<string, int>(items) : new Dictionary<string, int>();
    }
}
```

**Step 2: Añadir InventoryManager a GameManager.cs**

Añadir la propiedad junto a los otros managers:
```csharp
public InventoryManager InventoryManager { get; private set; }
```

En `_Ready()`, añadir la instanciación:
```csharp
InventoryManager = new InventoryManager();
```

En `LoadMeta()`, añadir la carga:
```csharp
InventoryManager.Load(Meta.Inventory);
```

En `SaveMeta()`, añadir el guardado:
```csharp
Meta.Inventory = InventoryManager.GetAllItems();
```

**Step 3: Build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 4: Commit**

```bash
git add Autoload/InventoryManager.cs Autoload/GameManager.cs
git commit -m "feat: añadir InventoryManager con Dictionary<string,int>"
```

---

### Task 3: Player.ApplyAllMultipliers → public

**Files:**
- Modify: `Scenes/Player/Player.cs`

**Step 1: Cambiar visibilidad**

En `Player.cs`, localizar la línea:
```csharp
private void ApplyAllMultipliers()
```

Cambiarla a:
```csharp
public void ApplyAllMultipliers()
```

**Step 2: Build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 3: Commit**

```bash
git add Scenes/Player/Player.cs
git commit -m "refactor: ApplyAllMultipliers público para uso desde BonfireMenu"
```

---

### Task 4: Input action 'interact' (paso manual en Godot)

**NOTA: Este paso lo realiza el desarrollador manualmente en el editor de Godot.**

1. Abrir Godot Editor
2. Ir a Project > Project Settings > Input Map
3. Añadir nueva acción: `interact`
4. Asignarle la tecla `E`
5. Guardar (los cambios se escriben en `project.godot`)

**Step de verificación:**

Tras añadir la acción, verificar que `project.godot` contiene:
```
[input]
interact={...}
```

---

### Task 5: ResonanceTrigger scene

**Files:**
- Create: `Core/Progression/ResonanceTrigger.cs`
- Create: `Core/Progression/ResonanceTrigger.tscn`

**Step 1: Crear Core/Progression/ResonanceTrigger.cs**

```csharp
using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Core.Progression;

/// <summary>
///     Area2D pickup. Player walks over it to collect a resonance item.
///     Place in level scenes. Configure collision mask = layer 2 (Player).
/// </summary>
[GlobalClass]
public partial class ResonanceTrigger : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        GameManager.Instance.InventoryManager.AddItem("resonance");
        GameManager.Instance.SaveMeta();
        GD.Print("ResonanceTrigger: Resonance collected!");
        QueueFree();
    }
}
```

**Step 2: Crear Core/Progression/ResonanceTrigger.tscn**

```
[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://Core/Progression/ResonanceTrigger.cs" id="1_resscript"]

[sub_resource type="CircleShape2D" id="1_resshape"]
radius = 16.0

[node name="ResonanceTrigger" type="Area2D" script=ExtResource("1_resscript")]
collision_layer = 0
collision_mask = 2

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
shape = SubResource("1_resshape")

[node name="Sprite2D" type="Sprite2D" parent="."]
modulate = Color(0.2, 0.8, 1, 1)
```

El `Sprite2D` es placeholder visual (cuadrado cyan). El desarrollador puede reemplazarlo con un sprite real más adelante.

**Step 3: Build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 4: Commit**

```bash
git add Core/Progression/ResonanceTrigger.cs Core/Progression/ResonanceTrigger.tscn
git commit -m "feat: añadir ResonanceTrigger — pickup de resonancias en nivel"
```

---

### Task 6: ElevationItem scene

**Files:**
- Create: `Core/Progression/ElevationItem.cs`
- Create: `Core/Progression/ElevationItem.tscn`

**Step 1: Crear Core/Progression/ElevationItem.cs**

```csharp
using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Core.Progression;

/// <summary>
///     Area2D pickup dropped by a boss. Allows advancing elevation at the bonfire.
///     Export: set Elevation to match the boss's elevation number (1, 2, 3...).
/// </summary>
[GlobalClass]
public partial class ElevationItem : Area2D
{
    [Export] public int Elevation { get; set; } = 1;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        string itemId = $"elevation_{Elevation}";
        GameManager.Instance.InventoryManager.AddItem(itemId);
        GameManager.Instance.SaveMeta();
        GD.Print($"ElevationItem: elevation_{Elevation} collected!");
        QueueFree();
    }
}
```

**Step 2: Crear Core/Progression/ElevationItem.tscn**

```
[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://Core/Progression/ElevationItem.cs" id="1_elevscript"]

[sub_resource type="CircleShape2D" id="1_elevshape"]
radius = 20.0

[node name="ElevationItem" type="Area2D" script=ExtResource("1_elevscript")]
collision_layer = 0
collision_mask = 2

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
shape = SubResource("1_elevshape")

[node name="Sprite2D" type="Sprite2D" parent="."]
modulate = Color(1, 0.8, 0.2, 1)
```

El `Sprite2D` es placeholder visual (cuadrado dorado).

**Step 3: Build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 4: Commit**

```bash
git add Core/Progression/ElevationItem.cs Core/Progression/ElevationItem.tscn
git commit -m "feat: añadir ElevationItem — drop del boss para ascender de elevación"
```

---

### Task 7: BonfireMenu

**Files:**
- Create: `Scenes/UI/BonfireMenu/BonfireMenu.cs`
- Create: `Scenes/UI/BonfireMenu/BonfireMenu.tscn`

**Step 1: Crear Scenes/UI/BonfireMenu/BonfireMenu.cs**

```csharp
using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Bonfire UI: activate resonances and advance elevation.
///     Lives in Main.tscn. Opens via Bonfire.cs (group "BonfireMenu").
///     Pauses the scene tree while open.
/// </summary>
public partial class BonfireMenu : CanvasLayer
{
    private Label _resonancesLabel;
    private Button _activateButton;
    private Label _elevationLabel;
    private Button _elevationButton;
    private Button _closeButton;

    private Player.Player _player;

    public override void _Ready()
    {
        AddToGroup("BonfireMenu");
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _resonancesLabel = GetNode<Label>("Panel/VBoxContainer/ResonancesLabel");
        _activateButton = GetNode<Button>("Panel/VBoxContainer/ActivateButton");
        _elevationLabel = GetNode<Label>("Panel/VBoxContainer/ElevationLabel");
        _elevationButton = GetNode<Button>("Panel/VBoxContainer/ElevationButton");
        _closeButton = GetNode<Button>("Panel/VBoxContainer/CloseButton");

        _activateButton.Pressed += OnActivatePressed;
        _elevationButton.Pressed += OnElevationPressed;
        _closeButton.Pressed += OnClosePressed;
    }

    public void Initialize(Player.Player player)
    {
        _player = player;
    }

    public void Open()
    {
        Visible = true;
        GetTree().Paused = true;
        Refresh();
    }

    public void ForceClose()
    {
        Visible = false;
        GetTree().Paused = false;
    }

    private void Refresh()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        int resonances = inv.GetQuantity("resonance");
        _resonancesLabel.Text = $"Resonancias disponibles: {resonances}";
        _activateButton.Disabled = resonances <= 0;

        string elevKey = $"elevation_{prog.CurrentElevation}";
        bool hasElevItem = inv.HasItem(elevKey);
        _elevationLabel.Visible = hasElevItem;
        _elevationButton.Visible = hasElevItem;
        if (hasElevItem)
            _elevationLabel.Text = $"Item de Elevación {prog.CurrentElevation} recogido";
    }

    private void OnActivatePressed()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        if (!inv.RemoveItem("resonance")) return;
        prog.ActivateResonance();
        _player?.ApplyAllMultipliers();
        GameManager.Instance.SaveMeta();
        Refresh();
    }

    private void OnElevationPressed()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        string elevKey = $"elevation_{prog.CurrentElevation}";
        if (!inv.RemoveItem(elevKey)) return;
        prog.AdvanceElevation();
        _player?.ApplyAllMultipliers();
        GameManager.Instance.SaveMeta();
        Refresh();
    }

    private void OnClosePressed()
    {
        ForceClose();
    }
}
```

**Step 2: Crear Scenes/UI/BonfireMenu/BonfireMenu.tscn**

```
[gd_scene load_steps=2 format=3]

[ext_resource type="Script" path="res://Scenes/UI/BonfireMenu/BonfireMenu.cs" id="1_bfmenuscript"]

[node name="BonfireMenu" type="CanvasLayer" script=ExtResource("1_bfmenuscript")]
layer = 10

[node name="Panel" type="Panel" parent="."]
offset_left = 200.0
offset_top = 100.0
offset_right = 600.0
offset_bottom = 480.0

[node name="VBoxContainer" type="VBoxContainer" parent="Panel"]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
offset_left = 20.0
offset_top = 20.0
offset_right = -20.0
offset_bottom = -20.0
theme_override_constants/separation = 12

[node name="TitleLabel" type="Label" parent="Panel/VBoxContainer"]
text = "Bonfire"

[node name="HSeparator" type="HSeparator" parent="Panel/VBoxContainer"]

[node name="ResonancesLabel" type="Label" parent="Panel/VBoxContainer"]
text = "Resonancias disponibles: 0"

[node name="ActivateButton" type="Button" parent="Panel/VBoxContainer"]
text = "Activar Resonancia"

[node name="HSeparator2" type="HSeparator" parent="Panel/VBoxContainer"]

[node name="ElevationLabel" type="Label" parent="Panel/VBoxContainer"]
text = "Item de Elevación recogido"
visible = false

[node name="ElevationButton" type="Button" parent="Panel/VBoxContainer"]
text = "Ascender de Elevación"
visible = false

[node name="HSeparator3" type="HSeparator" parent="Panel/VBoxContainer"]

[node name="CloseButton" type="Button" parent="Panel/VBoxContainer"]
text = "Cerrar"
```

**Step 3: Build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 4: Commit**

```bash
git add Scenes/UI/BonfireMenu/BonfireMenu.cs Scenes/UI/BonfireMenu/BonfireMenu.tscn
git commit -m "feat: añadir BonfireMenu — UI para activar resonancias y ascender"
```

---

### Task 8: Bonfire scene

**Files:**
- Create: `Scenes/World/Bonfire/Bonfire.cs`
- Create: `Scenes/World/Bonfire/Bonfire.tscn`

**Step 1: Crear Scenes/World/Bonfire/Bonfire.cs**

```csharp
using Godot;
using RotOfTime.Scenes.UI;

namespace RotOfTime.Scenes.World;

/// <summary>
///     Interactable bonfire placed in levels.
///     Player presses 'interact' (E) while inside DetectionArea to open BonfireMenu.
/// </summary>
public partial class Bonfire : Node2D
{
    private bool _playerInRange;

    public override void _Ready()
    {
        var area = GetNode<Area2D>("DetectionArea");
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        if (_playerInRange && Input.IsActionJustPressed("interact"))
            OpenMenu();
    }

    private void OpenMenu()
    {
        var menuNode = GetTree().GetFirstNodeInGroup("BonfireMenu");
        if (menuNode is BonfireMenu menu)
            menu.Open();
    }

    private void OnBodyEntered(Node2D body)
    {
        _playerInRange = true;
        GD.Print("Bonfire: player in range");
    }

    private void OnBodyExited(Node2D body)
    {
        _playerInRange = false;
    }
}
```

**Step 2: Crear Scenes/World/Bonfire/Bonfire.tscn**

```
[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://Scenes/World/Bonfire/Bonfire.cs" id="1_bonfirescr"]

[sub_resource type="CircleShape2D" id="1_bonfshape"]
radius = 48.0

[node name="Bonfire" type="Node2D" script=ExtResource("1_bonfirescr")]

[node name="Sprite2D" type="Sprite2D" parent="."]
modulate = Color(1, 0.5, 0.1, 1)

[node name="DetectionArea" type="Area2D" parent="."]
collision_layer = 0
collision_mask = 2

[node name="CollisionShape2D" type="CollisionShape2D" parent="DetectionArea"]
shape = SubResource("1_bonfshape")
```

El `Sprite2D` es placeholder (cuadrado naranja). Radio de detección: 48px.

**Step 3: Build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 4: Commit**

```bash
git add Scenes/World/Bonfire/Bonfire.cs Scenes/World/Bonfire/Bonfire.tscn
git commit -m "feat: añadir Bonfire — interactable que abre BonfireMenu con tecla E"
```

---

### Task 9: Añadir BonfireMenu a Main

**Files:**
- Modify: `Scenes/Main/Main.cs`
- Modify: `Scenes/Main/Main.tscn`

**Step 1: Modificar Main.cs**

Añadir el using al inicio:
```csharp
using RotOfTime.Scenes.UI;
```

Añadir la propiedad exportada junto a `_hud`:
```csharp
[Export] private BonfireMenu _bonfireMenu;
```

En `OnSceneChangeRequested`, después de `_hud.Initialize(_player)`:
```csharp
_bonfireMenu.Initialize(_player);
```

En `OnMenuChangeRequested`, después de `_hud.Teardown()`:
```csharp
_bonfireMenu?.ForceClose();
```

**Step 2: Añadir BonfireMenu a Main.tscn**

Abrir `Scenes/Main/Main.tscn` en editor de texto. Localizar el nodo `UI` (CanvasLayer que contiene HUD). Añadir BonfireMenu como hijo del mismo CanvasLayer o de Main directamente.

Si existe un nodo `UI` de tipo `CanvasLayer`:
```
[ext_resource type="PackedScene" path="res://Scenes/UI/BonfireMenu/BonfireMenu.tscn" id="XX_bfmenu"]

[node name="BonfireMenu" parent="UI" instance=ExtResource("XX_bfmenu")]
```

Si no existe nodo UI o se prefiere añadir directo a Main:
```
[node name="BonfireMenu" parent="." instance=ExtResource("XX_bfmenu")]
```

**IMPORTANTE:** Asignar el nodo BonfireMenu al export `_bonfireMenu` de Main en el editor (o añadir la propiedad NodePath en el .tscn junto al nodo Main).

**Step 3: Build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 4: Commit**

```bash
git add Scenes/Main/Main.cs Scenes/Main/Main.tscn
git commit -m "feat: integrar BonfireMenu en Main — inicializar con player al cargar nivel"
```

---

### Task 10: Marcar tarea en tasks.md + test manual

**Step 1: Actualizar docs/tasks.md**

Mover la tarea del pipeline de resonancias de "En el cajón" a "Completado":
```markdown
- [x] Resonance & Elevation pipeline (InventoryManager, ResonanceTrigger, ElevationItem, BonfireMenu, Bonfire)
```

**Step 2: Test manual en Godot (F5)**

1. Abrir Godot Editor → F5
2. **Test ResonanceTrigger:** Colocar una `ResonanceTrigger.tscn` en el nivel. Caminar encima. Verificar en Output: `"InventoryManager: +1x 'resonance'"`. El pickup desaparece.
3. **Test Bonfire:** Colocar `Bonfire.tscn` en el nivel. Acercarse y pulsar `E`. Verificar que BonfireMenu aparece y el juego se pausa.
4. **Test activar resonancia:** Con resonancia en inventario, pulsar "Activar Resonancia". Verificar en Output: `"ProgressionManager: Resonance activated (total: 1)"`. HP del player sube (+20%).
5. **Test ElevationItem:** Colocar `ElevationItem.tscn` (Elevation=1). Recoger. Verificar que el botón "Ascender de Elevación" aparece en BonfireMenu.
6. **Test ascender:** Pulsar "Ascender de Elevación". Verificar `CurrentElevation = 2` en Output.
7. **Test save/load:** Cerrar juego, relanzar. Verificar que resonancias activadas y elevación persisten.

**Step 3: Commit final**

```bash
git add docs/tasks.md
git commit -m "docs: marcar pipeline resonancias/elevaciones como completado"
```
