# Combat Stats Bible v1

**Fecha:** 2026-02-18
**Scope:** Vertical slice — BasicEnemy únicamente

---

## Filosofía de diseño

El BasicEnemy es **fodder de alto riesgo**: muere rápido, pero duele si te alcanza. La amenaza real es el grupo, no el individuo. Individualmente puedes despacharlos en pocos segundos; un cuarto lleno de ellos te mata si no esquivas.

**Objetivos de combate:**
- Player aguanta **3-5 hits** de un BasicEnemy antes de morir
- BasicEnemy muere en **2-3 Carbon Bolts** (ataque básico)
- Spells son más eficientes por activación, con trade-offs de cooldown

---

## Stats base

| Entidad | VitalityStat | AttackStat | DefenseStat |
|---|---|---|---|
| Player | 100 | 20 | 0 |
| BasicEnemy | 60 | 25 | 0 |

> **Nota DefenseStat = 0:** Con la fórmula de daño flat (`FinalDamage = max(1, RawDamage - Defense)`), la defensa es inestable con números pequeños. Se fija a 0 hasta que existan enemigos elite o artifacts de reducción de daño que justifiquen usarla.

---

## Fórmula de daño

```
RawDamage   = AttackStat × DamageCoefficient × DamageMultiplier
FinalDamage = max(1, RawDamage - DefenseStat)
```

---

## Coeficientes de ataque

| Ataque | Coef | Tipo | Cooldown | Daño vs BasicEnemy | Hits para matar |
|---|---|---|---|---|---|
| Carbon Bolt | 1.0x | Proyectil rápido | 0.4s | 20 | **3 hits** (1.2s) |
| Fireball | 1.5x | Proyectil lento | 2.0s | 30 | **2 hits** |
| IceShard (×3) | 0.5x c/u | Burst AoE | 2.0s | 10×3 = 30/burst | **2 bursts** |
| RockBody (enemy) | 1.0x | Melee | 1.5s | 25 | Player muere en **4 hits** |

### Notas de diseño por ataque

**Carbon Bolt** — ataque básico sin cooldown efectivo (0.4s). DPS de referencia: 50 dmg/s.

**Fireball** — fuerte en single-target. En los 2s de cooldown del Fireball, el Carbon Bolt dispara 5 veces (100 dmg). Fireball hace 60 dmg en el mismo tiempo → peor DPS, pero no requiere apuntar con tanta precisión.

**IceShard burst** — 3 proyectiles a 0.5x c/u. Si los 3 impactan, hace el mismo daño total que Fireball (30 dmg/burst) pero en área. Útil en grupos, subóptimo single-target. El coeficiente reducido por proyectil refleja el coste de ser un burst: individualmente débiles, colectivamente relevantes.

**RockBody** — ataque cuerpo a cuerpo del BasicEnemy. Con AttackStat=25 y Player Defense=0, hace 25 dmg por hit. El player muere en 4 hits = 6 segundos sin esquivar. Ese es el margen de error permitido.

---

## Progresión y escalado

El sistema de Resonancias añade `+20% HP` y `+10% DMG` por Resonancia desbloqueada (hasta 15).

| Resonancias | Player HP | Player Attack | Carbon Bolt dmg | Hits para matar BasicEnemy |
|---|---|---|---|---|
| 0 (base) | 100 | 20 | 20 | 3 |
| 3 (+60% HP, +30% DMG) | 160 | 26 | 26 | 3 |
| 6 (+120% HP, +60% DMG) | 220 | 32 | 32 | 2 |
| 15 (+300% HP, +150% DMG) | 400 | 50 | 50 | 2 |

> **Nota:** Los enemigos básicos no escalan. La progresión hace al player más resistente, no que el mundo sea más fácil — el diseño de niveles introduce enemigos más difíciles en floors superiores.

---

## Decisiones pendientes

- **DefenseStat del player**: Actualmente 0. Candidato a ser bonus de artifacts (e.g., `Escudo de Grafito` podría dar Defense=10 además de HP).
- **Enemigos elite**: Cuando existan (Phase 5+), definir su stat block usando esta tabla como referencia. Elite = ~2-3× el HP del básico, mismo daño o ligeramente superior.
- **Boss**: Diseñar stat block aparte cuando llegue Phase 6.
