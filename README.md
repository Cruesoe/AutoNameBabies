# Auto Name Babies

RimWorld 1.6 Biotech mod. When a baby is born, vanilla pops a letter and waits for you to name them. This mod names the baby immediately and skips that prompt.

- **First name** uses the same generator as vanilla's randomize button (culture / xenotype).
- **Last name** is the father's, the mother's, or a random parent surname. Change this under **Options → Mod options → Auto Name Babies**.

The birth letter still appears. It no longer auto-opens a naming dialog, and it no longer offers **Name baby** or **Postpone** — only **Jump to location** and **Close**.

Player babies still named "Baby" in an existing save are named when you load.

Requires [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) and **Biotech**.

## Install

Copy this folder to `RimWorld\Mods\`, or add it as a local mod in RimSort.

## Build

```
dotnet build Source\AutoNameBabies.csproj -c Debug
```

The DLL is copied to `1.6\Assemblies\AutoNameBabies.dll` and to `RimWorld\Mods\Auto Name Babies\1.6\Assemblies\` if that folder exists.
