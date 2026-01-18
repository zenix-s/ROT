using System.Collections.Generic;
using Godot;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Centralized registry of all attack data.
///     Balance all attacks from this single location.
/// </summary>
public static class AttackRegistry
{
    private static readonly Dictionary<AttackType, AttackData> _attacks = new()
    {
        { AttackType.None, new AttackData { Name = "None", DamageCoefficient = 0f } },
        { AttackType.RockBody, new AttackData { Name = "RockBody", DamageCoefficient = 1.0f } },
        { AttackType.Fireball, new AttackData { Name = "Fireball", DamageCoefficient = 1.5f } }
    };

    public static IReadOnlyDictionary<AttackType, AttackData> All => _attacks;

    public static AttackData GetAttackData(AttackType type)
    {
        if (_attacks.TryGetValue(type, out AttackData data))
            return data;
        GD.PrintErr($"AttackType {type} not found in registry");
        return _attacks[AttackType.None];
    }
}
