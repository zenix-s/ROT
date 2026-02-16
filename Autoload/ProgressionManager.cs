using System.Collections.Generic;
using Godot;

namespace RotOfTime.Autoload;

/// <summary>
///     Manages player progression: Elevations and Resonances.
///     Plain C# class owned by GameManager (not a Godot Node).
///     
///     Resonances: 3 per Elevation, 5 Elevations = 15 total.
///     Each Resonance grants +20% HP and +10% damage.
///     Completing 3 Resonances in an Elevation allows advancing to the next.
/// </summary>
public class ProgressionManager
{
    private HashSet<string> _unlockedResonances = new();

    public int CurrentElevation { get; set; } = 1;

    public void UnlockResonance(int elevation, int resonance)
    {
        string key = $"{elevation}_{resonance}";
        if (_unlockedResonances.Add(key))
        {
            GD.Print($"ProgressionManager: Unlocked Resonance {elevation}-{resonance}");

            if (GetResonanceCount(elevation) >= 3)
                GD.Print($"ProgressionManager: Elevation {elevation} complete!");
        }
    }

    public bool IsResonanceUnlocked(int elevation, int resonance)
    {
        return _unlockedResonances.Contains($"{elevation}_{resonance}");
    }

    public int GetResonanceCount(int elevation)
    {
        int count = 0;
        for (int i = 1; i <= 3; i++)
        {
            if (IsResonanceUnlocked(elevation, i))
                count++;
        }
        return count;
    }

    public bool IsElevationComplete(int elevation)
    {
        return GetResonanceCount(elevation) >= 3;
    }

    public float GetHealthMultiplier()
    {
        int total = CountAllResonances();
        return 1.0f + (total * 0.20f);
    }

    public float GetDamageMultiplier()
    {
        int total = CountAllResonances();
        return 1.0f + (total * 0.10f);
    }

    /// <summary>Returns resonance keys for persistence.</summary>
    public List<string> GetResonanceKeys() => new(_unlockedResonances);

    /// <summary>Restores resonances from saved data.</summary>
    public void LoadResonanceKeys(IEnumerable<string> keys)
    {
        _unlockedResonances = new HashSet<string>(keys);
    }

    private int CountAllResonances()
    {
        int total = 0;
        for (int e = 1; e <= CurrentElevation; e++)
            total += GetResonanceCount(e);
        return total;
    }
}
