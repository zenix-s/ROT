using System.Linq;
using Godot;
using RotOfTime.Core;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.GameData;

namespace RotOfTime.Autoload;

/// <summary>
///     Runtime manager for meta-progression.
///     Holds MetaData and provides money operations.
///     Owns SaveManager and MilestoneManager instances.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    ///     Permanent progression data. Never null after _Ready.
    /// </summary>
    public MetaData Meta { get; private set; }

    public SaveManager SaveManager { get; private set; }
    public GameStateManager GameStateManager { get; private set; }
    public AbilityManager AbilityManager { get; private set; }
    public ProgressionManager ProgressionManager { get; private set; }
    public ArtifactManager ArtifactManager { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        SaveManager = new SaveManager();
        GameStateManager = new GameStateManager();
        AbilityManager = new AbilityManager();
        ProgressionManager = new ProgressionManager();
        ArtifactManager = new ArtifactManager();
        LoadMeta();
    }

    private void LoadMeta()
    {
        Meta = SaveManager.LoadMeta() ?? new MetaData();
        GameStateManager.LoadMilestones(Meta.CompletedMilestones);
        ProgressionManager.CurrentElevation = Meta.CurrentElevation;
        ProgressionManager.LoadResonanceKeys(Meta.UnlockedResonances);
        ArtifactManager.MaxSlots = Meta.ArtifactMaxSlots;
        ArtifactManager.LoadFromPaths(Meta.OwnedArtifacts, Meta.EquippedArtifacts);
        GD.Print("GameManager: Meta loaded");
        GD.Print("Milestones: " + string.Join(", ", Meta.CompletedMilestones));
        GD.Print($"Progression: Elevation {ProgressionManager.CurrentElevation}, " +
                 $"HP mult {ProgressionManager.GetHealthMultiplier():F2}x, " +
                 $"DMG mult {ProgressionManager.GetDamageMultiplier():F2}x");
        GD.Print($"Artifacts: {ArtifactManager.Owned.Count} owned, " +
                 $"{ArtifactManager.Equipped.Count} equipped, " +
                 $"{ArtifactManager.UsedSlots}/{ArtifactManager.MaxSlots} slots");
    }

    public void SaveMeta()
    {
        Meta.CompletedMilestones = [.. GameStateManager.CompletedMilestones];
        Meta.CurrentElevation = ProgressionManager.CurrentElevation;
        Meta.UnlockedResonances = ProgressionManager.GetResonanceKeys();
        Meta.ArtifactMaxSlots = ArtifactManager.MaxSlots;
        Meta.OwnedArtifacts = ArtifactManager.GetOwnedPaths();
        Meta.EquippedArtifacts = ArtifactManager.GetEquippedPaths();
        SaveManager.SaveMeta(Meta);
    }

    public void PlayerDied()
    {
        GD.Print("GameManager: Player died. Resetting money to 0.");
        SceneManager.Instance.RequestMenuChange(SceneExtensionManager.MenuScene.Start);
    }
}
