using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Economy;
using RotOfTime.Core.Entities.Components;
using PlayerScene = RotOfTime.Scenes.Player.Player;
using PlayerAttackManager = RotOfTime.Scenes.Player.Components.PlayerAttackManager;
using PlayerAttackSlot = RotOfTime.Scenes.Player.PlayerAttackSlot;

namespace RotOfTime.Scenes.UI.HUD;

public partial class HUD : Control
{
    [Export] private ProgressBar _hpBar;
    [Export] private Label _isotopeLabel;
    [Export] private ProgressBar _basicIndicator;
    [Export] private ProgressBar _spell1Indicator;
    [Export] private ProgressBar _spell2Indicator;

    private EntityStatsComponent _statsComp;
    private EconomyManager _economy;
    private PlayerAttackManager _attackManager;

    public void Initialize(PlayerScene player)
    {
        _statsComp = player.EntityStatsComponent;
        _statsComp.HealthChanged += OnHealthChanged;
        OnHealthChanged(_statsComp.CurrentHealth);

        _economy = GameManager.Instance.EconomyManager;
        _economy.IsotopesChanged += OnIsotopesChanged;
        OnIsotopesChanged(_economy.Isotopes);

        _attackManager = player.AttackManager;

        Visible = true;
    }

    public void Teardown()
    {
        if (_statsComp != null) _statsComp.HealthChanged -= OnHealthChanged;
        if (_economy != null)   _economy.IsotopesChanged -= OnIsotopesChanged;
        _attackManager = null;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_attackManager == null) return;
        _basicIndicator.Value  = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.BasicAttack);
        _spell1Indicator.Value = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.Spell1);
        _spell2Indicator.Value = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.Spell2);
    }

    private void OnHealthChanged(int newHealth)
    {
        if (_statsComp == null) return;
        _hpBar.Value = (float)newHealth / _statsComp.MaxHealth;
    }

    private void OnIsotopesChanged(int amount)
    {
        _isotopeLabel.Text = $"{amount} isótopos";
    }
}
