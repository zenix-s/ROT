using Godot;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Datos de un tipo de dash. Exportable en escenas de DashSkill.
/// </summary>
[GlobalClass]
public partial class DashData : Resource
{
    [Export] public string DashName { get; set; } = "Dash";
    [Export] public float Speed { get; set; } = 600f;
    [Export] public float Duration { get; set; } = 0.15f;
    [Export] public float Cooldown { get; set; } = 0.5f;
}
