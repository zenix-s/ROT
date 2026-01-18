using Godot;

namespace RotOfTime.Core.Entities;

[GlobalClass]
public partial class EntityStats : Resource
{
    public EntityStats(int vitalityStat, int attackStat, int defenseStat)
    {
        VitalityStat = vitalityStat;
        AttackStat = attackStat;
        DefenseStat = defenseStat;
    }

    public EntityStats() : this(0, 0, 0)
    {
    }

    [Export] public int VitalityStat { get; set; }
    [Export] public int AttackStat { get; set; }
    [Export] public int DefenseStat { get; set; }
}
