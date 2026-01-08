using System.Text.Json.Serialization;
using Godot;

namespace RotOfTime.Core.GameData;

public class PlayerData
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }

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
