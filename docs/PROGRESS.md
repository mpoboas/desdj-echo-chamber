# Progress

Branch: `feature/unity-port`. Resume here after any interruption (also read `git log`).

## Final status (all milestones done)

| Milestone | State | Notes |
|---|---|---|
| M0 Project setup | done | folders, asmdefs, Cinemachine, glTFast, Newtonsoft, `.gitignore` |
| M1 Simulation | done | pure C# (`EchoChamber.Simulation`), 52 EditMode tests green, headless 3-day runner |
| M2 World | done | street, square, crosswalks, traffic lights, NavMesh, 14 neighbours driven by the utility AI, day/night |
| M3 Control room | done | room built by code, interactive objects, Cinemachine rig with 6 views, live CRT map |
| M4 Full turn (D2.2) | done | printer slips, stamps A/O/N, tube, lever, 26 s simulation, newspaper, HUD |
| M5 Discovery | done | click reactions to note them, cork board, perceived neighbourhood, newspaper without causes |
| M6 Full game (D3.1) | done | menu, 3 days, objectives, 5 endings, final report with chart, tutorial hints, restart |
| M7 Polish and build | done | procedural audio, time-of-day lighting, balance report, macOS build |

Completion criteria check:

- Menu, 3 days, 9 turns, no console errors: a QA bot (`DebugTurnBot`) played the full game twice in Play Mode through the menu (including restart) with no errors.
- 5 endings reachable: confirmed by the headless simulator (`docs/BALANCE.md`, unit tests `EveryEndingIsReachableWithAScriptedPlayer`).
- Discovery and "bairro percebido": HUD pill, newspaper, final report, cork board.
- macOS build: `Builds/macOS/EchoChamber.app` (214 MB) builds with 0 errors and launches (Metal, Apple M4) with no exceptions in `Player.log`. Not visually checked inside the build because the Mac screen was locked during the check; Play Mode in the Editor was checked with screenshots.
- 10 minutes of play: about 90 s per turn (printing 3 to 4 slips, decision time, 26 s simulation, newspaper) so 9 turns are 12 to 14 minutes plus intro and day cards. Not timed with a human.

## How to run everything

- Play: open `Assets/_Project/Scenes/Main.unity` and press Play (needs Unity 6000.6.1f1). Or run the build `Builds/macOS/EchoChamber.app`.
- Rebuild the scene from code: menu `Echo Chamber/Build Main Scene` (street, room, camera rig, UI, audio, NavMesh). Manual scene edits are lost on rebuild; change the builders in `Assets/_Project/Scripts/Editor`.
- Re-import content: menu `Echo Chamber/Import Content From docs-data` (`docs/data/*.json` to `GameContent` asset). Tuning numbers live in `Assets/_Project/Data/TuningConfig.asset`.
- Tests: menu `Echo Chamber/Run EditMode Tests`, results in `Logs/test-results.txt` (52 tests).
- Balance report: menu `Echo Chamber/Write Balance Report` writes `docs/BALANCE.md` (about 1 min).
- Console mirror: `Logs/console.txt` (written by `ConsoleCapture`).
- Build: `unity command build run --target StandaloneOSX --outputPath <path>.app --confirm true`, then poll `unity command build status`.
- QA bot: add `DebugTurnBot` to the GameManager object in Play Mode (not in the scene by default) and raise `GameManager.timeScale`.
- Controls: A amplify, O hide, N normal, Space lever, Esc back to desk view, mouse for everything else. Debug: `GameManager.timeScale`.

## Known problems and future improvements

- Reaction symbols use ASCII glyphs (`!`, `#`, `+`, `$`, `z`) instead of the prototype heart and euro icons.
- Room extras not ported: dust motes, light shafts, cables, framed poster artwork (posters are flat colours).
- Characters are small from the window view (same scale as the prototype). Window view camera shows the building edge at the bottom.
- Square string lights at night were not confirmed visually.
- Relations are discovered generously: one observation can reveal many pairs (same rule as the prototype), so the cork board can get crowded with red strings late in the game.
- Doing nothing (all NORMAL) ends as "Bairro vivo, app em perda" every time (see `docs/BALANCE.md`), as in the prototype numbers.
- The Unity AI generators (audio, image) report `NoSubscription` for this account, so audio is synthesised in code. Sound design is functional, not polished; levels were not checked by ear.
- URP logs that Depth of Field and Panini post-process shaders are stripped in the build. They are not used.
- Windows build not done (only the macOS module is installed).
- `unity test` cannot run while the Editor has the project open, hence the menu-based test runner.
