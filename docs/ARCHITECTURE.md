# Arquitectura Técnica del Proyecto

## Runic Gungeon — Documento de Arquitectura

---

## Visión general del sistema

El proyecto está organizado en sistemas independientes que se comunican a través de **referencias directas en el Inspector** y el patrón **Singleton** para los managers globales. Esta arquitectura evita el acoplamiento excesivo y permite que cada sistema sea modificado sin romper los demás.

---

## Diagrama de sistemas principales

```
┌─────────────────────────────────────────────────────────┐
│                    GAME FLOW MANAGER                     │
│         (Singleton — Estado global del juego)            │
│   Gameplay | Paused | GameOver | Victory | Options       │
└──────┬──────────┬──────────┬──────────┬─────────────────┘
       │          │          │          │
       ▼          ▼          ▼          ▼
  [Player]   [Enemies]  [Score]     [UI Panels]
  State Mgr  State Mgr  Manager     HUD / Pause / GameOver
  Health     FSM        Survival    Victory
       │          │          ▲
       ▼          ▼          │
  [Collectible Goal Manager]─┘
       ▲
       │
  [Collectible Items / Runa Drops]
```

---

## Descripción de módulos

### 1. GameFlowManager (Singleton)

**Archivo:** `GameFlowManager.cs`

El núcleo de la experiencia. Mantiene el estado global del juego y es el único responsable de:
- Transicionar entre estados (`Gameplay`, `Paused`, `GameOver`, `Victory`, `Options`)
- Detener/reanudar el tiempo (`Time.timeScale`)
- Deshabilitar controladores de jugador y enemigos al terminar la partida
- Congelar cámaras
- Mostrar/ocultar paneles de UI

**Patrón:** Singleton con `Instance` estático. Todos los sistemas que necesiten saber si el juego está activo consultan `GameFlowManager.Instance.IsGameplay`.

---

### 2. Sistema del Jugador

**Archivos:** `PlayerStateManager.cs`, `PlayerHealth.cs`

**Máquina de estados del jugador:**

```
Idle ──► Walking ──► Dodging ──► Idle
  ▲          │                    │
  └──────────┘                    │
                                  ▼
                                Dead (terminal)
```

- `PlayerStateManager` gestiona el movimiento con `Rigidbody2D.MovePosition` en `FixedUpdate`
- Durante `Dodging`, el jugador cambia de Layer a `Ignore Raycast` para lograr invulnerabilidad
- `PlayerHealth` gestiona los HP, reproduce animaciones de daño y notifica la muerte al `GameFlowManager`

**Comunicación con otros sistemas:**
- Consulta `GameFlowManager.Instance.IsGameplay` para bloquear inputs si el juego no está activo
- Usa `WrapMover2D.TryWrap()` para el efecto de Screen Wrap en cada `FixedUpdate`
- Al morir, `PlayerHealth` llama `GameFlowManager.Instance.RequestGameOver()`

---

### 3. Sistema de Enemigos (FSM)

**Archivo:** `EnemyStateManager.cs`

**Máquina de estados del enemigo melee:**

```
Patrol ──► Chase ──► Attack
  ▲           │         │
  └───────────┘         │ (jugador lejos)
                        ▼
                      Chase
```

- **Patrol:** Se mueve entre waypoints. Si el jugador entra al `detectionRange`, transiciona a Chase.
- **Chase:** Persigue al jugador con `chaseSpeed`. Si alcanza `attackRange`, transiciona a Attack.
- **Attack:** Ejecuta un *Dash Attack* usando `Rigidbody2D.linearVelocity`. La colisión física con el jugador aplica daño a través de `PlayerHealth.TakeDamage()`.

---

### 4. Sistema de Coleccionables y Objetivo

**Archivos:** `CollectibleItem.cs`, `CollectibleGoalManager.cs`

- `CollectibleItem` detecta colisión con el jugador mediante `OnTriggerEnter2D`
- Al ser recogido, notifica a `CollectibleGoalManager.RegisterCollectible(value)`
- `CollectibleGoalManager` acumula el conteo y, al alcanzar `collectiblesToWin`, llama `GameFlowManager.Instance.RequestVictory()`
- La victoria calcula bonos de puntuación antes de cambiar el estado

---

### 5. Sistema de Puntuación

**Archivo:** `ScoreManager.cs`

Tres fuentes de puntos:

| Fuente | Método | Cuándo |
|---|---|---|
| Coleccionables | `AddScore(value)` | Al recoger una Runa |
| Sobrevivencia | Auto (tick cada 1s) | Durante Gameplay activo |
| Bono Victoria | `AddVictoryBonus(100)` | Al ganar |
| Bono de Salud | `AddHealthBonus(HP, 25)` | Al ganar (HP restante × 25) |

---

### 6. Sistema de Mundo y Screen Wrap

**Archivos:** `WorldBounds2D.cs`, `WrapMover2D.cs`

- `WorldBounds2D` es un Singleton que expone los límites del mundo (minX, maxX, minY, maxY)
- `WrapMover2D` expone el método helper `TryWrap(ref Vector2 pos, out Vector2 offset)`
- El `PlayerStateManager` llama a `TryWrap()` en cada `FixedUpdate` antes de aplicar `MovePosition`
- Si ocurre un wrap, se notifica a `CameraFollow` para que haga un `InstantSnap()` y no anime el recorrido largo

---

### 7. Sistema de Cámaras

**Archivos:** `CameraFollow.cs`, `MinimapFollow2D.cs`

Ambas cámaras son independientes y comparten el mismo patrón:
- Siguen al jugador con `Vector3.SmoothDamp` / `Vector3.Lerp` en `LateUpdate`
- Tienen un método `SetFrozen(bool)` que el `GameFlowManager` llama al terminar la partida
- `MinimapFollow2D` también respeta `Time.timeScale == 0` para no moverse durante la pausa

---

### 8. Sistema de UI

**Archivos:** `UIStateManager.cs`, `MenuManager.cs`

- `UIStateManager` gestiona los paneles del juego en tiempo real (InGame, Paused, Options)
- `MenuManager` gestiona la navegación del menú principal
- `GameFlowManager` tiene su propio conjunto de paneles (`hudPanel`, `pausePanel`, `gameOverPanel`, `victoryPanel`) y los controla centralizadamente mediante `ShowPanels()`

---

## Flujo completo de una partida

```
1. Escena "MainMenu" carga → MenuManager inicia en MainMenu
2. Jugador presiona "Jugar" → SceneManager.LoadScene("Game")
3. GameFlowManager.Start() → ResumeGame() → Estado: Gameplay
4. Jugador mueve y esquiva → PlayerStateManager procesa input
5. Jugador recoge Runas → CollectibleGoalManager.RegisterCollectible()
   ├── Si meta alcanzada → RequestVictory()
   │     ├── ScoreManager calcula bonos
   │     └── EndRoutine() → Estado: Victory → ShowPanels(victory)
   └── Si jugador muere → PlayerHealth.Die() → RequestGameOver()
         └── EndRoutine() → Estado: GameOver → ShowPanels(gameOver)
6. Desde cualquier panel final → RestartScene() o BackToMenu()
```

---

## Dependencias entre scripts

```
GameFlowManager
    ├── ScoreManager
    ├── PlayerHealth
    ├── CameraFollow
    ├── PlayerController (MonoBehaviour)
    └── EnemyControllers (List<MonoBehaviour>)

PlayerHealth
    ├── PlayerStateManager (OnPlayerDeath)
    └── GameFlowManager (RequestGameOver)

CollectibleGoalManager
    ├── ScoreManager (AddScore)
    └── GameFlowManager (RequestVictory)

CollectibleItem
    └── CollectibleGoalManager (RegisterCollectible)

EnemyStateManager
    ├── PlayerHealth (TakeDamage)
    └── Rigidbody2D (movimiento físico)

PlayerStateManager
    ├── WrapMover2D (TryWrap)
    ├── CameraFollow (InstantSnap)
    └── WorldBounds2D (referencia a límites)
```
