---
applyTo: "**/*.cs"
description: "This file provides guidelines for writing clean, maintainable, and idiomatic C# code with a focus on functional patterns and proper abstraction."
---

# Coding Style

## Table of Contents

1. [General](#general)
   - [Description](#description)
   - [Requirements](#requirements)
2. [Formatting and Code Style Enforcement](#formatting-and-code-style-enforcement)
3. [Code Organization](#code-organization)
4. [Naming Conventions](#naming-conventions)
   - [Classes and Methods](#classes-and-methods)
   - [Variables](#variables)
   - [Constants](#constants)
5. [Whitespace](#whitespace)
   - [Indentation](#indentation)
   - [Spaces](#spaces)
   - [Newlines](#newlines)
6. [Braces](#braces)
7. [Parentheses](#parentheses)
8. [Usings and Namespaces](#usings-and-namespaces)
9. [Nullability](#nullability)
10. [Symbol References](#symbol-references)
11. [Related Guidelines](#related-guidelines)

## General:

#### Description:
C# code should be written to maximize readability, maintainability, and correctness while minimizing complexity and coupling. Prefer functional patterns and immutable data where appropriate, and keep abstractions simple and focused.

#### Requirements:
- Write clear, self-documenting code.
- Use meaningful names for variables, functions, and classes.
- Keep functions small and focused on a single task.
- Prefer immutability and pure functions where possible.
- Use pattern matching and other functional constructs to simplify code.
- Write unit tests to cover all new functionality.

## File Headers
- All C# files must include the copyright header as specified in .editorconfig:
  ```csharp
  // -----------------------------------------------------------------------
  // <copyright file="AddFilterInfoMiddlewareTests.cs" company="Karma, LLC">
  //   Copyright (c) Karma, LLC. All rights reserved.
  // </copyright>
  // -----------------------------------------------------------------------
  ```
- This header shall appear before all other content of the file.
- File headers are automatically applied during build/formatting via .editorconfig
- Do not manually add or modify file headers - they will be overwritten by the build process
- If file headers are missing, run the `dotnet format` command to apply them automatically

## Formatting and Code Style Enforcement:
- Always use the .editorconfig file at the solution root to enforce consistent code formatting and style across all projects.
  - If you need to change formatting or style rules, update the .editorconfig file and communicate changes to the team.
  - Ensure your IDE is configured to respect .editorconfig settings. In Visual Studio, check that Tools > Options > Text Editor > has "Enable EditorConfig support" enabled.
- Typically, the .editorconfig file should be named `.editorconfig` and placed in the root directory of the repository.
- The .editorconfig file should include rules for:
  - Indentation (recommend 2 spaces)
  - Newline at end of file
  - Space around operators
  - Space after commas
  - Naming conventions for files, classes, and methods
  - Whitespace rules
- Use the built-in C# formatter in Visual Studio or Visual Studio Code to automatically format code according to the rules defined in the .editorconfig file.
- Use the `dotnet format` command to apply formatting rules across the solution.
- Use the `dotnet format --check` command to verify that code adheres to the formatting rules without making changes.
- Use the `dotnet format --dry-run` command to preview formatting changes without applying them.
- Use the `dotnet format --fix` command to apply formatting changes automatically.

## Code Organization
- Organize code into logical namespaces that reflect the structure of the application.
- Do not match folder names to namespaces; instead, use meaningful namespaces that describe the purpose of the code.
- Use folders to organize files within the project:
    ```plaintext
    MyApp
    ├── Orders
    │   ├── OrderService.cs
    │   └── OrderController.cs
    └── Customers
        ├── CustomerService.cs
        └── CustomerController.cs
    ```
- Place related classes in the same namespace.
- Use partial classes to split large classes into smaller, more manageable files.
- Always place `using` directives at the top of the file, before the namespace declaration.
- Use `using` directives for namespaces that are used in the file, and remove unused `using` directives.
- Never use regions to hide code or organize code
  - Regions indicate poor code organization - use proper class structuring and namespaces instead.
  - If code seems to need regions, consider breaking it into smaller, more focused classes.
  - Exception: Regions may be acceptable in non-AI generated code files.
- Order members within a class alphabetically by decreasing visibility as follows:
  - Constants
  - Fields
  - Constructors
  - Properties
  - Methods
  - Events
  - Static members

## Naming Conventions

### General Naming Guidelines:
- Use meaningful names that clearly indicate the purpose of the item.
- Use consistent naming conventions throughout the codebase.
- Avoid abbreviations unless they are well-known and widely accepted.
- Use English for all names, even in non-English codebases.
- Avoid using reserved words or keywords as names.
- Avoid disinformation or misleading names that do not accurately represent the item.
  - Do not name something `people` for a collection of address ids.
- Use pronounceable names
- Use the correct spelling of names
- **Do not** use Hungarian notation or other prefixes for variable names.
  - Avoid naming a variable in a way that indicates the variable's implementation details, such as `strName` or `intCount`.
- **Do not** add gratuitous context to names that is already implied by the type or context.
    - Avoid naming a variable `personId` when it is already clear from the context that it is the id of a `Person`.
    - Good: `Person.Id`
    - Bad: `Person.PersonId`

### Classes and Methods:
- Use PascalCase for class names, method names, and public properties.
- Use Verbs or Verb-Phrase for method names to indicate actions or behaviors.
  - Good: `GetOrder`, `ProcessPayment`, `CalculateTotalPrice`.
  - Bad: `OrderGet`, `PaymentProcess`, `TotalPriceCalculate`.
- Use singular nouns for class names and plural nouns for collections.
- Prefix interfaces with `I` (e.g., `IOrderService`).
- Always Suffix asynchronous methods with `Async` (e.g., `GetOrderAsync`).
- Suffix abstractions with `Base` (e.g., `OrderServiceBase`).
- Use meaningful names that clearly indicate the purpose of the method, or class:
    ```csharp
    // Good: Clear intent
    public async Task<Result<Order>> ProcessOrderAsync(OrderRequest request, CancellationToken cancellationToken)

    // Avoid: Unclear abbreviations
    public async Task<Result<T>> ProcAsync<T>(ReqDto r, CancellationToken ct)
    ```

### Variables:
- Use meaningful names for variables that clearly indicate their purpose.
- Use descriptive names for local variables, avoiding single-letter names except for loop counters (e.g., `i`, `j`).
- Use camelCase for private fields and method parameters.
- **Do not** use Hungarian notation or other prefixes for variable names.
- Prefix private fields with an underscore (e.g., `_orderId`).
- Use compound assignment operators (`+=`, `-=`, `*=`, `/=`) for clarity when modifying variables.
  ```csharp
  // Good: Compound assignment
  totalPrice += line.Price * line.Quantity;

  // Avoid: Separate assignment
  totalPrice = totalPrice + (line.Price * line.Quantity);
  ```
- **Variable Type Declaration:** Follow the [.editorconfig](../../.editorconfig) rules for `var` usage:
  - **Avoid** `var` for built-in types (int, string, bool, etc.) - use explicit types instead.
  - **Avoid** `var` for local variables unless the type is immediately apparent from the right-hand side of the assignment.
  - **Use** `var` only when the type is immediately obvious from object creation or method calls
    ```csharp
    // Good: Type is clear from assignment
    var order = new Order();
    var stringBuilder = new StringBuilder();

    // Good: Explicit types for built-in types
    int count = 42;
    string name = "John Doe";
    bool isValid = true;
    decimal totalPrice = 99.99m;

    // Avoid: Using var for built-in types
    var count = 42; // Warning: Use explicit type for built-in types
    var name = "John Doe"; // Warning: Use explicit type for built-in types

    // Avoid: var when type is not immediately clear
    var lines = GetLines(); // What is the type of lines?
    ```
- Variables should be scoped as tightly as possible:
  - Declare variables in the smallest scope necessary.
  - Avoid declaring variables at the class level unless they are used across multiple methods.
  - Prefer local variables over class-level fields when possible.

|Type|Accessibility|Suggested Modifiers|Prefix|Casing|Example|
|---|---|---|---|---|---|
|Public Constant|Public|`public static readonly`|None|PascalCase|`public static readonly int MaxRetries = 3;`|
|Private Constant|Private|`private static readonly`|None|UPPER_SNAKE_CASE|`private static readonly int MAX_RETRIES = 3;`|
|Field|Private|`private`|`_`|camelCase|`private int _orderId;`|
|Local Variable|Local|`var` or explicit type|None|camelCase|`var order = new Order();` or `Order order = new Order();`|
|Method Parameter|Method|explicit type|None|camelCase|`public void ProcessOrder(Order order) { ... }`|

- Prefer inline variable declarations when the variable is only used once.
  ```csharp
  // Good: Inline declaration
  var order = await _orderService.GetOrderAsync(orderId, cancellationToken);

  // Avoid: Separate declaration
  Order order;
  order = await _orderService.GetOrderAsync(orderId, cancellationToken);
  ```
- **Do not** use magic numbers; use named constants instead.
  ```csharp
  // Good: Named constant
  const int MaxRetries = 3;
  
  for (int i = 0; i < MaxRetries; i++)
  {
      // Retry logic
  }

  // Avoid: Magic number
  for (int i = 0; i < 3; i++)
  {
      // Retry logic
  }
  ```
- **Do not** leave unused variables in the codebase; remove them to keep the code clean.
- **Always** use discards (`_`) for variables that are intentionally unused.
  ```csharp
  // Good: Using discard for unused variable
  _ = await SomeMethodAsync();

  // Avoid: Unused variable without discard
  var result = await SomeMethodAsync(); // Warning: 'result' is never used
  ```

### Constants:
- Use meaningful names that clearly indicate the purpose of the constant.
- Use PascalCase for public constant names (e.g. `MaximumRetries`).
- Use UPPER_SNAKE_CASE for private constants (e.g., `MAX_RETRIES`).

## Whitespace

### Indentation:
- Use consistent indentation throughout the codebase.
  - Use consistent indentation for multi-line statements, aligning subsequent lines with the first line.
  - Use consistent indentation for multi-line method calls, aligning subsequent lines with the first argument.
  - Use consistent indentation for multi-line array initializers, aligning subsequent lines with the first element.
  - Use consistent indentation for multi-line object initializers, aligning subsequent lines with the first property.
  - Use consistent indentation for multi-line lambda expressions, aligning subsequent lines with the first parameter.
  - Use consistent indentation for multi-line string literals, aligning subsequent lines with the first line.
  - Use consistent indentation for multi-line comments, aligning subsequent lines with the first line.
  - Use consistent indentation for multi-line attribute lists, aligning subsequent lines with the first attribute.
  - Use consistent indentation for multi-line `using` directives, aligning subsequent lines with the first directive.
  - Use consistent indentation for multi-line `namespace` declarations, aligning subsequent lines with the first declaration.
  - Use consistent indentation for multi-line `class` and `struct` definitions, aligning subsequent lines with the first definition.
  - Use consistent indentation for multi-line `interface` definitions, aligning subsequent lines with the first definition.
- Always indent code blocks inside control statements, methods, and classes.
- Use 2 or 4 spaces for indentation (as configured in [.editorconfig](../../.editorconfig)).
- Always use spaces for indentation, never tabs.

### Spaces:
- Use spaces to improve readability:
  - Always use spaces around keywords and operators
  - Always insert space after commas in method calls and definitions
  - Always insert space before and after binary operators (e.g., `+`, `-`, `*`, `/`, `==`, `!=`, etc.)
  - Always insert space after commas in method parameters and argument lists
  - Always insert space before and after colon for base class or interface in class definitions
    - Example: `public class MyClass : BaseClass, IMyInterface`
- Avoid trailing whitespace
- Always insert space after keywords like `if`, `for`, `while`, and `switch`
- Always insert space after colon in `for` statements
- Never insert space before and after parentheses in method calls and definitions
- Never insert space inside square brackets for array access or indexers

### Newlines:
- Use a single blank line to separate logical blocks of code.
- Always place a single blank line before and after class definitions.
- Always place a single blank line before and after method definitions.
- Always place a single blank line before and after property definitions.
- Always place `else`, `catch`, and `finally` on a new line.
- Always insert a single blank line before and after control statements (e.g., `if`, `for`, `while`, `switch`).
- Split long lines to a maximum of 120 characters, but prefer shorter lines where possible.
- Do not split lines in the middle of method calls or property accesses.
- **Do not** use multiple statements on a single line.
- **Do not** use empty lines at the end of files.
- **Do not** use multiple blank lines in a row; use a single blank line to separate code blocks.

## Braces
- Use a consistent style for braces throughout the codebase.
- Always use braces for all control statements, even if they contain only a single statement.
  - All `if`, `else if`, and `else` statements **must** use braces `{}` even if the body is a single statement.
  - This rule applies to all flow control statements, including `if`, `else`, `for`, `foreach`, `while`, and `do-while`.
  - Always use braces for `try`, `catch`, and `finally` blocks.
- **Do not** use single-line statements without braces, even for simple conditions.
- **Do not** create control statements without braces, even for single-line statements.
- Always place the opening brace on a new line for control statements.
- Always place the opening brace on a new line for class and method definitions.
- Always place the opening brace on a new line for local method definitions.
- Always place the opening brace on a new line for anonymous method definitions.
- Always place the opening brace on a new line for anonymous types.
- Always place the opening brace on a new line for properties, indexers, and events.
- Always place the closing brace on a new line.

## Parentheses
- Use parentheses to make the order of operations explicit.
- Avoid unnecessary parentheses.
- Always use parentheses around conditions in `if`, `while`, and `for` statements.
- Use parentheses around method parameters.
- Always use parentheses around parameters for lambda expressions, even if they have a single parameter.
  ```csharp
  // Good: Parentheses around lambda parameters
  var filter = (s) => s.StartsWith("test", StringComparison.OrdinalIgnoreCase);
  ```

## Usings and Namespaces
- Always place `using` directives at the top of the file, before the namespace declaration
- Use explicit usings instead of implicit usings for clarity
- Remove unused `using` directives
- Order `using` directives alphabetically
- See [Coding Guidelines](./coding-guidelines.instructions.md) for detailed guidance on usings and namespaces

## Nullability
- Use nullable reference types consistently
- Handle nulls explicitly with proper null checks
- Use nullability annotations appropriately
- See [Coding Guidelines](./coding-guidelines.instructions.md) for detailed nullability guidance

## Symbol References
- Always use the `nameof` operator instead of string literals for member names
- Use `nameof` in exception messages, logging, and method calls
- See [Coding Guidelines](./coding-guidelines.instructions.md) for detailed symbol reference guidance

## Related Guidelines
This document should be used in conjunction with the following guidelines:
- [Coding Guidelines](./coding-guidelines.instructions.md)
- [Testing Guidelines](./testing-mstest.instructions.md)
- [Editor Config](../../.editorconfig)

In case of conflicts, the `.editorconfig` file takes precedence for formatting rules