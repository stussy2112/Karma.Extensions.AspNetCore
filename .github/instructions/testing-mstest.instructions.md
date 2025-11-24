---
applyTo: "tests/**/*.cs"
description: "This file provides guidelines for writing effective, maintainable tests using MSTest and related tools"
---

# MSTest Testing Guidelines

## Table of Contents

1. [Role Definition](#role-definition)
2. [Quick Reference](#quick-reference)
3. [General](#general)
   - [Description](#description)
   - [Requirements](#requirements)
4. [Naming Conventions](#naming-conventions)
5. [Test Visibility](#test-visibility)
6. [Test Structure](#test-structure)
   - [File Organization](#file-organization)
   - [Class Organization](#class-organization)
   - [Method Organization](#method-organization)
   - [Partial Test Classes](#partial-test-classes)
   - [Setup and Teardown](#setup-and-teardown)
   - [Test Method Structure](#test-method-structure)
7. [Mocking and Dependencies](#mocking-and-dependencies)
   - [General Principles](#general-principles)
   - [When to Use Real Instances](#when-to-use-real-instances)
   - [When to Use Mocks (Moq)](#when-to-use-mocks-moq)
   - [Examples](#examples)
   - [Creating Test Implementations](#creating-test-implementations)
   - [Decision Tree: Real Instance vs. Mock](#decision-tree-real-instance-vs-mock)
   - [Benefits of Using Real Instances](#benefits-of-using-real-instances)
   - [Mock Configuration Best Practices](#mock-configuration-best-practices)
   - [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
   - [Migration Strategy](#migration-strategy)
8. [Error and Exception Testing](#error-and-exception-testing)
9. [Asynchronous Method Tests](#asynchronous-method-tests)
10. [Related Guidelines](#related-guidelines)

## Role Definition

- Test Engineer
- Quality Assurance Specialist

## Quick Reference

This section provides a quick lookup for common testing patterns and requirements.

### **Test Class Template**

```csharp
// -----------------------------------------------------------------------
// <copyright file="OrderServiceTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace MyApp.Tests
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class OrderServiceTests
  {
    private OrderService _sut;

    [TestInitialize]
    public void TestInitialize()
    {
      _sut = new OrderService();
    }

    [TestCleanup]
    public void TestCleanup()
    {
      _sut = null;
    }

    [TestMethod]
    public void When_condition_MethodName_expected_behavior()
    {
      // Arrange
      // Setup test data and dependencies

      // Act
      // Execute the method under test

      // Assert
      // Verify the expected outcome
    }
  }
}
```

### **Naming Conventions**

| Element | Convention | Example |
|---------|-----------|---------|
| Test Class | `{ClassName}Tests` | `OrderServiceTests` |
| Feature Test Class | `{FeatureName}Tests` | `UserAuthenticationTests` |
| Test Method | `When_{condition}_{State_Under_Test}_{Expected_Behavior}` | `When_order_is_null_CreateOrder_throws_ArgumentNullException` |
| Partial Test File | `{ClassName}Tests.{MethodName}.cs` | `OrderServiceTests.CreateOrder.cs` |

### **Common Assertions**

```csharp
// Boolean
Assert.IsTrue(condition);
Assert.IsFalse(condition);

// Equality
Assert.AreEqual(expected, actual);
Assert.AreNotEqual(expected, actual);

// Null checks
Assert.IsNull(obj);
Assert.IsNotNull(obj);

// Type checks
Assert.IsInstanceOfType(obj, typeof(ExpectedType));
Assert.IsNotInstanceOfType(obj, typeof(UnexpectedType));

// Collections
Assert.HasCount(3, collection);
Assert.Contains(item, collection);

// Exceptions (MANDATORY: Use ThrowsExactly only)
_ = Assert.ThrowsExactly<ArgumentException>(() => sut.DoSomething(null));
```

### **Test Method Patterns**

#### **Basic Test Pattern (AAA)**
```csharp
[TestMethod]
public void When_input_is_valid_Process_returns_success()
{
  // Arrange
  var input = new ValidInput();
  var sut = new Service();

  // Act
  Result result = sut.Process(input);

  // Assert
  Assert.IsTrue(result.IsSuccess);
}
```

#### **Exception Testing**
```csharp
[TestMethod]
public void When_input_is_null_Process_throws_ArgumentNullException()
{
  // Arrange
  var sut = new Service();

  // Act & Assert
  _ = Assert.ThrowsExactly<ArgumentNullException>(() => sut.Process(null));
}
```

#### **Async Method Testing**
```csharp
[TestMethod]
public void When_data_is_valid_ProcessAsync_returns_result()
{
  // Arrange
  var sut = new Service();
  var data = CreateValidData();

  // Act
  Result result = sut.ProcessAsync(data).GetAwaiter().GetResult();

  // Assert
  Assert.IsNotNull(result);
}
```

#### **Parameterized Testing**
```csharp
[TestMethod]
[DataRow(1, 2, 3)]
[DataRow(4, 5, 9)]
[DataRow(0, 0, 0)]
public void When_adding_numbers_Add_returns_sum(int a, int b, int expectedSum)
{
  // Arrange
  var calculator = new Calculator();

  // Act
  int result = calculator.Add(a, b);

  // Assert
  Assert.AreEqual(expectedSum, result);
}
```

### **Mocking Decision Matrix**

| Dependency Type | Use Real Instance? | Use Mock? | Example |
|----------------|-------------------|-----------|---------|
| Value Object / DTO | ✅ Yes | ❌ No | `Order`, `OrderRequest` |
| Pure Function | ✅ Yes | ❌ No | `PriceCalculator` |
| Collection | ✅ Yes | ❌ No | `List<T>`, `Dictionary<K,V>` |
| Domain Entity | ✅ Yes | ❌ No | `Customer`, `Product` |
| In-Memory Store | ✅ Yes (create test impl) | ❌ No | `InMemoryRepository` |
| Database | ❌ No | ✅ Yes | `IDbConnection` |
| File System | ❌ No | ✅ Yes | `IFileService` |
| HTTP Client | ❌ No | ✅ Yes | `HttpClient`, `IHttpClientFactory` |
| Email Service | ❌ No | ✅ Yes | `IEmailService` |
| Payment Gateway | ❌ No | ✅ Yes | `IPaymentGateway` |

### **Attributes Quick Reference**

| Attribute | Purpose | Usage |
|-----------|---------|-------|
| `[TestClass]` | Marks a class as containing tests | Required on test class (primary file only for partials) |
| `[TestMethod]` | Marks a method as a test | Required on each test method |
| `[TestInitialize]` | Runs before each test | Optional, use for setup |
| `[TestCleanup]` | Runs after each test | Optional, use for cleanup |
| `[DataRow]` | Provides data for parameterized tests | Use with `[TestMethod]` |
| `[ExcludeFromCodeCoverage]` | Excludes from code coverage | Required on test classes and test helpers |

### **When to Use Partial Test Classes**

✅ **Use partial classes when:**
- Test class has 20-30+ test methods
- Single production method requires 10+ tests
- Test file exceeds 500-1000 lines of code
- Tests naturally group by production method

❌ **Don't use partial classes when:**
- Test class has fewer than 20 test methods
- Tests don't group naturally by method
- Added complexity outweighs benefits
- For integration tests or end-to-end tests where grouping is more feature-based

### **Common Anti-Patterns to Avoid**

| ❌ Anti-Pattern | ✅ Correct Approach |
|----------------|-------------------|
| `async` test methods | Use `.GetAwaiter().GetResult()` or `Task.Run()` |
| `Assert.ThrowsException<T>` | Use `Assert.ThrowsExactly<T>` |
| Testing private members directly | Test through public API |
| Shared state between tests | Each test must be independent |
| External dependencies (DB, files) | Use mocks or in-memory implementations |
| Mocking value objects | Use real instances |
| Mocking collections | Use real `List<T>` or `Dictionary<K,V>` |
| Multiple assertions per test | One logical assertion per test |
| No assertions in test | Every test must have assertions |
| Using regions in tests | Organize with proper structure, no regions |

### **Mandatory Requirements Checklist**

- [ ] All test classes decorated with `[ExcludeFromCodeCoverage]`
- [ ] Test naming follows `When_{condition}_{State_Under_Test}_{Expected_Behavior}` pattern
- [ ] All tests use Arrange-Act-Assert (AAA) pattern
- [ ] Exception testing uses `Assert.ThrowsExactly<T>` only
- [ ] No `async` test methods
- [ ] Real instances used instead of mocks where possible
- [ ] Each test has at least one assertion
- [ ] Tests are independent (no shared state)
- [ ] No external system dependencies
- [ ] Tests executed after creation/modification
- [ ] Failed tests investigated before changing production code

### **File Organization for Partial Test Classes**

```plaintext
tests/
└── OrderServiceTests/                      // Folder for test class
    ├── OrderServiceTests.cs                // Primary: [TestClass], setup, helpers
    ├── OrderServiceTests.CreateOrder.cs    // Tests for CreateOrder method
    ├── OrderServiceTests.UpdateOrder.cs    // Tests for UpdateOrder method
    └── OrderServiceTests.DeleteOrder.cs    // Tests for DeleteOrder method
```

### **Test Implementation Template (In-Memory Repository)**

```csharp
[ExcludeFromCodeCoverage]
public class InMemoryOrderRepository : IOrderRepository
{
  private readonly Dictionary<int, Order> _storage = new Dictionary<int, Order>();

  public void Add(Order order) => _storage[order.Id] = order;

  public Order GetById(int id) => _storage.TryGetValue(id, out Order order) ? order : null;

  public bool Update(Order order)
  {
    if (!_storage.ContainsKey(order.Id))
    {
      return false;
    }

    _storage[order.Id] = order;
    return true;
  }

  public void Delete(int id) => _storage.Remove(id);

  // Test-specific helpers
  public void Clear() => _storage.Clear();
  public int Count => _storage.Count;
}
```

### **Moq Setup Patterns**

```csharp
// Basic setup
_mockService.Setup((x) => x.GetById(1)).Returns(expectedResult);

// Setup with any parameter
_mockService.Setup((x) => x.GetById(It.IsAny<int>())).Returns(expectedResult);

// Setup with parameter matching
_mockService.Setup((x) => x.GetById(It.Is<int>((id) => id > 0))).Returns(expectedResult);

// Setup to throw exception
_mockService.Setup((x) => x.GetById(1)).Throws<NotFoundException>();

// Verify method was called
_mockService.Verify((x) => x.GetById(1), Times.Once);

// Verify method was never called
_mockService.Verify((x) => x.Delete(It.IsAny<int>()), Times.Never);
```

### **TestContext Usage**

```csharp
public TestContext TestContext { get; set; }

[TestMethod]
public void When_test_runs_TestContext_provides_cancellation_token()
{
  // Arrange
  var service = new Service();
  CancellationToken cancellationToken = TestContext.CancellationToken;

  // Act
  var task = Task.Run(() => service.ProcessAsync(cancellationToken), cancellationToken);
  Result result = task.Result;

  // Assert
  Assert.IsNotNull(result);
}
```

## General:

**Description:**
Tests should be reliable, maintainable, and provide meaningful coverage. Use MSTest as the primary testing framework, with proper isolation and clear patterns for test organization and execution.

### **Requirements:**
- Always use `MSTest` as the testing framework, unless the existing codebase uses a different framework.
- Always follow these instructions.  These instructions override any existing test structure or patterns in the project/solution.
- Ensure test isolation
- Follow consistent patterns
- Maintain high code coverage
- All tests should be deterministic and repeatable.
- **MANDATORY REQUIREMENT**: Avoid rewriting the entire test class when creating or updating tests; instead, only provide the new or updated tests.
- **MANDATORY REQUIREMENT**: All tests **must** contain assertions to verify the expected behavior of the code under test.
- Do not create `async` test methods
- Use `MSTest` for unit testing.
- Use `Moq` for mocking dependencies in tests. **IMPORTANT**: Prefer real, concrete instances over mocks whenever possible. See [Mocking and Dependencies](#mocking-and-dependencies) for detailed guidance.
- Create test cases for both happy paths and edge cases.
- Create tests for error conditions and exceptions.
- Create tests for all public methods and properties.
- Never write tests that depend on external systems (e.g., databases, file systems, network).
- Never write tests that rely on shared state or global variables.
- Never expose members that are not part of the public API for the purpose of testing.
- Always decorate test classes with `[ExcludeFromCodeCoverage]` attribute to exclude them from code coverage analysis.
- Always decorate classes created for testing with `[ExcludeFromCodeCoverage]` attribute to exclude them from code coverage analysis:
    ```csharp
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class OrderServiceTests
    {
        // Test methods
    }
    ```
- **NEVER** decorate interfaces created for testing with `[ExcludeFromCodeCoverage]` attribute.  It is only valid on Assembly, Class, Struct, Constructor, Method, Property, and Event declarations.
- **MANDATORY REQUIREMENT**: Always execute tests after creation or modification to ensure correctness and reliability. This is not optional.
- When a test fails, confirm and correct the scenario and/or assumption before changing production code.
- Always follow code conventions and patterns established in the project
  - See [Coding Guidelines](./coding-guidelines.instructions.md), [Coding Styles](./coding-style.instructions.md) and [Editor Config](../../.editorconfig) for more details.
- Avoid testing `private`, `protected` and `internal` members directly.
  - If necessary, test them indirectly through public methods that use them.
  - Use the `InternalsVisibleTo` attribute to expose internal members to test projects if needed.

## Naming Conventions
- Use PascalCase for test class and method names.
- Use descriptive names that clearly indicate the purpose of the test.
- Use the following naming convention for test methods:
  - `When_{condition}_{State_Under_Test}_{Expected_Behavior}` (e.g., `When_user_exists_GetUser_returns_user`)
  - **MANDATORY REQUIREMENT** Separate words with an underscore (`_`) for readability, especially between the words in the condition and expectation.
    - EXAMPLE: `When_BetweenOperator_With_Insufficient_Values_BuildLambda_Returns_False` 
- Use the following naming convention for test classes:
  - `ClassNameTests` for testing a specific class (e.g., `OrderServiceTests`)
  - `FeatureNameTests` for testing a specific feature or functionality (e.g., `UserAuthenticationTests`)

## Test Visibility

- Test classes and methods should be public
- Helper methods within test classes should be private
- Use the `[ExcludeFromCodeCoverage]` attribute on test classes and dummy classes

## Test Structure

### **File Organization:**
-	Place each test class in its own file.
-	Name the file to match the test class.
-	Organize test files in a directory structure that mirrors the production code structure.
- Always place dummy classes created for tests in the same file as the test class
- Always decorate dummy classes with `[ExcludeFromCodeCoverage]` attribute to exclude them from code coverage analysis:
    ```csharp
    [ExcludeFromCodeCoverage]
    public class DummyClass
    {
        // Dummy class for testing purposes
    }
    ```

### **Class Organization:**
-	Each test class should target a single production class or feature.
-	Name test classes with the suffix Tests (e.g., `OrderServiceTests`).
-	Place test classes in a namespace that mirrors the structure of the code under test.
-	Use the [TestClass] attribute to mark a class as a test class.

### **Method Organization:**
-	Group related tests together within the same class.
- Never, ever, under any circumstances use regions to hide or organize code in test classes; prefer to keep code organized and readable without regions.
- Each test method should confirm a single, specific behavior or outcome.
-	Use [TestMethod] for each test case.
-	Use [TestInitialize] for setup logic that runs before each test.
-	Use [TestCleanup] for teardown logic that runs after each test.
-	Avoid sharing state between tests; tests must be independent.

### **Partial Test Classes:**

**Use partial classes to organize test classes when a production class has many methods that each require extensive testing.**

#### **When to Use Partial Test Classes:**

- When a test class contains more than 20-30 test methods
- When a single production method requires 10+ test methods
- When test methods naturally group by the production method they test
- When a test file exceeds 500-1000 lines of code

#### **Naming Conventions:**

- **Primary partial class file**: `ClassNameTests.cs` (contains `[TestClass]`, shared setup, and helper methods)
- **Method-specific partial class files**: `ClassNameTests.MethodName.cs` (contains tests for a specific method)

#### **File Organization:**

```plaintext
tests/
└── OrderServiceTests/
    ├── OrderServiceTests.cs                    // Primary file with [TestClass], setup, helpers
    ├── OrderServiceTests.CreateOrder.cs        // Tests for CreateOrder method
    ├── OrderServiceTests.UpdateOrder.cs        // Tests for UpdateOrder method
    ├── OrderServiceTests.CancelOrder.cs        // Tests for CancelOrder method
    └── OrderServiceTests.GetOrderStatus.cs     // Tests for GetOrderStatus method
```

#### **Example Structure:**

**Primary File: `OrderServiceTests.cs`**
```csharp
// -----------------------------------------------------------------------
// <copyright file="OrderServiceTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace MyApp.Tests
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public partial class OrderServiceTests
  {
    private Mock<IOrderRepository> _mockRepository;
    private OrderService _sut;

    [TestInitialize]
    public void TestInitialize()
    {
      _mockRepository = new Mock<IOrderRepository>();
      _sut = new OrderService(_mockRepository.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
      _mockRepository = null;
      _sut = null;
    }

    // Shared helper methods
    private Order CreateValidOrder() =>
      new Order { Id = 1, CustomerId = 100, Amount = 99.99m };

    private Order CreateInvalidOrder() =>
      new Order { Id = 0, CustomerId = 0, Amount = -1.0m };
  }
}
```

**Method-Specific File: `OrderServiceTests.CreateOrder.cs`**
```csharp
// -----------------------------------------------------------------------
// <copyright file="OrderServiceTests.CreateOrder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MyApp.Tests
{
  [ExcludeFromCodeCoverage]
  public partial class OrderServiceTests
  {
    [TestMethod]
    public void When_order_is_valid_CreateOrder_returns_created_order()
    {
      // Arrange
      Order validOrder = CreateValidOrder();
      _mockRepository.Setup((x) => x.Add(It.IsAny<Order>())).Returns(validOrder);

      // Act
      Order result = _sut.CreateOrder(validOrder);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(validOrder.Id, result.Id);
      _mockRepository.Verify((x) => x.Add(It.IsAny<Order>()), Times.Once);
    }

    [TestMethod]
    public void When_order_is_null_CreateOrder_throws_ArgumentNullException()
    {
      // Arrange
      Order nullOrder = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => _sut.CreateOrder(nullOrder));
    }

    [TestMethod]
    public void When_order_amount_is_negative_CreateOrder_throws_ArgumentException()
    {
      // Arrange
      Order invalidOrder = CreateInvalidOrder();

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentException>(() => _sut.CreateOrder(invalidOrder));
    }

    [TestMethod]
    public void When_repository_throws_exception_CreateOrder_propagates_exception()
    {
      // Arrange
      Order validOrder = CreateValidOrder();
      _mockRepository.Setup((x) => x.Add(It.IsAny<Order>())).Throws<InvalidOperationException>();

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidOperationException>(() => _sut.CreateOrder(validOrder));
    }

    // Additional tests for CreateOrder method...
  }
}
```

**Method-Specific File: `OrderServiceTests.UpdateOrder.cs`**
```csharp
// -----------------------------------------------------------------------
// <copyright file="OrderServiceTests.UpdateOrder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MyApp.Tests
{
  [ExcludeFromCodeCoverage]
  public partial class OrderServiceTests
  {
    [TestMethod]
    public void When_order_exists_UpdateOrder_updates_successfully()
    {
      // Arrange
      Order existingOrder = CreateValidOrder();
      _mockRepository.Setup((x) => x.Update(It.IsAny<Order>())).Returns(true);

      // Act
      bool result = _sut.UpdateOrder(existingOrder);

      // Assert
      Assert.IsTrue(result);
      _mockRepository.Verify((x) => x.Update(It.IsAny<Order>()), Times.Once);
    }

    [TestMethod]
    public void When_order_does_not_exist_UpdateOrder_returns_false()
    {
      // Arrange
      Order nonExistentOrder = CreateValidOrder();
      _mockRepository.Setup((x) => x.Update(It.IsAny<Order>())).Returns(false);

      // Act
      bool result = _sut.UpdateOrder(nonExistentOrder);

      // Assert
      Assert.IsFalse(result);
    }

    // Additional tests for UpdateOrder method...
  }
}
```

#### **Best Practices:**

1. **Single `[TestClass]` Attribute:** Only declare `[TestClass]` in the primary partial class file, not in method-specific files
2. **Shared Setup:** Place `[TestInitialize]` and `[TestCleanup]` in the primary file only
3. **Shared Fields:** Declare all shared fields in the primary file
4. **Helper Methods:** Place shared helper methods in the primary file; method-specific helpers in their respective partial files
5. **Consistent Namespace:** All partial class files must use the exact same namespace
6. **File Headers:** Include proper file headers in all partial class files
7. **Exclude from Coverage:** Add `[ExcludeFromCodeCoverage]` to all partial class declarations
8. **Test Independence:** Each test method must remain independent regardless of which partial file it's in

#### **When NOT to Use Partial Classes:**

- When a test class has fewer than 20 test methods
- When tests don't naturally group by production method
- When the added file organization complexity outweighs readability benefits
- For integration tests or end-to-end tests where grouping is more feature-based

#### **Alternative Approaches:**

If partial classes seem overly complex for your scenario, consider these alternatives:
- **Separate Test Classes:** Create entirely separate test classes (e.g., `CreateOrderTests`, `UpdateOrderTests`)
- **Refactor Production Code:** If a class requires hundreds of tests, consider whether the production class violates Single Responsibility Principle

### **Setup and Teardown:**
-	Use private fields for objects shared by multiple tests, initialized in [TestInitialize].
-	Release or reset resources in [TestCleanup] if needed.

### **Test Method Structure:**
-	Use the Arrange-Act-Assert (AAA) pattern:
  -	Arrange: Set up test data and dependencies.
  -	Act: Invoke the method under test.
  -	Assert: Verify the outcome.
  -	Clearly separate these sections with comments if helpful:
    ```csharp
    // Example of Arrange-Act-Assert pattern
    [TestMethod]
    public void When_request_is_valid_then_Post_returns_Ok() {
        // Arrange
        var request = new ValidRequest();

        // Act
        var response = sut.Post(request);

        // Assert
        Assert.IsTrue(response.IsSuccess);
    }
    ```
- **Always** use `Assert.ThrowsExactly<T>` for testing exceptions:
  ```csharp
    [TestMethod]
    public void When_invalid_input_DoSomething_throws_ArgumentException()
    {
        // Arrange
        var sut = new MyService();
    
        // Act & Assert
        _ = Assert.ThrowsExactly<ArgumentException>(() => sut.DoSomething(null));
    }

    // For async methods that throw exceptions
    [TestMethod]
    public void When_invalid_data_ProcessAsync_throws_ValidationException()
    {
        // Arrange
        var service = new OrderService();
    
        // Act & Assert
        var aggregateException = Assert.ThrowsExactly<AggregateException>(() => Task.Run(() => service.ProcessAsync(null)).Wait());
    
        Assert.IsInstanceOfType(aggregateException.InnerException, typeof(ValidationException));
    }
  ```
- Only put a single statement in `Assert.ThrowsExcept<T>` to ensure clarity about what is expected to throw the exception.

- Use `Assert.IsTrue` and `Assert.IsFalse` for boolean conditions:
  ```csharp
    [TestMethod]
    public void When_condition_is_true_then_assert_is_true()
    {
        Assert.IsTrue(condition);
    }

    [TestMethod]
    public void When_condition_is_false_then_assert_is_false()
    {
        Assert.IsFalse(condition);
    }
   ```
- Use `Assert.AreEqual` and `Assert.AreNotEqual` for value comparisons:
  ```csharp
  [TestMethod]
  public void When_values_are_equal_then_assert_are_equal()
  {
      Assert.AreEqual(expected, actual);
  }

  [TestMethod]
  public void When_values_are_not_equal_then_assert_are_not_equal()
  {
      Assert.AreNotEqual(expected, actual);
  }
  ```
- Use `Assert.IsNull` and `Assert.IsNotNull` for null checks:
  ```csharp
  [TestMethod]
  public void When_object_is_null_then_assert_is_null()
  {
      Assert.IsNull(obj);
  }

  [TestMethod]
  public void When_object_is_not_null_then_assert_is_not_null()
  {
      Assert.IsNotNull(obj);
  }
  ```
- Use `Assert.IsInstanceOfType` and `Assert.IsNotInstanceOfType` for type checks:
  ```csharp
  [TestMethod]
  public void When_object_is_of_expected_type_then_assert_is_instance_of_type()
  {
      Assert.IsInstanceOfType(obj, typeof(ExpectedType));
  }

  [TestMethod]
  public void When_object_is_not_of_expected_type_then_assert_is_not_instance_of_type()
  {
      Assert.IsNotInstanceOfType(obj, typeof(ExpectedType));
  }
  ```
- Use `Assert.HasCount` for checking the count of collection types:
  ```csharp
  [TestMethod]
  public void When_collection_has_expected_count_then_assert_has_count()
  {
      var collection = new List<int> { 1, 2, 3 };
      Assert.HasCount(3, collection);
  }
  ```
- Use `Assert.Contains` for checking if a collection contains a specific item:
  ```csharp
  [TestMethod]
  public void When_collection_contains_item_then_assert_contains()
  {
      var collection = new List<int> { 1, 2, 3 };
      Assert.Contains(2, collection);
  }
  ```
-	Use `[TestMethod]` and `[DataRow]` for parameterized tests when applicable.
    ```csharp
    [TestMethod]
    [DataRow(1, 2, 3)]
    [DataRow(4, 5, 9)]
    public void When_adding_numbers_Add_returns_sum(int a, int b, int expectedSum)
    {
        // Arrange
        var calculator = new Calculator();
        // Act
        var result = calculator.Add(a, b);
        // Assert
        Assert.AreEqual(expectedSum, result);
    }
    ```
## Mocking and Dependencies

**Prefer real, concrete instances over mocks whenever possible. Use mocking frameworks like Moq only when absolutely necessary.**

### **General Principles:**

1. **Real Implementations First**: Always prefer testing with real concrete implementations when they are available and practical
2. **Mock Only When Necessary**: Use mocks only for external dependencies, abstract classes, or when real implementations are impractical
3. **Test Behavior, Not Implementation**: Focus tests on observable behavior rather than internal implementation details
4. **Avoid Over-Mocking**: Over-mocking leads to brittle tests that break when refactoring and don't catch real integration issues

### **When to Use Real Instances:**

✅ **Always use real instances for:**
- **Value objects**: DTOs, configuration objects, request/response models
- **Pure functions**: Stateless utility classes and helper methods
- **Simple dependencies**: Classes with no external dependencies
- **Domain objects**: Entities and domain models
- **In-memory implementations**: Collections, caches, and data structures
- **Concrete service classes**: When they don't depend on external systems

### **When to Use Mocks (Moq):**

⚠️ **Only use mocks for:**
- **External dependencies**: Database connections, file system access, network calls, HTTP clients
- **Abstract classes or interfaces**: When no suitable concrete implementation exists for testing
- **Third-party services**: Payment gateways, email services, cloud services
- **Time-dependent code**: When you need to control system time or dates
- **Complex setup**: When creating a real instance requires extensive configuration or resources
- **Non-deterministic behavior**: Random number generators, GUIDs, external data sources

### **Examples:**

#### **Good: Using Real Instances**

```csharp
[ExcludeFromCodeCoverage]
[TestClass]
public class OrderValidatorTests
{
  private OrderValidator _sut;

  [TestInitialize]
  public void TestInitialize()
  {
    // Good: Using real validator instance - no need to mock
    _sut = new OrderValidator();
  }

  [TestMethod]
  public void When_order_is_valid_Validate_returns_true()
  {
    // Arrange
    Order order = new Order
    {
      Id = 1,
      CustomerId = 100,
      Amount = 99.99m,
      Items = new List<OrderItem>
      {
        new OrderItem { ProductId = "P1", Quantity = 2, Price = 49.99m }
      }
    };

    // Act
    bool result = _sut.Validate(order);

    // Assert
    Assert.IsTrue(result);
  }

  [TestMethod]
  public void When_order_amount_is_negative_Validate_returns_false()
  {
    // Arrange
    Order order = new Order
    {
      Id = 1,
      CustomerId = 100,
      Amount = -10.00m
    };

    // Act
    bool result = _sut.Validate(order);

    // Assert
    Assert.IsFalse(result);
  }
}
```

#### **Good: Using Real In-Memory Implementations**

```csharp
[ExcludeFromCodeCoverage]
[TestClass]
public class OrderServiceTests
{
  private OrderService _sut;
  private InMemoryOrderRepository _repository;

  [TestInitialize]
  public void TestInitialize()
  {
    // Good: Using real in-memory repository instead of mocking
    _repository = new InMemoryOrderRepository();
    _sut = new OrderService(_repository);
  }

  [TestMethod]
  public void When_order_is_saved_GetOrder_returns_saved_order()
  {
    // Arrange
    Order order = new Order { Id = 1, CustomerId = 100, Amount = 99.99m };
    _sut.SaveOrder(order);

    // Act
    Order result = _sut.GetOrder(1);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(1, result.Id);
    Assert.AreEqual(100, result.CustomerId);
  }
}

// Supporting test implementation
[ExcludeFromCodeCoverage]
public class InMemoryOrderRepository : IOrderRepository
{
  private readonly Dictionary<int, Order> _orders = new Dictionary<int, Order>();

  public void Add(Order order) => _orders[order.Id] = order;

  public Order GetById(int id) => _orders.TryGetValue(id, out Order order) ? order : null;

  public bool Update(Order order)
  {
    if (_orders.ContainsKey(order.Id))
    {
      _orders[order.Id] = order;
      return true;
    }

    return false;
  }

  public void Delete(int id) => _orders.Remove(id);
}
```

#### **Appropriate: Using Mocks for External Dependencies**

```csharp
[ExcludeFromCodeCoverage]
[TestClass]
public class OrderServiceTests
{
  private Mock<IEmailService> _mockEmailService;
  private Mock<IPaymentGateway> _mockPaymentGateway;
  private OrderService _sut;

  [TestInitialize]
  public void TestInitialize()
  {
    // Appropriate: Mocking external services that would send real emails or charge real cards
    _mockEmailService = new Mock<IEmailService>();
    _mockPaymentGateway = new Mock<IPaymentGateway>();
    _sut = new OrderService(_mockEmailService.Object, _mockPaymentGateway.Object);
  }

  [TestMethod]
  public void When_order_is_placed_ProcessOrder_sends_confirmation_email()
  {
    // Arrange
    Order order = new Order { Id = 1, CustomerId = 100, Amount = 99.99m };
    _mockPaymentGateway.Setup((x) => x.Charge(It.IsAny<decimal>())).Returns(true);

    // Act
    _sut.ProcessOrder(order);

    // Assert
    _mockEmailService.Verify((x) => x.SendEmail(
      It.IsAny<string>(), 
      It.Is<string>((s) => s.Contains("confirmation"))), 
      Times.Once);
  }
}
```

#### **Avoid: Over-Mocking Simple Dependencies**

```csharp
// Avoid: Unnecessary mocking of simple dependencies
[ExcludeFromCodeCoverage]
[TestClass]
public class ShoppingCartTests
{
  private Mock<IList<CartItem>> _mockItems; // ❌ Bad: Mocking a collection
  private Mock<IPriceCalculator> _mockCalculator; // ❌ Bad: Mocking pure function
  private ShoppingCart _sut;

  [TestInitialize]
  public void TestInitialize()
  {
    _mockItems = new Mock<IList<CartItem>>();
    _mockCalculator = new Mock<IPriceCalculator>();
    _sut = new ShoppingCart(_mockItems.Object, _mockCalculator.Object);
  }
}

// Good: Using real instances
[ExcludeFromCodeCoverage]
[TestClass]
public class ShoppingCartTests
{
  private ShoppingCart _sut;

  [TestInitialize]
  public void TestInitialize()
  {
    // ✅ Good: Using real list and calculator
    List<CartItem> items = new List<CartItem>();
    PriceCalculator calculator = new PriceCalculator();
    _sut = new ShoppingCart(items, calculator);
  }

  [TestMethod]
  public void When_item_is_added_GetTotal_returns_correct_amount()
  {
    // Arrange
    CartItem item = new CartItem { ProductId = "P1", Quantity = 2, Price = 10.00m };

    // Act
    _sut.AddItem(item);
    decimal total = _sut.GetTotal();

    // Assert
    Assert.AreEqual(20.00m, total);
  }
}
```

### **Creating Test Implementations:**

When you need a concrete implementation for testing, create lightweight test-specific implementations:

```csharp
// Production interface
public interface IOrderRepository
{
  void Add(Order order);
  Order GetById(int id);
  bool Update(Order order);
  void Delete(int id);
}

// Test implementation - lightweight, in-memory, deterministic
[ExcludeFromCodeCoverage]
public class InMemoryOrderRepository : IOrderRepository
{
  private readonly Dictionary<int, Order> _storage = new Dictionary<int, Order>();

  public void Add(Order order) => _storage[order.Id] = order;

  public Order GetById(int id) => _storage.TryGetValue(id, out Order order) ? order : null;

  public bool Update(Order order)
  {
    if (!_storage.ContainsKey(order.Id))
    {
      return false;
    }

    _storage[order.Id] = order;
    return true;
  }

  public void Delete(int id) => _storage.Remove(id);

  // Test-specific helper methods
  public void Clear() => _storage.Clear();

  public int Count => _storage.Count;
}
```

### **Decision Tree: Real Instance vs. Mock**

```plaintext
┌───────────────────────────────────┐
│ Does the dependency interact with │
│ external systems (DB, API, file)? │
└─────────────────┬─────────────────┘
                  │
     ┌────────────┴───────────┐
     │ YES                    │ NO
     ▼                        ▼
 Use Mock    ┌──────────────────────────────────┐
             │ Is it an abstract class or       │
             │ interface with no concrete impl? │
             └────────────────┬─────────────────┘
                              │
                  ┌───────────┴──────────┐
                  │ YES                  │ NO
                  ▼                      ▼
            Use Mock or           Use Real Instance
            Create Test Impl
```

### **Benefits of Using Real Instances:**

1. **Better Integration Testing**: Tests verify actual behavior, not just mocked expectations
2. **Refactoring Safety**: Tests remain valid when internal implementations change
3. **Simpler Tests**: Less setup code, no mock configuration, easier to read
4. **Catch Real Bugs**: Tests can find issues that mocks would hide
5. **Documentation**: Tests show how real objects interact

### **Mock Configuration Best Practices:**

When you must use mocks, follow these practices:

```csharp
[TestMethod]
public void When_payment_fails_ProcessOrder_throws_PaymentException()
{
  // Arrange
  Order order = new Order { Id = 1, Amount = 99.99m };
  
  // ✅ Good: Clear, explicit mock setup
  _mockPaymentGateway
    .Setup((x) => x.Charge(It.Is<decimal>((amt) => amt == 99.99m)))
    .Returns(false);

  // Act & Assert
  _ = Assert.ThrowsExactly<PaymentException>(() => _sut.ProcessOrder(order));
  
  // ✅ Good: Verify specific interactions
  _mockPaymentGateway.Verify((x) => x.Charge(99.99m), Times.Once);
  
  // ✅ Good: Verify no unexpected calls
  _mockEmailService.Verify((x) => x.SendEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
}
```

### **Anti-Patterns to Avoid:**

❌ **Don't mock everything "just in case"**
```csharp
// Bad: Unnecessary mocking
private Mock<ILogger> _mockLogger;
private Mock<IConfiguration> _mockConfig;
private Mock<DateTime> _mockDateTime; // Can't even mock DateTime!
```

❌ **Don't mock value objects or DTOs**
```csharp
// Bad: Mocking a simple DTO
Mock<OrderRequest> mockRequest = new Mock<OrderRequest>();
mockRequest.Setup((x) => x.OrderId).Returns(123);

// Good: Use real instance
OrderRequest request = new OrderRequest { OrderId = 123 };
```

❌ **Don't create complex mock setups**
```csharp
// Bad: Overly complex mock configuration
_mockRepo.Setup((x) => x.GetOrders(It.IsAny<int>()))
  .Returns((int customerId) => new List<Order>
  {
    new Order { Id = 1, CustomerId = customerId },
    new Order { Id = 2, CustomerId = customerId }
  })
  .Callback((int id) => Console.WriteLine($"Getting orders for {id}"));

// Good: Use real in-memory repository
```

### **Migration Strategy:**

If you have existing tests with excessive mocking:

1. **Identify Over-Mocked Tests**: Look for tests mocking value objects, simple classes, or collections
2. **Create Test Implementations**: Build lightweight in-memory or fake implementations
3. **Refactor Gradually**: Replace mocks one test at a time
4. **Verify Behavior**: Ensure tests still verify the same behavior with real instances

## Error and Exception Testing
- Always use `Assert.ThrowsExactly<T>` when testing for Exceptions:
```csharp
// Good: Uses ThrowsExactly
[TestMethod]
public void When_invalid_input_DoSomething_throws_ArgumentException()
{
    // Arrange
    var sut = new MyService();
    
    // Act & Assert
    _ = Assert.ThrowsExactly<ArgumentException>(() => sut.DoSomething(null));
}

// For async methods that throw exceptions
[TestMethod]
public void When_invalid_data_ProcessAsync_throws_ValidationException()
{
    // Arrange
    var service = new OrderService();
    
    // Act & Assert
    var aggregateException = Assert.ThrowsExactly<AggregateException>(() => service.ProcessAsync(null).Wait());
    
    Assert.IsInstanceOfType(aggregateException.InnerException, typeof(ValidationException));
}
```
- Do **not** use `Assert.ThrowsException<T>` or any other exception assertion method.

## Asynchronous Method Tests

- Do not create `async` test methods.
```csharp
// Preferred: Synchronous test with Task.Run
[TestMethod]
public void When_valid_data_ProcessAsync_completes_successfully()
{
    // Arrange
    var service = new OrderService();
    var order = CreateValidOrder();
    
    // Act
    var task = Task.Run(() => service.ProcessAsync(order));
    var result = task.Result;
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
}

// Alternative: Using GetAwaiter().GetResult() for better exception handling
[TestMethod]
public void When_valid_data_ProcessAsync_returns_expected_result()
{
    // Arrange
    var service = new OrderService();
    var order = CreateValidOrder();
    
    // Act
    var result = service.ProcessAsync(order).GetAwaiter().GetResult();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.IsTrue(result.IsSuccess);
}

// For testing exceptions in async methods
[TestMethod]
public void When_invalid_data_ProcessAsync_throws_ValidationException()
{
    // Arrange
    var service = new OrderService();
    
    // Act & Assert
    Assert.ThrowsExactly<ValidationException>(() => service.ProcessAsync(null).GetAwaiter().GetResult());
}
```
- Only in unit tests, use `Task.Run` to run asynchronous code synchronously when you need to test the task itself:
```csharp
[TestMethod]
public void When_async_method_is_called_then_task_completes_successfully()
{
    // Arrange
    var service = new OrderService();
    
    // Act
    var task = Task.Run(() => service.AsyncMethod());
    var result = task.Result;
    
    // Assert
    Assert.AreEqual(expected, result);
    Assert.IsTrue(task.IsCompletedSuccessfully);
}
```
- Always use overloads, when they exist, that accept `CancellationToken` from the `TestContext` when calling asynchronous methods in tests to avoid unintended cancellations:
```csharp
[TestMethod]
public void When_async_method_is_called_with_cancellation_token_then_task_completes_successfully()
{
    // Arrange
    var service = new OrderService();
    var cancellationToken = TestContext.CancellationToken;
    
    // Act
    var task = Task.Run(() => service.AsyncMethod(cancellationToken), cancellationToken);
    var result = task.Result;
    
    // Assert
    Assert.AreEqual(expected, result);
    Assert.IsTrue(task.IsCompletedSuccessfully);
}
```

## Related Guidelines

This document should be used in conjunction with the following guidelines:
- [Coding Guidelines](./coding-guidelines.instructions.md)
- [Coding Styles](./coding-style.instructions.md)
- [Editor Config](../../.editorconfig)

In case of conflicts, the `.editorconfig` file takes precedence for formatting rules.
