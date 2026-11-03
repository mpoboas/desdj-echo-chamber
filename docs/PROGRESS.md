# Progress

Branch: `feature/unity-port`. Resume here after any interruption (also read `git log`).

## Milestones

- M0 Project setup: done
- M1 Simulation (pure C#, tests, headless): done. 52 EditMode tests green, all 5 endings reachable headless
- M2 World: next
- M3 Control room: pending
- M4 Full turn (D2.2): pending
- M5 Discovery: pending
- M6 Full game (D3.1): pending
- M7 Polish and build: pending

## How to run things

- Tests: Unity menu `Echo Chamber/Run EditMode Tests`, result in `Logs/test-results.txt`.
- Re-import docs/data into assets: menu `Echo Chamber/Import Content From docs-data`.
- Headless run: `HeadlessRunner.Run(content, tuning, PolicyLibrary.TargetSleep(), seed)` (about 14 ms per full 3-day game).

## Headless results (40 seeds each)

- all-normal: Alive 100%
- all-amplify: mostly Locked
- all-hide: Alive 100%, revenue 0
- target-sleep: Sleep 24, Alive 15, Gentry 1
- target-locked: Locked 39
- target-alive: Alive 40
- target-balance: Balance 30, Alive 7, Sleep 2, Locked 1
- target-gentry: Gentry 21, Alive 9, Locked 7, Balance 3

## Known problems

- all-normal ends Alive with day revenue under target (cohesion high). Balance pass planned for M7.

## Next step

M2: street world scene (GLB import, WorldMap, NavMesh, neighbours bound to the simulation).
