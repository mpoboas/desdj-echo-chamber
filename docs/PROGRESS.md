# Progress

## ATUAL (2026-11-08): redesign v2, Fase 1 quase fechada

Estado: o **dia 1 joga-se de ponta a ponta** em `Assets/_Project/Scenes/Game.unity`. Falta o playtest com alguém externo. Nada da Fase 2 foi feito.

- **Como correr:** abrir `Game.unity` (ou menu `Echo Chamber/Build Game Scene (v2)` para a reconstruir) e Play. Clicar no LCD faz zoom, Esc volta à sala. Menus úteis: `Import Feed Content (v2)`, `Import Feed Clips`, `Run EditMode Tests` (resultado em `Logs/test-results.txt`, 105 testes a passar).
- **Feito:** simulação v2 em C# puro (`FeedSystem`, `FeedViewer`, `FeedDay`, `FeedCareer`), 47 posts (39 do jogador + 8 de fundo) com clip e texto, Central de Feeds em UI Toolkit no LCD 16:9, 11 vídeos 2D, rato lido à mão, telefone com a Sara e o Valente, fim de dia com avisos, janela com estores fechados, código v1 de apresentação removido.
- **Por fazer (Fase 2):** estores a abrir ao dia 3 e a rua com os 6 sinais ligados ao estado escondido dos vizinhos, caderno e cortiça, misturar e posts-ponte (dia 4), finais, dia 5. Calibração das metas só com playtest.
- **Conhecido:** o Game view do editor pode renderizar a meia resolução em ecrãs Retina (menu ⋮ do Game view, desmarcar "Low Resolution Aspect Ratios"). Antes de o dia começar o telemóvel em foco está preto. O simulador de rato do MCP só faz cliques simples: para testar arrastos, injetar estados de rato com `InputSystem.QueueStateEvent`.
- **Decisões:** `docs/DECISIONS.md`, entradas de 2026-11-08 (pivot, LCD, decisões da Fase 1). Plano: `PLAN.md`.
- **Referência visual:** `docs/prototypes/*.html` (só layout e animações, não fazem parte do jogo).

Tudo o que se segue é o histórico do v1 e **não descreve o jogo atual**.

---

Branch (v1): `feature/unity-port`. Resume here after any interruption (also read `git log`).

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

## Design iteration (2026-11-06)

Point 1 of the design review done: boss pressure and imposed posts (see `docs/DECISIONS.md`). 58 EditMode tests green. In `docs/BALANCE.md` all-normal went from 100% "Alive" to 85% (7% Locked), tension 25% to 47%; target-balance still wins 72%. Not yet checked visually in Play Mode: the "IMPOSTO" tag on the slip, the boss line and the end-of-day warning.
Point 4 done: limited stamp ink (2 hides per turn) plus resentment of silenced authors; all-hide went from 100% to 76% "Alive", target-balance 76%. 63 EditMode tests green. Silenced authors now show a visible `Silenciado` reaction (`x`, purple) that can be noted on the cork board; 67 tests green. 
Next from the review: point 2 (proposal approved in principle but parked, see `docs/PROPOSAL_PinsOnCorkBoard.md`; decisions still to confirm), point 3 is done (epilogue by tier and "Equilibrio raro" needs 40% understanding; the slip forecast is parked in `docs/PROPOSAL_ForecastOnSlip.md`). 72 EditMode tests green. Point 5 done: 15 dilemma posts imported (`dilemma: true` in `docs/data/posts.json`, reviewed by the user; post 12 is Seguros Fonseca), 50% of random slots. Balance report: oracle-per-post 84% Balance vs target-balance 52% (was 76%), so reading posts beats the category rule; 78 EditMode tests green. Side effect: all-normal is back to 95% Alive. Remaining from the review: 6 (playtest with someone who never saw the game).

Visual check (2026-11-07): the user played and tested all of points 1, 3, 4 and 5 (slip tags and ink label, boss lines, `x` Silenciado icon, epilogue, dilemma posts) in the editor and confirmed everything works. Sixth ending "Despedido" is deferred until there are more days and posts.

## Open items after the design review (2026-11-07)

- Point 6: playtest with someone who never saw the game (are the reactions readable from the window? is the tutorial enough?). Not done.
- Passivity is still safe: all-normal ends "Alive" in 95% of runs (`docs/BALANCE.md`).
- The "Bairro percebido" threshold for "Equilibrio raro" (40%) is lenient: 1 note per turn already passes. Raise `endingBalanceUnderstanding` in `TuningData` (and in `TuningConfig.asset`) if it should demand more.
- Dilemma posts make target-balance (rule by category) lock the neighbourhood in 16% of runs and the slip pattern "observado: medo x3" less reliable. Feel it in play, and add a tutorial hint that the category colour no longer says everything.
- Parked proposals: pins on the cork board (`docs/PROPOSAL_PinsOnCorkBoard.md`) and slip forecast (`docs/PROPOSAL_ForecastOnSlip.md`), best done together.
- Sixth ending "Despedido": only when there are more days and posts.
- Nothing from the design review is committed to git yet (points 1, 3, 4, 5 and the Silenciado reaction).
- `Builds/macOS/EchoChamber.app` was built before the design review changes; rebuild it before delivering. No Windows build.

## Known problems and future improvements

- Reaction symbols use ASCII glyphs (`!`, `#`, `+`, `$`, `z`) instead of the prototype heart and euro icons.
- Room extras not ported: dust motes, light shafts, cables, framed poster artwork (posters are flat colours).
- Characters are small from the window view (same scale as the prototype). Window view camera shows the building edge at the bottom.
- Square string lights at night were not confirmed visually.
- Relations are discovered generously: one observation can reveal many pairs (same rule as the prototype), so the cork board can get crowded with red strings late in the game.
- Doing nothing (all NORMAL) now ends as "Bairro vivo, app em perda" in 85% of seeds (was 100%), see `docs/BALANCE.md`. Hiding everything is 76% (stamp ink and silenced authors).
- The Unity AI generators (audio, image) report `NoSubscription` for this account, so audio is synthesised in code. Sound design is functional, not polished; levels were not checked by ear.
- URP logs that Depth of Field and Panini post-process shaders are stripped in the build. They are not used.
- Windows build not done (only the macOS module is installed).
- `unity test` cannot run while the Editor has the project open, hence the menu-based test runner.
