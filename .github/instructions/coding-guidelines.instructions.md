---
applyTo: "**/*.cs"
description: "This file provides guidelines for writing clean, maintainable, and idiomatic C# code with a focus on functional patterns and proper abstraction."
---

# Coding Guidelines

## Table of Contents

1. [Role Definition](#role-definition)
2. [Quick Reference](#quick-reference)
3. [General](#general)
4. [Documentation](#documentation)
   - [Comments](#comments)
5. [Testing](#testing)
6. [Code Quality and Compiler Diagnostics](#code-quality-and-compiler-diagnostics)
   - [Compiler Warnings and Errors](#compiler-warnings-and-errors-mandatory)
7. [Code Structure](#code-structure)
   - [Design](#design)
   - [Usings and Namespaces](#usings-and-namespaces)
   - [File-Scoped Namespaces](#file-scoped-namespaces)
   - [Global Usings Strategy](#global-usings-strategy)
   - [Classes and Methods](#classes-and-methods)
   - [Conditionals and Loops](#conditionals-and-loops)
   - [Symbol References](#symbol-references)
   - [Nullability](#nullability)
   - [Immutability](#immutability)
   - [Records and Record Types](#records-and-record-types)
   - [Pure Functions](#pure-functions)
   - [Collections and LINQ](#collections-and-linq)
   - [String Comparisons](#string-comparisons)
   - [Pattern Matching](#pattern-matching)
   - [Switch Expressions vs Switch Statements](#switch-expressions-vs-switch-statements)
   - [Extension Methods](#extension-methods)
   - [Abstraction and Separation of Concerns](#abstraction-and-separation-of-concerns)
   - [Asynchronous Programming](#asynchronous-programming)
   - [Expressions](#expressions)
      - [Expression-Bodied Members](#expression-bodied-members-mandatory)
      - [Expression Trees](#expression-trees)
8. [Dependency Management](#dependency-management)
9. [Safe Operations](#safe-operations)
10. [Exception Handling](#exception-handling)
11. [Performance Considerations](#performance-considerations)
12. [Security Considerations](#security-considerations)
13. [Modern C# Features](#modern-c-features)
    - [Nullable Reference Types](#nullable-reference-types)
    - [Asynchronous Streams](#asynchronous-streams)
    - [Other Modern C# Features](#other-modern-c-features)
14. [Related Guidelines](#related-guidelines)

## Role Definition

- C# Language Expert
- Software Architect
- Code Quality Specialist

## Quick Reference

### Mandatory Requirements (Non-Negotiable)

**⚠️ ZERO TOLERANCE:**
- ✅ **All compiler warnings and errors MUST be resolved** - No exceptions
- ✅ **Execute all unit tests after changes** - Tests must pass before commit
- ✅ **Always use braces `{}` for control structures** - Even single statements
- ✅ **Use expression-bodied members for single expressions** - Mandatory for IDE2003
- ✅ **Always use parentheses around lambda parameters** - `(x) => x * x`
- ✅ **No `else` after `return`, `throw`, or `continue`** - Early returns preferred
- ✅ **All public members MUST have XML documentation** - `/// <summary>`

### Namespace & Usings

```csharp
// ✅ DO: Explicit usings at top, traditional namespaces
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
  public class MyClass { }
}

// ❌ DON'T: File-scoped namespaces or implicit usings
namespace MyNamespace; // Not allowed
```

### Nullability

```csharp
// ✅ DO: Explicit nullability with proper checks
public string? ProcessOrder(Order? order)
{
  if (order is null)
  {
    throw new ArgumentNullException(nameof(order));
  }
  
  return order?.Name ?? "Unknown";
}

// ❌ DON'T: Implicit nullability
public string ProcessOrder(Order order) // Warning: Could be null
{
  return order.Name; // CS8602 warning
}
```

### Expression-Bodied Members (MANDATORY)

```csharp
// ✅ DO: Expression-bodied for single expressions
public string FullName => $"{FirstName} {LastName}";
public decimal Total() => Lines.Sum((line) => line.Price);
public void Log(string msg) => _logger.LogInformation(msg);

// ❌ DON'T: Traditional syntax for simple cases
public string FullName
{
  get { return $"{FirstName} {LastName}"; }
}
```

### Switch Expressions vs Statements

```csharp
// ✅ DO: Switch expressions for pure transformations
public string GetStatus(OrderStatus status) =>
  status switch
  {
    OrderStatus.Pending => "Pending",
    OrderStatus.Shipped => "Shipped",
    _ => "Unknown"
  };

// ✅ DO: Switch statements for side effects
public async Task ProcessAsync(Order order)
{
  switch (order.Status)
  {
    case OrderStatus.Pending:
      await _service.ProcessPaymentAsync(order);
      await _logger.LogAsync("Payment processed");
      break;
  }
}
```

### Conditionals (No else after return)

```csharp
// ✅ DO: Early returns without else
public string GetStatus(int code)
{
  if (code == 200)
  {
    return "OK";
  }
  
  if (code == 404)
  {
    return "Not Found";
  }
  
  return "Unknown";
}

// ❌ DON'T: else after return
public string GetStatus(int code)
{
  if (code == 200)
  {
    return "OK";
  }
  else if (code == 404) // ❌ Unnecessary else
  {
    return "Not Found";
  }
}
```

### Records for Immutable Data

```csharp
// ✅ DO: Sealed records for DTOs and value objects
public sealed record Customer(int Id, string Name, string Email);

public sealed record Order(OrderId Id, IReadOnlyList<OrderLine> Lines)
{
  public decimal Total => Lines.Sum((line) => line.Price * line.Quantity);
}

// ❌ DON'T: Mutable classes for simple data
public class Customer
{
  public int Id { get; set; }
  public string Name { get; set; }
}
```

### Collections

```csharp
// ✅ DO: Explicit type with collection expressions
List<OrderLine> lines = [
  new OrderLine { Id = 1, Price = 10.00m },
  new OrderLine { Id = 2, Price = 20.00m }
];

// Empty collections
List<OrderLine> empty = [];

// ❌ DON'T: var with collection expressions
var lines = [new OrderLine { Id = 1 }]; // Ambiguous type
```

### String Comparisons

```csharp
// ✅ DO: Always use StringComparison
if (string.Equals(name, "John", StringComparison.OrdinalIgnoreCase))
{
  // Case-insensitive comparison
}

var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

// ❌ DON'T: Culture-sensitive comparison
if (name == "John") // ❌ Culture-sensitive
```

### Async/Await

```csharp
// ✅ DO: Always use CancellationToken and ConfigureAwait(false)
public async Task<Order> GetOrderAsync(
  OrderId id,
  CancellationToken ct = default)
{
  return await _repository
    .FindAsync(id, ct)
    .ConfigureAwait(false);
}

// ❌ DON'T: Blocking calls or missing ConfigureAwait
public Order GetOrder(OrderId id)
{
  return _repository.GetOrderAsync(id).Result; // ❌ Deadlock risk
}
```

### Symbol References

```csharp
// ✅ DO: Always use nameof
throw new ArgumentNullException(nameof(order));
_logger.LogInformation("Processing {OrderId}", nameof(order.Id));

// ❌ DON'T: String literals for member names
throw new ArgumentNullException("order"); // ❌ No refactoring support
```

### Access Modifiers

```csharp
// ✅ DO: Always explicit, prefer restrictive
public class OrderService
{
  private readonly ILogger _logger;
  internal string ProcessingId { get; }
  public Task<Order> GetOrderAsync() { }
}

// ❌ DON'T: Implicit or overly permissive
class OrderService // ❌ No modifier
{
  ILogger _logger; // ❌ No modifier
}
```

### Documentation

```csharp
/// <summary>
/// Processes an order by validating and saving it to the repository.
/// </summary>
/// <param name="order">The order to process.</param>
/// <returns>The processed order with updated status.</returns>
/// <exception cref="ArgumentNullException">Thrown when order is null.</exception>
public Order ProcessOrder(Order order)
{
  // Implementation
}
```

### Common Warnings to Fix

| Warning | Description | Fix |
|---------|-------------|-----|
| **IDE2003** | Use expression body | Convert to `=>` syntax |
| **IDE0060** | Remove unused parameter | Remove or use parameter |
| **CS8618** | Non-nullable uninitialized | Initialize or mark nullable |
| **CS8602** | Possible null reference | Add null check or `?.` |
| **IDE0059** | Unnecessary assignment | Remove unused assignment |

### Testing Requirements

- ✅ Write unit tests for all new features and bug fixes
- ✅ Test behavior through public APIs, not implementation details
- ✅ Execute all tests and ensure they pass before committing
- ✅ If a test fails, verify the test assumption before changing code

### When to Use What

| Scenario | Use | Don't Use |
|----------|-----|-----------|
| **Immutable data** | `record` | `class` with setters |
| **Value transformation** | Switch expression | Switch statement |
| **Side effects** | Switch statement | Switch expression |
| **Pure function** | `static` method | Instance method |
| **Single expression** | Expression body `=>` | Block body `{ }` |
| **Nullable value** | `string?` with checks | Implicit nullability |
| **String comparison** | `StringComparison.OrdinalIgnoreCase` | `==` operator |
| **Collection params** | `IReadOnlyList<T>` | `List<T>` |
| **Async method** | `Task<T>` with `ConfigureAwait(false)` | `.Result` or `.Wait()` |

### Build Verification

```bash
# Must pass before commit
dotnet build --verbosity normal  # Check for warnings
dotnet test                      # All tests must pass
```

**Remember:** Clean compilation with zero warnings is mandatory. This is not optional—it's a prerequisite for professional software development.

## General

**Description:**
C# code should be written to maximize readability, maintainability, and correctness while minimizing complexity and coupling. Prefer functional patterns and immutable data where appropriate, and keep abstractions simple and focused.

**Requirements:**
- Write clear, self-documenting code
- Keep abstractions simple and focused
- Minimize dependencies and coupling
- Use modern C# features appropriately
- Follow functional programming patterns where applicable
- Always write unit tests to cover all new functionality
- **MANDATORY REQUIREMENT** Always execute unit tests after making changes and before committing code
- If existing code conventions or patterns conflict, these instructions take precedence

## Documentation

- All public classes and methods **must** have XML documentation comments
- XML comments are optional on private and internal members, but recommended for clarity
- Use XML documentation comments to describe the purpose and behavior of classes, methods, properties, and events:
  ```csharp
  /// <summary>
  /// Processes an order by validating and saving it to the repository.
  /// </summary>
  /// <param name="order">The order to process.</param>
  /// <returns>The processed order with updated status.</returns>
  /// <exception cref="ArgumentNullException">Thrown when order is null.</exception>
  public Order ProcessOrder(Order order)
  {
      // Implementation
  }
  ```
- Use `<summary>`, `<param>`, and `<returns>` tags to describe the purpose and behavior of methods
- Use `<remarks>` for additional details that are not covered in the summary
- Use `<exception>` tags to document exceptions that a method can throw
- Use `<inheritdoc/>` to inherit documentation from base classes or interfaces
- Use `<example>` to provide usage examples for complex methods or classes
- Use `<see cref="TypeName"/>` to reference other types or members in documentation
- Use `<seealso cref="TypeName"/>` to provide related types or members
- Use `<typeparam name="T">` to document type parameters in generic methods or classes
- Use `<value>` to document properties
- Use full sentences. Do not only repeat the name of the property or member in the XML comments

### Comments

- Use comments to explain why the code does something, not what it does
- Use inline comments sparingly and only when the code is not self-explanatory
- Use block comments to explain complex logic or algorithms
- Use `//` for single-line comments and `/* ... */` for multi-line comments
- Avoid commenting out code; instead, remove it or use version control to track changes
- Use comments to indicate TODOs, FIXMEs, or other work that needs to be done:
  ```csharp
  // TODO: Implement error handling
  // FIXME: Resolve performance issue
  ```

## Testing

- Write unit tests for all new features and bug fixes
- Design for testability: favor pure functions and minimal dependencies
- Test behavior through public APIs, not implementation details
- Focus on both happy paths and edge cases, including error conditions
- See [Testing Guidelines](./testing-mstest.instructions.md) for more details
- Execute all unit tests and ensure they pass before considering a change complete
- If a unit test fails, verify the scenario or assumption of the test before changing production code

## Code Quality and Compiler Diagnostics

### Compiler Warnings and Errors (MANDATORY)

**ALWAYS correct all compiler warnings and errors before considering code complete.** This is a mandatory requirement for maintaining code quality and preventing technical debt.

#### **Requirements:**

1. **Zero Tolerance for Errors**: All compilation errors MUST be resolved before committing code
   ```csharp
   // ERROR: Must be fixed immediately
   public void ProcessOrder(Order order)
   {
       order.Process(); // CS0117: 'Order' does not contain definition for 'Process'
   }
   ```

2. **Address All Warnings**: All compiler warnings MUST be addressed, not suppressed
   ```csharp
   // WARNING: IDE2003 - Use expression body for methods
   // Before (generates warning)
   private static string ConvertToString(object value)
   {
       return value.ToString() ?? string.Empty;
   }
   
   // After (warning resolved)
   private static string ConvertToString(object value) => value.ToString() ?? string.Empty;
   ```

3. **Common Warning Categories to Address**:
   - **IDE2003**: Use expression body for methods, properties, constructors
   - **IDE0060**: Remove unused parameters
   - **IDE0059**: Remove unnecessary assignment of values
   - **CS8618**: Non-nullable field/property is uninitialized
   - **CS8602**: Dereference of possibly null reference
   - **CS0162**: Unreachable code detected
   - **CS0219**: Variable assigned but never used

4. **Warning Resolution Strategy**:
   ```csharp
   // ✅ Fix the code to eliminate the warning
   public string ProcessName(string input) => input.Trim().ToUpperInvariant();
   
   // ❌ NEVER suppress warnings unless absolutely necessary
   #pragma warning disable CS8618 // Don't do this
   public string Name { get; set; }
   #pragma warning restore CS8618
   ```

5. **When Suppression is Acceptable** (rare cases only):
   - External library issues beyond your control
   - Framework limitations with clear documentation
   - Temporary suppression with immediate TODO/FIXME to resolve
   ```csharp
   // Only when absolutely necessary with clear justification
   #pragma warning disable CS8618 // TODO: Fix after library update
   public ExternalLibraryType Property { get; set; }
   #pragma warning restore CS8618
   ```

6. **IDE-Specific Diagnostics**:
   - **IDE0001-IDE0999**: Code style and formatting issues
   - **IDE1001-IDE1999**: Code quality suggestions
   - **IDE2001-IDE2999**: Expression and pattern matching improvements
   
7. **Verification Process**:
   - Build solution with zero warnings before committing
   - Use `dotnet build --verbosity normal` to see all warnings
   - Configure IDE to show warnings as errors during development
   - Run static analysis tools as part of CI/CD pipeline

8. **Project Configuration**: Ensure projects are configured to treat warnings appropriately:
   ```xml
   <PropertyGroup>
     <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
     <WarningsAsErrors />
     <WarningsNotAsErrors />
   </PropertyGroup>
   ```

#### **Enforcement:**
- **Pre-commit requirement**: All warnings and errors must be resolved
- **Code review requirement**: Code with warnings will not be approved
- **CI/CD integration**: Build pipelines should fail on warnings
- **Developer responsibility**: It is every developer's responsibility to ensure clean compilation

#### **Benefits:**
- Prevents technical debt accumulation
- Improves code quality and maintainability
- Reduces runtime errors and unexpected behavior
- Ensures consistent code standards across team
- Makes code more robust and reliable

**Remember**: Clean compilation is not optional—it's a prerequisite for professional software development.

## Code Structure

### Design

- Use interfaces to define contracts for behavior:
    ```csharp
    public interface IHasMetaData
    {
        string GetMetadata();
    }

    public class Order : IHasMetaData
    {
        public string GetMetadata() => $"Order ID: {Id}";
    }
    ```
- Design for testability:
    ```csharp
    // Good: Easy to test pure functions
    public static class PriceCalculator
    {
        public static decimal CalculateDiscount(
            decimal price,
            int quantity,
            CustomerTier tier) =>
            price * quantity * GetDiscountRate(tier);
            
        private static decimal GetDiscountRate(CustomerTier tier) =>
          tier switch
          {
              CustomerTier.Premium => 0.15m,
              CustomerTier.Standard => 0.05m,
              _ => 0m
          };
    }

    // Avoid: Hard to test due to hidden dependencies
    public decimal CalculateDiscount()
    {
        var user = _userService.GetCurrentUser();  // Hidden dependency
        var settings = _configService.GetSettings(); // Hidden dependency
        // Calculation
    }
    ```

### Usings and Namespaces

- Always place `using` directives at the top of the file, before the namespace declaration
- Use explicit usings:
```csharp
// Good: Explicit usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace MyNamespace
{
    public class MyClass
    {
        // Implementation
    }
}

// Avoid: Global usings in most cases (use sparingly)
// File: GlobalUsings.cs
global using System;
global using System.Linq;

// Avoid: Implicit usings (missing explicit using statements)
namespace MyNamespace
{
    public class MyClass
    {
        // Implementation using types without explicit usings
    }
}
```

### File-Scoped Namespaces

**DO NOT use file-scoped namespaces for single-namespace files.**

### Global Usings Strategy

**Use global usings sparingly and strategically to avoid namespace pollution while reducing repetitive using declarations.**

#### **Recommended Global Usings:**
Create a `GlobalUsings.cs` file in each project for commonly used namespaces:

```csharp
// File: GlobalUsings.cs
// Good: Strategic global usings for common namespaces
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// ASP.NET Core namespaces used across most controllers/services
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// Project-wide patterns
global using FluentValidation;
global using MediatR;
```

#### **When to Use Global Usings:**
1. **System namespaces** - Core .NET namespaces used in 80%+ of files
2. **Project-wide dependencies** - Namespaces used across most files in the project
3. **Framework namespaces** - Common framework namespaces (e.g., ASP.NET Core, Entity Framework)

#### **When NOT to Use Global Usings:**
```csharp
// Avoid: Too many global usings create namespace pollution
global using System.Text.Json;        // Only if used in most files
global using System.Security.Claims;  // Domain-specific, not global
global using Microsoft.EntityFrameworkCore; // Only for data projects

// Avoid: Domain-specific namespaces as global
global using MyCompany.Orders.Domain;
global using MyCompany.Payments.Services;
```

#### **Best Practices:**
- **Limit to 10-15 global usings** - More than this indicates over-use
- **Document rationale** - Include comments explaining why each namespace is global
- **Review regularly** - Remove global usings that are no longer widely used
- **Each project should have its own GlobalUsings.cs** - Avoid cross-project global usings
- **Prefer explicit usings in code files** - Reduce reliance on global usings for clarity

```csharp
// File: GlobalUsings.cs - Example for a web API project
// Core .NET namespaces used in 80%+ of files
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// ASP.NET Core namespaces used across most controllers/services
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// Project-wide patterns
global using FluentValidation;
global using MediatR;


// Avoid: Using global usings for rarely used namespaces
global using MyCompany.Orders.Domain; // Avoid domain-specific namespaces as global
```

### Classes and Methods

- Use small, focused classes and methods
- Each class should have a single responsibility
- Use meaningful names for classes and methods that clearly indicate their purpose
- Always use access modifiers (`public`, `private`, `protected`, `internal`) to control visibility
- Prefer more restrictive access levels:
  - Use `private` for implementation details within a class
  - Use `internal` for members shared within the assembly but not part of public API
  - Use `protected` only for members intended for inheritance in derived classes
  - Use `public` only for true public API members
  - Use `sealed` for classes not designed for inheritance
  - Use `protected internal` for members that are intended to be accessed from derived classes or within the same assembly
  - Use `private protected` for members that are intended to be accessed from derived classes within the same assembly
  - Use `static` classes for grouping related utility methods that do not require instantiation
  - Use `static` methods for utility functions that do not require instance state
- Prefer automatically implemented properties for simple getters and setters:
  ```csharp
  // Good: Automatically implemented property
  public string CustomerName { get; set; }

  // Avoid: Explicit backing field for simple properties
  private string _customerName;
  public string CustomerName
  {
      get => _customerName;
      set => _customerName = value;
  }
  ```
- Prefer object initializers for simple object creation:
  ```csharp
  // Good: Object initializer
  var order = new Order
  {
      CustomerName = "John Doe",
      TotalPrice = 100.00m
  };

  // Avoid: Constructor with many parameters when properties would suffice
  var order = new Order("John Doe", 100.00m, DateTime.Now, "pending");
  ```
- **Do not** initialize member variables outside of constructors
- Use properties for encapsulation and to provide a clear API
- **Do not** use public fields; use properties instead
- **Do not** use `this` to refer to instance members unless required for disambiguation:
  ```csharp
  // Good: No need for 'this'
  public string CustomerName { get; set; }
  
  public void SetCustomerName(string customerName)
  {
      CustomerName = customerName; // Clear without 'this'
  }
  
  // Only use 'this' when necessary for disambiguation
  public void SetCustomerName(string customerName)
  {
      this.CustomerName = customerName; // Only when parameter has same name
  }
  ```

### Conditionals and Loops
- **MANDATORY**: **Always** use braces `{}` for all control structures, even for single statements. This is a mandatory requirement to maintain consistency and readability across the codebase.
- Use `if`, `else if`, and `else` for conditional logic
- **Avoid** negated conditions; prefer positive logic for clarity
- **Avoid** complex boolean expressions; break them into smaller, named methods if needed
- **Avoid** ternary operators for complex conditions; use `if` statements instead
- **Avoid** nested ternary operators; they reduce readability
- Use `switch` statements or expressions for multiple discrete values
- Use pattern matching with `is` and `switch` for cleaner conditional logic
- Use `for`, `foreach`, `while`, and `do-while` loops for iteration
- Use `break` and `continue` judiciously to control loop flow
- Prefer LINQ for querying and transforming collections instead of manual loops where appropriate
- Avoid deeply nested conditionals; refactor into smaller methods if necessary
- **Do not** use `goto` statements
- **MANDATORY**: **Do not** use `else` after a `return`, `throw`, or `continue` statement:
```csharp
// Good: No else after return
public string GetStatus(int code)
{
    if (code == 200)
    {
        return "OK";
    }
      
    if (code == 404)
    {
        return "Not Found";
    }
      
    return "Unknown";
}

// Avoid: Else after return
public string GetStatus(int code)
{
    if (code == 200)
    {
        return "OK";
    }
    else if (code == 404)
    {
        return "Not Found";
    }
    else
    {
        return "Unknown";
    }
}
```

### Symbol References

- Always use the `nameof` operator instead of a string for a member name
- Use nameof in method calls, exception messages, and logging:
```csharp
public class OrderService
{
    public async Task<Order> GetOrderAsync(OrderId id, CancellationToken ct)
    {
        var order = await _repository.FindAsync(id, ct);

        if (order is null)
            throw new OrderNotFoundException(
                $"Order with {nameof(id)} '{id}' not found");

        if (!order.Lines.Any())
            throw new InvalidOperationException(
                $"{nameof(order.Lines)} cannot be empty");

        return order;
    }

    public void ValidateOrder(Order order)
    {
        if (order.Lines.Count == 0)
            throw new ArgumentException(
                "Order must have at least one line",
                nameof(order));
    }
}
```

### Nullability

- Use nullable reference types and handle nulls explicitly
```csharp
// Good: Explicit nullability
public class OrderProcessor
{
    private readonly ILogger<OrderProcessor>? _logger;
    private string? _lastError;

    public OrderProcessor(ILogger<OrderProcessor>? logger = null)
    {
        _logger = logger;
    }
}

// Avoid: Implicit nullability
public class OrderProcessor
{
    private readonly ILogger<OrderProcessor> _logger; // Warning: Could be null
    private string _lastError; // Warning: Could be null
}
```
- Use `?` for nullable reference types
- Use null-coalescing operators like `??` to provide default values for nullable types
- Use `??=` for null-coalescing assignment
- Use `?.` for null-conditional operations
- Use `!` to assert non-nullability when you are sure a value is not null
- Use nullability attributes: `[NotNull]`, `[NotNullWhen]`, etc., to indicate the nullability of reference types
- Do **not** use `null` as a default value; prefer using `default(T)` for value types or `string.Empty` for strings
- Use `null` only when it is semantically meaningful, such as representing the absence of a value
- Always check for null before dereferencing objects
- Use `is null` and `is not null` for null checks instead of `== null` and `!= null`
- Always use nullability annotations to indicate whether a method parameter or return value can be null:
```csharp
public class StringUtilities
{
    // Output is non-null if input is non-null
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToUpperCase(string? input) =>
        input?.ToUpperInvariant();

    // Method never returns null
    [return: NotNull]
    public static string EnsureNotNull(string? input) =>
        input ?? string.Empty;

    // Parameter must not be null when method returns true
    public static bool TryParse(string? input, [NotNullWhen(true)] out string? result)
    {
        result = null;
        if (string.IsNullOrEmpty(input))
            return false;

        result = input;
        return true;
    }
}
```

#### Nullable Reference Type Configuration

- All nullable warnings should be addressed, not suppressed
- Use nullable annotations consistently:
  ```csharp
  // Good: Explicit nullable annotation
  public string? ProcessOrder(Order? order)
  {
    if (order is null)
    {
        throw new ArgumentNullException(nameof(order));
    }

    return $"Processed {order.Id}";
  }
  ```

### Immutability

- Favor immutable objects where possible
- Use `readonly` for fields that should not change after initialization
- Use `record` types for immutable data structures:
  ```csharp
  // Good: Immutable record type
  public sealed record Order(OrderId Id, IReadOnlyList<OrderLine> Lines);

  // Avoid: Mutable class
  public class Order
  {
      public OrderId Id { get; set; }
      public List<OrderLine> Lines { get; set; }
  }
  ```
- Use `readonly` collections to prevent modification:
  ```csharp
  public sealed record Order(OrderId Id, IReadOnlyList<OrderLine> Lines);
  
  public static class OrderExtensions
  {
      public static IReadOnlyList<OrderLine> GetLines(this Order order) => order.Lines;
  }

  // Avoid: Exposing mutable collections
  public class Order
  {
      public OrderId Id { get; set; }
      public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
  }
  ```
- Avoid mutable state in methods that should be pure:
  ```csharp
  // Avoid: Mutating input parameters
  public void ProcessOrder(Order order)
  {
      order.Lines.Add(new OrderLine()); // Mutating input
  }
  
  // Good: Return new instance or use pure functions
  public Order AddLineToOrder(Order order, OrderLine newLine) =>
      order with { Lines = order.Lines.Append(newLine).ToList() };
  ```
- Use `IEnumerable<T>`, `IReadOnlyList<T>` or `IReadOnlyCollection<T>` for method parameters and return types when modification is not needed
- Use `ImmutableArray<T>`, `ImmutableList<T>`, or similar types from `System.Collections.Immutable` for collections that should not change
- Use `Span<T>` and `Memory<T>` for performance-sensitive scenarios where immutability is not required
- Use `readonly struct` for small, immutable value types to avoid boxing and improve performance:
  ```csharp
  public readonly struct Money
  {
      public decimal Amount { get; }
      public string Currency { get; }

      public Money(decimal amount, string currency)
      {
          Amount = amount;
          Currency = currency;
      }
  }
  ```
- Use `readonly` properties for computed values that do not change

### Records and Record Types

- Use record types for immutable data structures and value objects:
  ```csharp
  // Good: Simple record for immutable data
  public sealed record Customer(int Id, string Name, string Email);

  // Good: Record with validation and computed properties
  public sealed record Order(OrderId Id, CustomerId CustomerId, IReadOnlyList<OrderLine> Lines)
  {
      public decimal TotalPrice => Lines.Sum((line) => line.Price * line.Quantity);
      
      public bool IsValid => Lines.Count > 0 && Lines.All((line) => line.IsValid);
  }

  // Avoid: Mutable class for simple data structures
  public class Customer
  {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Email { get; set; }
  }
  ```
- Use `with` expression for non-destructive mutation:
  ```csharp
  var order2 = order1 with { Status = OrderStatus.Shipped };
  ```
- Use `record` for data classes and DTOs, especially with automatic property implementation:
  ```csharp
  // Good: Record for data transfer with automatic property implementation
  public record OrderDto(int OrderId, string ProductName, int Quantity);
  ```
- Prefer `init` accessors for properties that should be set at object creation time only:
  ```csharp
  public sealed record Order
  {
      public required OrderId Id { get; init; }
      public required CustomerId CustomerId { get; init; }
      public required DateTime OrderDate { get; init; }
  }
  ```
- Use `record` types for events in event-sourced systems:
  ```csharp
  public sealed record OrderPlacedEvent(Guid OrderId, DateTimeOffset PlacedAt) : OrderEvent(OrderId);
  ```
- Use positional records for simple cases, and declaration records for more complex scenarios:
  ```csharp
  // Good: Positional record for simple data
  public sealed record Point(int X, int Y);

  // Good: Declaration record for complex behavior
  public sealed record Circle
  {
      public required int Radius { get; init; }
      public required Point Center { get; init; }
      
      public double CalculateArea() => Math.PI * Math.Pow(Radius, 2);
  }
  ```
- Use `readonly` structs for small, immutable value types to avoid boxing and improve performance:
  ```csharp
  public readonly struct Money
  {
      public decimal Amount { get; }
      public string Currency { get; }

      public Money(decimal amount, string currency)
      {
          Amount = amount;
          Currency = currency;
      }
  }
  ```
- Use primary constructors for compact syntax when defining records:
  ```csharp
  public record Order(OrderId Id, CustomerId CustomerId, IReadOnlyList<OrderLine> Lines);
  ```
- Use `record` for any class that should have value-based equality semantics:
  ```csharp
  public record Address(string Street, string City, string State, string ZipCode);
  ```

#### **Record Inheritance Patterns:**
```csharp
// Good: Abstract record for shared behavior
public abstract record Shape(double Area);

public sealed record Circle(double Radius) : Shape(Math.PI * Radius * Radius);

public sealed record Rectangle(double Width, double Height) : Shape(Width * Height);

 // Usage with pattern matching
public static string DescribeShape(Shape shape) =>
  shape switch
  {
      Circle(var radius) => $"Processing circle with radius {radius}",
      Rectangle(var width, var height) => $"Processing rectangle {width}x{height}",
      _ => "Unknown shape"
  };
```

**2. Record hierarchy for domain events:**
```csharp
// Good: Record hierarchy for events
public abstract record DomainEvent(DateTime OccurredAt, string EventId);

public sealed record OrderCreated(
    DateTime OccurredAt, 
    string EventId, 
    OrderId OrderId, 
    CustomerId CustomerId
) : DomainEvent(OccurredAt, EventId);

public sealed record OrderCancelled(
    DateTime OccurredAt, 
    string EventId, 
    OrderId OrderId, 
    string Reason
) : DomainEvent(OccurredAt, EventId);
```

**3. Record with interface implementation:**
```csharp
// Good: Record implementing interfaces
public interface IHasTimestamp
{
    DateTime CreatedAt { get; }
}

public sealed record AuditEntry(
    string Action, 
    string UserId, 
    DateTime CreatedAt, 
    Dictionary<string, object> Data
) : IHasTimestamp;
```

#### **When to Use Records vs Classes:**

**Use Records For:**
- **Value objects** - Data containers with value equality semantics
- **DTOs** - Data transfer objects between layers
- **Configuration objects** - Immutable settings and options
- **Event objects** - Domain events and messages
- **API models** - Request/response models

**Use Classes For:**
- **Entity objects** - Mutable objects with identity
- **Services** - Objects with behavior and state
- **Complex business logic** - Objects requiring inheritance and polymorphism
- **Large objects** - Objects with many mutable properties

#### **Record Best Practices:**
```csharp
// Good: Sealed records prevent unintended inheritance
public sealed record CustomerDto(int Id, string Name, string Email);

// Good: Validation in record constructors
public sealed record Email
{
    public string Value { get; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
            
        if (!value.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(value));
            
        Value = value;
    }
}

// Good: Record with factory methods for complex creation
public sealed record Money(decimal Amount, string Currency)
{
    public static Money Dollars(decimal amount) => new(amount, "USD");
    public static Money Euros(decimal amount) => new(amount, "EUR");
    
    public Money Add(Money other) => Currency == other.Currency 
        ? new Money(Amount + other.Amount, Currency)
        : throw new InvalidOperationException("Cannot add different currencies");
}
```

### Pure Functions

- Prefer pure functions that do not have side effects:
  ```csharp
  // Good: Pure function
  public static decimal CalculateTotalPrice(
      IEnumerable<OrderLine> lines,
      decimal taxRate) =>
      lines.Sum(line => line.Price * line.Quantity) * (1 + taxRate);

  // Avoid: Method with side effects
  public void CalculateAndUpdateTotalPrice()
  {
      Total = Lines.Sum(l => l.Price * l.Quantity);
      UpdateDatabase(); // Side effect
  }
  ```

### Collections and LINQ

- Prefer using `IEnumerable<T>`, `IReadOnlyList<T>`, or `IReadOnlyCollection<T>` for method parameters and return types when modification is not needed
- Use collection expressions to create and manipulate collections:
    ```csharp
    // Good: Collection expression for creating a list
    List<OrderLine> orderLines = [
        new OrderLine { ProductId = "123", Quantity = 2, Price = 10.00m },
        new OrderLine { ProductId = "456", Quantity = 1, Price = 20.00m }
    ];

    // Avoid: Manual collection creation
    var orderLines = new List<OrderLine>();
    orderLines.Add(new OrderLine { ProductId = "123", Quantity = 2, Price = 10.00m });
    orderLines.Add(new OrderLine { ProductId = "456", Quantity = 1, Price = 20.00m });
    ```
- Use range operators for collections:
    ```csharp
    // Good: Range operator for slicing
    public IEnumerable<Order> GetRecentOrders() => _orders[^5..];

    // Avoid: Manual slicing
    public IEnumerable<Order> GetRecentOrders()
    {
        var count = _orders.Count;
        return _orders.Skip(count - 5).Take(5);
    }
    ```
- Use collection expressions for empty collections:
    ```csharp
    // Good: Collection expression for an empty list
    List<OrderLine> emptyLines = [];

    // Avoid: Manual creation of an empty list
    var emptyLines = new List<OrderLine>();
    ```
- **Never** use `var` when defining collections using collection expression, as it can lead to confusion about the type:
    ```csharp
    // Good: Explicit type for collections
    List<OrderLine> orderLines = [new OrderLine { ProductId = "123", Quantity = 2, Price = 10.00m }];
    
    // Good: var when type is obvious from constructor
    var orderLines = new List<OrderLine> { /* items */ };

    // Avoid: Using var for collection expressions
    var orderLines = [new OrderLine { ProductId = "123", Quantity = 2, Price = 10.00m }];
    ```
- Use `List<T>` for mutable collections that need to be modified
- Use `ImmutableArray<T>`, `ImmutableList<T>`, or similar types from `System.Collections.Immutable` for collections that should not change
- Use LINQ for querying and transforming collections instead of manual loops where appropriate

### String Comparisons

- **Always** use overloads that consume `StringComparison` or `StringComparer`
- Prefer case-insensitive string comparisons. Use overloads that consume `StringComparison.OrdinalIgnoreCase` or `StringComparer.OrdinalIgnoreCase` for case-insensitive comparisons:
    ```csharp
    // Good: Using StringComparison for case-insensitive comparison
    if (string.Equals(name, "John", StringComparison.OrdinalIgnoreCase))
    {
        // Do something
    }

    // Avoid: Using == for culture-sensitive comparison
    if (name == "John") // This is culture-sensitive
    {
        // Do something
    }
    ```
- When creating `Dictionary<TKey, TValue>` or `HashSet<TKey>` with a `string` key, use `StringComparer.OrdinalIgnoreCase` for case-insensitive keys:
    ```csharp
    // Good: Using StringComparer for case-insensitive keys
    var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    dictionary["key"] = "value";

    // Avoid: Using default comparer for case-insensitive keys
    var dictionary = new Dictionary<string, string>();
    dictionary["key"] = "value"; // This is case-sensitive
    ```


### Pattern Matching

- Use `is` for type checks and pattern matching instead of `as`:
    ```csharp
    if (obj is string str)
    {
        // Use str
    }
    ```
- Use pattern matching for type checks and deconstruction:
    ```csharp
    // Good: Pattern matching for type checks and deconstruction
    public void ProcessOrder(object order)
    {
        if (order is Order o)
        {
            Console.WriteLine($"Processing order {o.Id}");
        }
    }

    // Avoid: Type checks with `is` and casting
    public void ProcessOrder(object order)
    {
        if (order is Order)
        {
            var o = (Order)order;
            Console.WriteLine($"Processing order {o.Id}");
        }
    }
    ```
- Use `switch` expressions for concise pattern matching:
    ```csharp
    public string GetOrderStatus(Order order) =>
        order.Status switch
        {
            OrderStatus.Shipped => "Shipped",
            OrderStatus.Pending => "Pending",
            _ => "Unknown"
        };
    ```
- Use `switch` statements for complex pattern matching:
    ```csharp
    public void ProcessOrder(object order)
    {
        switch (order)
        {
            case Order o:
                Console.WriteLine($"Processing order {o.Id}");
                break;
        }
    }
    ```

### Switch Expressions vs Switch Statements

**Use switch expressions for functional-style transformations and switch statements for imperative operations with side effects.**

#### **Switch Expressions (Preferred for Pure Functions):**

**Use switch expressions when:**
- **Returning values** - Transforming input to output
- **Mapping operations** - Converting between types
- **Pure functions** - No side effects or state mutations
- **Concise transformations** - Simple, readable mappings

```csharp
// Good: Switch expression for value mapping
public static string GetOrderStatusText(OrderStatus status) =>
  status switch
  {
      OrderStatus.Draft => "Draft",
      OrderStatus.Pending => "Pending Payment",
      OrderStatus.Paid => "Payment Received",
      OrderStatus.Shipped => "Shipped",
      OrderStatus.Delivered => "Delivered",
      OrderStatus.Cancelled => "Cancelled",
      _ => throw new ArgumentException($"Unknown status: {status}", nameof(status))
  };

// Good: Switch expression with pattern matching
public static decimal CalculateShipping(Address address, decimal weight) =>
  address switch
  {
      { Country: "US", State: "CA" } => weight * 1.2m,
      { Country: "US" } => weight * 1.0m,
      { Country: "CA" } => weight * 1.5m,
      { Country: "MX" } => weight * 2.0m,
      _ => weight * 3.0m
  };

// Good: Switch expression with tuple patterns
public static string GetPriorityLevel(int urgency, int impact) =>
  (urgency, impact) switch
  {
      (5, 5) => "Critical",
      (5, _) or (_, 5) => "High",
      (4, 4) => "High",
      (3, 3) or (4, _) or (_, 4) => "Medium",
      (1, 1) or (2, 2) => "Low",
      _ => "Normal"
  };

// Good: Switch expression with record deconstruction
public static string DescribeShape(Shape shape) =>
  shape switch
  {
      Circle(var radius) when radius > 10 => $"Large circle (r={radius})",
      Circle(var radius) => $"Circle (r={radius})",
      Rectangle(var width, var height) when width == height => $"Square ({width}x{height})",
      Rectangle(var width, var height) => $"Rectangle ({width}x{height})",
      _ => "Unknown shape"
  };
```

#### **Switch Statements (Use for Side Effects):**

**Use switch statements when:**
- **Performing actions** - Methods that return void
- **Side effects** - Logging, database operations, UI updates
- **Complex logic** - Multiple statements per case
- **Resource management** - Operations requiring try/catch blocks

```csharp
// Good: Switch statement for operations with side effects
public async Task ProcessOrderAsync(Order order)
{
    switch (order.Status)
    {
        case OrderStatus.Draft:
            await _logger.LogInformationAsync("Order {OrderId} is still in draft", order.Id);
            break;
            
        case OrderStatus.Pending:
            await _paymentService.ProcessPaymentAsync(order.PaymentInfo);
            await _notificationService.SendPaymentReminderAsync(order.CustomerId);
            break;
            
        case OrderStatus.Paid:
            await _inventoryService.ReserveItemsAsync(order.Lines);
            await _shippingService.CreateShipmentAsync(order);
            break;
            
        case OrderStatus.Shipped:
            await _notificationService.SendShippingNotificationAsync(order.CustomerId, order.TrackingNumber);
            break;
            
        default:
            await _logger.LogWarningAsync("Unknown order status: {Status}", order.Status);
            break;
    }
}

// Good: Switch statement with complex logic
public void ValidateOrder(Order order)
{
    switch (order.Status)
    {
        case OrderStatus.Draft:
            if (order.Lines.Count == 0)
            {
                throw new ValidationException("Draft order must have at least one line item");
            }
            break;
            
        case OrderStatus.Pending:
            if (order.PaymentInfo is null)
            {
                throw new ValidationException("Pending order must have payment information");
            }
            
            if (order.Lines.Any((line) => line.Quantity <= 0))
            {
                throw new ValidationException("All line items must have positive quantity");
            }
            break;
            
        case OrderStatus.Shipped:
            if (string.IsNullOrEmpty(order.TrackingNumber))
            {
                throw new ValidationException("Shipped order must have tracking number");
            }
            break;
    }
}
```

#### **Conversion Guidelines:**

**Convert switch statements to switch expressions when:**
```csharp
// Before: Switch statement that only returns values
public string GetStatusColor(OrderStatus status)
{
    switch (status)
    {
        case OrderStatus.Draft:
            return "gray";
        case OrderStatus.Pending:
            return "yellow";
        case OrderStatus.Paid:
            return "blue";
        case OrderStatus.Shipped:
            return "green";
        case OrderStatus.Cancelled:
            return "red";
        default:
            return "black";
    }
}

// After: Switch expression (preferred)
public string GetStatusColor(OrderStatus status) =>
  status switch
  {
      OrderStatus.Draft => "gray",
      OrderStatus.Pending => "yellow", 
      OrderStatus.Paid => "blue",
      OrderStatus.Shipped => "green",
      OrderStatus.Cancelled => "red",
      _ => "black"
  };
```

#### **Pattern Matching Enhancements:**
```csharp
// Good: Advanced pattern matching in switch expressions
public static string AnalyzeCustomer(Customer customer) =>
  customer switch
  {
      { Age: < 18 } => "Minor customer",
      { Age: >= 18 and < 65, Orders.Count: > 10 } => "Loyal adult customer",
      { Age: >= 65, TotalSpent: > 1000 } => "Premium senior customer", 
      { IsVip: true } => "VIP customer",
      { LastOrderDate: var date } when date < DateTime.Now.AddYears(-1) => "Inactive customer",
      _ => "Regular customer"
  };

// Good: Switch expression with null checks
public static string GetCustomerName(Customer? customer) =>
  customer switch
  {
      null => "Unknown",
      { FirstName: not null, LastName: not null } => $"{customer.FirstName} {customer.LastName}",
      { FirstName: not null } => customer.FirstName,
      { LastName: not null } => customer.LastName,
      _ => "No name provided"
  };
```

### Extension Methods

- Prefer extension methods for adding functionality to existing types without modifying them
- Prefer extension methods for defined types over utility classes
- Use extension methods to add domain-specific operations to existing types:
    ```csharp
    // Good: Extension method for domain-specific operations
    public static class OrderExtensions
    {
        public static decimal CalculateTotalPrice(this Order order) =>
            order.Lines.Sum(line => line.Price * line.Quantity);
    }
    ```
- Avoid using extension methods for core functionality that should be part of the type itself


### Abstraction and Separation of Concerns

- Separate state from behavior when appropriate
- Avoid large classes with multiple responsibilities
- Use small, focused classes and methods
- Use interfaces to define contracts
- Prefer composition over inheritance
- Use functional patterns where appropriate


### Asynchronous Programming

- Use `async` and `await` for asynchronous programming to improve responsiveness and scalability
- Use `async` methods for I/O-bound operations and `Task.Run` for CPU-bound operations
- **Always** pass `CancellationToken` to asynchronous methods to allow cancellation of long-running operations:
    ```csharp
    public async Task<Order> GetOrderAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetOrderAsync(id, cancellationToken).ConfigureAwait(false);
    }
    ```
- Avoid blocking calls in asynchronous methods to prevent deadlocks:
    ```csharp
    // Good: Asynchronous method without blocking
    public async Task<Order> GetOrderAsync(OrderId id)
    {
        return await _repository.GetOrderAsync(id).ConfigureAwait(false);
    }

    // Avoid: Blocking call in asynchronous method
    public Order GetOrder(OrderId id)
    {
        return _repository.GetOrderAsync(id).Result; // Blocking call
    }
    ```
- **ALWAYS** use `ConfigureAwait(false)` in library code to avoid deadlocks and improve performance:
    ```csharp
    public async Task<Order> GetOrderAsync(OrderId id)
    {
        return await _repository.GetOrderAsync(id).ConfigureAwait(false);
    }
    ```
- Avoid using `async void` *except* for event handlers
- Prefer returning `Task` or `Task<T>` directly from asynchronous methods when no additional async work is needed:
    ```csharp
    public Task<Order> GetOrderAsync(OrderId id)
    {
        return _repository.GetOrderAsync(id);
    }
    ```


### Expressions

- **MANDATORY**: Use expression-bodied members for simple methods, properties, and constructors whenever possible to improve readability and reduce code verbosity
- Use conditional expressions for simple conditions
- Use switch expressions for simple pattern matching
- **Always use parentheses around lambda parameters**: This applies to all lambda expressions, including single parameters.  Parentheses are required for lambda expressions in C# to ensure clarity and avoid ambiguity, especially when the lambda is used as a delegate or in LINQ queries. This is a mandatory requirement to maintain consistency and readability across the codebase.
  ```csharp
  // Good: Parentheses around lambda parameters
  Func<int, int> square = (x) => x * x;
  // Avoid: Missing parentheses around lambda parameters
  Func<int, int> square = x => x * x; // This is not allowed in this context
  ```

#### Expression-Bodied Members (MANDATORY)

**Use expression-bodied members whenever the implementation is a single expression or statement.** This applies to:

1. **Properties** - Use for computed properties and simple getters:
   ```csharp
   // Good: Expression-bodied property
   public string FullName => $"{FirstName} {LastName}";
   public bool IsValid => Status == OrderStatus.Active && ExpiryDate > DateTime.Now;
   
   // Avoid: Traditional property syntax for simple cases
   public string FullName 
   { 
       get { return $"{FirstName} {LastName}"; }
   }
   ```

2. **Methods** - Use for single-expression methods:
   ```csharp
   // Good: Expression-bodied method
   public decimal CalculateTotal() => Lines.Sum((line) => line.Price * line.Quantity);
   public bool IsExpired() => ExpiryDate < DateTime.Now;
   public void LogMessage(string message) => _logger.LogInformation(message);
   
   // Avoid: Traditional method syntax for simple cases
   public decimal CalculateTotal()
   {
       return Lines.Sum((line) => line.Price * line.Quantity);
   }
   ```

3. **Constructors** - Use for simple parameter assignment:
   ```csharp
   // Good: Expression-bodied constructor
   public Order(OrderId id, Customer customer) => (Id, Customer) = (id, customer);
   
   // Traditional syntax when multiple statements are needed
   public Order(OrderId id, Customer customer, ILogger logger)
   {
       Id = id;
       Customer = customer;
       _logger = logger ?? throw new ArgumentNullException(nameof(logger));
   }
   ```

4. **Indexers** - Use for simple indexer implementations:
  ```csharp
   // Good: Expression-bodied indexer
   public OrderLine this[int index] => Lines[index];
  ```

5. **Operators** - Use for operator overloads:
  ```csharp
   // Good: Expression-bodied operator
  public static bool operator ==(OrderId left, OrderId right) => left.Equals(right);
  public static bool operator !=(OrderId left, OrderId right) => !(left == right);
  ```

**When NOT to use expression-bodied members:**
- When the implementation requires multiple statements
- When the implementation includes error handling (try/catch/finally)
- When the implementation involves complex control flow (if/else, loops)
- When the expression-bodied member would require complex or nested ternary operators that hurt readability
- When the implementation contains complex logic that benefits from traditional block syntax for readability
- When debugging would be significantly hampered (rare cases)
- When the expression would be too long and hurt readability (generally > 120 characters)
- When creating expression-bodied members would lead to multiple method overloads with the same signature, causing ambiguity

**Examples of mandatory conversions:**
```csharp
// Before: Traditional syntax
private static object ConvertToString(object value)
{
    return value.ToString() ?? string.Empty;
}

// After: Expression-bodied (MANDATORY)
private static string ConvertToString(object value) => value.ToString() ?? string.Empty;

// Before: Traditional property
public string Name 
{ 
    get { return _name; }
    set { _name = value; }
}

// After: Auto-property (when no additional logic needed)
public string Name { get; set; }
  
// Or expression-bodied when computed
public string DisplayName => $"{FirstName} {LastName}";
```
#### Expression Trees

- Expression trees cannot contain methods or invocations that have optional parameters
- When building expression trees, avoid calling methods with optional parameters directly in the expression tree, as this can lead to unexpected behavior or runtime errors:
  ```csharp
  // Avoid: Using methods with optional parameters in expression trees
  Expression<Func<string, bool>> filter = (s) => s.StartsWith("test");

  // Good: Use method overload without optional parameters
  Expression<Func<string, bool>> filter = (s) => s.StartsWith("test", StringComparison.OrdinalIgnoreCase);

  // Alternative: Create a helper method without optional parameters
  public static bool StartsWithIgnoreCase(string input, string prefix)
  {
      return input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
  }

  Expression<Func<string, bool>> filter = (s) => StartsWithIgnoreCase(s, "test");
  ```

## Dependency Management

- Use dependency injection to manage dependencies and promote loose coupling
- Prefer constructor injection for dependencies
- Minimize constructor dependencies (consider if too many dependencies indicate design issues)
- Prefer composition with interfaces


## Safe Operations

- Use null checks to avoid null reference exceptions
- Prefer `Try*` methods for operations that can fail without throwing exceptions:
  ```csharp
  // Good: Try method for operations that can fail
  public bool TryGetOrder(OrderId id, [NotNullWhen(true)] out Order? order)
  {
      order = _repository.GetOrder(id);
      return order != null;
  }
  ```


## Exception Handling

### General Guidelines

- Use exceptions for exceptional conditions, not for control flow
- It is not necessary to handle every exception, especially in low-level code
- Avoid logging in low-level code; allow the exception to bubble up to higher-level code where it can be handled appropriately
- Use `ArgumentNullException` for null checks on method parameters
- Use `ArgumentException` for invalid argument values
- Prefer `Try*` methods for operations that can fail without throwing exceptions
- Avoid catching any exception only to re-throw it without additional handling

### When to Catch General Exceptions

**Avoid catching general exceptions in most cases**, but there are specific scenarios where it's appropriate:

**Appropriate Use Cases:**
1. **Application Boundary/Entry Points** - Top-level exception handlers in web applications, main application entry points, middleware
2. **Resource Cleanup and Disposal** - When ensuring resources are properly disposed regardless of exception type
3. **Logging and Monitoring** - When all exceptions must be logged for monitoring purposes (always re-throw after logging unless at application boundary)
4. **Circuit Breaker and Resilience Patterns** - When implementing resilience patterns like circuit breakers or retries
5. **Background Services and Long-Running Tasks** - When prevention of a background service from crashing is critical
6. **Plugin/Extension Systems** - When loading plugins or extensions where exceptions may occur due to external code

**When NOT to Catch General Exceptions:**
- Business logic code
- Repository/data access methods (catch specific exceptions like `SqlException`, `DbUpdateException`)
- Validation methods
- Unit tests (let them fail to indicate issues)

## Performance Considerations

- Use `Span<T>` and `Memory<T>` for performance-sensitive scenarios involving slices of arrays or collections
- Use `ValueTask<T>` for performance-sensitive scenarios where the result might already be available
- Use `ConfigureAwait(false)` in library code to avoid deadlocks and improve performance
- Use `IAsyncEnumerable<T>` for asynchronous streaming of data
- Use `StringBuilder` for string concatenation in loops or performance-critical scenarios
- Use `IEnumerable<T>` for deferred execution and lazy evaluation

## Security Considerations

- Use `SecureString` for sensitive data like passwords (where supported)
- Use modern hashing algorithms for sensitive data instead of `MD5` or `SHA1`
- Use `RandomNumberGenerator` for generating cryptographically secure random numbers
- Never store sensitive data in plain text
- Prefer HTTPS for all network communications
- Validate and sanitize all user inputs

## Modern C# Features

**Description:**
This section provides guidelines for using modern C# features effectively and responsibly to improve code clarity, maintainability, and performance.

**Requirements:**
- Use modern C# features introduced in C# 8.0 and later, such as nullable reference types, asynchronous streams, and record types, where appropriate and beneficial.
- Follow the specific guidelines for each feature to ensure consistency and clarity.

### Nullable Reference Types

- **Enable nullable reference types** in project settings or on a per-file basis using `#nullable enable`:
  ```csharp
  #nullable enable

  public class OrderProcessor
  {
      private readonly ILogger<OrderProcessor>? _logger;
      private string? _lastError;

      public OrderProcessor(ILogger<OrderProcessor>? logger = null)
      {
          _logger = logger;
      }

      public void Process(Order order)
      {
          if (order is null)
              throw new ArgumentNullException(nameof(order));

          // ...
      }
  }
  ```
- Use `?` to indicate nullable reference types:
  ```csharp
  public class Customer
  {
      public string FirstName { get; set; }
      public string? LastName { get; set; } // LastName is optional
  }
  ```
- Use `??` and `??=` for null-coalescing:
  ```csharp
  string message = userMessage ?? "Default message";
  logger.LogInformation("Order processed at {Time}", DateTime.UtcNow.ToString("o") ?? "unknown time");
  ```
- Use `?.` for null-conditional member access:
  ```csharp
  var customerName = order.Customer?.Name;
  ```
- Use `!` for null-forgiving operator when you are sure a value is not null:
  ```csharp
  public string GetOrderId(Order order)
  {
      return order.Id!; // We're sure Id is not null
  }
  ```
- Use nullable reference type annotations in API contracts to indicate nullability expectations:
  ```csharp
  // Good: Nullable reference type annotations in API contracts
  public interface IOrderService
  {
      Order? GetOrderById(OrderId id);
  }
  ```
- Avoid using `Null` directly; prefer `Nullable<T>` or `?` syntax:
  ```csharp
  public DateTime? OrderDate { get; set; } // Preferred over DateTime Null
  ```
- Avoid using nullability annotations on parameters or return values that are never null:
  ```csharp
  // Good: No nullability annotations for non-nullable parameters
  public void ProcessOrder(Order order)
  {
      // ...
  }

  // Avoid: Unnecessary nullability annotation
  public void ProcessOrder(Order? order)
  {
      // ...
  }
  ```
- When using `TryParse` pattern, annotate the `out` parameter with `[NotNullIfNotNull(nameof(input))]`:
  ```csharp
  public static bool TryParse(string? input, [NotNullIfNotNull(nameof(input))] out Order? order)
  {
      // ...
  }
  ```
- Configure warnings as errors for nullable reference type violations:
  ```xml
  <PropertyGroup>
    <WarningsAsErrors>nullable</WarningsAsErrors>
  </PropertyGroup>
  ```
- Review and address all nullable warnings during development:
  - **CS8601**: Possible null reference assignment
  - **CS8602**: Dereference of a possibly null reference
  - **CS8618**: Non-nullable property 'X' is uninitialized
  ```csharp
  // Good: Addressing CS8602 warning
  public string GetOrderName(Order? order)
  {
      return order?.Name ?? "Unknown Order";
  }

  // Avoid: Dereferencing a possibly null reference
  public string GetOrderName(Order? order)
  {
      return order.Name; // CS8602 warning
  }
  ```

### Asynchronous Streams

- Use asynchronous streams for efficient data processing with `IAsyncEnumerable<T>`:
  ```csharp
  public async IAsyncEnumerable<Order> GetProcessedOrdersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
      var orders = GetPendingOrders();

      foreach (var order in orders)
      {
          // Simulate async processing
          await Task.Delay(1000, cancellationToken);

          yield return order;
      }
  }
  ```
- Await the `yield return` of an asynchronous stream in an asynchronous method:
  ```csharp
  await foreach (var order in orderService.GetProcessedOrdersAsync())
  {
      Console.WriteLine($"Processed order {order.Id}");
  }
  ```
- Use `await foreach` to consume asynchronous streams:
  ```csharp
  // Good: Awaiting asynchronous stream
  await foreach (var order in orderService.GetProcessedOrdersAsync())
  {
      Console.WriteLine($"Processed order {order.Id}");
  }

  // Avoid: Blocking call on asynchronous stream
  foreach (var order in orderService.GetProcessedOrdersAsync().ToEnumerable())
  {
      Console.WriteLine($"Processed order {order.Id}");
  }
  ```

### Other Modern C# Features

- Use `switch` expressions and statements for control flow based on pattern matching:
  ```csharp
  public string GetOrderDescription(Order order) =>
    order switch
    {
        { Status: OrderStatus.Shipped, TrackingNumber: var trackingNumber } => $"Shipped (Tracking: {trackingNumber})",
        { Status: OrderStatus.Pending } => "Pending",
        _ => "Unknown"
    };
  ```
- Use conversion operators for implicit or explicit type conversions:
  ```csharp
  public class OrderId
  {
      public string Value { get; }

      public OrderId(string value)
      {
          Value = value;
      }

      public static implicit operator string(OrderId orderId) => orderId.Value;
      public static explicit operator OrderId(string value) => new OrderId(value);
  }
  ```
- Use indexers for custom collection-like access:
  ```csharp
  public class OrderBook
  {
      private readonly Dictionary<OrderId, Order> _orders = new Dictionary<OrderId, Order>();

      public Order this[OrderId id]
      {
          get => _orders[id];
          set => _orders[id] = value;
      }
  }
  ```
- Use default interface methods for optional behavior in interfaces:
  ```csharp
  public interface IOrderService
  {
      void ProcessOrder(Order order);

      // Default interface method
      void LogOrder(Order order)
      {
          Console.WriteLine($"Order {order.Id} processed at {DateTime.UtcNow}");
      }
  }
  ```
- Use static abstract members in interfaces for constants or static methods:
  ```csharp
  public interface IShape
  {
      static abstract double Pi { get; }
  }

  public class Circle : IShape
  {
      public static double Pi => Math.PI;
  }
  ```
- Use interpolated string handlers for custom string interpolation behavior:
  ```csharp
  // Custom interpolated string handler
  public readonly struct SqlInterpolatedStringHandler
  {
      // ...
  }

  public void LogQuery([StringSyntax(Sql)] FormattableString query)
  {
      // Custom handling of interpolated SQL strings
  }
  ```
- Use `required` members for properties that must be initialized during object creation:
  ```csharp
  public class Order
  {
      public required OrderId Id { get; init; }
      public required CustomerId CustomerId { get; init; }
      public required DateTime OrderDate { get; init; }
  }
  ```

## Related Guidelines
This document should be used in conjunction with the following guidelines:
- [Copilot Instructions](../../copilot-instructions.md)
- [Coding Style Guidelines](./coding-style.instructions.md)
- [Testing Guidelines](./testing-mstest.instructions.md)
- [Editor Config](../../.editorconfig)

In case of conflicts, the `.editorconfig` file takes precedence for formatting rules.
