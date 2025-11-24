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
┌─────────────────────────────────────┐
│ Does the dependency interact with   │
│ external systems (DB, API, file)?   │
└─────────────────┬───────────────────┘
                  │
        ┌─────────┴─────────┐
        │ YES               │ NO
        ▼                   ▼
    Use Mock          ┌─────────────────────────────────┐
                      │ Is it an abstract class or      │
                      │ interface with no concrete impl?│
                      └─────────────┬───────────────────┘
                                    │
                          ┌─────────┴─────────┐
                          │ YES               │ NO
                          ▼                   ▼
                    Use Mock or          Use Real Instance
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
