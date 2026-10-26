# shmup scaling diff

A 2D shoot-'em-up built around resolution scaling. Unity 7000.0.0a10 (alpha), URP 2D renderer, new Input System.

## Rules

1. **Follow existing naming and style.** Read nearby scripts before writing new ones and match them. Until the project has its own conventions, use the standard Unity C# style:
   - `PascalCase` for types, methods, properties and public members. `camelCase` for locals and parameters. `_camelCase` for private fields.
   - Serialize private fields with `[SerializeField] private` instead of making them public.
   - Use one public type per file, named after the file. Put game code in a project namespace.
   - Use Allman braces and 4-space indents. Keep comments sparse and explain *why*, not *what*.
   - Put game code under `Assets/Scripts/` in feature folders.

2. **Reuse existing systems.** Before adding a manager, service, pool, event or utility, search the project for one that already does the job and extend it. Don't add a second way of doing the same thing. Prefer Unity's built-in features (Pixel Perfect Camera, Input System, URP) over custom replacements.

3. **Ask before package changes.** Don't add, remove, upgrade or downgrade anything in `Packages/manifest.json`, and don't import third-party assets or plugins, until the user has explicitly agreed. Say what the package is and why it's needed.

4. **Check for compile errors after every code change.** A Unity Editor is usually running and reachable through the Unity CLI. Run these from the project root:
   ```
   unity status                       # confirm the Editor is "ready"
   unity command console status       # compilationFailed must be false
   unity command console get --level error
   ```
   Wait for compilation to finish (`compiling: false`) before reading the result. Fix any new errors or warnings you introduced before saying you're done. If the Editor won't connect, it may be in Safe Mode because of compile errors: run `unity pipeline list` to check.

5. **Distinguish implemented from tested.** When you report work, state its exact status:
   - *Implemented*: the code is written and compiles cleanly.
   - *Tested*: you actually ran it, by entering Play mode, running tests (`unity command test ...`) or capturing a screenshot, and saw it behave correctly.

   Never call something working or done if you only implemented it. Say what you checked and what you didn't.

6. **Follow good programming practices.**
   - SOLID: give each class one responsibility, extend behavior instead of editing working code, depend on interfaces or abstractions where it helps, and keep interfaces small.
   - Prefer composition with small components over deep inheritance.
   - Configure data with ScriptableObjects or serialized fields instead of hard-coding values.
   - Avoid `Find`/`GetComponent` in `Update`. Cache references and pool frequently spawned objects such as bullets and enemies.
   - Keep changes focused on the task. Don't refactor unrelated code without asking.
   - Use `AGENTS.MD` (next to this file) as a reference for additional rules. It is a very long C# course transcript, so search it for the topic at hand rather than reading it whole. Where it conflicts with this file, this file wins.

## Working with the Editor

- When the Editor is reachable, change scenes, prefabs and assets through `unity command` (run `unity command` to list the groups) instead of hand-editing `.unity`, `.prefab` or `.asset` YAML.
- Save scenes with `unity command scene save` after editing them.
- Ask before changing project settings (player, quality, graphics, input).
