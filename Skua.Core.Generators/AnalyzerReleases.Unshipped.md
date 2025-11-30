; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SkuaGen_001 | Skua.Core.Generators.CallBindingGenerator | Error | Name collision for generated property
SkuaGen_002 | Skua.Core.Generators.CallBindingGenerator | Error | The Path for the binding is null, empty or whitespace
SkuaGen_003 | Skua.Core.Generators.ObjectBindingGenerator | Error | Name collision for generated property
SkuaGen_004 | Skua.Core.Generators.ObjectBindingGenerator | Error | The Path for the binding is null, empty or whitespace
SkuaGen_005 | Skua.Core.Generators.ModuleBindingGenerator | Error | Name collision for generated property
SkuaGen_006 | Skua.Core.Generators.ModuleBindingGenerator | Error | The Name for the binding is null, empty or whitespace
SkuaGen_007 | Skua.Core.Generators.MethodCallBindingGenerator | Error | Name collision for generated method
