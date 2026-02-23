# Swappable Dash System — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Convertir el dash en un sistema de skills intercambiables desde el GlobalMenu, siguiendo el patrón existente de ArtifactManager.

**Architecture:** `DashSkill` es una mini-jerarquía paralela a `ActiveSkill` (NO extiende `ActiveSkill` — `AttackContext` no aplica). `DashManager` (plain C#, owned by `GameManager`) gestiona owned/equipped scene paths. `Player` instancia programáticamente el `DashSkill` activo. `DashPanel` en `GlobalMenu` permite el swap.

**Tech Stack:** Godot 4.6, C# (.NET 10), patrón ArtifactManager, Godot Resources (GlobalClass)

---

## Archivos a crear
- `Core/Dash/DashData.cs` — Resource con Speed/Duration/Cooldown/DashName
- `Core/Dash/DashSkill.cs` — abstract Node2D con Timer de cooldown
- `Core/Dash/StandardDashSkill.cs` — implementación concreta
- `Core/Dash/DashManager.cs` — plain C# manager
- `Resources/Dashes/StandardDash.tres` — **en Godot editor**
- `Core/Dash/Scenes/StandardDashSkill.tscn` — **en Godot editor**
- `Scenes/UI/GlobalMenu/DashPanel.cs` — UI tab

## Archivos a modificar
- `Core/GameData/MetaData.cs` — añadir OwnedDashes, EquippedDash
- `Autoload/GameManager.cs` — añadir DashManager, wire load/save
- `Scenes/Player/Player.cs` — añadir DashSkill property + EquipDash()
- `Scenes/Player/StateMachine/States/DashState.cs` — usar DashSkill
- `Scenes/Player/StateMachine/States/IdleState.cs` — check IsReady
- `Scenes/Player/StateMachine/States/MoveState.cs` — check IsReady
- `Scenes/UI/GlobalMenu/GlobalMenu.cs` — añadir tab Dash
- `Scenes/UI/GlobalMenu/GlobalMenu.tscn` — **en Godot editor**

---

### Task 1: DashData Resource

**Files:**
- Create: `Core/Dash/DashData.cs`

**Step 1: Crear el archivo**

```csharp
using Godot;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Datos de un tipo de dash. Exportable en escenas de DashSkill.
/// </summary>
[GlobalClass]
public partial class DashData : Resource
{
    [Export] public string DashName { get; set; } = "Dash";
    [Export] public float Speed { get; set; } = 600f;
    [Export] public float Duration { get; set; } = 0.15f;
    [Export] public float Cooldown { get; set; } = 0.5f;
}
```

**Step 2: Verificar build**

```bash
dotnet build
```
Esperado: sin errores.

---

### Task 2: DashSkill abstract + StandardDashSkill

**Files:**
- Create: `Core/Dash/DashSkill.cs`
- Create: `Core/Dash/StandardDashSkill.cs`

**Step 1: DashSkill abstract**

```csharp
using Godot;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Base para todos los tipos de dash intercambiables.
///     Mini-jerarquía paralela a ActiveSkill — NO usa AttackContext.
///     Timer interno gestiona cooldown igual que ActiveSkill.
/// </summary>
public abstract partial class DashSkill : Node2D
{
    [Export] public DashData Data { get; set; }

    private Timer _timer;

    public override void _EnterTree()
    {
        _timer = new Timer { OneShot = true };
        AddChild(_timer);
    }

    public bool IsReady => _timer.IsStopped();

    public float GetCooldownProgress() =>
        _timer.IsStopped() ? 0f : Mathf.Clamp((float)(_timer.TimeLeft / _timer.WaitTime), 0f, 1f);

    /// <summary>
    ///     Intenta ejecutar el dash. Devuelve false si está en cooldown.
    ///     Si ejecuta: aplica velocidad al owner y arranca cooldown.
    /// </summary>
    public bool TryExecute(Vector2 direction, Node2D owner)
    {
        if (!IsReady || Data == null) return false;
        Execute(direction, owner);
        if (Data.Cooldown > 0f) _timer.Start(Data.Cooldown);
        return true;
    }

    protected abstract void Execute(Vector2 direction, Node2D owner);
}
```

**Step 2: StandardDashSkill concreto**

```csharp
using Godot;
using RotOfTime.Scenes.Player;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Dash horizontal estándar. Delega en EntityMovementComponent.Dash().
/// </summary>
public partial class StandardDashSkill : DashSkill
{
    protected override void Execute(Vector2 direction, Node2D owner)
    {
        if (owner is not Player player) return;
        player.EntityMovementComponent.Dash(direction, Data.Speed);
    }
}
```

**Step 3: Verificar build**

```bash
dotnet build
```
Esperado: sin errores.

---

### Task 3: DashManager

**Files:**
- Create: `Core/Dash/DashManager.cs`

**Step 1: Crear el manager**

Patrón idéntico a `ArtifactManager` — plain C#, no Node.

```csharp
using System.Collections.Generic;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Gestiona el dash equipado y los dashes desbloqueados del jugador.
///     Plain C# class owned by GameManager — mismo patrón que ArtifactManager.
///     Los paths son rutas de PackedScene (res://...).
/// </summary>
public class DashManager
{
    /// <summary>Path de escena de la escena de dash por defecto.</summary>
    public const string DefaultDashPath = "res://Core/Dash/Scenes/StandardDashSkill.tscn";

    private readonly List<string> _ownedPaths = new();
    public string EquippedScenePath { get; private set; } = DefaultDashPath;

    public IReadOnlyList<string> OwnedScenePaths => _ownedPaths;

    public void Equip(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath)) return;
        EquippedScenePath = scenePath;
    }

    public void Load(string equippedPath, IEnumerable<string> ownedPaths)
    {
        EquippedScenePath = string.IsNullOrEmpty(equippedPath) ? DefaultDashPath : equippedPath;
        _ownedPaths.Clear();
        _ownedPaths.AddRange(ownedPaths);

        // El dash por defecto siempre está disponible
        if (!_ownedPaths.Contains(DefaultDashPath))
            _ownedPaths.Insert(0, DefaultDashPath);
    }

    public List<string> GetOwnedPaths() => new(_ownedPaths);
}
```

**Step 2: Verificar build**

```bash
dotnet build
```

---

### Task 4: MetaData + GameManager

**Files:**
- Modify: `Core/GameData/MetaData.cs`
- Modify: `Autoload/GameManager.cs`

**Step 1: Añadir campos a MetaData**

En `Core/GameData/MetaData.cs`, añadir al final de la clase (antes del `}`):

```csharp
    public List<string> OwnedDashes { get; set; } = [];
    public string EquippedDash { get; set; } = "";
```

**Step 2: Añadir DashManager a GameManager**

En `Autoload/GameManager.cs`:

Añadir el using al inicio:
```csharp
using RotOfTime.Core.Dash;
```

Añadir la propiedad junto a los demás managers (línea ~29):
```csharp
    public DashManager DashManager { get; private set; }
```

En `_Ready()`, añadir después de `EconomyManager = new EconomyManager();`:
```csharp
        DashManager = new DashManager();
```

En `LoadMeta()`, añadir al final:
```csharp
        DashManager.Load(Meta.EquippedDash, Meta.OwnedDashes);
```

En `SaveMeta()`, añadir junto a los demás:
```csharp
        Meta.OwnedDashes = DashManager.GetOwnedPaths();
        Meta.EquippedDash = DashManager.EquippedScenePath;
```

**Step 3: Verificar build**

```bash
dotnet build
```

---

### Task 5: Player.cs — DashSkill property + EquipDash

**Files:**
- Modify: `Scenes/Player/Player.cs`

**Step 1: Añadir using**

```csharp
using RotOfTime.Core.Dash;
```

**Step 2: Añadir propiedad DashSkill**

Junto a los demás `[Export]` properties, debajo de `AttackManager`:
```csharp
    public DashSkill DashSkill { get; private set; }
```

**Step 3: Llamar InitializeDashSkill en _Ready()**

Al final del bloque `_Ready()`, antes del cierre `}`:
```csharp
        InitializeDashSkill();
```

**Step 4: Añadir métodos de dash**

Añadir antes de `#region Event Handlers`:

```csharp
    private void InitializeDashSkill()
    {
        var path = GameManager.Instance?.DashManager?.EquippedScenePath
                   ?? DashManager.DefaultDashPath;
        SpawnDashSkill(path);
    }

    /// <summary>
    ///     Equipa un nuevo dash, swapeando el nodo activo.
    ///     Llamado por DashPanel.
    /// </summary>
    public void EquipDash(string scenePath)
    {
        DashSkill?.QueueFree();
        GameManager.Instance?.DashManager?.Equip(scenePath);
        SpawnDashSkill(scenePath);
        GameManager.Instance?.SaveMeta();
    }

    private void SpawnDashSkill(string scenePath)
    {
        var packed = GD.Load<PackedScene>(scenePath);
        if (packed == null)
        {
            GD.PrintErr($"Player: No se pudo cargar DashSkill en '{scenePath}'");
            return;
        }
        DashSkill = packed.Instantiate<DashSkill>();
        AddChild(DashSkill);
    }
```

**Step 5: Verificar build**

```bash
dotnet build
```

---

### Task 6: DashState + IdleState + MoveState refactor

**Files:**
- Modify: `Scenes/Player/StateMachine/States/DashState.cs`
- Modify: `Scenes/Player/StateMachine/States/IdleState.cs`
- Modify: `Scenes/Player/StateMachine/States/MoveState.cs`

**Step 1: Reemplazar DashState.cs completo**

```csharp
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class DashState : State<Player>
{
    private float _dashTimeRemaining;

    public override void Enter()
    {
        Vector2 direction = TargetEntity.EntityInputComponent.Direction;
        bool executed = TargetEntity.DashSkill.TryExecute(direction, TargetEntity);
        if (!executed)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }
        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        _dashTimeRemaining = TargetEntity.DashSkill.Data.Duration;
    }

    public override void PhysicsProcess(double delta)
    {
        _dashTimeRemaining -= (float)delta;

        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        TargetEntity.MoveAndSlide();

        if (_dashTimeRemaining > 0) return;

        if (!TargetEntity.IsOnFloor())
        {
            StateMachine.ChangeState<FallingState>();
            return;
        }

        Vector2 direction = TargetEntity.EntityInputComponent.Direction;
        if (direction != Vector2.Zero)
            StateMachine.ChangeState<MoveState>();
        else
            StateMachine.ChangeState<IdleState>();
    }
}
```

**Step 2: IdleState — añadir check IsReady**

En `IdleState.cs`, localizar el bloque de dash (línea ~35):
```csharp
        if (input.IsDashJustPressed && input.Direction != Vector2.Zero)
```
Reemplazar con:
```csharp
        if (input.IsDashJustPressed && input.Direction != Vector2.Zero
            && TargetEntity.DashSkill?.IsReady == true)
```

**Step 3: MoveState — mismo cambio**

En `MoveState.cs`, localizar el bloque de dash (línea ~15):
```csharp
        if (input.IsDashJustPressed && input.Direction != Vector2.Zero)
```
Reemplazar con:
```csharp
        if (input.IsDashJustPressed && input.Direction != Vector2.Zero
            && TargetEntity.DashSkill?.IsReady == true)
```

**Step 4: Verificar build**

```bash
dotnet build
```

---

### Task 7: Crear recursos en Godot Editor

> **Hacer en el editor Godot antes de continuar.**

**7a: Crear DashData resource**

1. En el FileSystem: `Resources/Dashes/` (crear carpeta si no existe)
2. Right-click → New Resource → buscar `DashData`
3. Guardar como `StandardDash.tres`
4. En el Inspector: `DashName = "Dash Estándar"`, `Speed = 600`, `Duration = 0.15`, `Cooldown = 0.5`

**7b: Crear StandardDashSkill scene**

1. En el FileSystem: `Core/Dash/Scenes/` (crear carpeta si no existe)
2. Scene → New Scene → Root node: Node2D, renombrar a `StandardDashSkill`
3. Attach Script → `Core/Dash/StandardDashSkill.cs`
4. En el Inspector: `Data = StandardDash.tres` (arrastrar desde FileSystem)
5. Guardar como `Core/Dash/Scenes/StandardDashSkill.tscn`

**7c: Verificar en Godot**

F5 → el jugador debe poder dashear. Comprobar en Output que no hay errores de carga de escena.

---

### Task 8: DashPanel UI

**Files:**
- Create: `Scenes/UI/GlobalMenu/DashPanel.cs`

**Step 1: Crear el panel**

Mismo patrón que `ArtifactsPanel`. Lista los dashes owned con botón Equipar.

```csharp
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Tab de Dash dentro del GlobalMenu.
///     Lista los dashes desbloqueados y permite equipar uno.
/// </summary>
public partial class DashPanel : VBoxContainer
{
    private VBoxContainer _dashListContainer;

    public override void _Ready()
    {
        _dashListContainer = GetNode<VBoxContainer>("DashListContainer");
    }

    public void Open()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Node child in _dashListContainer.GetChildren())
            child.QueueFree();

        var dm = GameManager.Instance.DashManager;
        string equipped = dm.EquippedScenePath;

        foreach (string path in dm.OwnedScenePaths)
        {
            // Cargar nombre del dash desde la escena
            string dashName = GetDashName(path);
            bool isEquipped = path == equipped;

            var row = new HBoxContainer();

            var label = new Label
            {
                Text = isEquipped ? $"[Equipado] {dashName}" : dashName,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            var btn = new Button
            {
                Text = "Equipar",
                Disabled = isEquipped
            };

            if (!isEquipped)
            {
                string captured = path;
                btn.Pressed += () =>
                {
                    var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
                    player?.EquipDash(captured);
                    Refresh();
                };
            }

            row.AddChild(label);
            row.AddChild(btn);
            _dashListContainer.AddChild(row);
        }
    }

    private static string GetDashName(string scenePath)
    {
        // Carga la escena para extraer el DashName del recurso Data.
        // Si falla, usa el nombre del archivo como fallback.
        var packed = GD.Load<PackedScene>(scenePath);
        if (packed == null) return scenePath;

        var instance = packed.Instantiate();
        if (instance is Core.Dash.DashSkill skill && skill.Data != null)
        {
            string name = skill.Data.DashName;
            instance.QueueFree();
            return name;
        }
        instance.QueueFree();
        return System.IO.Path.GetFileNameWithoutExtension(scenePath);
    }
}
```

**Step 2: Verificar build**

```bash
dotnet build
```

---

### Task 9: GlobalMenu — añadir tab Dash

**Files:**
- Modify: `Scenes/UI/GlobalMenu/GlobalMenu.cs`
- Modify: `Scenes/UI/GlobalMenu/GlobalMenu.tscn` **(en Godot editor)**

**Step 1: Modificar GlobalMenu.cs**

Añadir campo privado junto a los otros panels:
```csharp
    private DashPanel _dashPanel;
```

En `_Ready()`, después de la línea que obtiene `_artifactsPanel`:
```csharp
        _dashPanel = GetNode<DashPanel>("Container/Panel/VBoxContainer/DashPanel");
        GetNode<Button>("Container/Panel/VBoxContainer/TabsRow/DashTabButton").Pressed += OnDashTabPressed;
```

Añadir handler privado:
```csharp
    private void OnDashTabPressed()
    {
        ShowTab(_dashPanel);
        _dashPanel.Open();
    }
```

Actualizar `ShowTab()` para incluir DashPanel:
```csharp
    private void ShowTab(Control active)
    {
        _bonfirePanel.Visible = active == _bonfirePanel;
        _artifactsPanel.Visible = active == _artifactsPanel;
        _dashPanel.Visible = active == _dashPanel;
    }
```

**Step 2: Modificar GlobalMenu.tscn en Godot editor**

1. Abrir `Scenes/UI/GlobalMenu/GlobalMenu.tscn`
2. En `TabsRow` HBoxContainer: añadir Button `DashTabButton`, Text = "Dash"
3. En `VBoxContainer` principal: añadir VBoxContainer `DashPanel`
   - Attach Script → `Scenes/UI/GlobalMenu/DashPanel.cs`
   - Visible = false
   - Añadir hijo VBoxContainer `DashListContainer`
4. Guardar

**Step 3: Verificar build + test**

```bash
dotnet build
```

F5 → abrir GlobalMenu (K) → tab "Dash" debe mostrar "Dash Estándar" como equipado.

---

### Task 10: Verificación final

**Checklist de prueba manual en Godot:**

- [ ] F5 → jugador puede dashear normalmente
- [ ] Dash tiene cooldown visible (no se puede spamear)
- [ ] Abrir GlobalMenu (K) → tab "Dash" aparece
- [ ] Tab "Dash" muestra "Dash Estándar [Equipado]"
- [ ] `dotnet build` sin warnings nuevos
- [ ] Output de Godot sin errores de carga de escena

---

## Notas de scope

- **StandardDashSkill** es el único dash concreto por ahora. El sistema está listo para añadir más sin cambios de arquitectura.
- **Cooldown default:** 0.5s — ajustable en `StandardDash.tres` sin tocar código.
- **DashPanel.GetDashName()** instancia y destruye la escena para leer el nombre. Para muchos dashes, habría que cachear. Con 3-6 dashes no es problema.
- **Unlock system:** Por ahora todos los dashes en `OwnedScenePaths` son accesibles. El unlock (via progresión/crafting) se añade cuando haya más dashes.
