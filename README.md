# HuckHub Registry

This repository is the public module registry for **HuckHub** — a lightweight editor dashboard that ships with our Unity packages.

## Why this exists

We develop a growing set of Unity editor tools built on top of Game Creator 2. Each tool — buff systems, wave spawners, skill trees, relationship mechanics — started as its own isolated package. As the number of modules grew, so did the friction: customers had no single place to see what was installed, whether updates existed, or which modules worked well together.

HuckHub was our answer. It is a small editor window (`Window > Huck > Hub`) that detects installed modules automatically and shows their status, dependencies, and documentation links in one place. No configuration required — modules register themselves through a shared interface and Unity's `TypeCache` handles discovery.

That solved the local side. But it couldn't answer questions like "what else is available?" or "is there a newer version?". So we added a second data source: this repository. Every HuckHub instance makes a single GET request to a JSON file hosted here. That gives us a central, transparent place to maintain version numbers, Asset Store links, and brief announcements — without shipping package updates just to change metadata.

## How it works

HuckHub combines two data sources:

**Local (TypeCache)** — Each installed module implements `IHuckModule` in its editor assembly. When the Hub window opens, Unity's `TypeCache` finds all implementations instantly. This provides the real status: installed version, health checks, settings access. Works fully offline, zero configuration.

**Remote (this repo)** — On first open per editor session, HuckHub fetches `modules.json` from this repository. The response is cached in `SessionState` — no repeated calls, no polling. If the request fails (offline, timeout, firewall), the Hub simply shows local data only. Nothing breaks.

The Hub merges both sources:
- **Installed modules** get an update badge when the remote version is newer
- **Available modules** (remote but not installed) appear in a separate sidebar section with Asset Store and documentation links
- **Announcements** from the registry are shown on the welcome page

One request, one JSON file, no tracking, no telemetry.

## Repository structure

```
huckhub-registry/
├── modules.json          ← The registry manifest (fetched by HuckHub)
└── changelogs/           ← Per-module changelogs (linked, not fetched)
    ├── BSM.md
    ├── SWS.md
    ├── PTB.md
    ├── WAS.md
    ├── RS.md
    └── CPS.md
```

### modules.json

The manifest contains three sections:

**`schemaVersion`** — Integer for forward compatibility. HuckHub checks this before parsing.

**`modules`** — Array of module entries. Each entry describes one package:

| Field | Purpose |
|---|---|
| `abbreviation` | Short ID used for dependency matching (`"BSM"`, `"RS"`) |
| `displayName` | Full name shown in the Hub |
| `description` | One-line summary |
| `latestVersion` | Current release version (compared against local) |
| `minUnityVersion` | Minimum supported Unity version |
| `assetStoreUrl` | Direct link to the Asset Store listing |
| `documentationUrl` | Link to documentation |
| `changelogUrl` | Link to the changelog in this repo |
| `requiredDependencies` | Packages the module needs (e.g. `"GC2 Core"`) |
| `optionalDependencies` | Other Huck modules that add synergies |

**`announcements`** — Array of brief messages displayed on the Hub welcome page. Each entry has a `title`, `body`, optional `url`, and a `type` (`"info"` or `"warning"`). This covers release notes, compatibility notices, deprecations, or any other communication — without needing additional endpoints or files.

### Changelogs

The `changelogs/` folder contains markdown files linked from `changelogUrl` in the manifest. These are not fetched by HuckHub — they open in the browser when the user clicks "Changelog" in the Hub. This keeps the JSON payload small and avoids loading content that most users won't read in-editor.

## Module ecosystem

| Abbr | Module | Description |
|---|---|---|
| BSM | Buff Status Effect Manager | Buffs, debuffs, DoT, HoT, stacking, immunities, auras |
| SWS | Spawn & Wave System | Wave-based spawning, object pooling, entity tracking |
| PTB | Progression Tree Builder | Skill trees, progression paths, visual tree editor |
| WAS | World Activity System | World events, activities, temporal scheduling |
| RS | Relationship System | Factions, diplomacy, reputation, NPC relations |
| CPS | Companion System | AI companions, follow, commands, formations *(in development)* |

All modules are built for Game Creator 2 and designed to work independently. Cross-module integrations (e.g. BSM buffs triggered by RS reputation changes) activate automatically when both modules are present.

## For module developers

If you are building a module that should appear in HuckHub:

1. Reference `HuckHub.Common.Editor` in your editor assembly definition
2. Create a class implementing `IHuckModule` with a parameterless constructor
3. That's it — `TypeCache` handles the rest

The interface requires basic metadata (name, version, description, dependencies) and an optional `GetStatusMessage()` for runtime health checks. See the `BSMModuleInfo.example.cs` in the HuckHub package for a complete reference implementation.

To appear in the "Available" section for users who don't have your module installed, add an entry to `modules.json` in this repository.

## Design decisions

**Why a flat JSON file instead of an API?** Simplicity. A static file on GitHub is free to host, trivial to update, and transparent — anyone can see exactly what data HuckHub reads. No server to maintain, no auth, no rate limits.

**Why `SessionState` instead of `EditorPrefs`?** SessionState clears on editor restart, which means HuckHub always gets fresh data at the start of a session. EditorPrefs would persist stale data indefinitely.

**Why not use Unity's Package Manager?** Our modules are distributed via the Asset Store, not UPM. The Hub needs to work with .unitypackage imports where UPM metadata isn't available.

**Why one JSON file for everything?** One HTTP request serves all the data the Hub needs. Splitting into multiple files would mean multiple requests or a more complex fetching strategy — overhead that isn't justified by the amount of data involved.

## Contributing

This repository is maintained by [c-huck.com](https://c-huck.com). If you find incorrect version numbers, broken links, or want to suggest an improvement, feel free to open an issue.
