using System.Text.Json.Serialization;
using Godot;

namespace RotOfTime.Core.GameData;

public class PlayerData
{
    // Position
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    // Stats
    public SerializableStats Stats { get; set; } = new();
    public int CurrentHealth { get; set; } = 100;

    [JsonIgnore]
    public Vector2 Position
    {
        get => new(PositionX, PositionY);
        set
        {
            PositionX = value.X;
            PositionY = value.Y;
        }
    }
}
