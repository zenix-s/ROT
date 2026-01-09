using RotOfTime.Core.Entities;

namespace RotOfTime.Core.GameData;

public class SerializableStats : IEntityStats
{
    public int MaxHealth { get; set; } = 100;
    public int Attack { get; set; } = 1;
    public int Defense { get; set; } = 10;

    public void CopyFrom(IEntityStats source)
    {
        if (source == null) return;
        MaxHealth = source.MaxHealth;
        Attack = source.Attack;
        Defense = source.Defense;
    }
}
