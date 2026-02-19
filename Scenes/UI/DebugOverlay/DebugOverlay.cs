using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities;
using RotOfTime.Scenes.Player;

namespace RotOfTime.Scenes.UI.DebugOverlay;

/// <summary>
///     Pantalla de debug. Activar con input action "debug_toggle" (añadir en Project Settings > Input Map).
///     Muestra estado completo del juego: player, progresión, inventario, economía, artefactos.
/// </summary>
public partial class DebugOverlay : CanvasLayer
{
    private Label _label;

    public override void _Ready()
    {
        _label = GetNode<Label>("Container/Panel/ScrollContainer/Label");
        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("debug_toggle"))
            Visible = !Visible;
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        _label.Text = BuildDebugText();
    }

    private string BuildDebugText()
    {
        var gm = GameManager.Instance;
        if (gm == null) return "=== DEBUG ===\nGameManager no disponible";

        var sb = new StringBuilder();
        sb.AppendLine("=== DEBUG ===");

        // --- Player ---
        var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
        if (player != null)
        {
            var stats = player.EntityStatsComponent;
            var am    = player.AttackManager;

            sb.AppendLine("[PLAYER]");
            sb.AppendLine($"  HP:  {stats.CurrentHealth} / {stats.MaxHealth}");
            sb.AppendLine($"  ATK: {stats.AttackPower}  DEF: {stats.EntityStats.DefenseStat}");
            sb.AppendLine($"  HpMult: {stats.HealthMultiplier:F2}x  DmgMult: {stats.DamageMultiplier:F2}x");
            sb.AppendLine($"  Pos: {player.GlobalPosition.X:F0}, {player.GlobalPosition.Y:F0}");

            if (am != null)
            {
                float basic  = am.GetCooldownProgress(PlayerAttackSlot.BasicAttack);
                float spell1 = am.GetCooldownProgress(PlayerAttackSlot.Spell1);
                float spell2 = am.GetCooldownProgress(PlayerAttackSlot.Spell2);
                sb.AppendLine($"  CD  Basic:{basic:F2}  Sp1:{spell1:F2}  Sp2:{spell2:F2}");
            }
        }
        else
        {
            sb.AppendLine("[PLAYER] sin instancia");
        }

        // --- Progresión ---
        var prog = gm.ProgressionManager;
        sb.AppendLine("[PROGRESION]");
        sb.AppendLine($"  Elevacion: {prog.CurrentElevation}");
        sb.AppendLine($"  Resonancias activadas: {prog.ActivatedResonances}");
        sb.AppendLine($"  HpMult: {prog.GetHealthMultiplier():F2}x  DmgMult: {prog.GetDamageMultiplier():F2}x");

        // --- Inventario ---
        var inv   = gm.InventoryManager;
        var items = inv.GetAllItems();
        sb.AppendLine("[INVENTARIO]");
        if (items.Count == 0)
            sb.AppendLine("  (vacio)");
        else
            foreach (var (key, qty) in items)
                sb.AppendLine($"  {key} x{qty}");

        // --- Economía ---
        sb.AppendLine("[ECONOMIA]");
        sb.AppendLine($"  Isotopos: {gm.EconomyManager.Isotopes}");

        // --- Artefactos ---
        var arts = gm.ArtifactManager;
        sb.AppendLine("[ARTEFACTOS]");
        sb.AppendLine($"  Slots: {arts.UsedSlots}/{arts.MaxSlots}");
        if (arts.Equipped.Count == 0)
            sb.AppendLine("  equipados: ninguno");
        else
            foreach (var a in arts.Equipped)
                sb.AppendLine($"  [E] {a.ArtifactName} ({a.SlotCost} slot)");
        if (arts.Owned.Count > arts.Equipped.Count)
            foreach (var a in arts.Owned)
                if (!arts.Equipped.Any(e => e == a))
                    sb.AppendLine($"  [ ] {a.ArtifactName}");

        return sb.ToString();
    }
}
