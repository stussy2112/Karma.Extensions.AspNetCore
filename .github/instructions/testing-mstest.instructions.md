---
description: "This file provides guidelines for writing effective, maintainable tests using MSTest and related tools"
applyTo: "tests/**/*.cs"
---

# MSTest Testing Guidelines

## Table of Contents

1. [Role Definition](#role-definition)
2. [General](#general)
   - [Description](#description)
   - [Requirements](#requirements)
3. [Naming Conventions](#naming-conventions)
4. [Test Structure](#test-structure)
   - [File Organization](#file-organization)
   - [Class Organization](#class-organization)
   - [Method Organization](#method-organization)
   - [Setup and Teardown](#setup-and-teardown)
   - [Test Method Structure](#test-method-structure)
5. [Test Visibility](#test-visibility)
6. [Error and Exception Testing](#error-and-exception-testing)
7. [Asynchronous Method Tests](#asynchronous-method-tests)
8. [Related Guidelines](#related-guidelines)

## Role Definition

- Test Engineer
- Quality Assurance Specialist

## General:

**Description:**
Tests should be reliable, maintainable, and provide meaningful coverage. Use MSTest as the primary testing framework, with proper isolation and clear patterns for test organization and execution.

### **Requirements:**
- Always use MSTest as the testing framework, unless the existing codebase uses a different framework.
- Ensure test isolation
- Follow consistent patterns
- Maintain high code coverage
- All tests should be deterministic and repeatable.
- Avoid rewriting the entire test class when creating or updating tests; instead, only provide the new or updated tests.
- **MANDATORY REQUIREMENT**: All tests **must** contain assertions to verify the expected behavior of the code under test.
- Do not create `async` test methods
- Use `MSTest` for unit testing.
- Use `Moq` for mocking dependencies in tests.
- Create test cases for both happy paths and edge cases.
- Create tests for error conditions and exceptions.
- Create tests for all public methods and properties.
- Never write tests that depend on external systems (e.g., databases, file systems, network).
- Never write tests that rely on shared state or global variables.
- Never expose members that are not part of the public API for the purpose of testing.
- Always decorate test class with `[ExcludeFromCodeCoverage]` attribute
- Always decorate classes created for testing with `[ExcludeFromCodeCoverage]` attribute to exclude them from code coverage analysis:
    ```csharp
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class OrderServiceTests
    {
        // Test methods
    }
    ```
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
        _ = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            sut.DoSomething(null);
        });
    }

    // For async methods that throw exceptions
    [TestMethod]
    public void When_invalid_data_ProcessAsync_throws_ValidationException()
    {
        // Arrange
        var service = new OrderService();
    
        // Act & Assert
        var aggregateException = Assert.ThrowsExactly<AggregateException>(() =>
        {
            Task.Run(() => service.ProcessAsync(null)).Wait();
        });
    
        Assert.IsInstanceOfType(aggregateException.InnerException, typeof(ValidationException));
    }
  ```
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
-	Use `[DataTestMethod]` and `[DataRow]` for parameterized tests when applicable.
    ```csharp
    [DataTestMethod]
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
    _ = Assert.ThrowsExactly<ArgumentException>(() =>
    {
        sut.DoSomething(null);
    });
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
    Assert.ThrowsExactly<ValidationException>(() =>
    {
        service.ProcessAsync(null).GetAwaiter().GetResult();
    });
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

## Related Guidelines

This document should be used in conjunction with the following guidelines:
- [Coding Guidelines](./coding-guidelines.instructions.md)
- [Coding Styles](./coding-style.instructions.md)
- [Editor Config](../../.editorconfig)

In case of conflicts, the `.editorconfig` file takes precedence for formatting rules.
