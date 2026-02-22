# Stack Tecnológico
* **Motor de Juego:** Godot 4.6
* **Lenguaje de Programación:** C# (.NET 10.0)
* **Formato de solución:** `.slnx` (XML-based, no `.sln` - Godot regenera `.sln` en cada build)
* **Estado actual:** Pre-alpha
 
# Introducción y concepto del juego
El juego se llama **Rot of Time** (La Putrefacción del Tiempo).

Es un **Action-RPG Metroidvania con vista top-down** centrado en combate direccional activo, exploración significativa, y personalización de build mediante artefactos.

**Duración objetivo:** 8-10 horas  
**Scope:** Shippable en 12-18 meses (desarrollo solo)

**Core Loop:** Exploración de la Torre → Combate y Recolección de Isótopos → Crafteo de Artefactos en la Base → Acceso a nuevas áreas mediante Elevaciones.

# Contexto del universo y trasfondo
En el contexto de la situación hace 1 año aparecio una torre y la orden a la que pertenece el protagonista empieza a investigarla, este no es un evento ni comuno ni tampoco fuera completo de lugar, muchos sintonizadores de mas alta elevacion son propietarios de torres, mazmorras o sus propios castillos, aqui tienen sus artefactos y conocimiento, por lo general estos estan ocultos o activamente usados por una orden que estos mismos gestionan.

Pero hace mas de 100 años muchos de estos grandes sintonizadores fueron llamados a una guerra fuera del planeta en una guerra contra una civilizacion conocida como los xylos, la raza humana acudio en respuesta a la solicitud de otra civilizacion conocida como los draconis, los xylos son tanto una raza como un solo individuo al ser una mente colmena.

Muchos sintonizadores han muerto en esta guerra, y sus bases en los planetas de la humanidad han quedado abandonados y cuando las protecciones de ocultacion caen, las ordenes de sintonizadores que quedan en estos planetas se avalanzan para reclamar todo lo que puedan conseguir, este podria ser uno de estos casos perfectamente.


# Protagonista y punto de partida
En el juego encarnas a uno de estos sintonizadores, eres un descendiente de la familia Teran y tu forma de usar la onda se basa en la creacion y manipulacion de carbono para crear artefactos y hechizos.

Aqui es donde nuestro protagonista llegua el es descendiente de Ismael Teran un sintonizador de 6 elevacion que fallecio hace 75 años en la guerra, como su descendiente se tienen grandes esperanzas en ti al tener la misma afinidad por tu forma de manipular la onda que tu antepasado.

Cuando llegas a la torre sabes que ya se han emepzado a explorar los pisos y tu cometido como 1 elevacion es reparar algunos artefactos que se estan usando en la torre para la exploracion, pero conforme exploras los dos primeros pisos que ya estan explorados, vas encontrando enemigos y tu forma de onda te permite acceder a unos pocos sitios que no han sido explorados.

Aqui empiezas a encontrar algunas cosas raras, y cuando eventualmente gracias a ti se consigue empezar a llegar al 3 piso es donde se da el primer giro de trama señales de infeccion xylos.

# Historia de la torre y personajes clave
El contexto de porque esto ocurre es necesario, esto no es una torre normal y aqui es donde defino gran parte de la historia del juego, esta torre no es solo un viejo fuerte abandonado es un laboratorio montado por Ismael y Alejandro Olmedo(El mayor sintonizador de la humanidad de 6 elevacion y se estima que llegara a la 7 eventualmente, poniendole al nivel de los lideres de la raza xylos y draconica).

Esta torre se monto por ellos dos porque hace algo de mas de 75 años durante la guerra Ismael fue infectado por un virus xylos del propio lider xylos, para prevenir una muerte y estudiar la infeccion, los dos sintonizadores montan la torre y simulan un funeral para Ismael y en la torre Ismael estudia la infeccion y el como poder pararla y quizas curarse.

Alejandro tiene que irse por sus responsabilidades como principal representante de la raza humana en la guerra, y Ismael se queda estudiando esta enfermedad que poco a poco lo esta consumiendo.

Ismael y Alejandro han mantenido comunicaciones durante los 65 primeros años desde el aislamiento de Ismael en la torre pero los ultimos 10 años Alejandro ha estado incomunicado y no sabe de lo ultimo que Ismael descubrio en ese tiempo.

Durante esos 10 años ocurren 2 cosas lo primero es que en 5 años Ismael casi encuentra la manera de parar la infeccion y matarla pero la infeccion muta y ahi es donde Ismael se da cuenta que no esta estudiando bien la enfermadad sino los sintomas y que la raiz de la investigacion estaba errada llevandole practicamente al colapso y la desesperacion sumado a no poder comunicarse con Alejandro.

# Mecánicas de progresión y exploración
Cuando el protagonista entra a la torreo Ismael no solo lo detecta como de su sangre sino tambien como alguien compatible con lu longitud de onda y ahi es donde empiza a enviarle partes de su alma que debera derrotar ya que estas tambien estan infectadas(esto serian boses), y el protagonista ha de absorber estos pedazos de alma para seguir mejorando y subir su elevacion de forma mas rapida.


## Progresión: Elevaciones y Resonancias
La progresión en el juego se basa en las Elevaciones, que son los grandes hitos de poder y acceso. Hay **5 Elevaciones principales**, y cada una funciona como una puerta lógica que te permite acceder a nuevas zonas, mecánicas y desafíos. No hay un sistema de experiencia tradicional ni grind: aquí, cada avance real se siente como un salto cualitativo, no cuantitativo.

Cada Elevación está dividida en **3 Resonancias** (15 objetivos totales). Las Resonancias no se consiguen matando enemigos al azar, sino superando retos concretos: derrotar bosses, resolver puzzles, descubrir salas secretas o cumplir objetivos especiales. 

**Cada Resonancia otorga:**
- +20% vida base
- +10% daño base

Solo al completar las 3 Resonancias de una Elevación puedes absorber un fragmento de alma de Ismael (boss fight) y avanzar a la siguiente.

### Progresión final del jugador
El juego termina con el jugador en Elevación 5 (400% HP, 250% daño).

## Economía: Isótopos y Artefactos
La economía del juego gira en torno a los **Isótopos de Carbono**. Los enemigos sueltan estos isótopos, que funcionan como moneda para craftear y mejorar artefactos.

**Sistema de Artefactos (inspirado en Charms de Hollow Knight):**
- Empiezas con 1 slot de artefacto, llegas a 3 slots máximo
- Cada artefacto cuesta 1-3 slots según su poder
- Efectos: curación, daño, defensa, utilidad
- 10-15 artefactos totales en el juego
- Crafteo con Isótopos (+ materiales especiales para artefactos avanzados)

**Ejemplos:**
- Vial de Curación (1 slot): +1 carga de poción
- Catalizador (2 slots): -25% cooldown de hechizos
- Forma Etérea (3 slots): Dash te hace invulnerable brevemente

Puedes cambiar tu build de artefactos en la base sin costo, fomentando la experimentación.

## Sistema de recolección de notas e información
Durante el juego iremos poco a poco descubriendo la historia a través de notas de investigación que encontraremos en la exploración. Estas notas cuentan la caída de Ismael, su lucha contra la infección Xylos, y los experimentos que realizó.

**Sistema de combate:**
- 1 ataque básico (siempre equipado, sin cooldown)
- 2 slots de hechizos (cooldown, configurables)
- 8-10 hechizos totales desbloqueados por progresión
- Puedes cambiar tu loadout en la base sin costo

# Final del juego
El juego culmina en el enfrentamiento con Ismael Xylos en el piso 10, donde deberás usar todo lo aprendido para liberarlo de la infección (o sucumbir a ella).

# Estructura de la torre

Para fomentar la exploracion y que no se vea muy lineal, los pisos de la torre van tal que asi:

piso 0 - centro de la orden donde esta la base

piso 1 - exploracion inicial + tutoriales + secretos que desbloquear con habilidades que conseguimos despues

piso 2 - otro piso con lore y personejes, zonas con enemigos menores mas bien robots de seguridad, y cosas sencillas

piso 3 - boss y revelacion

piso 4 - 5 -6 interconectados y enemigos medio xylos por infeccion de robots de seguridad y cosas varias, boses de zona que hay que derrotar para obtener fragmetos de alma y poder acceder a zona superior de la torre

piso 7 - 8 - 9 intercontectados y similar a anterior pero aqui existen unos nodos que Ismael nos dice que tenemos que destruir para parar la infeccion xylos(esto termina resultando una trampa, la locura y influencia del ejambre por darnos su alma se ha intensificado y el realidad estamos liberando el cuerpo de ismael del piso 10 donde el se encerro por voluntad propia al ver que se estaba volviendo loco y su alma fragmentandose,

piso 10 enfrentameitno final con Ismael Xylos. 

# Arquitectura del Código

## Estructura de Directorios

```
ROT/
├── Autoload/           # Singletons globales (GameManager, SceneManager)
│                       # + clases C# planas (SaveManager, ProgressionManager, etc.)
├── Core/               # Lógica reutilizable independiente de escenas
│   ├── Animation/      # Componentes de animación
│   ├── Combat/         # Sistema de combate (2 capas: datos + comportamiento)
│   │   ├── Attacks/    # Resources: AttackData, ProjectileData, IAttack
│   │   ├── Calculations/ # DamageCalculator (C# puro, sin dependencias Godot)
│   │   ├── Components/ # Hitbox, Hurtbox, AttackManagerComponent
│   │   └── Results/    # AttackResult (Resource), DamageResult (record)
│   ├── Entities/       # Componentes de entidad genéricos
│   │   ├── Components/ # Input, Movement, Stats
│   │   └── StateMachine/ # Patrón state machine genérico (IState, State<T>, StateMachine<T>)
│   └── GameData/       # Persistencia (MetaData)
├── Resources/          # .tres files (CarbonBolt, etc.)
├── Scenes/             # Escenas Godot (.tscn) y scripts asociados
│   ├── Attacks/        # Implementaciones de ataques (Projectile, Fireball, RockBody)
│   ├── Enemies/        # Enemigos con StateMachine propia
│   ├── Levels/         # Niveles de la torre
│   ├── Main/           # Escena principal (punto de entrada del juego)
│   ├── Menus/          # UI (StartMenu)
│   └── Player/         # Player + PlayerAttackManager + StateMachine
└── Assets/             # Sprites, texturas, audio
```

## Sistema de Combate (Post-Refactor)

El sistema de combate usa una arquitectura de **2 capas** que separa datos de comportamiento:

**Capa de datos (Resources .tres):**
- `AttackData`: Nombre, coeficiente de daño, cooldown, escena del ataque
- `ProjectileData` (hereda de AttackData): Velocidad, aceleración, lifetime

**Capa de comportamiento (Escenas que implementan IAttack):**
- Las escenas de ataque no conocen sus propias stats hasta que `Execute()` las inyecta
- Ejemplos: Projectile (base), Fireball (con explosión), RockBody (melee)

**Flujo:** Player → PlayerAttackManager → Instancia escena → IAttack.Execute()

Cooldowns gestionados por Timers de Godot (OneShot), no por tracking manual de delta.

## Sistema de Progresion (Elevaciones y Resonancias)

`ProgressionManager` es una clase C# plana (no Node de Godot) propiedad de `GameManager`. Trackea Resonancias desbloqueadas y calcula multiplicadores de stats.

**Flujo de datos:**
```
GameManager.LoadMeta() → ProgressionManager.LoadResonanceKeys()
Player._Ready() → ApplyAllMultipliers()
  → Lee ProgressionManager.GetHealthMultiplier() / GetDamageMultiplier()
  → Lee ArtifactManager.GetTotalHealthBonus() / GetTotalDamageBonus()
  → Combina ambos en EntityStatsComponent.HealthMultiplier / DamageMultiplier
  → EntityStatsComponent.RecalculateStats()
```

**Principio clave:** `EntityStatsComponent` no tiene dependencia de `GameManager` ni de `ProgressionManager`. Usa propiedades simples (`HealthMultiplier`, `DamageMultiplier`) que el coordinador (`Player.cs`) setea. Esto mantiene los componentes reutilizables para enemigos y otras entidades.

## Sistema de Artefactos (Post-Implementacion)

`ArtifactManager` es una clase C# plana propiedad de `GameManager`. Gestiona artefactos owned/equipped con sistema de slots.

**Estructura:**
- `ArtifactData` (Resource): Nombre, Descripcion, SlotCost (1-3), HealthBonus, DamageBonus
- `ArtifactManager`: Equip/Unequip con validacion de slots, suma de bonuses, persistencia via resource paths
- 3 artefactos de ejemplo: Escudo de Grafito (+20% HP), Lente de Foco (+15% DMG), Nucleo Denso (+25% HP +15% DMG, 2 slots)

**Slots:** Empieza con 1, max 3 (se desbloquean en Elevaciones 3 y 5).

## Patrones Clave

- **Resources** (`[GlobalClass]`) para datos serializables: EntityStats, AttackData, AttackResult
- **Records C#** para datos internos: DamageResult
- **Signals** para desacoplamiento entre componentes
- **State Machine genérica** (`StateMachine<T>`, `State<T>`) usada por Player y Enemies
- **Clases abstractas genéricas** para lógica compartida (no se pueden usar directamente en .tscn)
- **Ataques se instancian en** contenedor `Main/Attacks` (nodo hijo de la escena principal)

# Decisiones de Desarrollo

## 2026-02-15: Simplificación del Scope

Se redujo el scope del juego para hacerlo viable en 12-18 meses de desarrollo solo:
- **5 Elevaciones × 3 Resonancias = 15 objetivos** (en vez de sistema abierto)
- **1 ataque básico + 2 slots de hechizo** (8-10 hechizos totales)
- **Sistema de artefactos** tipo Charms de Hollow Knight (10-15 artefactos, 1-3 slots)
- **1 final sólido** (en vez de múltiples finales)
- **8-10 horas** de gameplay objetivo
- **Sin grinding:** progresión por exploración y retos, no por farmeo

## 2026-02-15: Refactor del Sistema de Ataques

Se simplificó la arquitectura de ataques que estaba sobreingeniería:

| | Antes | Después |
|---|---|---|
| **Capas** | 5 (Player → AttackManager → AttackSlot → SpawnerComponent → IAttack) | 2 (Player → AttackManagerComponent → IAttack) |
| **Líneas de código** | ~600 | ~200 |
| **Slots de ataque** | 4 (Ability1-4) | 3 (BasicAttack + Spell1 + Spell2) |
| **Fase de casteo** | Sí (CastingState con duración configurable) | No (todos los ataques son instantáneos) |
| **Cooldowns** | Tracking manual con `_cooldownRemaining` y delta | Timers de Godot (OneShot) |
| **Configuración** | AttackSlot intermedios como nodos | AttackData exportados directamente |

**Archivos eliminados:** AttackSlot.cs, AttackSpawnerComponent.cs, SingleShotSpawner.cs, BurstSpawner.cs, TurretSpawner.cs, CastingState.cs

**Resultado:** -914 lineas netas, 10 archivos eliminados. Sistema funcional confirmado en Godot.

## 2026-02-16: Sistema de Progresion (Elevaciones y Resonancias)

Se implemento el sistema de progresion como clase C# plana en vez de componente Godot:

| | Plan original | Implementacion final |
|---|---|---|
| **Ubicacion** | `ProgressionComponent` (Node hijo de Player) | `ProgressionManager` (clase C# en GameManager) |
| **Datos** | `ResonanceData` + `ElevationData` Resources | Sin Resources (YAGNI: todas las resonancias son identicas) |
| **Dependencia** | EntityStatsComponent referencia ProgressionComponent | EntityStatsComponent sin dependencias externas |
| **Coordinacion** | Directa via Export | Player.cs como puente entre ProgressionManager y EntityStatsComponent |

**Razon del cambio:** La progresion es estado global del juego, no especifico de una entidad. Sigue el patron existente de GameManager (SaveManager, GameStateManager, AbilityManager).

**Archivos creados:** `Autoload/ProgressionManager.cs`
**Archivos modificados:** `GameManager.cs`, `EntityStatsComponent.cs`, `MetaData.cs`, `Player.cs`
**Archivos eliminados:** `ResonanceData.cs`, `ElevationData.cs` (YAGNI)

## 2026-02-16: Sistema de Artefactos

Se implemento el sistema de artefactos como clase C# plana, mismo patron que ProgressionManager:

| | Plan original | Implementacion final |
|---|---|---|
| **Manager** | `ArtifactManagerComponent` (Node hijo de Player) | `ArtifactManager` (clase C# en GameManager) |
| **Efectos** | Enum `ArtifactEffect` con tipos hardcodeados | `HealthBonus`/`DamageBonus` floats directos (YAGNI) |
| **Persistencia** | No definida | Resource paths en MetaData |
| **Coordinacion** | Directa via Export | `Player.ApplyAllMultipliers()` combina progresion + artefactos |

**Razon:** Efectos mecanicos (pociones, regen, invulnerabilidad) requieren sistemas que no existen. Solo implementamos multiplicadores de stats que ya tenemos.

**Archivos creados:** `Core/Artifacts/ArtifactData.cs`, `Core/Artifacts/ArtifactManager.cs`, 3 `.tres`
**Archivos modificados:** `GameManager.cs`, `MetaData.cs`, `Player.cs`
