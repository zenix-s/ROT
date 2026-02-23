# Side-scroller Transition Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Convertir el juego de top-down a side-scroller 2D platformer (estilo Blasphemous/HK) manteniendo todos los sistemas core intactos.

**Architecture:** Se añade gravedad y salto a `EntityMovementComponent`. Los estados del player se actualizan para movimiento horizontal + salto. Los enemigos reciben gravedad en una segunda fase. El sistema de ataque (mouse aim) y todos los managers (Progression, Artifact, Economy) no se tocan.

**Tech Stack:** Godot 4.6, C# .NET 10.0, CharacterBody2D (ya existente), TileMap para nivel grey-box.

**Verificación:** No hay test suite. Cada tarea se verifica con `dotnet build` + F5 en Godot Editor.

---

## Phase 1 — Input Actions (manual en Godot Editor)

### Task 1: Reconfigurar input map para platformer

**Manual — hacer en Godot Editor → Project → Project Settings → Input Map:**

1. Añadir nueva acción `jump` → asignar tecla `Space`
2. Cambiar acción `dash` → quitar `Space`, asignar `Shift`
3. Guardar (Ctrl+S)

**Verificar:** Abrir `Project Settings → Input Map` y confirmar:
- `jump` → Space
- `dash` → Shift
- `attack` → LMB (sin cambios)
- `skill_1`, `skill_2` → 1, 2 (sin cambios)
- `move_left` → A, `move_right` → D (sin cambios)

---

### Task 2: Añadir `IsJumpJustPressed` a EntityInputComponent

**Archivo:** `Core/Entities/Components/EntityInputComponent.cs`

Añadir una sola propiedad al final de la clase:

```csharp
public bool IsJumpJustPressed => !IsBlocked && Input.IsActionJustPressed("jump");
```

También cambiar `Direction` para retornar solo el eje horizontal (el movimiento vertical de WASD ya no se usa para moverse):

```csharp
public Vector2 Direction => IsBlocked
    ? Vector2.Zero
    : new Vector2(Input.GetAxis("move_left", "move_right"), 0f);
```

**Verificar:** `dotnet build` sin errores.

**Commit:**
```
git add Core/Entities/Components/EntityInputComponent.cs
git commit -m "feat: adaptar EntityInputComponent para side-scroller (horizontal + jump)"
```

---

## Phase 2 — Física del player

### Task 3: Añadir gravedad y salto a EntityMovementComponent

**Archivo:** `Core/Entities/Components/EntityMovementComponent.cs`

Reemplazar el contenido completo con:

```csharp
using Godot;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityMovementComponent : Node
{
    [Export] public float Gravity = 980f;
    [Export] public float JumpVelocity = -420f;

    public Vector2 Velocity { get; set; } = Vector2.Zero;

    public void StopMovement()
    {
        Velocity = new Vector2(0f, Velocity.Y); // preservar Y (gravedad)
    }

    public void KnockBack(Vector2 direction, float force)
    {
        Velocity += direction.Normalized() * force;
    }

    public void Move(Vector2 direction, float speed)
    {
        Velocity = new Vector2(direction.X * speed, Velocity.Y); // solo X, preservar Y
    }

    public void Dash(Vector2 direction, float dashSpeed)
    {
        Velocity = new Vector2(direction.X * dashSpeed, 0f); // dash horizontal, sin gravedad
    }

    public void ApplyGravity(double delta)
    {
        Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * (float)delta);
    }

    public void Jump()
    {
        Velocity = new Vector2(Velocity.X, JumpVelocity);
    }
}
```

**Notas importantes:**
- `StopMovement()` ya NO pone Y a 0 — preserva la velocidad vertical para que la gravedad funcione en Idle
- `Move()` ya NO sobreescribe Y — solo setea X
- `Dash()` pone Y a 0 (freeze gravity durante el dash, comportamiento estándar de platformer)
- `ApplyGravity()` acumula gravedad en Y cada frame
- `Jump()` setea Y al valor negativo (arriba en Godot es -Y)

**Verificar:** `dotnet build`. Puede haber errores en los estados del enemy si usaban `Move()` para moverse en 2D — los arreglamos en la Phase 5.

**Commit:**
```
git add Core/Entities/Components/EntityMovementComponent.cs
git commit -m "feat: añadir gravedad y salto a EntityMovementComponent"
```

---

## Phase 3 — Estados del player

### Task 4: Actualizar IdleState

**Archivo:** `Scenes/Player/StateMachine/States/IdleState.cs`

```csharp
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class IdleState : State<Player>
{
    public override void Enter()
    {
        TargetEntity.EntityMovementComponent.StopMovement();
    }

    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        var movement = TargetEntity.EntityMovementComponent;

        movement.ApplyGravity(delta);
        TargetEntity.Velocity = movement.Velocity;
        TargetEntity.MoveAndSlide();
        movement.Velocity = TargetEntity.Velocity; // sync post-MoveAndSlide

        if (!TargetEntity.IsOnFloor())
        {
            StateMachine.ChangeState<FallingState>();
            return;
        }

        if (input.IsJumpJustPressed)
        {
            StateMachine.ChangeState<JumpingState>();
            return;
        }

        if (input.IsDashJustPressed && input.Direction != Vector2.Zero)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        var fireResult = TargetEntity.TryFireAttack();
        if (fireResult == AttackFireResult.FiredInstant)
            return;

        if (input.Direction != Vector2.Zero)
            StateMachine.ChangeState<MoveState>();
    }
}
```

**Nota:** El `movement.Velocity = TargetEntity.Velocity` después de `MoveAndSlide()` sincroniza la velocidad real (Godot puede modificarla al resolver colisiones) de vuelta al componente. Importante para que la gravedad acumulada sea correcta.

---

### Task 5: Actualizar MoveState

**Archivo:** `Scenes/Player/StateMachine/States/MoveState.cs`

```csharp
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class MoveState : State<Player>
{
    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        var movement = TargetEntity.EntityMovementComponent;

        movement.ApplyGravity(delta);

        if (input.IsDashJustPressed && input.Direction != Vector2.Zero)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        if (input.IsJumpJustPressed && TargetEntity.IsOnFloor())
        {
            StateMachine.ChangeState<JumpingState>();
            return;
        }

        if (!TargetEntity.IsOnFloor())
        {
            StateMachine.ChangeState<FallingState>();
            return;
        }

        var fireResult = TargetEntity.TryFireAttack();
        if (fireResult == AttackFireResult.FiredInstant)
            return;

        if (input.Direction == Vector2.Zero)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        movement.Move(input.Direction, Player.Speed);
        TargetEntity.Velocity = movement.Velocity;
        TargetEntity.MoveAndSlide();
        movement.Velocity = TargetEntity.Velocity;
    }
}
```

---

### Task 6: Crear JumpingState

**Archivo nuevo:** `Scenes/Player/StateMachine/States/JumpingState.cs`

```csharp
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class JumpingState : State<Player>
{
    public override void Enter()
    {
        TargetEntity.EntityMovementComponent.Jump();
    }

    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        var movement = TargetEntity.EntityMovementComponent;

        movement.ApplyGravity(delta);
        movement.Move(input.Direction, Player.Speed);

        TargetEntity.Velocity = movement.Velocity;
        TargetEntity.MoveAndSlide();
        movement.Velocity = TargetEntity.Velocity;

        TargetEntity.TryFireAttack();

        // Apex del salto: velocidad Y se vuelve positiva → caída
        if (TargetEntity.Velocity.Y >= 0f)
            StateMachine.ChangeState<FallingState>();
    }
}
```

---

### Task 7: Crear FallingState

**Archivo nuevo:** `Scenes/Player/StateMachine/States/FallingState.cs`

```csharp
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class FallingState : State<Player>
{
    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        var movement = TargetEntity.EntityMovementComponent;

        movement.ApplyGravity(delta);
        movement.Move(input.Direction, Player.Speed);

        TargetEntity.Velocity = movement.Velocity;
        TargetEntity.MoveAndSlide();
        movement.Velocity = TargetEntity.Velocity;

        TargetEntity.TryFireAttack();

        if (TargetEntity.IsOnFloor())
        {
            if (input.Direction != Vector2.Zero)
                StateMachine.ChangeState<MoveState>();
            else
                StateMachine.ChangeState<IdleState>();
        }
    }
}
```

---

### Task 8: Actualizar DashState para horizontal

**Archivo:** `Scenes/Player/StateMachine/States/DashState.cs`

El único cambio real: `Dash()` ya maneja horizontal-only en EntityMovementComponent. Pero hay que añadir la transición a `FallingState` si el dash termina en el aire:

```csharp
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class DashState : State<Player>
{
    private const float DashSpeed = 600f;
    private const float DashDuration = 0.15f;

    private float _dashTimeRemaining;

    public override void Enter()
    {
        Vector2 direction = TargetEntity.EntityInputComponent.Direction;
        TargetEntity.EntityMovementComponent.Dash(direction, DashSpeed);
        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        _dashTimeRemaining = DashDuration;
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

---

### Task 9: Registrar nuevos estados en Player.tscn (manual en Godot Editor)

Los estados `JumpingState` y `FallingState` son scripts nuevos — hay que añadirlos como nodos hijos de `PlayerStateMachine` en la escena:

1. Abrir `Scenes/Player/Player.tscn` en Godot Editor
2. Seleccionar el nodo `PlayerStateMachine`
3. Añadir nodo hijo → tipo `Node` → asignar script `JumpingState.cs`
4. Repetir con `FallingState.cs`
5. Guardar

**Verificar:** `dotnet build` sin errores. Abrir Godot Editor, F5, el player debería caer al suelo y poder saltar con Space.

**Commit:**
```
git add Scenes/Player/StateMachine/States/
git commit -m "feat: añadir JumpingState y FallingState — player physics side-scroller"
```

---

## Phase 4 — Sprite flipping

### Task 10: Flip de sprite según dirección horizontal

**Archivo:** `Scenes/Player/Player.cs`

Añadir un método público que los estados pueden llamar, y un `[Export]` para el sprite:

En la clase Player, añadir:

```csharp
[Export] public Sprite2D Sprite; // o AnimatedSprite2D según el nodo en Player.tscn

public void UpdateFacing(float horizontalVelocity)
{
    if (Sprite == null) return;
    if (horizontalVelocity > 0.1f)
        Sprite.FlipH = false;
    else if (horizontalVelocity < -0.1f)
        Sprite.FlipH = true;
}
```

Luego en `MoveState.PhysicsProcess`, `JumpingState.PhysicsProcess`, y `FallingState.PhysicsProcess`, añadir al final:

```csharp
TargetEntity.UpdateFacing(TargetEntity.Velocity.X);
```

**Manual en Godot Editor:**
1. Seleccionar el nodo Player en `Player.tscn`
2. En el Inspector, asignar el export `Sprite` al nodo de sprite correspondiente

**Verificar:** F5 — el sprite debería voltear al moverse izquierda/derecha.

**Commit:**
```
git add Scenes/Player/Player.cs Scenes/Player/StateMachine/States/
git commit -m "feat: flip de sprite según dirección horizontal"
```

---

## Phase 5 — Cámara

### Task 11: Añadir Camera2D al Player (manual en Godot Editor)

1. Abrir `Scenes/Player/Player.tscn`
2. Añadir nodo hijo → `Camera2D`
3. En el Inspector de Camera2D:
   - `Position Smoothing → Enabled`: true
   - `Position Smoothing → Speed`: 5.0
   - `Zoom`: (1, 1) — ajustar al gusto después
4. Guardar

No se necesita código — Camera2D como hijo del Player lo sigue automáticamente en Godot.

**Verificar:** F5 — la cámara sigue al player.

---

## Phase 6 — Nivel grey-box (manual en Godot Editor)

### Task 12: Crear primer nivel side-scroller

Crear una escena nueva o modificar la existente `Scenes/Main/Main.tscn`:

1. Abrir la escena principal (`Scenes/Main/Main.tscn`)
2. Eliminar (o desactivar) el TileMap top-down actual
3. Añadir un nuevo `TileMap` con un tileset básico (puede ser tiles de colores planos)
4. Dibujar con el TileMap:
   - Suelo largo (al menos 20 tiles de ancho)
   - 3-4 plataformas a diferentes alturas
   - Paredes en los extremos
5. Ajustar la posición de spawn del player sobre el suelo
6. Colocar 1-2 `BasicEnemy` en el nivel (no van a tener gravedad aún — flotan, está bien por ahora)
7. Guardar

**Verificar:** F5 — player cae al suelo, puede moverse y saltar entre plataformas.

---

## Phase 7 — Gravedad en enemigos

### Task 13: Añadir gravedad a BasicEnemy

**Archivo:** `Scenes/Enemies/BasicEnemy/BasicEnemy.cs`

Añadir exports de gravedad al BasicEnemy (análogo al player):

```csharp
[Export] public float Gravity = 980f;
```

**Archivo:** `Scenes/Enemies/BasicEnemy/StateMachine/States/ChasingState.cs`

Leer el archivo actual y modificar `PhysicsProcess` para:
1. Aplicar gravedad manualmente (similar al player pero inline, sin EntityMovementComponent gravity — el enemy no lo usa de la misma forma)
2. Cambiar el chase a solo eje X (horizontal hacia el player)

El patrón es:
```csharp
// Aplicar gravedad
var velocity = TargetEntity.Velocity;
velocity.Y += TargetEntity.Gravity * (float)delta;

// Chase horizontal únicamente
float dirX = Mathf.Sign(target.GlobalPosition.X - TargetEntity.GlobalPosition.X);
velocity.X = dirX * chaseSpeed;

TargetEntity.Velocity = velocity;
TargetEntity.MoveAndSlide();
```

Repetir el bloque de gravedad en `IdleState` del enemy (para que caiga al suelo cuando no persigue).

**Manual en Godot Editor:**
- Ajustar la collision shape del BasicEnemy si es un círculo — cambiar a `CapsuleShape2D` vertical para que encaje bien en las plataformas

**Verificar:** F5 — enemigos caen al suelo, persiguen al player horizontalmente.

**Commit:**
```
git add Scenes/Enemies/
git commit -m "feat: añadir gravedad y chase horizontal al BasicEnemy"
```

---

## Checklist de verificación final

Antes de considerar la transición completa:

- [ ] Player cae al suelo con gravedad realista
- [ ] Space salta, Shift dashea horizontalmente
- [ ] El dash funciona tanto en suelo como en el aire
- [ ] El sprite voltea según la dirección
- [ ] La cámara sigue al player suavemente
- [ ] Los ataques (LMB, 1, 2) funcionan en todas las situaciones (suelo, aire, dash)
- [ ] El player muere al recibir daño del enemigo
- [ ] Los enemigos caen al suelo y persiguen horizontalmente
- [ ] El BonfireMenu, ArtifactsMenu y HUD siguen funcionando
- [ ] `dotnet build` sin errores ni warnings nuevos

---

## Fuera del scope de este plan

Estas tareas van en el siguiente plan:

- Refactor de SoulFragment1 (boss) para física platformer
- Nivel grey-box completo con TileMap propio (Floors 1-3)
- Arte del player (modelo 3D → spritesheet)
- Ajuste de números (gravity, jump velocity, speed) tras playtest
- Coyote time y jump buffer (mejoras de feel — post-verificación)
