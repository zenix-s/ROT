using System;
using Godot;

namespace RotOfTime.Core.Progression;

/// <summary>
///     Tracks activated resonances and current elevation.
///     Plain C# class owned by GameManager (not a Godot Node).
/// </summary>
public class ProgressionManager
{
    public event Action StatsChanged;

    public int CurrentElevation { get; set; } = 1;
    public int ActivatedResonances { get; private set; }

    public void ActivateResonance()
    {
        ActivatedResonances++;
        StatsChanged?.Invoke();
    }

    /// <summary>True when the player has activated 3 resonances in the current elevation.</summary>
    public bool CanAdvanceElevation() => ActivatedResonances >= CurrentElevation * 3;

    public void AdvanceElevation()
    {
        CurrentElevation++;
        StatsChanged?.Invoke();
    }

    public float GetHealthMultiplier() => 1.0f + ActivatedResonances * 0.20f;
    public float GetDamageMultiplier() => 1.0f + ActivatedResonances * 0.10f;

    public void Load(int activatedResonances, int currentElevation)
    {
        ActivatedResonances = activatedResonances;
        CurrentElevation = currentElevation;
    }
}
