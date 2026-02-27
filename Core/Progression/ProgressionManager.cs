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

    /// <summary>
    ///     Bonus plano de HP por elevaciones completadas. +10 HP por elevación = +1 máscara.
    ///     Elevation 1 (inicio): +0. Elevation 2 cleared: +10. ... Elevation 5 cleared: +40.
    /// </summary>
    public int GetHealthBonus() => (CurrentElevation - 1) * 10;

    public float GetDamageMultiplier() => 1.0f + ActivatedResonances * 0.10f;

    public void Load(int activatedResonances, int currentElevation)
    {
        ActivatedResonances = activatedResonances;
        CurrentElevation = currentElevation;
    }
}
