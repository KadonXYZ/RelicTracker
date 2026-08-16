# RelicTracker -- PUBLIC-BETA VERSION for V.0.111.0 // 14.08.2026

A Slay the Spire 2 mod that tracks relic usage and shows the stats on relic tooltips.

Fork of [rmac-silva/RelicTracker](https://github.com/rmac-silva/RelicTracker) (originally by gilbio), updated for the game's beta branch.

## Installation

1. Download the latest `RelicTracker_*.zip` from [Releases](https://github.com/KadonXYZ/RelicTracker/releases).
2. Extract it into your game's `mods` folder (create the folder if it does not exist):
  - Windows: `Steam/steamapps/common/Slay the Spire 2/mods/`
3. Launch the game and enable **RelicTracker** in the Mods menu.

### Optional: Better Mod Menu config

[Better Mod Menu](https://github.com/Hellfrosted/BetterModMenu) can open RelicTracker’s config from its **Config** button if [BaseLib](https://github.com/Alchyr/BaseLib-StS2) is also installed (Steam Workshop or a manual copy in `mods/BaseLib`). RelicTracker still runs without BaseLib; the Config button stays unavailable until BaseLib is present.

From that page you can:

- Enable or disable RelicTracker without unloading the DLL
- Show or hide stats on relic tooltips
- Show or hide the “No data yet” line
- Keep recording stats even when tooltips are hidden

## Build it yourself

### Requirements

- [Slay the Spire 2](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/) installed via Steam
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Godot 4.5.1 Mono](https://godotengine.org/download/archive/4.5.1-stable/) (optional; only needed for `.pck` export via `dotnet publish`)

> If you export a `.pck`, use Godot **4.5.1** specifically. Newer Godot versions can break mod packaging for this game. RelicTracker ships as DLL-only (`has_pck: false`), so a normal `dotnet build` does not require Godot.

### 1. Configure paths

Open `RelicTracker.csproj` and set these for your machine:


| Setting            | What to set                                                                                                       |
| ------------------ | ----------------------------------------------------------------------------------------------------------------- |
| `GodotPath`        | Full path to Godot 4.5.1 Mono (only if you need Publish/`.pck`)                                                    |
| `SteamLibraryPath` | Your `steamapps` folder that contains `common/Slay the Spire 2` (only if not in the default Steam library)         |


Windows example:

```xml
<GodotPath>C:\Path\To\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64.exe</GodotPath>
<SteamLibraryPath Condition="'$(SteamLibraryPath)' == ''">D:\SteamLibrary\steamapps</SteamLibraryPath>
```

The project auto-detects the default Steam library when possible. It needs the game's `data_sts2_*` folder so it can reference `sts2.dll` and `0Harmony.dll`.

Building also needs `BaseLib.dll` (compile-time only; it is not a required runtime dependency). RelicTracker looks for it in this order:

1. `Slay the Spire 2/mods/BaseLib/BaseLib.dll` (manual install)
2. Steam Workshop: `steamapps/workshop/content/2868840/3737335127/BaseLib/BaseLib.dll`
3. A `lib/BaseLib.dll` copy in this repo

The Workshop copy is enough; you do not need a second BaseLib in the game’s `mods` folder.

### 2. Build

From the repo root:

```bash
dotnet build
```

On success, the build copies these into `Slay the Spire 2/mods/RelicTracker/`:

- `RelicTracker.dll`
- `RelicTracker.BaseLib.dll` (only if BaseLib is available at compile time)
- `RelicTracker.json`
- `Localization/`

Close the game before building if it is running, otherwise Windows may lock `RelicTracker.dll` and the copy step will fail.

### 3. Run

Start Slay the Spire 2, enable the mod, and hover a relic during a run to see tracked stats.

## Credits

- Original mod: gilbio
- Upstream repo: [rmac-silva/RelicTracker](https://github.com/rmac-silva/RelicTracker)

