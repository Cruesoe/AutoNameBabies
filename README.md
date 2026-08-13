# Auto Name Babies

RimWorld 1.6 Biotech mod. When a baby is born, vanilla pops a letter with a **Name baby** option. This mod names the baby immediately and removes that option.

- First name: same generator as the vanilla randomize button (culture / xenotype).
- Last name: father, mother, or random among parents. Change this in **Options → Mod options → Auto Name Babies**.

The birth letter still appears so you know a child was born. It no longer auto-opens the naming dialog.

## Install

Copy this folder to `RimWorld\Mods\`, or add it as a local mod in RimSort.

Requires **Harmony** and **Biotech**.

## Build

```
dotnet build Source\AutoNameBabies.csproj -c Debug
```

The DLL is copied to `1.6\Assemblies\AutoNameBabies.dll`.
