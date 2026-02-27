using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities.Components;

namespace RotOfTime.Scenes.Player.Components;

/// <summary>
///     Extiende EntityStatsComponent con recálculo automático desde
///     ProgressionManager y ArtifactManager.
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

        // HP: bonus plano (int). Elevaciones + artefactos. Sin multiplicadores flotantes.
        MaxHealthBonus = (prog?.GetHealthBonus() ?? 0) + (arts?.GetTotalHealthBonus() ?? 0);

        // Daño: multiplicador aditivo. Resonancias + artefactos.
        DamageMultiplier = (prog?.GetDamageMultiplier() ?? 1.0f) + (arts?.GetTotalDamageBonus() ?? 0f);

        RecalculateStats();
    }
}
