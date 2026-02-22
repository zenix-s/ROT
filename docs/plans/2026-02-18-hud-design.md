# HUD Design — Vertical Slice

**Fecha:** 2026-02-18
**Alcance:** Mínimo funcional — HP bar, contador de isótopos, 3 indicadores de cooldown

---

## Arquitectura

Sin nuevo Autoload. El HUD vive como hijo del nodo `UI` (CanvasLayer) que ya existe en `Main.tscn`. `Main.cs` actúa como coordinador: llama `Initialize` al crear el Player y `Teardown` al destruirlo.

```
Main.tscn
  ├── WorldContainer
  ├── Camera
  ├── Attacks
  └── UI (CanvasLayer)  ← ya existe
        └── HUD (Control)          ← nuevo
              ├── HPBar (ProgressBar)
              ├── IsotopeLabel (Label)
              └── AttackSlots (HBoxContainer)
                    ├── BasicAttackIndicator (ProgressBar)
                    ├── Spell1Indicator (ProgressBar)
                    └── Spell2Indicator (ProgressBar)
```

---

## HUD.cs

### Initialize / Teardown

```csharp
public void Initialize(Player player)
{
    // HP — Godot signal
    _statsComp = player.EntityStatsComponent;
    _statsComp.HealthChanged += OnHealthChanged;
    OnHealthChanged(_statsComp.CurrentHealth, _statsComp.MaxHealth); // estado inicial

    // Isótopos — C# event
    _economy = GameManager.Instance.EconomyManager;
    _economy.IsotopesChanged += OnIsotopesChanged;
    OnIsotopesChanged(_economy.Isotopes); // estado inicial

    // Cooldowns — referencia para polling en _Process
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
```

### _Process — cooldowns

Polling cada frame con `GetCooldownProgress()`. No se añade signal en AttackManager — leer 3 floats por frame es insignificante y evita sobrediseño.

```csharp
public override void _Process(double delta)
{
    if (_attackManager == null) return;
    _basicIndicator.Value  = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.BasicAttack);
    _spell1Indicator.Value = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.Spell1);
    _spell2Indicator.Value = 1f - _attackManager.GetCooldownProgress(PlayerAttackSlot.Spell2);
}
```

### Handlers

```csharp
private void OnHealthChanged(int current, int max)
{
    _hpBar.Value = (float)current / max;
}

private void OnIsotopesChanged(int amount)
{
    _isotopeLabel.Text = $"{amount} isótopos";
}
```

---

## Cambios en Main.cs

Dos líneas añadidas al código existente:

```csharp
[Export] private HUD _hud;

private void OnSceneChangeRequested(GameScene gameScene)
{
    // ... código existente ...
    InstantiatePlayer();
    _hud.Initialize(_player);  // ← añadir
}

private void OnMenuChangeRequested(MenuScene menuScene)
{
    _hud.Teardown();           // ← añadir al inicio
    // ... código existente ...
}
```

---

## Archivos a crear / modificar

| Acción | Archivo |
|--------|---------|
| Crear | `Scenes/UI/HUD/HUD.tscn` |
| Crear | `Scenes/UI/HUD/HUD.cs` |
| Modificar | `Scenes/Main/Main.tscn` — añadir HUD como hijo de UI, exportar `_hud` |
| Modificar | `Scenes/Main/Main.cs` — añadir `[Export] private HUD _hud` + 2 líneas |

---

## Nodos de HUD.tscn

| Nodo | Tipo | Config |
|------|------|--------|
| HUD | Control | Anchor: full rect |
| HPBar | ProgressBar | min 0, max 1 |
| IsotopeLabel | Label | — |
| AttackSlots | HBoxContainer | — |
| BasicAttackIndicator | ProgressBar | min 0, max 1 |
| Spell1Indicator | ProgressBar | min 0, max 1 |
| Spell2Indicator | ProgressBar | min 0, max 1 |

Sin estilos — Godot default para el vertical slice.

---

## Decisiones descartadas

- **HUDManager como Autoload**: descartado — añade ciclo de vida complejo y acoplamiento Player→UI sin beneficio real para una sola escena de juego.
- **Player llama a HUDManager**: descartado — Player no debe conocer sistemas de UI.
