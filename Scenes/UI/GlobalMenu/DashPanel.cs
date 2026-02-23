using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Dash;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Tab de Dash dentro del GlobalMenu.
///     Lista los dashes desbloqueados y permite equipar uno.
/// </summary>
public partial class DashPanel : VBoxContainer
{
    private VBoxContainer _dashListContainer;

    public override void _Ready()
    {
        _dashListContainer = GetNode<VBoxContainer>("DashListContainer");
    }

    public void Open() => Refresh();

    public void Refresh()
    {
        foreach (Node child in _dashListContainer.GetChildren())
            child.QueueFree();

        var dm = GameManager.Instance.DashManager;

        foreach (DashType type in dm.Owned)
        {
            bool isEquipped = type == dm.Equipped;
            string displayName = DashManager.DisplayNames.TryGetValue(type, out string name)
                ? name
                : type.ToString();

            var row = new HBoxContainer();

            var label = new Label
            {
                Text = isEquipped ? $"[Equipado] {displayName}" : displayName,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            var btn = new Button
            {
                Text = "Equipar",
                Disabled = isEquipped
            };

            if (!isEquipped)
            {
                var captured = type;
                btn.Pressed += () =>
                {
                    var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
                    player?.EquipDash(captured);
                    Refresh();
                };
            }

            row.AddChild(label);
            row.AddChild(btn);
            _dashListContainer.AddChild(row);
        }
    }
}
