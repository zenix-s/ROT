# ArtifactsMenu Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Crear un menú dedicado de artefactos (Mesa de Artefactos) accesible con K en cualquier momento, con tabs Equipar y Craftear; eliminar los artefactos de BonfireMenu.

**Architecture:** `ArtifactsMenu` es un CanvasLayer siempre instanciado en `Main.tscn` bajo el nodo `UI`, con `Visible=false`. Toggle con input action `artifacts_menu` (K). Dos sub-paneles (`EquipPanel` / `CraftPanel`) intercambiables mediante tabs. `BonfireMenu` vuelve a ser el menú simple de resonancias.

**Tech Stack:** Godot 4.6, C# (.NET 10), sin suite de tests — verificación: `dotnet build` + F5 en Godot.

---

### Task 1: Añadir `IsotopeCost` a `ArtifactData` y actualizar los .tres

**Files:**
- Modify: `Core/Artifacts/ArtifactData.cs`
- Modify: `Resources/Artifacts/EscudoDeGrafito.tres`
- Modify: `Resources/Artifacts/LenteDeFoco.tres`
- Modify: `Resources/Artifacts/NucleoDenso.tres`

**Step 1: Añadir el campo en `ArtifactData.cs`**

Añadir después de `DamageBonus`:
```csharp
[Export] public int IsotopeCost { get; set; } = 0;
```

**Step 2: Actualizar los .tres**

`EscudoDeGrafito.tres` — añadir línea al final del bloque `[resource]`:
```
IsotopeCost = 30
```

`LenteDeFoco.tres` — añadir:
```
IsotopeCost = 30
```

`NucleoDenso.tres` — añadir:
```
IsotopeCost = 60
```

**Step 3: Build**
```
dotnet build
```
Esperado: `Build succeeded. 0 Error(s)`

**Step 4: Commit**
```bash
git add Core/Artifacts/ArtifactData.cs Resources/Artifacts/
git commit -m "feat: añadir IsotopeCost a ArtifactData para crafteo"
```

---

### Task 2: Registrar input action `artifacts_menu` (K) en `project.godot`

**Files:**
- Modify: `project.godot`

**Step 1: Añadir la acción después del bloque `debug_toggle`**

Localizar la línea `debug_toggle={...}` (aprox. línea 106) y añadir justo después:
```
artifacts_menu={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":75,"key_label":0,"unicode":107,"location":0,"echo":false,"script":null)
]
}
```

**Step 2: Verificar en Godot**
Abrir `Project > Project Settings > Input Map` y confirmar que aparece `artifacts_menu` mapeado a K.

**Step 3: Commit**
```bash
git add project.godot
git commit -m "feat: registrar input action artifacts_menu (tecla K)"
```

---

### Task 3: Limpiar BonfireMenu — eliminar todo lo relacionado con artefactos

**Files:**
- Modify: `Scenes/UI/BonfireMenu/BonfireMenu.tscn`
- Modify: `Scenes/UI/BonfireMenu/BonfireMenu.cs`

**Step 1: Actualizar `BonfireMenu.tscn`**

Eliminar los nodos `ArtifactsButton` y `ArtifactsPanel` (y todos sus hijos). El archivo debe quedar así (los nodos bajo `Container/Panel/VBoxContainer`):
- TitleLabel
- HSeparator
- ResonancesLabel
- ActivateButton
- HSeparator2
- ElevationLabel
- ElevationButton
- HSeparator3
- CloseButton

**Step 2: Reemplazar `BonfireMenu.cs` con la versión limpia**

```csharp
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Bonfire UI: activate resonances and advance elevation.
///     Instantiated on demand by Bonfire.cs. Destroys itself on close.
///     Player input is blocked via GameManager.IsMenuOpen while open.
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
        _resonancesLabel = GetNode<Label>("Container/Panel/VBoxContainer/ResonancesLabel");
        _activateButton = GetNode<Button>("Container/Panel/VBoxContainer/ActivateButton");
        _elevationLabel = GetNode<Label>("Container/Panel/VBoxContainer/ElevationLabel");
        _elevationButton = GetNode<Button>("Container/Panel/VBoxContainer/ElevationButton");
        _closeButton = GetNode<Button>("Container/Panel/VBoxContainer/CloseButton");

        _activateButton.Pressed += OnActivatePressed;
        _elevationButton.Pressed += OnElevationPressed;
        _closeButton.Pressed += OnClosePressed;
    }

    public void Open()
    {
        AddToGroup(Groups.BonfireMenu);
        _player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
        GameManager.Instance.IsMenuOpen = true;
        Visible = true;
        Refresh();
    }

    private void Close()
    {
        GameManager.Instance.IsMenuOpen = false;
        QueueFree();
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
        Close();
    }
}
```

> Nota: los node paths cambian de `"Panel/VBoxContainer/..."` a `"Container/Panel/VBoxContainer/..."` por el wrapper `Container` que añadió el editor en la sesión anterior.

**Step 3: Build**
```
dotnet build
```
Esperado: `Build succeeded. 0 Error(s)`

**Step 4: Commit**
```bash
git add Scenes/UI/BonfireMenu/
git commit -m "refactor: eliminar artefactos de BonfireMenu"
```

---

### Task 4: Crear `ArtifactsMenu.tscn`

**Files:**
- Create: `Scenes/UI/ArtifactsMenu/ArtifactsMenu.tscn`

**Step 1: Crear el directorio y el archivo .tscn**

Crear `Scenes/UI/ArtifactsMenu/ArtifactsMenu.tscn` con el siguiente contenido (el script se referencia por path; Godot genera el .uid en el siguiente build):

```
[gd_scene format=3]

[node name="ArtifactsMenu" type="CanvasLayer"]
layer = 10

[node name="Container" type="Control" parent="."]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2

[node name="Panel" type="Panel" parent="Container"]
layout_mode = 1
anchors_preset = 8
anchor_left = 0.5
anchor_top = 0.5
anchor_right = 0.5
anchor_bottom = 0.5
offset_left = -210.0
offset_top = -150.0
offset_right = 210.0
offset_bottom = 150.0
grow_horizontal = 2
grow_vertical = 2

[node name="VBoxContainer" type="VBoxContainer" parent="Container/Panel"]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
offset_left = 16.0
offset_top = 12.0
offset_right = -16.0
offset_bottom = -12.0
grow_horizontal = 2
grow_vertical = 2
theme_override_constants/separation = 8

[node name="TitleLabel" type="Label" parent="Container/Panel/VBoxContainer"]
layout_mode = 2
text = "Mesa de Artefactos"

[node name="HSeparator" type="HSeparator" parent="Container/Panel/VBoxContainer"]
layout_mode = 2

[node name="TabsRow" type="HBoxContainer" parent="Container/Panel/VBoxContainer"]
layout_mode = 2
theme_override_constants/separation = 4

[node name="EquipTabButton" type="Button" parent="Container/Panel/VBoxContainer/TabsRow"]
layout_mode = 2
size_flags_horizontal = 3
text = "Equipar"

[node name="CraftTabButton" type="Button" parent="Container/Panel/VBoxContainer/TabsRow"]
layout_mode = 2
size_flags_horizontal = 3
text = "Craftear"

[node name="HSeparator2" type="HSeparator" parent="Container/Panel/VBoxContainer"]
layout_mode = 2

[node name="EquipPanel" type="VBoxContainer" parent="Container/Panel/VBoxContainer"]
layout_mode = 2
size_flags_vertical = 3
theme_override_constants/separation = 6

[node name="SlotsLabel" type="Label" parent="Container/Panel/VBoxContainer/EquipPanel"]
layout_mode = 2
text = "Slots: 0/1"

[node name="ArtifactsListContainer" type="VBoxContainer" parent="Container/Panel/VBoxContainer/EquipPanel"]
layout_mode = 2
size_flags_vertical = 3
theme_override_constants/separation = 6

[node name="CraftPanel" type="VBoxContainer" parent="Container/Panel/VBoxContainer"]
visible = false
layout_mode = 2
size_flags_vertical = 3
theme_override_constants/separation = 6

[node name="CraftListContainer" type="VBoxContainer" parent="Container/Panel/VBoxContainer/CraftPanel"]
layout_mode = 2
size_flags_vertical = 3
theme_override_constants/separation = 6
```

> Nota: No hay `[ext_resource]` para el script todavía — se añade en el paso siguiente.

**Step 2: Commit**
```bash
git add Scenes/UI/ArtifactsMenu/ArtifactsMenu.tscn
git commit -m "feat: añadir escena ArtifactsMenu (sin script aún)"
```

---

### Task 5: Crear `ArtifactsMenu.cs` y conectarlo al .tscn

**Files:**
- Create: `Scenes/UI/ArtifactsMenu/ArtifactsMenu.cs`
- Modify: `Scenes/UI/ArtifactsMenu/ArtifactsMenu.tscn` (añadir ext_resource + script)

**Step 1: Crear `ArtifactsMenu.cs`**

```csharp
using System.Linq;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Mesa de Artefactos — toggle con tecla K (input action "artifacts_menu").
///     Siempre presente en Main.tscn bajo UI. Visible=false por defecto.
///     Tab Equipar: equip/unequip de artefactos poseídos.
///     Tab Craftear: craftear artefactos con isótopos.
/// </summary>
public partial class ArtifactsMenu : CanvasLayer
{
    private static readonly string[] CraftablePaths =
    [
        "res://Resources/Artifacts/EscudoDeGrafito.tres",
        "res://Resources/Artifacts/LenteDeFoco.tres",
        "res://Resources/Artifacts/NucleoDenso.tres",
    ];

    private VBoxContainer _equipPanel;
    private Label _slotsLabel;
    private VBoxContainer _artifactsListContainer;

    private VBoxContainer _craftPanel;
    private VBoxContainer _craftListContainer;

    private Player.Player _player;

    public override void _Ready()
    {
        _equipPanel = GetNode<VBoxContainer>("Container/Panel/VBoxContainer/EquipPanel");
        _slotsLabel = GetNode<Label>("Container/Panel/VBoxContainer/EquipPanel/SlotsLabel");
        _artifactsListContainer = GetNode<VBoxContainer>("Container/Panel/VBoxContainer/EquipPanel/ArtifactsListContainer");

        _craftPanel = GetNode<VBoxContainer>("Container/Panel/VBoxContainer/CraftPanel");
        _craftListContainer = GetNode<VBoxContainer>("Container/Panel/VBoxContainer/CraftPanel/CraftListContainer");

        GetNode<Button>("Container/Panel/VBoxContainer/TabsRow/EquipTabButton").Pressed += OnEquipTabPressed;
        GetNode<Button>("Container/Panel/VBoxContainer/TabsRow/CraftTabButton").Pressed += OnCraftTabPressed;

        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!Input.IsActionJustPressed("artifacts_menu")) return;

        if (Visible)
            Close();
        else if (!GameManager.Instance.IsMenuOpen)
            Open();
    }

    private void Open()
    {
        _player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
        GameManager.Instance.IsMenuOpen = true;
        _equipPanel.Visible = true;
        _craftPanel.Visible = false;
        Visible = true;
        RefreshEquip();
    }

    private void Close()
    {
        GameManager.Instance.IsMenuOpen = false;
        Visible = false;
    }

    private void OnEquipTabPressed()
    {
        _equipPanel.Visible = true;
        _craftPanel.Visible = false;
        RefreshEquip();
    }

    private void OnCraftTabPressed()
    {
        _equipPanel.Visible = false;
        _craftPanel.Visible = true;
        RefreshCraft();
    }

    private void RefreshEquip()
    {
        var am = GameManager.Instance.ArtifactManager;
        _slotsLabel.Text = $"Slots: {am.UsedSlots}/{am.MaxSlots}";

        foreach (Node child in _artifactsListContainer.GetChildren())
            child.QueueFree();

        foreach (ArtifactData artifact in am.Owned)
        {
            var row = new HBoxContainer();

            string hpText = artifact.HealthBonus > 0 ? $" +{artifact.HealthBonus * 100:F0}%HP" : "";
            string dmgText = artifact.DamageBonus > 0 ? $" +{artifact.DamageBonus * 100:F0}%DMG" : "";

            var label = new Label
            {
                Text = $"{artifact.ArtifactName} ({artifact.SlotCost}sl){hpText}{dmgText}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            bool isEquipped = am.Equipped.Contains(artifact);
            var btn = new Button
            {
                Text = isEquipped ? "Desequipar" : "Equipar",
                Disabled = !isEquipped && !am.CanEquip(artifact)
            };

            var captured = artifact;
            if (isEquipped)
                btn.Pressed += () =>
                {
                    am.Unequip(captured);
                    _player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    RefreshEquip();
                };
            else
                btn.Pressed += () =>
                {
                    am.Equip(captured);
                    _player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    RefreshEquip();
                };

            row.AddChild(label);
            row.AddChild(btn);
            _artifactsListContainer.AddChild(row);
        }
    }

    private void RefreshCraft()
    {
        var am = GameManager.Instance.ArtifactManager;
        var eco = GameManager.Instance.EconomyManager;

        foreach (Node child in _craftListContainer.GetChildren())
            child.QueueFree();

        foreach (string path in CraftablePaths)
        {
            var artifact = GD.Load<ArtifactData>(path);
            if (artifact == null) continue;

            bool alreadyOwned = am.Owned.Any(a => a.ResourcePath == path);
            var row = new HBoxContainer();

            var label = new Label
            {
                Text = alreadyOwned
                    ? $"{artifact.ArtifactName} — ya obtenido"
                    : $"{artifact.ArtifactName} — {artifact.IsotopeCost} isótopos",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            var btn = new Button
            {
                Text = "Craftear",
                Disabled = alreadyOwned || eco.Isotopes < artifact.IsotopeCost
            };

            if (!alreadyOwned)
            {
                var captured = artifact;
                btn.Pressed += () =>
                {
                    if (!eco.SpendIsotopes(captured.IsotopeCost)) return;
                    am.AddOwned(captured);
                    GameManager.Instance.SaveMeta();
                    RefreshCraft();
                };
            }

            row.AddChild(label);
            row.AddChild(btn);
            _craftListContainer.AddChild(row);
        }
    }
}
```

**Step 2: Conectar el script al .tscn**

Añadir al principio de `ArtifactsMenu.tscn`, después de `[gd_scene format=3]`:
```
[ext_resource type="Script" path="res://Scenes/UI/ArtifactsMenu/ArtifactsMenu.cs" id="1_amscript"]
```

Y en el nodo raíz añadir:
```
script = ExtResource("1_amscript")
```

El bloque del nodo raíz debe quedar:
```
[node name="ArtifactsMenu" type="CanvasLayer"]
layer = 10
script = ExtResource("1_amscript")
```

**Step 3: Build**
```
dotnet build
```
Esperado: `Build succeeded. 0 Error(s)`

**Step 4: Commit**
```bash
git add Scenes/UI/ArtifactsMenu/
git commit -m "feat: implementar ArtifactsMenu con tabs Equipar/Craftear"
```

---

### Task 6: Añadir `ArtifactsMenu` a `Main.tscn`

**Files:**
- Modify: `Scenes/Main/Main.tscn`

**Step 1: Añadir ext_resource**

Después de la última línea `[ext_resource]` existente (actualmente `id="3_dbg"`), añadir:
```
[ext_resource type="PackedScene" path="res://Scenes/UI/ArtifactsMenu/ArtifactsMenu.tscn" id="4_artmenu"]
```

**Step 2: Añadir instancia bajo el nodo `UI`**

Después del nodo `HUD`, añadir:
```
[node name="ArtifactsMenu" parent="UI" instance=ExtResource("4_artmenu")]
layout_mode = 3
```

**Step 3: Build**
```
dotnet build
```
Esperado: `Build succeeded. 0 Error(s)`

**Step 4: Verificar en Godot (F5)**
1. Abrir el juego. Pulsar K → aparece el panel "Mesa de Artefactos" con tab Equipar activo.
2. Tab Equipar: 3 artefactos listados (del seed), "Slots: 0/1".
3. Equipar Escudo de Grafito → "Slots: 1/1", Nucleo Denso disabled.
4. Desequipar → slots liberados.
5. Tab Craftear: los 3 artefactos con sus costes. Botones disabled (seed ya los añadió como owned, todos muestran "ya obtenido").
6. Abrir BonfireMenu (E en bonfire) → ya no tiene botón "Artefactos ▶".
7. K cierra el menú. K no abre si BonfireMenu está abierto.

**Step 5: Commit**
```bash
git add Scenes/Main/Main.tscn
git commit -m "feat: añadir ArtifactsMenu a Main.tscn"
```

---

### Task 7: Actualizar `docs/tasks.md`

**Files:**
- Modify: `docs/tasks.md`

**Step 1: Marcar C-1 y C-2 como completados**

Mover a la sección `## Completado`:
```
- [x] Bloque C-1 + C-2: ArtifactsMenu (Mesa de Artefactos) — equip/unequip + crafteo con isótopos. Accesible con K desde cualquier punto del juego.
```

Eliminar el bloque `[~] Bloque C-1` y el bloque `Bloque C-2` del cajón.

**Step 2: Commit**
```bash
git add docs/tasks.md
git commit -m "docs: marcar C-1 y C-2 como completados en tasks.md"
```
