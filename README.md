### 1. Concepto General

Un **Action-RPG Metroidvania** en perspectiva cenital (*Top-Down*). El jugador encarna a un mago que debe escalar una torre misteriosa mientras progresa a través de 10 Círculos de Poder, combinando la intensidad del combate de hordas con la narrativa y dificultad de un Soulslike.

### 2. Sistema de Combate y Controles (PC)

Se abandona el auto-gameplay pasivo por un sistema de **combate activo y direccional**. El jugador tiene el control total de la puntería, lo que permite un diseño de encuentros mucho más desafiante.

| Acción | Mando (Layout Universal) | Lógica de Diseño |
| --- | --- | --- |
| **Movimiento** | **Stick Izquierdo** | Movimiento fluido en 360°. |
| **Apuntar** | **Stick Derecho** | El mago encara la dirección del stick. |
| **Ataque Principal** | **R2 / RT** (Gatillo Der.) | Permite disparar mientras apuntas con el stick derecho. |
| **Dash Elemental** | **L1 / LB** (Bumper Izq.) | Acceso rápido para reacciones defensivas. |
| **Detección de Maná** | **L2 / LT** (Gatillo Izq.) | Se suele mantener pulsado para "analizar" el entorno. |
| **Habilidades (1-4)** | **Botones Frontales (A,B,X,Y)** | Hechizos potentes o de utilidad. |
| **Interactuar / Hablar** | **R1 / RB** (Bumper Der.) | Para no interrumpir el movimiento al hablar con NPCs. |
| **Menú / Inventario** | **Start / Options** | Gestión de Círculos y estadísticas. |

| Acción | Control (Teclado/Ratón) | Efecto en el Gameplay |
| --- | --- | --- |
| **Movimiento** | `W`, `A`, `S`, `D` | Movimiento libre en 360° (Sprites de 8 direcciones). |
| **Apuntar** | `Ratón` | El mago orienta su cuerpo y sus hechizos hacia el cursor. |
| **Ataque Principal** | `Click Izquierdo` | Hechizo básico del Círculo actual (consume poca stamina). |
| **Dash Elemental** | `Espacio` | Esquiva con frames de invulnerabilidad; permite cruzar trampas. |
| **Detección de Maná** | `Shift Izquierdo` | Resalta secretos, caminos ocultos y debilidades de jefes. |
| **Habilidades de Círculo** | `1`, `2`, `3`, `4` | Hechizos poderosos con tiempos de recarga (*cooldowns*). |

---

### 3. Narrativa y Mundo

* **Contexto:** Eres un mago recién graduado enviado a una torre "segura". Los maestros (Círculo 3-6) ya han limpiado los dos primeros pisos. Tu misión es limpiar enemigos residuales y entrenar.
* **El Giro:** La torre empieza a cambiar y el peligro escala más allá de lo que los maestros pueden manejar. Lo que era un entrenamiento se convierte en una lucha por la supervivencia y la ascensión divina.
* **Jerarquía de Poder:** La mayoría de magos son Círculo 4. El Director de tu escuela es Círculo 6. Tú aspiras a romper el límite humano hasta el **Círculo 10 (Nivel 100)**.

### 4. Estructura de Progresión (Círculos)

El progreso es manual y se realiza en los **Centros de Teletransporte/Hogueras** al inicio de cada piso.

* **Niveles:** 10 niveles por cada Círculo.
* **Requisitos de Ascensión:** Para pasar al siguiente Círculo (Nivel 10, 20, etc.), se requiere:
1. Haber alcanzado el nivel máximo del círculo actual.
2. Encontrar un **Material de Catalización** (exploración).
3. Superar la prueba de un **Maestro** (Boss/NPC).


* **Bloqueo de Habilidad:** No puedes usar hechizos o habilidades de un círculo superior al tuyo (ej. no hay Dash Elemental hasta el nivel 10).

### 5. Level Design y Navegación

* **Pisos Diseñados a Mano:** Escenarios fijos para fomentar el aprendizaje del mapa y la colocación estratégica de secretos.
* **Interconectividad:** * **Hubs de Piso:** Puntos de teletransporte que requieren "Registro de Firma Mágica" para activarse.
* **Atajos (Shortcuts):** Escaleras, palancas y ascensores que conectan el final del piso con el inicio.
* **Backtracking:** Con habilidades nuevas (como el Dash), el jugador regresa a pisos inferiores para acceder a zonas que antes eran una muerte segura (ej. el pasillo de fuego del Piso 1).
