# Interview Test Gem Collector

## Build scene

Open the project in Unity, then run:

`Interview Test > Build Gem Collector Scene`

The builder creates:

- `Assets/Scene/Interview_Test_Landscape.unity`
- `Assets/InterviewTest/Prefabs/Interview Player.prefab`
- `Assets/InterviewTest/Prefabs/Interview Gem.prefab`
- `Assets/InterviewTest/Data/Interview_Player.controller`
- Three gem type assets: common, rare, epic

## Notes

- Runtime scripts only drive gameplay logic.
- Canvas, buttons, joystick, win panel, player, gem prefab, animator controller, climbable walls, spawn area, and managers are built as Unity scene/prefab objects by the Editor builder.
- The project currently contains `Idle`, `Run`, `Climb`, and `Jumping` animation clips, but no `Attack.anim`. The builder still creates an `Attack` animator state and trigger, using `Jumping.anim` as a temporary fallback. Replace that state's motion with a real attack clip if one is added later.
- Score and collected gem count are saved through `PlayerPrefs` after every successful gem collection.
- Win condition is based on score: reach the target score to show `You Win`.
- Gem scores: Common = 1, Rare = 2, Epic = 3.
