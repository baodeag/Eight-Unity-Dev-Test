# Eight Unity Dev Test

Unity third-person collection game built for the Eight Unity developer test.

## Overview

The player starts from a landscape scene, watches a short camera intro around the map, then controls a third-person character with a virtual joystick. Gems spawn randomly on the ground over time. The player moves, climbs low obstacles, attacks nearby gems to collect them, and wins after reaching the target score.

## Unity Version

- Unity `6000.5.8f1`
- Universal Render Pipeline
- Input System package enabled
- Main scene: `Assets/Game/Scenes/Landscape.unity`

## How To Run

1. Open the project in Unity `6000.5.8f1` or a compatible Unity 6 version.
2. Open `Assets/Game/Scenes/Landscape.unity`.
3. Press Play.
4. Click `Start` to begin the intro and enter gameplay.

## Android Build Notes

The project is configured for landscape gameplay on Android.

Player Settings:

- Default Orientation: `Auto Rotation`
- Allowed orientations: `Landscape Left`, `Landscape Right`
- Portrait orientations are disabled

To build:

1. Switch platform to Android in `File > Build Profiles`.
2. Confirm `Landscape.unity` is included in Scenes In Build.
3. Build APK/AAB.

## Controls

- Virtual joystick: move the player relative to the camera direction
- Swipe on the gameplay area: rotate the third-person camera
- Multi-touch support: hold the joystick with one finger and rotate the camera with another
- Attack button: play attack animation and collect the nearest gem inside attack range
- Reset button: reload the current scene and clear saved score

## Implemented Requirements

- Landscape gameplay screen
- Intro camera orbit around the map, then smooth blend behind the player
- Third-person follow camera with swipe rotation
- Player movement by virtual joystick, relative to camera angle
- Idle and run animation control
- Boundary clamp to keep the player inside the map
- Climb detection and climb animation for low obstacles
- Attack action while movement remains available
- Random gem spawning over time
- Object pooling for gems
- Multiple gem types with different scores and spawn weights
- Gem collection animation flying toward the UI icon
- Score saving with `PlayerPrefs`
- Win condition at target score `10`
- Win panel and `ConfettiBlastRainbow` particle effect
- Start and Reset buttons

## Code Structure

```text
Assets/Game/Scripts/
  Camera/
    CameraController.cs
    IntroCameraSequence.cs
    Billboard.cs
  Core/
    GameManager.cs
    GameState.cs
    SaveManager.cs
    ScoreManager.cs
  Gems/
    Gem.cs
    GemFactory.cs
    GemPool.cs
    GemSpawner.cs
    GemType.cs
  Player/
    PlayerController.cs
    ClimbDetector.cs
  UI/
    AttackButton.cs
    UIManager.cs
    VirtualJoystick.cs
  World/
    Boundary.cs
```

## Design Notes

- `GameManager` controls game states: waiting, intro, playing, win.
- `ScoreManager` owns score, gem count, target score, and win event.
- `SaveManager` wraps `PlayerPrefs` persistence.
- `GemPool` avoids repeated gem instantiation during gameplay.
- `GemFactory` chooses gem types by weighted random values.
- `Gem` uses `MaterialPropertyBlock` for per-instance color and emission without creating runtime material copies.
- `CameraController` ignores touches over UI, so joystick input does not block camera swipe input from another finger.

## Verification

The C# project was verified with:

```powershell
dotnet restore Assembly-CSharp.csproj
dotnet build Assembly-CSharp.csproj --no-restore
```

Expected result: build succeeds with `0 Warning(s), 0 Error(s)`.
