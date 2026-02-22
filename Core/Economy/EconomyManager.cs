using Godot;

namespace RotOfTime.Core.Economy;

/// <summary>
///     Manages isotope currency for crafting.
///     Plain C# class owned by GameManager (not a Godot Node).
///     Same pattern as ProgressionManager and ArtifactManager.
/// </summary>
public class EconomyManager
{
    private int _isotopes;

    public int Isotopes
    {
        get => _isotopes;
        private set => _isotopes = Mathf.Max(0, value);
    }

    /// <summary>
    ///     Callback invoked when isotope count changes.
    ///     Not a Godot signal (EconomyManager is not a Node).
    ///     GameManager or Player can subscribe to this for HUD updates.
    /// </summary>
    public event System.Action<int> IsotopesChanged;

    public void AddIsotopes(int amount)
    {
        if (amount <= 0) return;
        Isotopes += amount;
        IsotopesChanged?.Invoke(Isotopes);
    }

    public bool SpendIsotopes(int amount)
    {
        if (amount <= 0 || _isotopes < amount)
            return false;

        Isotopes -= amount;
        IsotopesChanged?.Invoke(Isotopes);
        return true;
    }

    /// <summary>Restore isotopes from save data.</summary>
    public void Load(int savedIsotopes)
    {
        _isotopes = Mathf.Max(0, savedIsotopes);
    }
}
