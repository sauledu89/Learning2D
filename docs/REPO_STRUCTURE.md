# Organización del Repositorio

## Runic Gungeon — Estructura y contenido del repositorio

---

## Estructura de carpetas

```
runic-gungeon/
│
├── README.md                        ← Documentación principal del proyecto
│
├── Assets/                          ← Contenido del proyecto Unity
│   │
│   ├── Scenes/
│   │   ├── MainMenu.unity           ← Escena del menú principal (punto de entrada)
│   │   └── Game.unity               ← Escena jugable principal
│   │
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerStateManager.cs
│   │   │   ├── PlayerHealth.cs
│   │   │   └── WeaponController.cs  ← [NUEVO P3]
│   │   │
│   │   ├── Enemy/
│   │   │   ├── EnemyStateManager.cs
│   │   │   ├── Enemy.cs
│   │   │   ├── EnemyShooter.cs      ← [NUEVO P3]
│   │   │   ├── EnemyHealth.cs       ← [NUEVO P3]
│   │   │   └── EnemySpawner.cs      ← [NUEVO P3]
│   │   │
│   │   ├── Projectiles/
│   │   │   ├── Bullet.cs            ← [NUEVO P3]
│   │   │   └── EnemyBullet.cs       ← [NUEVO P3]
│   │   │
│   │   ├── Collectibles/
│   │   │   ├── CollectibleItem.cs
│   │   │   └── CollectibleGoalManager.cs
│   │   │
│   │   ├── Managers/
│   │   │   ├── GameFlowManager.cs
│   │   │   ├── ScoreManager.cs
│   │   │   ├── UIStateManager.cs
│   │   │   └── MenuManager.cs
│   │   │
│   │   └── World/
│   │       ├── WorldBounds.cs
│   │       ├── WrapMover2D.cs
│   │       ├── CameraFollow.cs
│   │       └── MinimapFollow2D.cs
│   │
│   ├── Prefabs/
│   │   ├── Player.prefab
│   │   ├── Enemy_Melee.prefab
│   │   ├── Enemy_Shooter.prefab     ← [NUEVO P3]
│   │   ├── Bullet.prefab            ← [NUEVO P3]
│   │   ├── EnemyBullet.prefab       ← [NUEVO P3]
│   │   └── Runa.prefab
│   │
│   ├── Sprites/                     ← Assets gráficos pixel art
│   ├── Animations/                  ← Controladores y clips de animación
│   └── Audio/                       ← Música y efectos de sonido
│
├── docs/
│   ├── ARCHITECTURE.md              ← Arquitectura técnica del proyecto
│   ├── CHANGELOG_P3.md              ← Cambios específicos del 3er parcial
│   ├── REPO_STRUCTURE.md            ← Este archivo
│   └── images/
│       ├── gameplay.png
│       ├── gameover.png
│       ├── victory.png
│       ├── mainmenu.png
│       └── shooter_enemy.png
│
└── .gitignore                       ← Exclusiones de Unity (Library/, Temp/, etc.)
```

---

## Archivos excluidos del repositorio

El archivo `.gitignore` está configurado con la plantilla oficial de Unity para excluir:

| Carpeta / Archivo | Razón de exclusión |
|---|---|
| `Library/` | Cache generado automáticamente por Unity (puede ser >1 GB) |
| `Temp/` | Archivos temporales de compilación |
| `Logs/` | Logs de sesión del editor |
| `obj/` | Archivos de compilación intermedios |
| `*.csproj`, `*.sln` | Archivos de solución generados automáticamente |
| `UserSettings/` | Configuración local del editor (varía por máquina) |

---

## Archivos esenciales incluidos

| Archivo / Carpeta | Por qué está incluido |
|---|---|
| `Assets/Scenes/` | Contiene las escenas necesarias para ejecutar el juego |
| `Assets/Scripts/` | Todo el código fuente del proyecto |
| `Assets/Prefabs/` | Objetos reutilizables (jugador, enemigos, proyectiles) |
| `Assets/Sprites/` | Arte visual del juego |
| `Assets/Animations/` | Animaciones del jugador y enemigos |
| `ProjectSettings/` | Configuración de Unity (capas, tags, física, calidad) |
| `Packages/manifest.json` | Dependencias del proyecto (Input System, TextMeshPro) |
| `README.md` | Documentación principal |
| `docs/` | Documentación técnica complementaria |

---

## Convención de commits

Los commits en este repositorio siguen la estructura:

```
<tipo>: <descripción breve en inglés>
```

| Tipo | Cuándo usarlo |
|---|---|
| `Add` | Nueva funcionalidad o archivo |
| `Fix` | Corrección de un bug |
| `Refactor` | Reorganización de código sin cambio de comportamiento |
| `Update` | Modificación de funcionalidad existente |
| `Docs` | Cambios en documentación |
| `Remove` | Eliminación de código o archivos |

**Ejemplos de commits en este proyecto:**
```
Add WeaponController with fire cooldown and magazine system
Add EnemyShooter FSM with patrol, chase and shoot states
Add EnemySpawner with configurable interval and max enemies
Fix runa drop reference not found on first frame
Update PlayerStateManager to disable weapon during Dodge
Docs: Add ARCHITECTURE.md with system dependency diagram
Refactor EnemyStateManager to support multiple attack patterns
```

---

## Cómo navegar el repositorio

Para revisar el proyecto sin abrirlo en Unity:

1. **Entender el juego** → Leer `README.md`
2. **Entender la arquitectura** → Leer `docs/ARCHITECTURE.md`
3. **Ver los cambios del parcial** → Leer `docs/CHANGELOG_P3.md`
4. **Revisar el código** → Navegar `Assets/Scripts/` por carpeta de sistema
5. **Ver evidencias visuales** → Revisar `docs/images/`
