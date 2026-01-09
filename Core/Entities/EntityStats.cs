using Godot;

namespace RotOfTime.Core.Entities;

[GlobalClass]
public partial class EntityStats : Resource, IEntityStats
{
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int Attack { get; set; } = 1;
    [Export] public int Defense { get; set; } = 10;
}
