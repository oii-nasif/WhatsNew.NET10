# C# 14 Features - Currently Available in .NET 10

**Note:** C# 14 is the upcoming language version. Most features are still in preview and will ship with .NET 11.
Only the feature below is currently available in .NET 10 SDK 10.0.101.

---

## Available Feature

## nameof with Unbound Generic Types

**Old Way** - Required type arguments or string manipulation:
```csharp
// Had to use type arguments
var name = nameof(List<int>);  // Returns "List"

// Or use Type.Name which includes arity
var genericTypeName = typeof(List<>).Name;  // Returns "List`1"
```

**New Way** - Direct support for unbound generics:
```csharp
// nameof now works on unbound generic types
var listName = nameof(List<>);           // Returns "List`1"
var dictName = nameof(Dictionary<,>);    // Returns "Dictionary`2"
var funcName = nameof(Func<,,>);         // Returns "Func`3"
var actionName = nameof(Action<,,,>);    // Returns "Action`4"
```

**Use Cases:**
- Generic type introspection without reflection
- Better logging for generic types
- Source generators and metaprogramming
- Type registration systems

---

## Coming Soon (Not Yet Available)

The following C# 14 features are proposed but **not yet implemented** in .NET 10:

| Feature | Description |
|---------|-------------|
| `field` keyword | Access compiler-generated backing fields in custom accessors |
| `?.=` operator | Null-conditional assignment |
| Lambda parameter modifiers | `ref`, `in`, `out` without explicit types |
| Custom `++`, `--` operators | User-defined increment/decrement behavior |
| Custom `+=`, `-=` operators | User-defined compound assignment |
| Extension blocks | Static extension methods and properties |
| Partial constructors/events | Better source generator support |
| Implicit Span conversions | First-class implicit conversions |

*These features are expected to ship with .NET 11 / C# 14 final release.*
