using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WhatsNew.NET10;

/// <summary>
/// EF Core 10.0 Feature Examples
/// Demonstrates LeftJoin, RightJoin, and parameterized collection improvements
/// </summary>
public class EFCore10Examples
{
    public static async Task RunAllExamplesAsync()
    {
        Console.WriteLine("=== EF Core 10.0 Feature Examples ===\n");

        await LeftJoinExampleAsync();
        await RightJoinExampleAsync();
        await ParameterizedCollectionExampleAsync();

        Console.WriteLine("\n=== All EF Core examples completed ===");
    }

    // ============================================================
    // 1. LEFT JOIN - New LeftJoin Method
    // ============================================================
    private static async Task LeftJoinExampleAsync()
    {
        Console.WriteLine("--- 1. LeftJoin Method ---");

        using var context = new DemoDbContext();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Add sample data
        if (!await context.Students.AnyAsync())
        {
            context.Students.AddRange(
                new Student { FirstName = "Alice", LastName = "Johnson", DepartmentID = 1 },
                new Student { FirstName = "Bob", LastName = "Smith", DepartmentID = 2 },
                new Student { FirstName = "Charlie", LastName = "Brown", DepartmentID = null }  // No department
            );
            context.Departments.AddRange(
                new Department { ID = 1, Name = "Computer Science" },
                new Department { ID = 2, Name = "Mathematics" }
            );
            await context.SaveChangesAsync();
        }

        Console.WriteLine("\nOld way (GroupJoin + SelectMany + DefaultIfEmpty):");
        Console.WriteLine("Complex, hard to read, error-prone");

        Console.WriteLine("\nNew way (LeftJoin):");
        var studentsWithDepartments = await context.Students
            .LeftJoin(
                context.Departments,
                student => student.DepartmentID,
                department => department.ID,
                (student, department) => new
                {
                    student.FirstName,
                    student.LastName,
                    Department = department != null ? department.Name : "[NONE]"
                })
            .ToListAsync();

        foreach (var item in studentsWithDepartments)
        {
            Console.WriteLine($"  {item.FirstName} {item.LastName} - {item.Department}");
        }

        Console.WriteLine("\nTranslated SQL: LEFT JOIN");
    }

    // ============================================================
    // 2. RIGHT JOIN - New RightJoin Method
    // ============================================================
    private static async Task RightJoinExampleAsync()
    {
        Console.WriteLine("\n--- 2. RightJoin Method ---");

        using var context = new DemoDbContext();

        // Add a department with no students
        if (!await context.Departments.AnyAsync(d => d.ID == 3))
        {
            context.Departments.Add(new Department { ID = 3, Name = "Physics" });
            await context.SaveChangesAsync();
        }

        var departmentsWithStudents = await context.Departments
            .RightJoin(
                context.Students,
                department => department.ID,
                student => student.DepartmentID,
                (department, student) => new
                {
                    Department = department != null ? department.Name : "[NONE]",
                    student.FirstName,
                    student.LastName
                })
            .ToListAsync();

        Console.WriteLine("\nDepartments (RightJoin keeps all Students):");
        foreach (var item in departmentsWithStudents)
        {
            Console.WriteLine($"  {item.Department} - {item.FirstName} {item.LastName}");
        }

        Console.WriteLine("\nTranslated SQL: RIGHT JOIN");
    }

    // ============================================================
    // 3. PARAMETERIZED COLLECTIONS - New Default Translation
    // ============================================================
    private static async Task ParameterizedCollectionExampleAsync()
    {
        Console.WriteLine("\n--- 3. Parameterized Collection Translation ---");

        using var context = new DemoDbContext();

        // EF 10.0: Each value becomes a parameter
        int[] ids = [1, 2, 3];

        Console.WriteLine($"\nQuerying with IDs: [{string.Join(", ", ids)}]");

        var blogs = await context.Blogs
            .Where(b => ids.Contains(b.Id))
            .OrderBy(b => b.Id)
            .ToListAsync();

        Console.WriteLine("\nNew default translation: Scalar parameters");
        Console.WriteLine("SQL: WHERE [b].[Id] IN (@ids1, @ids2, @ids3)");
        Console.WriteLine("Benefits:");
        Console.WriteLine("  - Same SQL structure for different collections");
        Console.WriteLine("  - Query plan reuse (cache efficiency)");
        Console.WriteLine("  - Cardinality info preserved for optimizer");

        // Demonstrate padding (if more than ~5 values, EF pads to nearest 5/10)
        int[] manyIds = [1, 2, 3, 4, 5, 6, 7, 8];
        Console.WriteLine($"\nWith {manyIds.Length} values, EF may pad to 10 parameters:");
        Console.WriteLine("SQL: WHERE [b].[Id] IN (@ids1, ..., @ids8, @ids9, @ids10)");
        Console.WriteLine("  @ids9, @ids10 duplicate @ids8 to reduce SQL variations");

        Console.WriteLine("\nTranslation modes:");
        Console.WriteLine("  - Parameter (default): Scalar parameters");
        Console.WriteLine("  - Constant: Inline values (old behavior)");
        Console.WriteLine("  - JsonArray: Single JSON parameter (EF 8/9)");
    }
}

// ============================================================
// Demo Models and DbContext
// ============================================================

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? DepartmentID { get; set; }
    public Department? Department { get; set; }
}

public class Department
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Student> Students { get; set; } = new();
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DemoDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use in-memory database for demo
        optionsBuilder
            .UseInMemoryDatabase("EFCore10Demo")
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();
    }
}

// ============================================================
// Extension Methods for Configuration Examples
// ============================================================

public static class EFCore10ConfigurationExamples
{
    // Global configuration example (in your DbContext)
    public static void ConfigureParameterizedCollections(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseSqlServer(connectionString,
            o => o.UseParameterizedCollectionMode(Microsoft.EntityFrameworkCore.ParameterTranslationMode.Parameter));
        // Other modes: Constant, JsonArray
    }

    // Per-query override example
    public static async Task<List<Blog>> QueryWithConstantMode(DbContext context, int[] ids)
    {
        return await context.Set<Blog>()
            .Where(b => Microsoft.EntityFrameworkCore.EF.Constant(ids).Contains(b.Id))
            .ToListAsync();
    }
}
