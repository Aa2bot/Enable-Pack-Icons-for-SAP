# Hover Inspector

MelonLoader mod that keeps opponent pack icons visible on every scoreboard (mini/full) by forcing the pack graphics to stay active and opaque. It mimics the lightweight approach of EnableMiniLives while logging API calls for diagnostics.

## Features
- Always reveals `Content/Pack/Standard` and `Content/Pack/Standard/Image` inside Scoreboard and ScoreboardMini entries.
- Skips the presumed local player (first entry) so only opponents' packs stay visible.
- Logs every UnityWebRequest request as `[API] METHOD URL`.
- Pressing F6 logs the pack assets that were reinforced on that frame.

## Build
```bash
dotnet build -c Release


Install

Drop bin/Release/net6.0/HoverInspector.dll into:
%LOCALAPPDATA%/Low/Super Auto Pets/Mods
(or the equivalent Steam mods folder).
