# Diseño de Tipos de Ataque — Post Vertical Slice

> Documento de referencia para cuando se amplíe el sistema de combate tras completar el vertical slice.
> No es una spec técnica — las decisiones de implementación se tomarán cuando llegue el momento.

---

## Contexto

El sistema actual solo soporta ataques activos que generan proyectiles. El jugador tiene 2 spell slots intercambiables en la hoguera. Este documento define los tipos de ataque que deben poder ocupar esos slots.

---

## Tipos de Ataque

### Active (proyectiles — ya existe)
El jugador activa el ataque manualmente. Genera uno o varios proyectiles en la dirección apuntada. Tiene cooldown visible en el HUD.

Ejemplos actuales: Carbon Bolt, Fireball, Ice Shard burst.

### Melee
El jugador activa el ataque manualmente. En lugar de un proyectil, genera una zona de daño cercana al jugador en la dirección apuntada — un swing, empuje, o explosión cuerpo a cuerpo. Tiene cooldown visible en el HUD.

La animación del personaje debe acompañar el ataque. La sincronización se resuelve haciendo que ambos (hitbox y animación) arranquen al mismo tiempo, con duraciones que coincidan manualmente.

### Pasivo — Trigger
No requiere activación manual. Se activa automáticamente cuando se cumple una condición en el juego (recibir daño, matar un enemigo, etc.). El slot no muestra cooldown en el HUD o muestra un indicador de estado diferente.

Ejemplos de concepto: escudo temporal al recibir un golpe, explosión al matar.

### Pasivo — Aura
No requiere activación manual. Está siempre activo mientras ocupa el slot. Genera un efecto continuo alrededor del jugador o sobre sus stats.

Ejemplos de concepto: zona de ralentización a los enemigos cercanos, aura que daña a enemigos al contacto.

---

## Slots y Equipamiento

Los cuatro tipos van en los mismos 2 spell slots. El jugador puede mezclar tipos libremente — por ejemplo Spell1 = Melee y Spell2 = Pasivo Aura. El intercambio se hace en la hoguera sin coste.

El ataque básico (Carbon Bolt) no ocupa spell slot y no es reemplazable.

---

## Notas de Diseño

- Los pasivos no deben hacer el juego trivial — deben costar un slot y tener efectos que compitan con un ataque activo.
- Un slot pasivo es una decisión de build: renuncias a un ataque activo a cambio de un beneficio continuo o reactivo.
- Los melee deben tener cooldowns más cortos que los proyectiles pero menos rango — compensación clara.
