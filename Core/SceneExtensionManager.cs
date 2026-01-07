using System.Collections.Generic;

namespace RotOfTime.Core;

public static class SceneExtensionManager
{
    public enum MenuScene
    {
        Start,
        SaveSelection,
    }

    public enum GameScene
    {
        MainPage,
        PlayerPage
    }

    public static readonly Dictionary<MenuScene, string> MenuPaths = new()
    {
        { MenuScene.Start, "res://Scenes/Menus/StartMenu/StartMenu.tscn" },
        { MenuScene.SaveSelection, "res://Scenes/Menus/SaveSelection/SaveSelection.tscn" }
    };

    public static readonly Dictionary<GameScene, string> ScenePaths = new()
    {
        { GameScene.MainPage, "res://Scenes/Pages/MainPage/MainPage.tscn" },
        { GameScene.PlayerPage, "res://Scenes/Pages/PlayerPage/PlayerPage.tscn" }
    };
}
