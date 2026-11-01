# METRO RUNNER 3D - Setup, Prefabs & Cross-Platform Deployment Guide

This document provides a comprehensive setup, architecture reference, prefab hierarchy, asset specification, and deployment guide for **Metro Runner 3D** (Original 3D Endless Runner Game).

---

## 📁 1. Project Directory Structure

```
MetroRunner3D/
├── Assets/
│   ├── Animations/
│   │   ├── Player_Idle.anim
│   │   ├── Player_Run.anim
│   │   ├── Player_Jump.anim
│   │   ├── Player_Roll.anim
│   │   ├── Player_Hit.anim
│   │   └── Player_Victory.anim
│   ├── Audio/
│   │   ├── BGM_Arcade_Synth.wav
│   │   ├── SFX_Coin_Pickup.wav
│   │   ├── SFX_Jump_Whoosh.wav
│   │   ├── SFX_Slide_Swoosh.wav
│   │   ├── SFX_Hoverboard_Power.wav
│   │   └── SFX_Crash_Impact.wav
│   ├── Materials/
│   │   ├── Mat_TrackGround.mat
│   │   ├── Mat_RailSteel.mat
│   │   ├── Mat_CoinGold.mat
│   │   └── Mat_NeonSign.mat
│   ├── Prefabs/
│   │   ├── Collectibles/
│   │   │   ├── Prefab_Coin.prefab
│   │   │   ├── Prefab_Gem.prefab
│   │   │   ├── Prefab_Key.prefab
│   │   │   └── Prefab_MysteryBox.prefab
│   │   ├── Environment/
│   │   │   ├── Track_CityStreet.prefab
│   │   │   ├── Track_SubwayTunnel.prefab
│   │   │   ├── Track_Bridge.prefab
│   │   │   └── Track_ParkWay.prefab
│   │   ├── Obstacles/
│   │   │   ├── Obstacle_TrainCar.prefab
│   │   │   ├── Obstacle_LowHurdle.prefab
│   │   │   ├── Obstacle_HighBarricade.prefab
│   │   │   └── Obstacle_TrafficCones.prefab
│   │   └── Powerups/
│   │       ├── Powerup_Magnet.prefab
│   │       ├── Powerup_Jetpack.prefab
│   │       ├── Powerup_Multiplier.prefab
│   │       └── Powerup_Hoverboard.prefab
│   └── Scripts/
│       ├── Core/ (GameManager, ObjectPooler, SaveManager, InputManager)
│       ├── Player/ (PlayerController, CameraFollow)
│       ├── Track/ (TrackGenerator, TrackChunk)
│       ├── Systems/ (PowerUpManager, EconomySystem, MissionSystem, AudioManager, EnvironmentManager)
│       └── UI/ (UIManager, HUDView, ShopView, SettingsView)
└── Setup_And_Deployment_Guide.md
```

---

## 🎮 2. Original Character & Board Specifications

| Item ID | Name | Category | Base Attributes | Price (Coins / Gems) |
|---|---|---|---|---|
| `alex` | Alex (Metro Dash) | Character | Balanced runner (Default) | Free (Unlocked) |
| `maya` | Maya (Spark Dancer) | Character | +15% Agility & Jump Height | 500 Coins |
| `bolt` | Bolt (Street Beats) | Character | +25% Coin Magnet Radius | 1,200 Coins |
| `phantom` | Phantom Ninja | Character | Faster Lane Switching | 2,500 Coins / 10 Gems |
| `cyber` | Cyber Runner 2099 | Character | Holographic speed trail & +35% Speed | 5,000 Coins / 25 Gems |
| `hover_prime` | Hover Prime | Board | Crash Protection (Default) | Free (Unlocked) |
| `flame_rider` | Flame Rider 3000 | Board | Plasma Exhaust & +20% Speed | 800 Coins |
| `cyber_pulse` | Cyber Pulse Neon | Board | Magnetic Aura & Crash Shield | 2,000 Coins |

---

## ⚙️ 3. Unity & .NET Setup Instructions

### Unity (C#) Setup
1. **Open Unity 2022.3 LTS or later**.
2. Create a new **3D Project**.
3. Copy the contents of `Assets/Scripts/` into your Unity project's `Assets/Scripts/` folder.
4. Create an empty GameObject named `[GameManager]` and attach:
   - `GameManager.cs`
   - `ObjectPooler.cs`
   - `SaveManager.cs`
   - `InputManager.cs`
   - `PowerUpManager.cs`
   - `MissionSystem.cs`
   - `EnvironmentManager.cs`
5. Create a `Player` GameObject in the scene with a `CharacterController` component and attach `PlayerController.cs`.
6. Attach `CameraFollow.cs` to the Main Camera.

### ASP.NET Core 8.0 & Web Engine Setup
1. Run the local backend server:
   ```powershell
   & "C:\Program Files\dotnet\dotnet.exe" run --urls "http://localhost:5000"
   ```
2. Navigate to `http://localhost:5000` in any WebGL-capable browser.

---

## 🚀 4. Mobile & Web Deployment Guidelines

### Android (APK / AAB)
- In Unity Build Settings, select **Android**.
- Set Target API Level to **34 (Android 14)** or higher.
- Enable **IL2CPP** scripting backend and check **ARM64**.
- Enable **Texture Compression**: ASTC.

### iOS (Xcode Project)
- In Unity Build Settings, select **iOS**.
- Enable **Metal Graphics API**.
- Set Target iOS version to **13.0+**.
- Export Xcode project and sign with Apple Developer certificate.

### WebGL
- Set WebGL Memory Size to **512 MB**.
- Enable **Gzip / Brotli Compression**.
