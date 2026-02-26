using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities.Components;

namespace RotOfTime.Scenes.Player.Components;

/// <summary>
///     Extends EntityStatsComponent with automatic multiplier recalculation
///     from ProgressionManager and ArtifactManager. Subscribe/unsuscribe
///     to StatsChanged events from both managers.
/// </summary>
[GlobalClass]
public partial class PlayerStatsComponent : EntityStatsComponent
{
    public override void _Ready()
    {
        base._Ready();
        SubscribeToManagerEvents();
        RecalculateFromManagers();
        ResetStats();
    }

    public override void _ExitTree()
    {
        UnsubscribeFromManagerEvents();
    }

    private void SubscribeToManagerEvents()
    {
        var prog = GameManager.Instance?.ProgressionManager;
        var arts = GameManager.Instance?.ArtifactManager;

        if (prog != null) prog.StatsChanged += RecalculateFromManagers;
        if (arts != null) arts.StatsChanged += RecalculateFromManagers;
    }

    private void UnsubscribeFromManagerEvents()
    {
        var prog = GameManager.Instance?.ProgressionManager;
        var arts = GameManager.Instance?.ArtifactManager;

        if (prog != null) prog.StatsChanged -= RecalculateFromManagers;
        if (arts != null) arts.StatsChanged -= RecalculateFromManagers;
    }

    private void RecalculateFromManagers()
    {
        var prog = GameManager.Instance?.ProgressionManager;
        var arts = GameManager.Instance?.ArtifactManager;

        float hpMult  = prog?.GetHealthMultiplier() ?? 1.0f;
        float dmgMult = prog?.GetDamageMultiplier() ?? 1.0f;

        hpMult  += arts?.GetTotalHealthBonus() ?? 0f;
        dmgMult += arts?.GetTotalDamageBonus() ?? 0f;

        HealthMultiplier = hpMult;
        DamageMultiplier = dmgMult;
        RecalculateStats();
    }
}
