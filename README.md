# Mimic Party Access

Screen-reader accessibility mod for [Mimic Party](https://store.steampowered.com/app/5053820/) (Steam,
Windows x64), built on BepInEx 6 and [PRISM](https://github.com/ethindp/prism): NVDA, JAWS, Narrator/SAPI and
other screen readers, with braille output where the reader supports it.

> Español: el mod hace accesible Mimic Party con lector de pantalla (NVDA, JAWS…). Instrucciones de
> instalación abajo; las teclas y los anuncios siguen el idioma del juego (español, inglés o portugués).

## What it does

**Menus — every screen, with the keyboard**
- Up/Down move through the controls of the menu on top, in on-screen order; Home/End jump to the first/last.
- Left/Right change sliders and lists; Enter activates; Escape behaves as in the game.
- Opening a menu or popup reads its title (and a popup's message) and the focused control; side panels such
  as the friends drawer are entered like submenus.
- Controls read their state: switches, sliders, lists, text fields, room visibility (public/private), team
  search, pack votes, equipped/locked characters, animations and auras, season pass tiers, objectives, prices.
- Lists of information (friends, packs, rooms, players) are readable row by row.

**Match**
- Player introductions, rounds, phases, the name of the sound to imitate, "your turn to speak", the recording
  countdown, whose imitation is playing, scores, round results, the wheel result and its sabotages.
- When the wheel asks you to pick a target, a list of players appears: arrows to choose, Enter to confirm.
- **S** reads the round and everyone's score.
- While you record, a soft **guide tone** follows the loudness of the reference sound — what sighted players
  see as the waveform on the performance bar. Use headphones (speakers would leak it into your take), or turn
  it off in the configuration.
- Microphone failures, connection problems and the host leaving are announced.

**Lobby, packs and shop** — downloads (started, progress, finished), pack votes, the workshop browser, the
pack creator, the season pass, objectives and the MimiBucks shop.

**Pack creator keys** — Ctrl+Up/Down move the focused sound in the list; with the trim panel open,
Ctrl+Left/Right move the start and Ctrl+Shift+Left/Right change the length; Ctrl+I offers import (sounds,
pack image, folder); Ctrl+R reads the pack's progress (or the trim values).

Ctrl+Alt+R repeats the last announcement; Ctrl+Alt+F1 reads the key help.

## Install

1. Install [BepInEx 6 bleeding edge](https://builds.bepinex.dev/projects/bepinex_be), IL2CPP **win-x64**,
   build 788 or newer, by extracting it into the game folder (Steam: right-click Mimic Party → Manage →
   Browse local files). Start the game once and close it: BepInEx generates its interop assemblies.
2. Mimic Party runs on Unity 6, whose generated interop is broken by a known Il2CppInterop bug. Repair it once
   (and again after every game update) with the bundled tool:
   `dotnet run --project tools/InteropFixer -- "<game folder>\BepInEx\interop" --apply`
3. Copy the mod into `BepInEx\plugins\MimicPartyAccess\`: `MimicPartyAccess.dll`, `prism.dll` (from
   PRISM's `prism-windows-x64.zip`, `dynamic/release/bin/`) and the `localization` folder.
4. Start the game with your screen reader running. You should hear "Mimic Party Access loaded".

## Configuration

`BepInEx\config\MimicPartyAccess.cfg`: `Features.Menus`, `Features.GameAnnouncements`,
`Features.RecordingGuide` and `Features.RecordingGuideVolume`. The `Debug` section (speech log, UI log, UI
dump) is for troubleshooting; to hide the BepInEx console set `[Logging.Console] Enabled = false` in
`BepInEx\config\BepInEx.cfg`.

## Building

Requires the .NET SDK and the game with BepInEx installed (set `GameDir` in `MimicPartyAccess.csproj`). Put
`prism.dll` in `native\`. `dotnet build -c Release` builds and deploys to the game; `dotnet test tests` runs
the engine-free tests.

The menu engine (`AccessKit/`) is game-agnostic — it reads any Unity uGUI game — and game knowledge lives in
`Game/`. Status and derived facts: [STATUS.md](STATUS.md).

## Credits

- Match announcement texts and the first list of game hooks come from the MelonLoader mod
  [mimic-access](https://github.com/azurejoga/mimic-access).
- [PRISM](https://github.com/ethindp/prism) by ethindp (screen-reader output; ship its `LICENSES`/`NOTICE`
  with `prism.dll`).
- [BepInEx](https://github.com/BepInEx/BepInEx) and Il2CppInterop.

Unofficial fan project, not affiliated with FoliesAPP. Code under the [MIT license](LICENSE).
