using Godot;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Combat.Components.AttackSpawnerComponents;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Player.Components;

/// <summary>
///     Player-specific attack manager. Maps PlayerAttackSlot enum values
///     to AttackSpawnerComponent children discovered in the scene tree.
///     Spawners are expected to be child nodes named matching the enum values.
/// </summary>
[GlobalClass]
public partial class PlayerAttackManager : AttackManagerComponent<PlayerAttackSlot>
{
    [Export] public AttackSpawnerComponent BasicAttackSpawner { get; set; }
    [Export] public AttackSpawnerComponent Ability1Spawner { get; set; }
    [Export] public AttackSpawnerComponent Ability2Spawner { get; set; }
    [Export] public AttackSpawnerComponent Ability3Spawner { get; set; }
    [Export] public AttackSpawnerComponent Ability4Spawner { get; set; }

    public override void _Ready()
    {
        RegisterExportedSpawners();
    }

    private void RegisterExportedSpawners()
    {
        if (BasicAttackSpawner != null)
            RegisterSlot(PlayerAttackSlot.BasicAttack, BasicAttackSpawner);

        if (Ability1Spawner != null)
            RegisterSlot(PlayerAttackSlot.Ability1, Ability1Spawner);

        if (Ability2Spawner != null)
            RegisterSlot(PlayerAttackSlot.Ability2, Ability2Spawner);

        if (Ability3Spawner != null)
            RegisterSlot(PlayerAttackSlot.Ability3, Ability3Spawner);

        if (Ability4Spawner != null)
            RegisterSlot(PlayerAttackSlot.Ability4, Ability4Spawner);
    }
}
