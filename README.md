# 🛡️ Tower Defense — Simplified OOP Project

## Project Overview
A full-featured Tower Defense game developed with C# and Avalonia UI. 
The codebase is designed for accessibility (A1-B1 English) and follows a "public-by-default" architecture for rapid learning and easy state access.

---

## Architecture Principles

### 1. Simplified Data Access
- All critical fields and properties are `public` for direct access.
- Encourages developer discipline over strict encapsulation.
- Simplifies the game loop and state management for beginners.

### 2. Inheritance Hierarchy
```
GameObject (abstract)
├── Tower (abstract)
│   ├── ArrowTower
│   ├── CannonTower
│   └── IceTower
├── Enemy (abstract)
│   ├── BasicEnemy
│   ├── FastEnemy
│   └── BossEnemy
└── Shot (abstract)
    ├── Bullet
    ├── Ball
    └── Ice
```

### 3. Polymorphism
- `GameObject.Update()` and `Draw()` are implemented differently by each class.
- `Tower.TryShoot()` generates different `Shot` types.
- `Enemy.TakeDamage()` is overridden in `BossEnemy` for armor calculation.
- Looping over `List<Tower>` allows each tower to perform its unique behavior.

### 4. Interfaces
| Interface | Description | Implemented By |
|-----------|-------------|----------------|
| `IUpgradeable` | Upgrade/Sell | Tower |
| `IWaveSpawner` | Spawn waves | GameManager |
| `ISaveable` | Save/Load | GameManager, ScoreManager |

### 5. Custom Exceptions
| Exception | When it is thrown |
|-----------|-------------------|
| `GameException` | General game errors |
| `TowerPlacementException` | Invalid tower position |
| `InsufficientGoldException` | Not enough gold |
| `SaveLoadException` | File I/O errors |

---

## Features

### Gameplay Mechanics
- **Create**: Place towers by clicking on the map.
- **Read**: View tower stats, leaderboard, and game status.
- **Update**: Upgrade towers (up to Level 3).
- **Delete**: Sell towers to regain gold.

### Data Persistence
- High Scores: `Data/scores.json` (JSON format).

---

## Controls

| Control | Description |
|---------|-------------|
| Left Click (panel) | Place tower / Select tower |
| Right Click | Cancel selection |
| ⬆ Upgrade | Upgrade selected tower (max Lv.3) |
| 💰 Sell | Sell tower for gold |
| ▶ Send Wave | Start the next enemy wave |

---

## Project Structure
```
TowerDefense/
├── Core/
│   ├── GameObject.cs      # Abstract base
│   └── Interfaces.cs      # IUpgradeable, IWaveSpawner, ISaveable
├── Towers/
│   ├── Tower.cs           # Abstract tower base
│   └── TowerTypes.cs      # Arrow, Cannon, Ice, etc.
├── Enemies/
│   ├── Enemy.cs           # Abstract enemy base
│   └── EnemyTypes.cs      # Basic, Fast, Boss, etc.
├── Shots/
│   └── Shot.cs            # Abstract + Bullet, Ball, Ice
├── Managers/
│   ├── GameManager.cs     # Main game logic
│   ├── MapManager.cs      # Map + Grid handling
│   └── ScoreManager.cs    # Scores + JSON persistence
├── Effects/
│   └── Effects.cs         # Particle systems & visual effects
├── Exceptions/
│   └── GameException.cs   # Custom exception hierarchy
└── Program.cs             # Entry point
```

---

## Installation & Running
```bash
git clone <repo>
cd TowerDefense
dotnet run
```
