# Project workflow

- Treat `C:\Users\crues\source\repos\AutoNameBabies` as the authoritative repository.
- Treat `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\Auto Name Babies` as the local Steam upload/deployment folder, not as a repository mirror.
- The local Steam mod folder must contain only files required to run and upload the mod: `1.6`, `About`, and `Languages`.
- Never deploy repository-only files or directories, including `.git`, `.agents`, `.codex`, `Source`, `AGENTS.md`, `CLAUDE.md`, `README.md`, IDE metadata, or build intermediates.
- After every completed mod change, successfully build the mod and deploy the allowed upload content to the local Steam mod folder. Remove any files there that are not present in the allowed repository content.
- Verify deployment by comparing relative file paths and file hashes for the allowed content; completion requires zero missing, extra, or mismatched files in the local Steam mod folder.

## Steam Workshop descriptions

- Write for players, not developers.
- Keep descriptions concise, friendly, and easy to scan.
- Lead with what the mod does and why it is useful.
- Use Steam-compatible BBCode, not Markdown.
- Prefer a short introduction followed by a compact feature list.
- Mention important compatibility behavior only when players may encounter it.
- Do not list dependencies.
- Do not include implementation details, class names, patches, file paths, build information, hashes, or testing notes.
- Do not claim universal compatibility or bug-free operation.
- Do not add installation instructions.
- Do not include changelog material unless explicitly requested.
- Return text ready to paste into Steam, without commentary before or after it.
