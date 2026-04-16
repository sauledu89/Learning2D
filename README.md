# Learning2D
Proyecto de videojuego 2D top-down desarrollado en Unity.
Recreación de Enter The Gungeon

Este proyecto se trabaja en la materia Tópicos Avanzados de programación 

# 🔫 Recreación de Enter The Gungeon — Proyecto Integrador 2D

> Recreación inspirada en *Enter The Gungeon* — Top-Down Bullet Hell 2D desarrollado en Unity

---

## 📖 Descripción general del juego

Es un juego de acción *bullet hell* con perspectiva Top-Down 2D donde el jugador debe sobrevivir oleadas de enemigos y recolectar **Blanks** para conseguir la victoria. El escenario es continuo gracias a un sistema de *Screen Wrap* (estilo Pac-Man), por lo que no existen bordes: salir por un lado significa aparecer por el opuesto.

El jugador cuenta con movimiento en 8 direcciones, una esquivada (*Dodge Roll*) con invulnerabilidad temporal, un arma de fuego apuntada con el ratón, y un sistema de salud. Los enemigos patrullan el mapa, persiguen al jugador al detectarlo y atacan mediante embestidas o disparos a distancia.

---

## 🎯 Objetivo del proyecto

Desarrollar un videojuego funcional en **Unity** aplicando conceptos avanzados de programación orientada a objetos, máquinas de estados finitos (FSM), arquitectura de sistemas desacoplados, y buenas prácticas de desarrollo de software.

El proyecto busca transformar un prototipo de movimiento base en una experiencia de acción completa con gameloop definido (condición de victoria y derrota), sistema de puntuación, gestión de enemigos y mecánicas de combate.

A largo plazo la recreación del gameloop y mecánicas esenciales de un Bullet Hell como lo es Enter the Gungeon 

---

## 🕹️ Mecánicas principales del juego

| Mecánica | Descripción |

| **Movimiento Top-Down** | El jugador se mueve en 8 direcciones usando WASD con un Rigidbody2D |
| **Dodge Roll** | Esquivada con invulnerabilidad temporal (Spacebar). Cooldown incluido |
| **Screen Wrap** | Al salir del mapa por un borde, el jugador reaparece en el lado opuesto |
| **Sistema de Salud** | El jugador tiene HP limitado; al llegar a 0 se activa Game Over |
| **Coleccionables (Blanks)** | Recoger N runas activa la condición de Victoria |
| **Enemigo Melee (FSM)** | Enemigo con estados: Patrulla → Persecución → Ataque (embestida física) |
| **Score** | Puntuación por coleccionables, sobrevivencia por tiempo, y bonos al ganar |
| **Minimapa** | Cámara secundaria que sigue al jugador |
| **Pausa** | El juego se puede pausar con `Esc` o `P` en cualquier momento |
| **GameFlow Manager** | Gestiona los estados globales: Gameplay, Paused, GameOver, Victory |
| **Menú Principal** | Pantalla de inicio con opciones de jugar, opciones y salir |

---

## ✨ Mecánicas implementadas en el 3er Parcial

### 1. 🔫 Pistola del Jugador
El jugador obtiene un arma de fuego que rota 360° siguiendo la posición del cursor del ratón. Dispara proyectiles (prefabs) con `Clic Izquierdo` y recarga con la tecla `R`. Incluye gestión de cargadores y cooldown de disparo.

**Problema que resuelve:** El jugador solo podía evadir amenazas, pero no eliminarlas. Esta mecánica añade interacción ofensiva y una capa estratégica de manejo de munición.

### 2. 🤖 Enemigo con Pistola (Shooter Enemy)
Un nuevo tipo de enemigo que, en lugar de embestir, mantiene una distancia de seguridad con el jugador y le dispara proyectiles. Su comportamiento está integrado en una FSM con estados: Patrulla, Persecución y Disparo.

**Problema que resuelve:** Diversifica los enfrentamientos y obliga al jugador a tomar decisiones distintas (cubrirse vs. perseguir), aumentando la profundidad táctica.

### 3. 🌀 Spawn de Enemigos + Drop de Runas
Un sistema de spawning genera enemigos de forma continua en puntos específicos del mapa. Al morir, los enemigos tienen probabilidad de soltar (drop) una Runa. Las runas ya no están fijas en el escenario, sino que se obtienen en combate.

**Problema que resuelve:** Crea un bucle de juego dinámico e infinito. El jugador debe combatir activamente para progresar, reemplazando la recolección estática por una recolección basada en habilidad.

---

## 🛠️ Tecnologías utilizadas

| Tecnología | Rol en el proyecto |
|---|---|
| **Unity 6000.2.6f2 LTS** | Motor de juego principal — física, renderizado, escenas |
| **C#** | Lenguaje de programación para todos los scripts |
| **Visual Studio Communityr** | Entorno de desarrollo y depuración |
| **Git & GitHub** | Control de versiones y repositorio remoto |
| **Unity Input System** | Manejo de entradas de teclado y ratón |
| **TextMeshPro** | Renderizado de texto en la UI del juego |
| **Aseprite** | Creación de sprites y animaciones pixel art |

---

## ▶️ Instrucciones de ejecución

### Requisitos
- **Unity Hub** instalado
- **Unity 6000.2.6f2 LTS** (o versión compatible)

### Pasos para abrir el proyecto

```
1. Clonar o descargar el repositorio:
   git clone https://github.com/[usuario]/runic-gungeon.git

2. Abrir Unity Hub

3. Hacer clic en "Add project from disk"

4. Seleccionar la carpeta raíz del repositorio

5. Unity abrirá el proyecto automáticamente

6. En la ventana Project, ir a:
   Assets > Scenes > MainMenu

7. Presionar el botón ▶ Play para ejecutar
```

### Controles
| Acción | Control |
|---|---|
| Moverse | `W A S D` |
| Dodge Roll | `Spacebar` (con dirección) |
| Disparar | `Clic Izquierdo` |
| Recargar | `R` |
| Pausar | `Esc` o `P` |

---

## 📁 Estructura general del proyecto

```
Assets/
├── Scenes/
│   ├── MainMenu.unity       # Escena del menú principal
│   └── Game.unity           # Escena principal de juego
│
├── Scripts/
│   ├── Player/              # Movimiento, salud, arma del jugador
│   ├── Enemy/               # FSM de enemigos, spawner
│   ├── Collectibles/        # Runas y sistema de objetivos
│   ├── Managers/            # GameFlow, Score, UI
│   └── World/               # WorldBounds, WrapMover
│
├── Prefabs/
│   ├── Player.prefab
│   ├── Enemy_Melee.prefab
│   ├── Enemy_Shooter.prefab
│   ├── Bullet.prefab
│   └── Runa.prefab
│
├── Sprites/                 # Assets gráficos y pixel art
├── Animations/              # Controladores de animación
├── Audio/                   # Efectos de sonido y música
└── UI/                      # Fuentes, iconos y elementos de interfaz

docs/
├── ARCHITECTURE.md          # Arquitectura técnica del proyecto
├── CHANGELOG_P3.md          # Cambios del 3er parcial
└── REPO_STRUCTURE.md        # Organización del repositorio
```

---

## ⚙️ Scripts y sistemas principales

| Script | Función |
|---|---|
| `PlayerStateManager.cs` | Máquina de estados del jugador (Idle, Walking, Dodging, Dead). Controla movimiento y esquivada |
| `PlayerHealth.cs` | Gestiona HP del jugador, animaciones de daño y dispara el evento de muerte |
| `EnemyStateManager.cs` | FSM del enemigo melee: Patrulla, Persecución y Ataque (Dash físico) |
| `GameFlowManager.cs` | Singleton que controla el estado global: Gameplay, Paused, GameOver, Victory |
| `ScoreManager.cs` | Acumula puntos por coleccionables, tiempo de sobrevivencia y bonos de victoria |
| `CollectibleGoalManager.cs` | Registra runas recolectadas y activa Victoria al alcanzar la meta |
| `CollectibleItem.cs` | Comportamiento individual de cada runa (trigger de colisión con el jugador) |
| `WrapMover2D.cs` | Lógica de Screen Wrap: teletransporta objetos al borde opuesto del mundo |
| `WorldBounds2D.cs` | Define y expone los límites del mundo como Singleton |
| `CameraFollow.cs` | Seguimiento suavizado de la cámara principal con soporte para congelado |
| `MinimapFollow2D.cs` | Cámara del minimapa con seguimiento y soporte para congelado |
| `UIStateManager.cs` | Maneja los paneles de UI: InGame, Paused, Options |
| `MenuManager.cs` | Controla la navegación del menú principal entre sus estados |

---

## 🖼️ Evidencias visuales

<img width="1367" height="771" alt="image" src="https://github.com/user-attachments/assets/f3a82707-30d0-4e98-a2a7-86b09bd540e1" />
<img width="1362" height="766" alt="image" src="https://github.com/user-attachments/assets/1dcf6c8f-a303-4511-8e0c-dd36c7856bf3" />

---

## 👥 Créditos e integrantes

| Campo | Información |
|---|---|
| **Alumno** | Saul Eduardo Gonzalez Vargas |
| **Colaborador** | Gerardo Herrera Ortiz |
| **Docente** | Francisco Emiliano Aguayo Serrano |
| **Materia** | Tópicos Avanzados de Programación |
| **Subtema** | Desarrollo de videojuegos y medios interactivos |
| **Institución** | Universidad Cuauhtémoc |
| **Fecha** | Marzo - Abril 2026 |

---

## 📊 Estado actual del proyecto y conclusiones

El proyecto ha evolucionado de un prototipo de movimiento a un videojuego de acción funcional con gameloop completo. Durante el 3er parcial se añadieron las mecánicas ofensivas (arma del jugador), un nuevo tipo de enemigo inteligente y un sistema dinámico de spawning con drops, transformando la experiencia de recolección pasiva en combate activo.

**Posibles mejoras futuras:**
- Object Pooling para balas y enemigos (optimización de rendimiento)
- Más tipos de armas con diferentes comportamientos
- Sistema de oleadas (waves) con dificultad progresiva
- Efectos de partículas y feedback visual mejorado
- Soporte para gamepad / controlador
