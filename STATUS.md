# STATUS — Mimic Party

**Last updated:** 2026-10-05 (0.3.0, repo github.com/Ali-Bueno/mimic-party-access)

## Identity

- **Engine / framework:** Unity 6000.4.2f1, IL2CPP (metadata v39) / BepInEx 6.0.0-be.788 + interop fixer
- **Screen-reader transport:** C# P/Invoke → `prism.dll` v0.18.3 (shipped next to the plugin)
- **Build command:** `dotnet build -c Release` (deploys to `BepInEx\plugins\MimicPartyAccess\`); tests: `dotnet test tests`
- **Mod install path:** `D:\games\steam\steamapps\common\Mimic Party\BepInEx\plugins\MimicPartyAccess\`
- **Run / test:** launch from Steam with NVDA running; log in `BepInEx\LogOutput.log` (`[speech]`, `[ui]`), UI dump in the plugin folder
- **Test loop:** the user tests live with NVDA while work happens — read the log instead of injecting keys into the game; the DLL is locked while the game runs, so deploy after it closes and ask for a restart

## Section status

| Section / feature | Status | Notes |
|---|---|---|
| Adapter + Speech (PRISM) | done | NVDA backend verified in-game |
| Menus (generic uGUI pipeline) | wip | Main menu, playtest popup, room browser, friends drawer verified; settings labels fixed (sibling labels → nearest control), awaiting user check; lobby, results unverified |
| Custom switch states | done | Room visibility, team search, friends drawer (`MimicUiProfile.GetWidgetState`) |
| Lobby + packs | wip | Lobby, rules, roster, friends drawer verified; workshop browser isolated as overlay + row titles on Download/Preview/Ban — awaiting user check |
| Match announcements | done | Verified in a 5-round match with a friend: rounds, phases, reference sound name, your turn, scores, round results, wheel spin |
| Match events | wip | Whose imitation plays, mic failure, network/host lost, versus team turns, game finished, malus lineup (`MatchEventPatches`) — all patches apply; untested in play |
| Recording guide tone | wip | `OnPerformanceReady` reference envelope (RMS, 50 ms) → `ToneCue` volume while `IsRecording`; config `Features.RecordingGuide(Volume)`; needs headphones |
| Season pass / objectives / shop | wip | `Game/Screens`: tier names+states from `SeasonPass` data, claim/purchase/balance/level-up announcements, pack buy names, cosmetic prices; objective rows are text-only (summary on open) |
| Score key (S) | wip | During a match: round label + every player's total, highest first (`RoundController.Roster`) |
| Pack downloads | wip | `LobbyThemePanel.StartDownload/StartDownloadAll/OnInstalledChanged` + static `Progress(itemId)` polled per second: started, 25 % steps, finished |
| Cosmetic cells | wip | Characters/animations/auras: equipped / locked / available from the cells' `check`/`padlock` images (shared field convention) |
| Player intro banner | wip | `PlayerIntroBanner.Show(playerName, wins)` → "name: N wins" — awaiting a match |
| Wheel target choice | wip | Game picks by clicking a 3D character (`Mimick.Stage.WheelTargeting`); mod offers `ChoicePrompt` of the `allowed` players → `RoundController.ChooseWheelTarget`; fallback `FallbackTarget` if nobody picks |
| Wheel result + pack votes | wip | `OnWheelStopped` (slot's localized title/description), `OnWheelTargetingStarted`, `OnWheelEvent`; vote buttons read selected/not via tint — awaiting a match |
| Language | done | Follows the game's Unity Localization locale; strings en/es/pt |

## Derived facts

| Fact | Value | Source |
|---|---|---|
| Game code assembly | `Mimick.Runtime.dll`, namespaces `Mimick.*` (no `Il2Cpp` prefix) | BepInEx interop |
| Base screens | derive `Mimick.UI.UIScreen` (MainMenu, Lobby, Matchmaking, Game, Results, Solo) | interop API dump |
| Overlays | `PersistentSingleton<T>` panels, own root canvas, `SortingOrder` 28500–30000, root `CanvasGroup`, full-screen `Backdrop`, `titleLabel`/`messageLabel` | ui-dump.txt + API dump |
| Escape | handled by the panels themselves (`PauseMenu.Update`); the mod must not close panels | old MelonLoader mod re-opened Settings |
| EventSystem | `InputSystemUIInputModule`; legacy `Input` also enabled | ui-dump.txt |
| Key capture | `PauseMenu.capturingKey` while rebinding push-to-talk | API dump |
| Language | `LocalizationSettings.SelectedLocale.Identifier.Code` | runtime log (`es`) |
| Interop repair | 20 orphan rows in 6 DLLs after each regeneration | interop fixer |

## Next step

User testing of 0.3.0 (all patches apply; behaviour unverified): a match (intro, wheel target choice, guide tone,
S key, playback/network events), pack creator keys (reorder, trim), season pass/shop/objectives readouts.

## Known issues / open questions


- `Common.close` is spoken on one room-browser button (untranslated key in the game itself).
- Match announcement hooks are untested at runtime; `RoundStandings` reads `Roster` via `TryCast<ICollection<>>`.
