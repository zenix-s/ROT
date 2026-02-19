using Godot;

namespace RotOfTime.Autoload;

/// <summary>
///     Tracks activated resonances and current elevation.
///     Plain C# class owned by GameManager (not a Godot Node).
/// </summary>
public class ProgressionManager
{
    public int CurrentElevation { get; set; } = 1;
    public int ActivatedResonances { get; private set; }

    public void ActivateResonance()
    {
        ActivatedResonances++;
    }

    public void AdvanceElevation()
    {
        CurrentElevation++;
    }

    public float GetHealthMultiplier() => 1.0f + ActivatedResonances * 0.20f;
    public float GetDamageMultiplier() => 1.0f + ActivatedResonances * 0.10f;

    public void Load(int activatedResonances, int currentElevation)
    {
        ActivatedResonances = activatedResonances;
        CurrentElevation = currentElevation;
    }
}
