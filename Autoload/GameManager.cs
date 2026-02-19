using System.Linq;
using Godot;
using RotOfTime.Core;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Economy;
using RotOfTime.Core.GameData;

namespace RotOfTime.Autoload;

/// <summary>
///     Runtime manager for meta-progression state.
///     Holds MetaData and coordinates all manager instances.
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
    public EconomyManager EconomyManager { get; private set; }
    public InventoryManager InventoryManager { get; private set; }

    /// <summary>True while any game menu (bonfire, etc.) is open. Blocks player input.</summary>
    public bool IsMenuOpen { get; set; }

    public override void _Ready()
    {
        Instance = this;
        SaveManager = new SaveManager();
        GameStateManager = new GameStateManager();
        AbilityManager = new AbilityManager();
        ProgressionManager = new ProgressionManager();
        ArtifactManager = new ArtifactManager();
        EconomyManager = new EconomyManager();
        InventoryManager = new InventoryManager();
        LoadMeta();
    }

    private void LoadMeta()
    {
        Meta = SaveManager.LoadMeta() ?? new MetaData();
        GameStateManager.LoadMilestones(Meta.CompletedMilestones);
        ProgressionManager.Load(Meta.ActivatedResonances, Meta.CurrentElevation);
        ArtifactManager.MaxSlots = Meta.ArtifactMaxSlots;
        ArtifactManager.LoadFromPaths(Meta.OwnedArtifacts, Meta.EquippedArtifacts);
        EconomyManager.Load(Meta.Isotopes);
        InventoryManager.Load(Meta.Inventory);
    }

    public void SaveMeta()
    {
        Meta.CompletedMilestones = [.. GameStateManager.CompletedMilestones];
        Meta.CurrentElevation = ProgressionManager.CurrentElevation;
        Meta.ActivatedResonances = ProgressionManager.ActivatedResonances;
        Meta.ArtifactMaxSlots = ArtifactManager.MaxSlots;
        Meta.OwnedArtifacts = ArtifactManager.GetOwnedPaths();
        Meta.EquippedArtifacts = ArtifactManager.GetEquippedPaths();
        Meta.Isotopes = EconomyManager.Isotopes;
        Meta.Inventory = InventoryManager.GetAllItems();
        SaveManager.SaveMeta(Meta);
    }

    public void PlayerDied()
    {
SceneManager.Instance.RequestMenuChange(SceneExtensionManager.MenuScene.Start);
    }
}
