# EF Core 10.0 Features - What's New in .NET 10

**Part of the What's New in .NET 10 project** · Entity Framework Core 10.0 introduces several improvements for LINQ queries and SQL translation.

---

## Available Features

### 1. LeftJoin and RightJoin Operators

**Old Way** - Complex GroupJoin + SelectMany + DefaultIfEmpty:
```csharp
var query = context.Students
    .GroupJoin(
        context.Departments,
        student => student.DepartmentID,
        department => department.ID,
        (student, departments) => new { student, departments })
    .SelectMany(
        x => x.departments.DefaultIfEmpty(),
        (x, department) => new
        {
            x.student.FirstName,
            x.student.LastName,
            Department = department.Name ?? "[NONE]"
        });
```

**New Way** - Simple LeftJoin method:
```csharp
var query = context.Students
    .LeftJoin(
        context.Departments,
        student => student.DepartmentID,
        department => department.ID,
        (student, department) => new
        {
            student.FirstName,
            student.LastName,
            Department = department.Name ?? "[NONE]"
        });
```

**SQL Translation:**
```sql
SELECT [s].[FirstName], [s].[LastName],
       COALESCE([d].[Name], '[NONE]') AS [Department]
FROM [Students] AS [s]
LEFT JOIN [Departments] AS [d] ON [s].[DepartmentID] = [d].[ID]
```

**RightJoin Example:**
```csharp
var query = context.Departments
    .RightJoin(
        context.Students,
        department => department.ID,
        student => student.DepartmentID,
        (department, student) => new
        {
            Department = department.Name ?? "[NONE]",
            student.FirstName,
            student.LastName
        });
```

**SQL Translation:**
```sql
SELECT COALESCE([d].[Name], '[NONE]') AS [Department],
       [s].[FirstName], [s].[LastName]
FROM [Departments] AS [d]
RIGHT JOIN [Students] AS [s] ON [d].[ID] = [s].[DepartmentID]
```

---

### 2. Parameterized Collection Translation

**Old Way (EF ≤ 8.0)** - Inlined constants (plan cache bloat):
```csharp
int[] ids = [1, 2, 3];
var blogs = await context.Blogs
    .Where(b => ids.Contains(b.Id))
    .ToListAsync();
```
```sql
-- Generated SQL changes for different collections
SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Id] IN (1, 2, 3)
```

**EF 8.0** - JSON array (lost cardinality info):
```sql
@__ids_0='[1,2,3]'

SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Id] IN (
    SELECT [i].[value]
    FROM OPENJSON(@__ids_0) WITH ([value] int '$') AS [i]
)
```

**New Way (EF 10.0)** - Scalar parameters with padding:
```csharp
int[] ids = [1, 2, 3];
var blogs = await context.Blogs
    .Where(b => ids.Contains(b.Id))
    .ToListAsync();
```
```sql
-- Same SQL structure, values as parameters
SELECT [b].[Id], [b].[Name]
FROM [Blogs] AS [b]
WHERE [b].[Id] IN (@ids1, @ids2, @ids3)
```

**With Padding** (8 values → 10 parameters):
```sql
WHERE [b].[Id] IN (@ids1, @ids2, @ids3, @ids4, @ids5, @ids6, @ids7, @ids8, @ids9, @ids10)
-- @ids9, @ids10 duplicate @ids8's value to reduce plan cache variations
```

**Configuration Control:**

Global configuration:
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder
        .UseSqlServer("<CONNECTION STRING>",
            o => o.UseParameterizedCollectionMode(ParameterTranslationMode.Constant));
```

Per-query override:
```csharp
var blogs = await context.Blogs
    .Where(b => EF.Constant(ids).Contains(b.Id))
    .ToListAsync();
```

**Translation Modes:**
| Mode | Description |
|------|-------------|
| `Constant` | Inline values (old behavior) |
| `Parameter` | Scalar parameters (new default) |
| `JsonArray` | Single JSON parameter (EF 8/9) |

---

## Summary

| Feature | Benefit |
|---------|---------|
| `LeftJoin` / `RightJoin` | Cleaner, more intuitive LEFT/RIGHT JOIN syntax |
| Parameterized collections | Better query plan caching with cardinality info |
| Configurable translation | Control over SQL generation strategy |

**Limitations:**
- Query syntax (`from ... join ...`) doesn't yet support LeftJoin/RightJoin
- Parameterized collection strategy may require tuning per workload

**References:**
- [LeftJoin Request #12793](https://github.com/dotnet/efcore/issues/12793)
- [RightJoin Request #353](https://github.com/dotnet/efcore/issues/353)
- [Parameterized Collections #13617](https://github.com/dotnet/efcore/issues/13617)
