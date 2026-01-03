// See https://aka.ms/new-console-template for more information

using WhatsNew.NET10;

Console.WriteLine("WhatsNew.NET10 - .NET 10 Features Demo");
Console.WriteLine("====================================\n");

// C# 14 Examples
CSharp14Examples.RunAllExamples();

Console.WriteLine();

// EF Core 10 Examples
await EFCore10Examples.RunAllExamplesAsync();
