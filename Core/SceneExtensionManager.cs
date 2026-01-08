using System.Collections.Generic;

namespace RotOfTime.Core;

public static class SceneExtensionManager
{
    public enum TowerLevel
    {
        Level0,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
    }

    public enum GameScene
    {
        TowerLevel0,
    }

    public enum MenuScene
    {
        Start,
        SaveSelection
    }

    public static readonly Dictionary<MenuScene, string> MenuPaths = new()
    {
        { MenuScene.Start, "res://Scenes/Menus/StartMenu/StartMenu.tscn" },
        { MenuScene.SaveSelection, "res://Scenes/Menus/SaveSelection/SaveSelection.tscn" }
    };

    public static readonly Dictionary<GameScene, string> ScenePaths = new()
    {
        { GameScene.TowerLevel0, "res://Scenes/Levels/Tower/Level0/Level.tscn" },
    };

    public static readonly Dictionary<TowerLevel, GameScene> TowerLevelScenes = new()
    {
        { TowerLevel.Level0, GameScene.TowerLevel0 },
        // Add more mappings as levels are created
    };

    public static GameScene TowerLevelToGameScene(TowerLevel level)
    {
        return TowerLevelScenes.TryGetValue(level, out var scene) ? scene : GameScene.TowerLevel0;
    }
}
