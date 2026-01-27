using Godot;
using RotOfTime.Core;

namespace RotOfTime.Core.Entities;

[GlobalClass]
public partial class EntityStats : Resource
{
    public EntityStats(int vitalityStat, int attackStat, int defenseStat,
        GameConstants.Faction faction = GameConstants.Faction.Player)
    {
        VitalityStat = vitalityStat;
        AttackStat = attackStat;
        DefenseStat = defenseStat;
        Faction = faction;
    }

    public EntityStats() : this(0, 0, 0)
    {
    }

    [Export] public int VitalityStat { get; set; }
    [Export] public int AttackStat { get; set; }
    [Export] public int DefenseStat { get; set; }
    [Export] public GameConstants.Faction Faction { get; set; } = GameConstants.Faction.Player;
}
