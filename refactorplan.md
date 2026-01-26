### 1. El Contrato Base (`IAttack.cs`)

Ubicación: `Core/Combat/Data/IAttack.cs`

Añadimos `Execute` para dar el pistoletazo de salida al ataque y el `UpdateStats` para la inyección de estadísticas del mago.

```csharp
namespace RotOfTime.Core.Combat.Data;

public interface IAttack
{
    AttackDamageComponent DamageComponent { get; }
    AttackHitboxComponent HitboxComponent { get; }
    
    void UpdateStats(EntityStats ownerStats);
    void Execute(Vector2 direction, Vector2 position, EntityStats ownerStats);
}

```

---

### 2. Clase Base de Movimiento (`AttackMovementComponent.cs`)

Ubicación sugerida: `Core/Combat/Components/AttackMovementComponent.cs`

Esta es la abstracción que permitirá que luego crees el "ZigZag" o "Homing" sin tocar el proyectil.

```csharp
namespace RotOfTime.Core.Combat.Components;

public abstract partial class AttackMovementComponent : Node
{
    [Export] public float Speed { get; set; } = 300f;
    [Export] public float Acceleration { get; set; } = 0f;

    // Devuelve la nueva velocidad calculada
    public abstract Vector2 CalculateVelocity(Vector2 currentVelocity, Vector2 direction, double delta);
}

```

---

### 3. Implementación Base: Proyectil (`Projectile.cs`)

Ubicación: `Scenes/Attacks/Projectiles/Projectile.cs`

Aquí aplicamos la **composición**: el proyectil ya no sabe "moverse", le pregunta al componente.

```csharp
using RotOfTime.Core.Combat.Data;
using RotOfTime.Core.Combat.Components;

public partial class Projectile : CharacterBody2D, IAttack
{
    [Export] public AttackDamageComponent DamageComponent { get; set; }
    [Export] public AttackHitboxComponent HitboxComponent { get; set; }
    [Export] public AttackMovementComponent MovementComponent { get; set; }

    protected Vector2 Direction;
    private bool _isActive;

    public virtual void UpdateStats(EntityStats ownerStats)
    {
        DamageComponent?.UpdateStats(ownerStats);
    }

    public virtual void Execute(Vector2 direction, Vector2 position, EntityStats ownerStats)
    {
        GlobalPosition = position;
        Direction = direction.Normalized();
        Rotation = Direction.Angle();
        UpdateStats(ownerStats);
        
        _isActive = true;
        SetupComponents();
    }

    private void SetupComponents()
    {
        if (HitboxComponent != null)
            HitboxComponent.AttackConnected += OnImpact;
        
        // Aquí podrías iniciar un Timer de lifetime si lo prefieres
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isActive) return;

        if (MovementComponent != null)
        {
            Velocity = MovementComponent.CalculateVelocity(Velocity, Direction, delta);
            if (MoveAndSlide()) OnImpact();
        }
    }

    // Template Method para que Fireball o Electric extiendan lógica
    protected virtual void OnImpact()
    {
        _isActive = false;
        QueueFree();
    }
}

```

---

### 4. Implementación de Movimiento Simple (`LinearMovement.cs`)

Crea este script para tus proyectiles básicos actuales.

```csharp
public partial class LinearMovement : AttackMovementComponent
{
    public override Vector2 CalculateVelocity(Vector2 currentVelocity, Vector2 direction, double delta)
    {
        // Movimiento lineal simple sin aceleración por ahora
        return direction * Speed;
    }
}

```

---

### 5. ¿Cómo encaja el "Fireball"?

Ubicación: `Scenes/Attacks/Projectiles/Fireball/Fireball.cs`

Gracias a la base, el script de la bola de fuego solo se preocupa de sus **especificidades** (ej: explotar).

```csharp
public partial class Fireball : Projectile
{
    [Export] public PackedScene ExplosionEffect;

    protected override void OnImpact()
    {
        SpawnExplosion();
        base.OnImpact(); // Destruye el proyectil
    }

    private void SpawnExplosion()
    {
        if (ExplosionEffect == null) return;
        // Instanciar el efecto visual/daño de área aquí
    }
}

```