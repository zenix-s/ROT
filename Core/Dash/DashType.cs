using System.Linq;
using RotOfTime.Autoload;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Identificador estable de cada tipo de dash.
///     Guardado en MetaData como int — nunca guardar res:// paths en el save.
/// </summary>
public enum DashType
{
    Standard = 0,
}

public static class DashTypeExtensions
{
    private static DashManager Manager => GameManager.Instance.DashManager;

    public static bool IsOwned(this DashType type) =>
        Manager.Owned.Contains(type);

    public static bool IsEquipped(this DashType type) =>
        Manager.Equipped == type;

    public static void Equip(this DashType type) =>
        Manager.Equip(type);

    public static void AddOwned(this DashType type) =>
        Manager.AddOwned(type);
}
