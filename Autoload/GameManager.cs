using System.Linq;
using Godot;
using RotOfTime.Core;
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

    public override void _Ready()
    {
        Instance = this;
        SaveManager = new SaveManager();
        GameStateManager = new GameStateManager();
        AbilityManager = new AbilityManager();
        LoadMeta();
    }

    private void LoadMeta()
    {
        Meta = SaveManager.LoadMeta() ?? new MetaData();
        GameStateManager.LoadMilestones(Meta.CompletedMilestones);
        GD.Print("GameManager: Meta loaded");
        GD.Print("Milestones: " + string.Join(", ", Meta.CompletedMilestones));
    }

    public void SaveMeta()
    {
        Meta.CompletedMilestones = [.. GameStateManager.CompletedMilestones];
        SaveManager.SaveMeta(Meta);
    }

    public void PlayerDied()
    {
        GD.Print("GameManager: Player died. Resetting money to 0.");
        SceneManager.Instance.RequestMenuChange(SceneExtensionManager.MenuScene.Start);
    }
}
