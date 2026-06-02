# DayNight — system spec

A time-of-day cycle that drives lighting, day counting, and scheduled scene events. Producer / consumer split via ScriptableObject channels — the controller writes `TimeOfDay`; everything else reads.

```mermaid
classDiagram
    class DayNightData {
        +float CycleSeconds
        +float DawnThreshold
        +float DuskThreshold
    }
    class DayNightController {
        +DayPhase CurrentPhase
        +bool IsRunning
        +Tick(float)
        +Run() / Pause()
    }
    class DayNightView {
        +Light sun
        +AnimationCurve daylightCurve
        +sun / skybox / ambient
    }
    class DayNightSchedule {
        +Entry[] entries
        (day, hour) -> raise channel
    }
    class FloatVariable
    class IntVariable
    class GameEvent

    DayNightController --> DayNightData
    DayNightController --> FloatVariable : write timeOfDay
    DayNightController --> IntVariable : write dayCount
    DayNightController --> GameEvent : raise dawn/dusk/newDay
    DayNightView --> FloatVariable : read timeOfDay
    DayNightSchedule --> FloatVariable : read timeOfDay
    DayNightSchedule --> IntVariable : read dayCount
    DayNightSchedule --> GameEvent : raise
```

## What's here

- **DayNightData** (SO) — cycle length in seconds + dawn / dusk thresholds (0..1).
- **DayNightController** — advances `timeOfDay` each frame, computes `Day` vs `Night`, raises `onDawn` / `onDusk` / `onNewDay` channels on transitions, increments `dayCount` at dawn.
- **DayNightView** — reads `timeOfDay`, rotates the sun, lerps sun intensity / skybox exposure / ambient brightness via a single `daylightCurve`. Owns a runtime instance of the skybox material so the shared asset isn't dirtied on disk.
- **DayNightSchedule** — fires `GameEvent` channels at specific `(day, hour)` targets. One-shot per entry; past targets at `OnEnable` are skipped.

## Lifetime / wiring

- One controller per scene. View and Schedule are independent — drop them in or omit them.
- All cross-component communication runs through the SO channels (`FloatVariable`, `IntVariable`, `GameEvent` in `Core/`). No direct component references.

## Why

- **SO channels decouple producer from consumer** — adding a new listener (a screen vignette, an enemy spawner, a worker schedule) means dragging the channel asset into a new component, no edits to the controller.
- **Single `daylightCurve`** drives sun / skybox / ambient together so the look stays coherent — tune one curve, the whole world changes.
- **Schedule order at dawn:** `onDawn` raised → `dayCount` incremented → `onNewDay` raised. Listeners that need the pre-increment moment subscribe to `onDawn`; those that need the new day number subscribe to `onNewDay`.
- **View instances the skybox material at runtime** — without this, a single Play session would permanently dirty the on-disk skybox asset, polluting Git diffs.
