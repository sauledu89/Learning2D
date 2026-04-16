# Cambios del Tercer Parcial

## Runic Gungeon — Historial de desarrollo P3

**Período:** Marzo – Abril 2026
**Alumno:** Saul Eduardo Gonzalez Vargas

---

## Estado del proyecto al inicio del parcial

Al comenzar el tercer parcial, el proyecto contaba con:

- ✅ Movimiento Top-Down con máquina de estados (Idle, Walking, Dodging)
- ✅ Dodge Roll con invulnerabilidad temporal
- ✅ Screen Wrap (efecto Pac-Man)
- ✅ Sistema de salud del jugador (`PlayerHealth`)
- ✅ Enemigo melee con FSM (Patrol → Chase → Attack con Dash físico)
- ✅ Runas colocadas estáticamente en el escenario
- ✅ Sistema de puntuación básico
- ✅ GameFlowManager con condiciones de victoria y derrota
- ✅ Cámara principal y minimapa
- ✅ Menú principal

**Diagnóstico:** El juego carecía de interacción ofensiva por parte del jugador (no podía eliminar enemigos), los coleccionables eran estáticos y no existía un bucle de combate real.

---

## Mecánicas añadidas en el 3er Parcial

---

### Mecánica 1: Pistola del Jugador

**Descripción técnica:**
Se implementó un sistema de arma de fuego que rota 360° siguiendo la posición del cursor del ratón en el espacio del mundo (`Camera.main.ScreenToWorldPoint`). Dispara instancias de un prefab `Bullet` con `Clic Izquierdo` y recarga el cargador con la tecla `R`.

**Scripts creados:**
- `WeaponController.cs` — Controla rotación del arma, cooldown de disparo, lógica de cargador y recarga
- `Bullet.cs` — Comportamiento del proyectil: movimiento, colisión y autodestucción por tiempo o impacto

**Scripts modificados:**
- `PlayerStateManager.cs` — Se añadió llamada para desactivar el objeto "Arma" durante el estado `Dodging` para evitar desfase visual
- `PlayerHealth.cs` — Sin cambios estructurales, pero se verificó integración con el nuevo sistema de daño

**Decisiones técnicas importantes:**
- El prefab de bala usa un `Rigidbody2D` cinemático para mayor control sobre la trayectoria
- Se usó `Physics2D.IgnoreCollision` entre la bala del jugador y el propio jugador para evitar auto-daño
- El arma se desactiva (no destruye) durante el Dodge Roll mediante `weaponObject.SetActive(false)`

**Problemas encontrados:**
- Desfase visual entre la mano animada y el sprite del arma durante la esquivada → Resuelto desactivando el GameObject del arma en el estado Dodging

---

### Mecánica 2: Enemigo con Pistola (Shooter Enemy)

**Descripción técnica:**
Se creó un nuevo tipo de enemigo que integra una FSM extendida. A diferencia del enemigo melee, este mantiene una `preferredDistance` con el jugador y en el estado de ataque dispara proyectiles en lugar de embestir.

**Scripts creados:**
- `EnemyShooter.cs` — FSM con estados: Patrol, Chase, Shoot. Hereda la lógica base de movimiento y patrullaje, añadiendo el estado Shoot con cooldown de disparo
- `EnemyBullet.cs` — Proyectil del enemigo; al colisionar con el jugador llama `PlayerHealth.TakeDamage()`

**Scripts modificados:**
- `GameFlowManager.cs` — Se añadió el nuevo prefab del enemigo shooter a la lista `enemyControllers` para que sea deshabilitado correctamente al terminar la partida

**Decisiones técnicas importantes:**
- El enemigo shooter no ataca si hay una distancia mayor a su `shootRange` ni si el jugador está demasiado cerca (se aleja primero)
- La bala enemiga usa un Layer separado (`EnemyBullet`) para que las físicas no interfieran con las balas del jugador

**Problemas encontrados:**
- El enemigo disparaba a través de objetos sin física → Pendiente de resolver con Raycasting de línea de visión en versión futura
- Precisión excesiva al inicio del testing → Balanceado añadiendo variación aleatoria en el ángulo de disparo (`Random.Range(-spreadAngle, spreadAngle)`)

---

### Mecánica 3: Spawn de Enemigos + Drop de Runas

**Descripción técnica:**
Se implementó un sistema de generación continua de enemigos en puntos específicos del mapa (`SpawnPoint[]`). Al morir, cada enemigo evalúa una probabilidad configurada y puede instanciar una Runa en su posición. Las runas fijas del escenario se eliminaron.

**Scripts creados:**
- `EnemySpawner.cs` — Genera enemigos periódicamente con un intervalo configurable. Soporta un límite máximo de enemigos activos simultáneos para controlar el rendimiento
- `EnemyHealth.cs` — Sistema de vida para enemigos. Al llegar a 0, evalúa la probabilidad de drop y notifica al spawner

**Scripts modificados:**
- `CollectibleItem.cs` — Sin cambios en lógica, pero ahora también se instancia dinámicamente desde `EnemyHealth` además de estar colocado en escena
- `CollectibleGoalManager.cs` — Sin cambios; funciona correctamente con runas tanto estáticas como instanciadas en tiempo de ejecución

**Decisiones técnicas importantes:**
- El spawner respeta el límite `maxEnemiesAlive` para evitar sobrecarga
- La probabilidad de drop se configura por inspector (0.0 a 1.0) usando `Random.value <= dropChance`
- Se dejó preparada la estructura para implementar **Object Pooling** en una iteración futura (actualmente usa `Instantiate/Destroy`)

**Problemas encontrados:**
- Acumulación de enemigos en el mismo SpawnPoint causaba solapamiento visual → Resuelto añadiendo un pequeño offset aleatorio al punto de aparición (`Random.insideUnitCircle * 0.5f`)
- El contador de runas en la UI no actualizaba en la primera Runa dropeada → El `goalManager` se encontraba mediante `FindFirstObjectByType` con un frame de retraso; resuelto pre-asignando la referencia en el prefab de Runa instanciada

---

## Resumen de cambios por archivo

| Archivo | Tipo de cambio | Descripción |
|---|---|---|
| `WeaponController.cs` | **Nuevo** | Control de arma del jugador |
| `Bullet.cs` | **Nuevo** | Proyectil del jugador |
| `EnemyShooter.cs` | **Nuevo** | FSM del enemigo con pistola |
| `EnemyBullet.cs` | **Nuevo** | Proyectil del enemigo |
| `EnemySpawner.cs` | **Nuevo** | Generador de enemigos |
| `EnemyHealth.cs` | **Nuevo** | Vida de enemigos y drop de runas |
| `PlayerStateManager.cs` | **Modificado** | Desactivar arma en Dodging |
| `GameFlowManager.cs` | **Modificado** | Soporte para múltiples tipos de enemigo |
| `CollectibleItem.cs` | Sin cambios lógicos | Ahora también usado por drops |

---

## Resultados obtenidos

- El juego ahora tiene un **bucle de combate completo**: el jugador puede eliminar enemigos con su arma
- Los enemigos generan **presión activa** (shooter) y **presión pasiva** (melee), diversificando la estrategia
- Las Runas se obtienen en combate, haciendo que la recolección dependa de la habilidad del jugador
- El **gameloop es funcional**: el jugador puede ganar (runas) o perder (sin HP), con puntuación que refleja el desempeño
