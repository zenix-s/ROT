using System.Collections.Generic;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Gestiona el dash equipado y los dashes desbloqueados del jugador.
///     Plain C# class owned by GameManager — mismo patrón que ArtifactManager.
///     MetaData guarda DashType (enum/int), nunca res:// paths.
/// </summary>
public class DashManager
{
    /// <summary>Lookup table: DashType → scene path. Fuente de verdad de paths.</summary>
    public static readonly Dictionary<DashType, string> ScenePaths = new()
    {
        [DashType.Standard] = "res://Core/Dash/StandardDashSkill/StandardDashSkill.tscn",
    };

    /// <summary>Nombres de display para la UI.</summary>
    public static readonly Dictionary<DashType, string> DisplayNames = new()
    {
        [DashType.Standard] = "Dash Estándar",
    };

    private readonly List<DashType> _owned = [];
    public DashType Equipped { get; private set; } = DashType.Standard;

    public IReadOnlyList<DashType> Owned => _owned;

    public string EquippedScenePath => ScenePaths[Equipped];

    public void Equip(DashType type) => Equipped = type;

    public void Load(DashType equipped, IEnumerable<DashType> owned)
    {
        Equipped = equipped;
        _owned.Clear();
        _owned.AddRange(owned);

        // El dash estándar siempre disponible
        if (!_owned.Contains(DashType.Standard))
            _owned.Insert(0, DashType.Standard);
    }

    public List<DashType> GetOwned() => [.. _owned];
}
