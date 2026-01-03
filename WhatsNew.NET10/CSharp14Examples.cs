using System;
using System.Collections.Generic;

namespace WhatsNew.NET10;

/// <summary>
/// C# 14 Feature Examples
/// Demonstrates C# 14 features currently available in .NET 10
/// </summary>
public class CSharp14Examples
{
    public static void RunAllExamples()
    {
        Console.WriteLine("=== C# 14 Features (Available in .NET 10) ===\n");

        NameofUnboundGenericsExample();

        Console.WriteLine("\n=== Examples completed ===");
    }

    // ============================================================
    // NAMEOF WITH UNBOUND GENERIC TYPES
    // ============================================================
    private static void NameofUnboundGenericsExample()
    {
        Console.WriteLine("--- nameof with Unbound Generics ---");
        Console.WriteLine();

        // C# 14: Can use nameof on unbound generic types
        Console.WriteLine(nameof(List<>));
        Console.WriteLine(nameof(Dictionary<,>));
        Console.WriteLine(nameof(Func<,,>));
        Console.WriteLine(nameof(Action<,,,>));
        Console.WriteLine(nameof(Func<,,,,>));

        Console.WriteLine();
        Console.WriteLine("This is useful for:");
        Console.WriteLine("- Generic type introspection");
        Console.WriteLine("- Better logging without type arguments");
        Console.WriteLine("- Reflection and code generation scenarios");

        Console.WriteLine();
    }
}
