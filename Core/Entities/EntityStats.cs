using Godot;

namespace RotOfTime.Core.Entities;

[GlobalClass]
public partial class EntityStats : Resource
{
    public EntityStats(int vitalityStat, int attackStat,
        GameConstants.Faction faction = GameConstants.Faction.Player)
    {
        VitalityStat = vitalityStat;  // setter aplica clamp
        AttackStat   = attackStat;
        Faction      = faction;
    }

    public EntityStats() : this(0, 0) { }

    private int _vitalityStat;

    /// <summary>
    ///     Vida base. Siempre múltiplo de 10 (1 máscara = 10 HP).
    ///     El setter clampea automáticamente: 45→40, 55→50.
    /// </summary>
    [Export]
    public int VitalityStat
    {
        get => _vitalityStat;
        set => _vitalityStat = (value / 10) * 10;
    }

    [Export] public int AttackStat { get; set; }
    [Export] public GameConstants.Faction Faction { get; set; } = GameConstants.Faction.Player;
}
