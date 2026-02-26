# EquipmentPanel + CraftingPanel Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Convertir EquipmentPanel y CraftingPanel en paneles funcionales con componentes autocontenidos que gestionan su propio estado y comunican hacia arriba via signals.

**Architecture:** Cada componente (ArtifactRow, CraftingRow) recibe un `ArtifactType` via factory constructor, carga sus propios datos en `_Ready()`, gestiona su UI internamente y emite `ActionPerformed` al padre cuando el usuario actúa. Los paneles solo instancian, conectan la señal y hacen AddChild.

**Tech Stack:** Godot 4.6, C# (.NET 10), sin test suite — verificación via `dotnet build` + F5 en Godot Editor.

---

### Task 1: Refactor ArtifactRow — factory + signal + comportamiento propio

**Files:**
- Modify: `Scenes/UI/GlobalMenu/EquipmentPanel/Components/ArtifactRow/ArtifactRow.cs`
- No changes a `ArtifactRow.tscn` — scene ya tiene `NameLabel` y `ActionButton` wired

**Step 1: Reemplazar ArtifactRow.cs completo**

```csharp
using Godot;
using RotOfTime.Core.Artifacts;

public partial class ArtifactRow : HBoxContainer
{
    [Export] public Label NameLabel;
    [Export] public Button ActionButton;

    [Signal] public delegate void ActionPerformedEventHandler();

    private ArtifactType _type;

    public static ArtifactRow Create(PackedScene scene, ArtifactType type)
    {
        var row = scene.Instantiate<ArtifactRow>();
        row._type = type;
        return row;
    }

    public override void _Ready()
    {
        var data = _type.LoadData();
        NameLabel.Text = data.ArtifactName;
        ActionButton.Text = _type.IsEquipped() ? "Desequipar" : "Equipar";
        ActionButton.Disabled = !_type.IsEquipped() && !_type.CanEquip();
        ActionButton.Pressed += OnActionPressed;
    }

    private void OnActionPressed()
    {
        if (_type.IsEquipped()) _type.Unequip();
        else _type.Equip();
        EmitSignal(SignalName.ActionPerformed);
    }
}
```

**Step 2: Verificar build**

```bash
dotnet build
```

Expected: 0 errors. Si hay error en `ArtifactRow`, verificar que `ArtifactTypeExtensions` en `Core/Artifacts/ArtifactType.cs` expone `LoadData()`, `IsEquipped()`, `CanEquip()`, `Equip()`, `Unequip()` — están definidos en el mismo archivo.

**Step 3: Commit**

```bash
git add Scenes/UI/GlobalMenu/EquipmentPanel/Components/ArtifactRow/ArtifactRow.cs
git commit -m "refactor: ArtifactRow autocontenido con factory + signal ActionPerformed"
```

---

### Task 2: Crear componente CraftingRow

**Files:**
- Create: `Scenes/UI/GlobalMenu/CraftingPanel/Components/CraftingRow/CraftingRow.cs`
- Create: `Scenes/UI/GlobalMenu/CraftingPanel/Components/CraftingRow/CraftingRow.tscn`

**Step 1: Crear directorio**

```bash
mkdir -p Scenes/UI/GlobalMenu/CraftingPanel/Components/CraftingRow
```

**Step 2: Crear CraftingRow.cs**

```csharp
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;

public partial class CraftingRow : HBoxContainer
{
    [Export] public Label NameLabel;
    [Export] public Label StatusLabel;
    [Export] public Button ActionButton;

    [Signal] public delegate void ActionPerformedEventHandler();

    private ArtifactType _type;

    public static CraftingRow Create(PackedScene scene, ArtifactType type)
    {
        var row = scene.Instantiate<CraftingRow>();
        row._type = type;
        return row;
    }

    public override void _Ready()
    {
        var data = _type.LoadData();
        var eco = GameManager.Instance.EconomyManager;
        bool isOwned = _type.IsOwned();

        NameLabel.Text = data.ArtifactName;
        StatusLabel.Text = isOwned ? "ya obtenido" : $"{data.IsotopeCost} isótopos";
        ActionButton.Text = "Craftear";
        ActionButton.Disabled = isOwned || eco.Isotopes < data.IsotopeCost;

        if (!isOwned)
            ActionButton.Pressed += OnActionPressed;
    }

    private void OnActionPressed()
    {
        var data = _type.LoadData();
        var eco = GameManager.Instance.EconomyManager;
        if (!eco.SpendIsotopes(data.IsotopeCost)) return;
        _type.AddOwned();
        EmitSignal(SignalName.ActionPerformed);
    }
}
```

**Step 3: Crear CraftingRow.tscn**

```
[gd_scene format=3 uid="uid://craftingrow0001"]

[ext_resource type="Script" path="res://Scenes/UI/GlobalMenu/CraftingPanel/Components/CraftingRow/CraftingRow.cs" id="1_crow"]

[node name="CraftingRow" type="HBoxContainer" node_paths=PackedStringArray("NameLabel", "StatusLabel", "ActionButton")]
anchors_preset = 14
anchor_top = 0.5
anchor_right = 1.0
anchor_bottom = 0.5
offset_top = -11.5
offset_bottom = 19.5
grow_horizontal = 2
grow_vertical = 2
size_flags_horizontal = 3
script = ExtResource("1_crow")
NameLabel = NodePath("Name")
StatusLabel = NodePath("Status")
ActionButton = NodePath("ActionButton")

[node name="Name" type="Label" parent="." unique_id=301000001]
layout_mode = 2
size_flags_horizontal = 3
text = "Item name"

[node name="Status" type="Label" parent="." unique_id=301000002]
layout_mode = 2
text = "Status"

[node name="ActionButton" type="Button" parent="." unique_id=301000003]
layout_mode = 2
text = "Craftear"
```

> Nota: Godot puede regenerar el `uid` al abrir el editor — es comportamiento esperado. Si ocurre, actualizar el `uid` en el `ext_resource` de `CraftingPanel.tscn` (Task 4).

**Step 4: Verificar build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 5: Commit**

```bash
git add Scenes/UI/GlobalMenu/CraftingPanel/Components/
git commit -m "feat: componente CraftingRow autocontenido con factory + signal ActionPerformed"
```

---

### Task 3: Reescribir EquipmentPanel

**Files:**
- Rewrite: `Scenes/UI/GlobalMenu/EquipmentPanel/EquipmentPanel.tscn`
- Modify: `Scenes/UI/GlobalMenu/EquipmentPanel/EquipmentPanel.cs`

**Step 1: Reescribir EquipmentPanel.tscn**

Reemplazar el contenido completo del archivo. Mantener los `uid` del root scene y del nodo raíz para que `GlobalMenu.tscn` siga funcionando sin cambios.

```
[gd_scene format=3 uid="uid://bnbfigeiu7cfu"]

[ext_resource type="Script" uid="uid://caoy8ebab82gn" path="res://Scenes/UI/GlobalMenu/EquipmentPanel/EquipmentPanel.cs" id="1_1xam5"]
[ext_resource type="PackedScene" uid="uid://ccvaqiyvx307i" path="res://Scenes/UI/GlobalMenu/EquipmentPanel/Components/ArtifactRow/ArtifactRow.tscn" id="2_artrow"]

[node name="EquipmentPanel" type="Control" unique_id=754230239 node_paths=PackedStringArray("ArtifactsListContainer", "SlotsLabel")]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
script = ExtResource("1_1xam5")
ArtifactRowScene = ExtResource("2_artrow")
ArtifactsListContainer = NodePath("MarginContainer/VBoxContainer/ArtifactsListContainer")
SlotsLabel = NodePath("MarginContainer/VBoxContainer/SlotsLabel")

[node name="MarginContainer" type="MarginContainer" parent="." unique_id=1626518886]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
theme_override_constants/margin_left = 5
theme_override_constants/margin_top = 5
theme_override_constants/margin_right = 5
theme_override_constants/margin_bottom = 5

[node name="VBoxContainer" type="VBoxContainer" parent="MarginContainer" unique_id=334202551]
layout_mode = 2
theme_override_constants/separation = 4

[node name="SlotsLabel" type="Label" parent="MarginContainer/VBoxContainer" unique_id=334202552]
layout_mode = 2
text = "Slots: 0/1"

[node name="ArtifactsListContainer" type="VBoxContainer" parent="MarginContainer/VBoxContainer" unique_id=334202553]
layout_mode = 2
theme_override_constants/separation = 4
```

**Step 2: Reemplazar EquipmentPanel.cs**

```csharp
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Entities;

public partial class EquipmentPanel : Control
{
    [Export] public PackedScene ArtifactRowScene;
    [Export] public VBoxContainer ArtifactsListContainer;
    [Export] public Label SlotsLabel;

    public void Open() => Refresh();

    public void Refresh()
    {
        foreach (Node child in ArtifactsListContainer.GetChildren())
            child.QueueFree();

        var am = GameManager.Instance.ArtifactManager;
        var player = GetTree().GetFirstNodeInGroup(Groups.Player) as RotOfTime.Scenes.Player.Player;

        SlotsLabel.Text = $"Slots: {am.UsedSlots}/{am.MaxSlots}";

        foreach (ArtifactType type in am.Owned)
        {
            var row = ArtifactRow.Create(ArtifactRowScene, type);
            row.ActionPerformed += () =>
            {
                player?.ApplyAllMultipliers();
                GameManager.Instance.SaveMeta();
                Refresh();
            };
            ArtifactsListContainer.AddChild(row);
        }
    }
}
```

**Step 3: Verificar build**

```bash
dotnet build
```

Expected: 0 errors.

**Step 4: Commit**

```bash
git add Scenes/UI/GlobalMenu/EquipmentPanel/EquipmentPanel.tscn
git add Scenes/UI/GlobalMenu/EquipmentPanel/EquipmentPanel.cs
git commit -m "refactor: EquipmentPanel usa ArtifactRow.Create() + signal ActionPerformed"
```

---

### Task 4: Actualizar CraftingPanel

**Files:**
- Modify: `Scenes/UI/GlobalMenu/CraftingPanel/CraftingPanel.cs`
- Rewrite: `Scenes/UI/GlobalMenu/CraftingPanel/CraftingPanel.tscn`

**Step 1: Reemplazar CraftingPanel.cs**

```csharp
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;

public partial class CraftingPanel : Control
{
    [Export] public PackedScene CraftingRowScene;
    [Export] public VBoxContainer CraftListContainer;

    public void Open() => Refresh();

    public void Refresh()
    {
        foreach (Node child in CraftListContainer.GetChildren())
            child.QueueFree();

        foreach (ArtifactType type in ArtifactManager.ResourcePaths.Keys)
        {
            var artifact = type.LoadData();
            if (artifact == null) continue;

            var row = CraftingRow.Create(CraftingRowScene, type);
            row.ActionPerformed += () =>
            {
                GameManager.Instance.SaveMeta();
                Refresh();
            };
            CraftListContainer.AddChild(row);
        }
    }
}
```

**Step 2: Reescribir CraftingPanel.tscn**

Mantener el `uid` del root scene (`uid://dc14b5aag3rpm`) y el `unique_id=428595855` del nodo raíz para que `GlobalMenu.tscn` siga funcionando.

El `uid` del ext_resource de CraftingRow (`uid://craftingrow0001`) debe coincidir con el `uid` que Godot asignó en Task 2. Si Godot regeneró el UID al abrir el proyecto, usar el valor del archivo `CraftingRow.tscn`.

```
[gd_scene format=3 uid="uid://dc14b5aag3rpm"]

[ext_resource type="Script" uid="uid://c1sas7n38uw6x" path="res://Scenes/UI/GlobalMenu/CraftingPanel/CraftingPanel.cs" id="1_o0e11"]
[ext_resource type="PackedScene" uid="uid://craftingrow0001" path="res://Scenes/UI/GlobalMenu/CraftingPanel/Components/CraftingRow/CraftingRow.tscn" id="2_crow"]

[node name="CraftingPanel" type="Control" unique_id=428595855 node_paths=PackedStringArray("CraftListContainer")]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
script = ExtResource("1_o0e11")
CraftingRowScene = ExtResource("2_crow")
CraftListContainer = NodePath("MarginContainer/VBoxContainer")

[node name="MarginContainer" type="MarginContainer" parent="." unique_id=1990185143]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
theme_override_constants/margin_left = 5
theme_override_constants/margin_top = 5
theme_override_constants/margin_right = 5
theme_override_constants/margin_bottom = 5

[node name="VBoxContainer" type="VBoxContainer" parent="MarginContainer" unique_id=1076427224]
layout_mode = 2
theme_override_constants/separation = 4
```

**Step 3: Verificar build**

```bash
dotnet build
```

Expected: 0 errors. Si hay error `CraftingRowScene` not found, verificar que el nombre del export en CraftingPanel.cs coincide con la propiedad en el .tscn.

**Step 4: Commit**

```bash
git add Scenes/UI/GlobalMenu/CraftingPanel/CraftingPanel.cs
git add Scenes/UI/GlobalMenu/CraftingPanel/CraftingPanel.tscn
git commit -m "refactor: CraftingPanel usa CraftingRow.Create() + signal ActionPerformed"
```

---

### Task 5: Limpieza + verificación en Godot

**Files:**
- Delete: `Scenes/UI/GlobalMenu/ArtifactsPanel.cs`
- Delete: `Scenes/UI/GlobalMenu/ArtifactsPanel.cs.uid`

**Step 1: Borrar dead code**

```bash
rm Scenes/UI/GlobalMenu/ArtifactsPanel.cs
rm Scenes/UI/GlobalMenu/ArtifactsPanel.cs.uid
```

**Step 2: Verificar build final**

```bash
dotnet build
```

Expected: 0 errors. Si algún archivo sigue referenciando `ArtifactsPanel`, localizarlo con:

```bash
grep -r "ArtifactsPanel" --include="*.cs" --include="*.tscn" .
```

**Step 3: Verificar en Godot Editor (F5)**

1. Abrir Godot Editor — dejarlo resolver UIDs automáticamente si avisa
2. F5 para iniciar la escena
3. Pulsar K para abrir GlobalMenu
4. Tab Crafting (2): verificar nombre + costo/estado + botón disabled si ya poseído
5. Tab Equipment (3): verificar que solo aparecen artefactos poseídos, botón "Equipar"/"Desequipar" correcto, disabled si slots llenos

**Step 4: Commit final**

```bash
git add -u
git commit -m "chore: eliminar ArtifactsPanel legacy (dead code)"
```
