# Decisions

Format: date, decision, reason.

- 2026-11-03: GLB sources live in `prototipo-assets/` (the docs say `assets/`). Reason: `assets/` would collide with `Assets/` on case-insensitive macOS. Imported copies go to `Assets/_Project/Art/`.
- 2026-11-03: No Git LFS. Reason: `git-lfs` is not installed and the binaries are small (about 5 MB); the user chose to skip it. No `.gitattributes` LFS rules.
- 2026-11-03: Commits stay local on `feature/unity-port`, no pushes, no co-author trailer. Author is the repo git identity. Reason: user instruction.
- 2026-11-03: Simulation assembly (`EchoChamber.Simulation`) has `noEngineReferences`. Data classes are plain `[Serializable]` C# classes; ScriptableObjects in `EchoChamber.Game` only wrap them. Reason: keeps the simulation testable and runnable headless without a scene.
- 2026-11-03: One `EchoChamber.Game` assembly covers `Scripts/Game`, `Scripts/Room` and `Scripts/UI`. Editor tooling is `EchoChamber.Editor`; tests are `EchoChamber.Tests` (EditMode). Reason: fewer cross-assembly references, same separation that matters (Simulation vs presentation).
- 2026-11-03: Input uses the new Input System only (project already has `activeInputHandler: 1`).
- 2026-11-03: Windows build is out of scope tonight: only the macOS Standalone module is installed. The completion build is macOS.
