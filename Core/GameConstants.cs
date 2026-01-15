using System;

namespace RotOfTime.Core;

public class GameConstants
{
    [Flags]
    public enum GameLayers
    {
        None = 0,
        World = 1,
        Player = 2,
        Enemies = 4,
        Collectibles = 8,
        Interactables = 16,
        PlayerDamageBox = 32,
        EnemyDamageBox = 64,
        PlayerAttack = 128,
        EnemyAttack = 256,
        Projectile = 512,
    }

    public enum Faction
    {
        Player,
        Ally,
        Enemy,
    }
}
