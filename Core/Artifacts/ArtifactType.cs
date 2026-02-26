using RotOfTime.Autoload;

namespace RotOfTime.Core.Artifacts;

/// <summary>
///     Identificador estable de cada artefacto.
///     Guardado en MetaData como int — nunca guardar res:// paths en el save.
/// </summary>
public enum ArtifactType
{
    EscudoDeGrafito = 0,
    LenteDeFoco = 1,
    NucleoDenso = 2,
}

public static class ArtifactTypeExtensions
{
    private static ArtifactManager Manager => GameManager.Instance.ArtifactManager;

    public static ArtifactData LoadData(this ArtifactType type) =>
        ArtifactManager.LoadData(type);

    public static bool IsOwned(this ArtifactType type) =>
        Manager.IsOwned(type);

    public static bool IsEquipped(this ArtifactType type) =>
        Manager.IsEquipped(type);

    public static bool CanEquip(this ArtifactType type) =>
        Manager.CanEquip(type);

    public static bool Equip(this ArtifactType type) =>
        Manager.Equip(type);

    public static bool Unequip(this ArtifactType type) =>
        Manager.Unequip(type);

    public static void AddOwned(this ArtifactType type) =>
        Manager.AddOwned(type);
}
