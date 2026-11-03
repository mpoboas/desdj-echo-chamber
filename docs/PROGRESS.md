# Progress

Branch: `feature/unity-port`. Resume here after any interruption (also read `git log`).

## Milestones

- M0 Project setup: done
- M1 Simulation (pure C#, tests, headless): done. 52 EditMode tests green, all 5 endings reachable headless
- M2 World: done (street, square, crosswalks, traffic lights, NavMesh, 14 neighbours following the simulation, day/night). Verified in Play Mode
- M3 Control room: done (room built by code, KayKit furniture, interactive objects, Cinemachine rig with 6 views, live CRT map via RenderTexture, mixer, lever, stamps, clock). Verified in Play Mode
- M4 Full turn (D2.2): done. Printer slips, stamps A/O/N, tube, lever, 26 s simulation, newspaper, day-end card, HUD, subtitles. A QA bot (`DebugTurnBot`) played 3 full days in Play Mode with no console errors
- M5 Discovery: done. Click neighbours with a visible reaction to note it (subtitles), cork board fills with traits, chips and red strings, slip footers show observed patterns, perceived neighbourhood in HUD, newspaper and final report. Newspaper never states causes
- M6 Full game (D3.1): done. Main menu, how-to card, 3 days with scripted posts, day objectives, 5 endings (headless-verified), final report with coesao/tensao chart, tutorial hints, restart. A bot played the whole game and restarted without console errors
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

- Reaction symbols use ASCII glyphs; prototype used heart/euro icons.
- Characters look good but small from the window view (same scale as prototype).
- Square string lights at night were not confirmed visually.
- Room extras not ported yet: dust motes, light shafts, cables.
- Window view camera sits outside the building and shows the wall edge at the bottom.
- all-normal ends Alive with day revenue under target (cohesion high). Balance pass planned for M7.

## Next step

M7: procedural audio, lighting polish, balance with the headless simulator, macOS build, 10+ minutes of play.
