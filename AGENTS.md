# Agent Guidelines - AIDigitalProject

Guidelines for agentic coding agents operating in this Unity-based repository.

---

## 1. Build, Lint, and Test Commands

Unity Version: **2022.3.62f3c1**

### Build Commands
```bash
# General Build (Windows)
"C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" -batchmode -nographics -projectPath . -executeMethod <Namespace.ClassName.MethodName> -logFile - -quit
```

### Test Commands
Tests use Unity Test Framework (NUnit).

```bash
# Run all PlayMode tests
"C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode -testResults results_play.xml -logFile -

# Run all EditMode tests
"C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults results_edit.xml -logFile -

# Run a specific test class/method
"C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" -batchmode -nographics -projectPath . -runTests -testFilter <TestClassNameOrMethodName> -testResults results.xml -logFile -

# Run tests for specific assembly (e.g., UniGLTF)
"C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -assemblyNames UniGLTF.Tests -testResults results.xml -logFile -
```

### Linting
No automated linter configured. Follow C# style guidelines below strictly.

---

## 2. Code Style Guidelines

### Formatting
- **Indentation:** 4 spaces (no tabs)
- **Line Endings:** CRLF (Windows standard)
- **Braces:** Allman style (new line) for namespaces, classes, methods
- **Control Flow:** Single-line `if (condition) return;` acceptable; otherwise use braces

```csharp
namespace Example
{
    public class MyClass
    {
        void MyMethod()
        {
            if (condition)
            {
                DoSomething();
            }
        }
    }
}
```

### Naming Conventions
- **Classes/Structs/Interfaces:** `PascalCase` (interfaces prefixed with `I`)
- **Methods:** `PascalCase`
- **Public Fields:** `camelCase` (Unity convention)
- **Public Properties:** `PascalCase`
- **Private/Protected Fields:** `_camelCase`
- **Local Variables:** `camelCase`
- **Constants/Enums:** `PascalCase`

### Namespaces
- **DigitalHuman:** `DigitalHuman.Core`, `DigitalHuman.Data`, `DigitalHuman.UI`, `DigitalHuman.Animation`
- **uLipSync:** `uLipSync`
- Always declare namespace at the top of the file

### Imports (Using Statements)
Order outside namespace:
1. `UnityEngine`
2. `Unity.*` (`Unity.Collections`, `Unity.Jobs`, `Unity.Mathematics`)
3. `System.*` (`System.Collections.Generic`, `System.Threading.Tasks`)
4. Project namespaces (`DigitalHuman.Core`, `uLipSync`)

### Attributes
Place on separate line above target:
```csharp
[SerializeField]
[Range(0, 1)]
private float _volume;
```

Use Unity attributes appropriately:
- `[SerializeField]` for private fields needing Inspector exposure
- `[Range(min, max)]` for numeric sliders
- `[Tooltip("...")]` for Inspector context
- `[HideInInspector]` for hidden public fields
- `[BurstCompile]` for Job System methods
- `[CreateAssetMenu]` for ScriptableObjects

### Types and Logic
- Prefer Unity types (`Vector3`, `Mathf`, `Debug.Log`)
- For compute-heavy logic, use `Unity.Jobs` with `NativeArray`
- Use `[BurstCompile]` where applicable
- Use `ScriptableObject` for data configuration
- Use `async/await` for asynchronous operations

### Error Handling
- Use `Debug.LogError()`, `Debug.LogWarning()` for Unity-visible errors
- Wrap I/O operations in `try-catch` blocks
- Use platform defines: `#if UNITY_EDITOR`, `#if UNITY_WEBGL`

### XML Documentation
Use `///` comments for public API members:
```csharp
/// <summary>
/// Processes AI response with audio input.
/// </summary>
/// <param name="audioBase64">Base64 encoded audio data</param>
/// <returns>Task representing async operation</returns>
```

---

## 3. Project Structure

### Directory Layout
```
Assets/
├── Scripts/
│   └── DigitalHuman/          # Main application code
│       ├── Network/           # API and Network services (LLM, TTS, ASR)
│       ├── Audio/             # Audio input and processing
│       ├── Animation/         # Animation controllers and logic
│       ├── Data/              # Data models and settings
│       └── UI/                # UI controllers
├── uLipSync/                  # LipSync plugin
│   ├── Runtime/               # Runtime scripts
│   ├── Editor/                # Editor extensions
│   └── Samples/               # Example scenes
├── Avatar/                    # VRM model assets
└── Scenes/                    # Unity scenes
```

### Assembly Definitions
- Each module has its own `.asmdef` file
- Editor scripts must be in folders with Editor-constrained `.asmdef`
- Avoid circular dependencies
- Key assemblies: `uLipSync.Runtime`, `uLipSync.Editor`

---

## 4. Version Control & Safety

- **Meta Files:** Never delete `.meta` files; move them with their assets
- **NativeArray:** Always dispose to prevent memory leaks
- **Large Files:** Avoid committing binaries; use LFS if needed
- **Ignored:** `Library/`, `Temp/`, `Obj/`, `*.csproj` (auto-generated)

---

## 5. Key Technologies
- Unity 2022.3.62f3c1
- C# 9.0+
- Unity Job System & Burst Compiler
- VRM 0.x/1.0 (UniVRM)
- uLipSync (MFCC-based lip sync)
- Async/await patterns

---

## 6. Agent Best Practices

- Verify code compiles after modifications
- Create Custom Inspectors for complex MonoBehaviours
- Use placeholder primitives (Cube, Sphere) when assets missing
- Test with sample models in `Assets/Avatar/`
- Follow existing code patterns in the module you're editing
