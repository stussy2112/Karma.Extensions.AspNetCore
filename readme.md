# Karma.Extensions.AspNetCore

[![Build Status](https://dev.azure.com/stussy2112/Web%20Utilities/_apis/build/status/Karma.Extensions.AspNetCore?branchName=main)](https://dev.azure.com/stussy2112/Web%20Utilities/_build/latest?definitionId=3&branchName=main)

[![Build Status](https://github.com/stussy2112/Karma.Extensions.AspNetCore/actions/workflows/build.yaml/badge.svg)](https://github.com/stussy2112/Karma.Extensions.AspNetCore/actions/workflows/build.yaml)

A powerful ASP.NET Core library that provides automatic model binding for filtering, sorting, and pagination query string parameters. This library simplifies building REST APIs with advanced querying capabilities by parsing complex query strings into strongly-typed objects.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage Approaches](#usage-approaches)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
  - [PageInfo](#pageinfo)
  - [FilterInfo and FilterInfoCollection](#filterinfo-and-filterinfocollection)
  - [SortInfo](#sortinfo)
  - [Operators](#operators)
- [Configuration](#configuration)
  - [Basic Setup](#basic-setup)
  - [Custom Configuration](#custom-configuration)
- [Usage Examples](#usage-examples)
  - [Pagination](#pagination)
  - [Filtering](#filtering)
  - [Sorting](#sorting)
  - [Combined Operations](#combined-operations)
- [Query String Syntax](#query-string-syntax)
  - [Filter Syntax](#filter-syntax)
  - [Sort Syntax](#sort-syntax)
  - [Page Syntax](#page-syntax)
- [API Reference](#api-reference)
- [License](#license)

## Features

- ✅ **Automatic Query String Parsing** - Converts query string parameters into strongly-typed objects
- ✅ **Advanced Filtering** - Supports 18+ comparison operators (eq, ne, lt, gt, contains, regex, etc.)
- ✅ **Flexible Sorting** - Multi-field sorting with ascending/descending directions
- ✅ **Multiple Pagination Strategies** - Offset-based and cursor-based pagination support
- ✅ **Logical Grouping** - Filter conditions with AND/OR conjunctions
- ✅ **Type Safety** - Immutable records with compile-time type checking
- ✅ **Extensible** - Custom pattern providers and parsing strategies
- ✅ **LINQ Integration** - Extension methods for applying operations to IEnumerable<T>
- ✅ **Expression Tree Building** - Compiled expression generation for optimal performance with Entity Framework Core

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Karma.Extensions.AspNetCore
```

Or via the Package Manager Console:

```powershell
Install-Package Karma.Extensions.AspNetCore
```

**Supported Frameworks:**
- .NET 8.0 (library target). Tests and samples in the repository may target other TFMs such as .NET 10.0.

## Usage Approaches

This library supports **automatic model binding** through ASP.NET Core's model binding infrastructure. Query string parameters are automatically parsed and bound to strongly-typed parameters in your controller actions.

### Model Binding Approach (Recommended)

The **model binding approach** uses ASP.NET Core's built-in model binding to automatically convert query string parameters into strongly-typed objects (`FilterInfoCollection`, `SortInfo`, `PageInfo`) that are passed directly to your controller action parameters.

**Benefits:**
- ✅ Clean, declarative syntax
- ✅ Automatic parameter binding
- ✅ Type-safe controller signatures
- ✅ Works with `[FromQuery]` attribute
- ✅ Integrates with ASP.NET Core validation

**Setup:**
```csharp
// Configure in Program.cs
builder.Services.AddControllers()
    .AddQueryStringInfoParameterBinding();
```

**Controller Usage:**
```csharp
[HttpGet]
public IActionResult GetProducts(
    [FromQuery(Name = "filter")] FilterInfoCollection? filters = null,
    [FromQuery(Name = "sort")] IEnumerable<SortInfo>? sortInfos = null,
    [FromQuery(Name = "page")] PageInfo? pageInfo = null)
{
    var products = _repository.GetAll();
    return Ok(products.Filter(filters).Sort(sortInfos).Page(pageInfo));
}
```

This approach is **recommended** for most applications as it provides the cleanest separation of concerns and leverages ASP.NET Core's built-in dependency injection and model binding features.

### Extension Method Approach

Once you have the strongly-typed objects (from model binding), use the **extension methods** to apply operations to your collections:

```csharp
// Apply operations using extension methods
IEnumerable<Product> products = _repository.GetAll();

// Option 1: Explicit calls with null-coalescing
var filtered = filters?.Apply(products) ?? products;
var sorted = sortInfos?.Apply(filtered) ?? filtered;
var paged = pageInfo?.Apply(sorted) ?? sorted;

// Option 2: Fluent chaining (shorter syntax)
var result = products
    .Filter(filters)      // Returns products if filters is null
    .Sort(sortInfos)    // Returns input if sortInfos is null
    .Page(pageInfo);    // Returns input if pageInfo is null
```

The extension methods are designed to handle `null` values gracefully, so you can safely chain operations without checking for null at each step.

## Quick Start

### 1. Configure Services

In your `Program.cs` or `Startup.cs`:

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Register model binders and value providers for automatic parameter binding
builder.Services.AddControllers()
    .AddQueryStringInfoParameterBinding();

var app = builder.Build();
app.MapControllers();
app.Run();
```

**What `AddQueryStringInfoParameterBinding()` registers:**
- `IParseStrategy<FilterInfoCollection>` (singleton service)
- `IParseStrategy<PageInfo>` (singleton service)
- `CompleteKeyedQueryStringValueProviderFactory` for filter and page parameters
- `DelimitedQueryStringValueProviderFactory` for sort parameters
- `FilterInfoModelBinderProvider` for automatic `FilterInfoCollection` binding
- `PageInfoModelBinderProvider` for automatic `PageInfo` binding
- `SortInfoModelBinderProvider` for automatic `IEnumerable<SortInfo>` binding

### 2. Create a Controller

```csharp
using Karma.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
        [FromQuery(Name = "filter")] FilterInfoCollection? filters = null,
        [FromQuery(Name = "sort")] IEnumerable<SortInfo>? sortInfos = null,
        [FromQuery(Name = "page")] PageInfo? pageInfo = null)
    {
        IEnumerable<Product> products = await _repository.GetAllAsync();

        // Apply filters
        var filtered = filters?.Apply(products) ?? products;
        
        // Apply sorting
        var sorted = sortInfos?.Apply(filtered) ?? filtered;
        
        // Apply pagination
        var paged = pageInfo?.Apply(sorted) ?? sorted;

        return Ok(paged);
    }
}
```

**Working Sample:** See [WeatherForecastController.cs](Samples/Karma.Extensions.AspNetCore.Samples.WebApi/Controllers/WeatherForecastController.cs) for a complete example including advanced filter groups and OR operations.

**Alternative: Entity Framework Core**

```csharp
using Karma.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
        [FromQuery(Name = "filter")] FilterInfoCollection? filters = null,
        [FromQuery(Name = "sort")] IEnumerable<SortInfo>? sortInfos = null,
        [FromQuery(Name = "page")] PageInfo? pageInfo = null,
        CancellationToken cancellationToken = default)
    {
        // Build query - note the Apply() pattern
        IQueryable<Product> query = _context.Products.Filter(filters);
        query = sortInfos?.Apply(query) ?? query;
        query = pageInfo?.Apply(query) ?? query;

        // Execute query at database level
        var results = await query.ToListAsync(cancellationToken);
        return Ok(results);
    }
}
```

### 3. Make API Calls

```http
GET /api/products?filter[name][$contains]=laptop&sort=-price&page[offset]=0&page[limit]=10
```

## Core Concepts

### PageInfo

Represents pagination information with support for both offset-based and cursor-based pagination.

**Properties:**
- `Offset` (uint) - Zero-based index of the first item (default: 0)
- `Limit` (uint) - Maximum number of items to return (default: uint.MaxValue)
- `After` (string) - Cursor for items after this position
- `Before` (string) - Cursor for items before this position

**Constructors:**
```csharp
// Offset-based pagination
var pageInfo = new PageInfo(offset: 20, limit: 10);

// Cursor-based pagination
var pageInfo = new PageInfo(after: "cursor123", limit: 10);

// Default constructor
var pageInfo = new PageInfo(); // Returns all items
```

> Note: the `PageInfo(string after, uint limit = ...)` constructor validates `after` and will throw an `ArgumentException` if `after` is null or whitespace. `Limit` defaults to `uint.MaxValue` when not specified.

### FilterInfo and FilterInfoCollection

`FilterInfo` represents a single filter condition, while `FilterInfoCollection` groups multiple filters with logical conjunctions.

**FilterInfo Properties:**
- `Name` (string) - Filter identifier
- `Path` (string) - Property path to filter on
- `Operator` (Operator) - Comparison operator
- `Value` (IReadOnlyCollection&lt;object&gt;) - Values to compare against
- `MemberOf` (string) - Parent group name

**FilterInfoCollection Properties:**
- `Name` (string) - Collection identifier
- `Conjunction` (Conjunction) - AND or OR logical operation
- `Count` (int) - Number of filters in collection
- `MemberOf` (string) - Parent group name

**Examples:**
```csharp
// Single filter
var filter = new FilterInfo("price", "Price", Operator.GreaterThan, 100);

// Filter collection with AND conjunction
var filters = new FilterInfoCollection("products", Conjunction.And, new[]
{
    new FilterInfo("price", "Price", Operator.GreaterThan, 100),
    new FilterInfo("category", "Category", Operator.EqualTo, "Electronics")
});
```

### SortInfo

Represents sorting information for a single field.

**Properties:**
- `FieldName` (string) - The field to sort by
- `Direction` (ListSortDirection) - Ascending or Descending
- `OriginalFieldName` (string) - Original field name from query string

**Constructors:**
```csharp
using System.ComponentModel;

// Ascending sort
var sort = new SortInfo("Name", ListSortDirection.Ascending);

// Descending sort (using prefix)
var sort = new SortInfo("-Price");

// Implicit conversion from string
SortInfo sort = "Name";
SortInfo sort = "-Price"; // Descending
```

> Note: `SortInfo` constructor validates the provided field name and will throw `ArgumentException` for invalid inputs (for example, a string that is just `"-"`). The `SortInfoModelBinder`/binder logic will skip invalid sort entries when parsing request data.

### Operators

The library supports 18 comparison operators:

| Operator | Enum Value | Query String | Description |
|----------|-----------|--------------|-------------|
| Equal To | `EqualTo` | `eq` | Equality comparison |
| Not Equal To | `NotEqualTo` | `ne` | Inequality comparison |
| Less Than | `LessThan` | `lt` | Less than comparison |
| Less Than Or Equal | `LessThanOrEqualTo` | `le`, `lte` | Less than or equal comparison |
| Greater Than | `GreaterThan` | `gt` | Greater than comparison |
| Greater Than Or Equal | `GreaterThanOrEqualTo` | `ge`, `gte` | Greater than or equal comparison |
| Between | `Between` | `between` | Inclusive range check |
| Not Between | `NotBetween` | `notbetween` | Exclusive range check |
| Contains | `Contains` | `contains` | String contains check |
| Not Contains | `NotContains` | `notcontains` | String does not contain |
| Starts With | `StartsWith` | `startswith` | String starts with check |
| Ends With | `EndsWith` | `endswith` | String ends with check |
| In | `In` | `in` | Membership in set |
| Not In | `NotIn` | `notin` | Not in set |
| Is Null | `IsNull` | `null` | Null check |
| Is Not Null | `IsNotNull` | `notnull` | Not null check |
| Regex | `Regex` | `regex` | Regular expression match |

## Configuration

### Basic Setup

The simplest configuration binds all three parameter types with default settings:

```csharp
builder.Services.AddControllers()
    .AddQueryStringInfoParameterBinding();
```

This registers model binders and value providers that automatically bind query string parameters to strongly-typed objects in your controller actions.

### Custom Configuration

Customize parameter names and parsing behavior:

```csharp
builder.Services.AddControllers()
    .AddQueryStringInfoParameterBinding(options =>
    {
        // Customize filter parameter
        options.FilterBindingOptions.ParameterKey = "f";
        
        // Customize sort parameter
        options.SortBindingOptions.ParameterKey = "s";
        
        // Customize page parameter
        options.PageBindingOptions.ParameterKey = "p";
    });
```

### Individual Parameter Binding

Configure only specific parameter types:

```csharp
var mvcBuilder = builder.Services.AddControllers();

// Add only filtering
mvcBuilder.AddFilterInfoParameterBinding("filter");

// Add only pagination
mvcBuilder.AddPagingInfoParameterBinding("page");

// Add only sorting
mvcBuilder.AddSortInfoParameterBinding("sort");
```

## Usage Examples

### Pagination

#### Offset-Based Pagination

```csharp
[HttpGet]
public IActionResult GetProducts([FromQuery(Name = "page")] PageInfo? pageInfo = null)
{
    var products = _repository.GetAll();
    var paged = pageInfo?.Apply(products) ?? products;
    return Ok(paged);
}
```

**Query Examples:**
```http
GET /api/products?page[offset]=0&page[limit]=10    # First page, 10 items
GET /api/products?page[offset]=10&page[limit]=10   # Second page, 10 items
GET /api/products?page[limit]=5                    # First 5 items
```

#### Cursor-Based Pagination

```csharp
[HttpGet]
public IActionResult GetProducts([FromQuery(Name = "page")] PageInfo? pageInfo = null)
{
    var products = _repository.GetAll();
    var paged = pageInfo?.Apply(products) ?? products;
    return Ok(paged);
}
```

**Query Examples:**
```http
GET /api/products?page[after]=cursor123&page[limit]=10
GET /api/products?page[before]=cursor456&page[limit]=10
```

> Note: Cursor-based overloads that accept a cursor property require the cursor value type to implement `IComparable<T>` and `IParsable<T>` (e.g., `int`, `Guid`, `DateTime`). Example usage (IEnumerable):
```csharp
var cursorPaged = pageInfo?.Apply(products, p => p.Id) ?? products;
```

### Filtering

#### Single Filter

```csharp
[HttpGet]
public IActionResult GetProducts(
    [FromQuery(Name = "filter")] FilterInfoCollection? filters = null)
{
    var products = _repository.GetAll();
    var filtered = filters?.Apply(products) ?? products;
    return Ok(filtered);
}
```

**Query Examples:**
```http
# Equal to
GET /api/products?filter[category][$eq]=Electronics
GET /api/products?filter[category]=Electronics      // Defaults to $eq

# Greater than
GET /api/products?filter[price][$gt]=100

# Contains (case-sensitive)
GET /api/products?filter[name][$contains]=laptop

# Between
GET /api/products?filter[price][$between]=100,500

# In set
GET /api/products?filter[status][$in]=active,pending

# Is null
GET /api/products?filter[deletedAt][$null]=

# Regular expression
GET /api/products?filter[sku][$regex]=^PRD-\d{4}$
```

#### Multiple Filters with AND

By default, multiple filters are combined with AND logic:

```http
GET /api/products?filter[category][$eq]=Electronics&filter[price][$gt]=100&filter[inStock][$eq]=true
```

This returns products where:
- Category equals "Electronics" AND
- Price is greater than 100 AND
- InStock equals true

#### Multiple Filters with OR

For simple OR conditions on the same field, use the `$in` operator:

```http
GET /api/products?filter[category][$in]=Electronics,Computers
```

For complex conditions, use filter groups with `$or`:

```http
filter[group][$or]=search&filter[search][0][name][$contains]=laptop&filter[search][1][sku][$contains]=laptop
```

### Sorting

#### Single Field Sort

```csharp
[HttpGet]
public IActionResult GetProducts(
    [FromQuery(Name = "sort")] IEnumerable<SortInfo>? sortInfos = null)
{
    var products = _repository.GetAll();
    var sorted = sortInfos?.Apply(products) ?? products;
    return Ok(sorted);
}
```

**Query Examples:**
```http
# Ascending sort
GET /api/products?sort=name

# Descending sort (using minus prefix)
GET /api/products?sort=-price
```

#### Multi-Field Sort

Sort by multiple fields in priority order:

```http
# Sort by category ascending, then price descending
GET /api/products?sort=category,-price

# Sort by multiple fields
GET /api/products?sort=inStock,-rating,-price
```

**How it works:**
- Fields are sorted in the order they appear
- Use `-` prefix for descending order
- No prefix means ascending order
- First field has highest priority

**Examples:**
```http
# E-commerce product listing
GET /api/products?sort=-featured,category,name
# Priority: featured DESC, then category ASC, then name ASC

# User management
GET /api/users?sort=-isActive,-lastLogin,lastName,firstName
# Active users first, then by most recent login, then alphabetically

# Order processing
GET /api/orders?sort=-priority,createdAt
# High priority orders first, then oldest first
```

### Combined Operations

Combine filtering, sorting, and pagination in a single request:

```csharp
[HttpGet]
public IActionResult GetProducts(
    [FromQuery(Name = "filter")] FilterInfoCollection? filters = null,
    [FromQuery(Name = "sort")] IEnumerable<SortInfo>? sortInfos = null,
    [FromQuery(Name = "page")] PageInfo? pageInfo = null)
{
    IEnumerable<Product> products = _repository.GetAll();

    // IMPORTANT: Apply operations in this order for best results
    var filtered = filters?.Apply(products) ?? products;
    var sorted = sortInfos?.Apply(filtered) ?? filtered;
    var paged = pageInfo?.Apply(sorted) ?? sorted;

    return Ok(paged);
}
```

**Complex Query Example:**
```http
GET /api/products?filter[family.name][$eq]=laptop&filter[specs.ram][$gte]=8&filter[specs.storage][$lte]=512&sort=-price&page[offset]=0&page[limit]=10
```

This query:
1. Filters products where the family name is "laptop"
2. Filter results where RAM is greater than or equal to 8
3. Filter results where storage is less than or equal to 512
4. Sorts the results by price in descending order
5. Returns the first 10 results

**Real-World Examples:**

```http
# Product search with filters, sort, and pagination
GET /api/products?filter[category][$in]=Electronics,Computers&filter[price][$lt]=1000&filter[rating][$gte]=4&sort=-rating,-reviewCount&page[offset]=0&page[limit]=25

# User dashboard with status filter and sorting
GET /api/users?filter[status][$ne]=deleted&filter[lastLogin][$notnull]=&sort=-lastLogin,lastName,firstName&page[offset]=0&page[limit]=50

# Order management with date range and status
GET /api/orders?filter[createdAt][$between]=2024-01-01,2024-12-31&filter[status][$in]=pending,processing&sort=-createdAt&page[offset]=0&page[limit]=100
```

## Query String Syntax

### Filter Syntax

#### Basic Format
```
filter[fieldName][$operator]=value
```

**Note:** If no operator is specified, the filter defaults to `$eq` (equality).

#### Examples by Operator

**Equality Operators:**
```http
filter[status][$eq]=active
filter[status][$ne]=inactive
```

**Comparison Operators:**
```http
filter[price][$gt]=100
filter[price][$ge]=100
filter[price][$lt]=1000
filter[price][$le]=1000
```

**Range Operators:**
```http
filter[price][$between]=100,500
filter[date][$notbetween]=2023-01-01,2023-12-31
```

**String Operators:**
```http
filter[name][$contains]=laptop
filter[name][$notcontains]=refurbished
filter[email][$startswith]=admin
filter[filename][$endswith]=.pdf
```

**Set Membership:**
```http
filter[category][$in]=Electronics,Computers,Gaming
filter[status][$notin]=deleted,archived
```

**Null Checks:**
```http
filter[deletedAt][$null]=
filter[updatedAt][$notnull]=
```

**Pattern Matching:**
```http
filter[sku][$regex]=^[A-Z]{3}-\d{4}$
```

#### Group Syntax

Filter groups enable complex logical expressions by combining multiple filters with AND or OR conjunctions.

**Define a Group:**
```http
# OR Group
filter[group][$or]=groupName

# AND Group
filter[group][$and]=groupName
```

**Add Filters to Group:**
```http
filter[groupName][index][fieldName][$operator]=value
```

**Complete OR Group Example:**
```http
filter[group][$or]=search&filter[search][0][name][$contains]=laptop&filter[search][1][sku][$contains]=laptop
```

**Complete AND Group Example:**
```http
filter[group][$and]=premium&filter[premium][0][price][$gte]=1000&filter[premium][1][rating][$gte]=4.5&filter[premium][2][certified][$eq]=true
```

**Group Components:**
- `filter[group]` - Indicates a group definition
- `[$and]` or `[$or]` - The logical conjunction for the group
- `groupName` - Unique identifier for the group
- `[index]` - Zero-based index for each filter (0, 1, 2, ...)
- `[fieldName]` - Property name to filter on
- `[$operator]` - Comparison operator (optional, defaults to `$eq`)

**Nested Paths in Groups:**
```http
filter[group][$or]=addresses&filter[addresses][0][billing][city][$eq]=Seattle&filter[addresses][1][shipping][city][$eq]=Seattle
```

**Multiple Groups:**
```http
# Define first OR group
filter[group][$or]=activeStatus&filter[activeStatus][0][status][$eq]=active&filter[activeStatus][1][status][$eq]=pending

# Define second AND group
filter[group][$and]=verified&filter[verified][0][emailVerified][$eq]=true&filter[verified][1][phoneVerified][$eq]=true
```

### Sort Syntax

#### Basic Format
```http
sort=fieldName              # Ascending
sort=-fieldName             # Descending
sort=field1,-field2,field3  # Multiple fields
```

#### Examples
```http
# Single field ascending
sort=name

# Single field descending
sort=-createdAt

# Multiple fields
sort=category,name         # Category ASC, Name ASC
sort=priority,-createdAt   # Priority ASC, CreatedAt DESC
sort=-rating,-price,name   # Rating DESC, Price DESC, Name ASC
```

#### Sort Direction Rules
- **No prefix** = Ascending order (A→Z, 0→9, oldest→newest)
- **Minus prefix (-)** = Descending order (Z→A, 9→0, newest→oldest)

#### Multi-Field Sort Priority
Fields are sorted in left-to-right order. First field has highest priority:

```http
# Sort by category first, then by price within each category
sort=category,price

# Sort by active status (active first), then by creation date (newest first)
sort=-isActive,-createdAt
```

#### Sort Examples by Scenario

**E-Commerce Product Listing:**
```http
# Featured products first, then by rating, then by price
sort=-featured,-rating,price

# In-stock items first, then by popularity
sort=-inStock,-salesCount

# New arrivals first, then alphabetically
sort=-createdAt,name
```

**User Management:**
```http
# Active users first, then by last login, then alphabetically
sort=-isActive,-lastLogin,lastName,firstName

# Sort by role, then by email
sort=role,email
```

**Content Management:**
```http
# Published content first, then by date, then by title
sort=-isPublished,-publishedAt,title

# Priority content first, then recent
sort=-priority,-updatedAt
```

**Order Processing:**
```http
# Urgent orders first, then by date
sort=-priority,createdAt

# Sort by status, then newest first
sort=status,-createdAt
```

### Page Syntax

#### Offset-Based Pagination
```
page[offset]=<number>&page[limit]=<number>
```

**Examples:**
```http
page[offset]=0&page[limit]=10      # Page 1 (records 1-10)
page[offset]=10&page[limit]=10     # Page 2 (records 11-20)
page[offset]=20&page[limit]=10     # Page 3 (records 21-30)
```

**How to Calculate Offset:**
```
offset = (pageNumber - 1) * pageSize

# Page 1, 10 per page: offset = 0
# Page 2, 10 per page: offset = 10
# Page 5, 25 per page: offset = 100
```

#### Cursor-Based Pagination
```
page[after]=<cursor>&page[limit]=<number>
page[before]=<cursor>&page[limit]=<number>
```

**Examples:**
```http
# Get next 10 items after cursor
page[after]=eyJpZCI6MTIzfQ&page[limit]=10

# Get previous 10 items before cursor
page[before]=eyJpZCI6MTIzfQ&page[limit]=10
```

#### Pagination Parameters

| Parameter | Type | Description | Default | Example |
|-----------|------|-------------|---------|---------|
| `offset` | uint | Zero-based index of first item | 0 | `page[offset]=20` |
| `limit` | uint | Maximum number of items to return | uint.MaxValue | `page[limit]=50` |
| `after` | string | Cursor for items after this position | "" | `page[after]=cursor123` |
| `before` | string | Cursor for items before this position | "" | `page[before]=cursor456` |

#### Pagination Best Practices

**Offset-Based Pagination:**
- ✅ Simple to implement and understand
- ✅ Easy to jump to specific pages
- ❌ Performance degrades with large offsets
- ❌ Can skip or duplicate records if data changes

```http
# Standard pagination
GET /api/products?page[offset]=0&page[limit]=25
GET /api/products?page[offset]=25&page[limit]=25
GET /api/products?page[offset]=50&page[limit]=25
```

**Cursor-Based Pagination:**
- ✅ Consistent results even when data changes
- ✅ Better performance for large datasets
- ✅ No skipped or duplicated records
- ❌ Cannot jump to arbitrary pages
- ❌ Requires stateful cursor management

```http
# Forward pagination
GET /api/products?page[after]=lastItemCursor&page[limit]=25

# Backward pagination
GET /api/products?page[before]=firstItemCursor&page[limit]=25
```

#### Pagination Examples

**Basic Pagination:**
```http
# First page, 10 items
GET /api/users?page[limit]=10

# Third page, 25 items per page
GET /api/users?page[offset]=50&page[limit]=25
```

**Pagination with Filters:**
```http
GET /api/products?filter[category]=Electronics&page[offset]=0&page[limit]=20
```

**Pagination with Sorting:**
```http
GET /api/orders?sort=-createdAt&page[offset]=0&page[limit]=50
```

**Full Example (Filter + Sort + Page):**
```http
GET /api/products?filter[price][$between]=50,200&filter[inStock]=true&sort=-rating,name&page[offset]=0&page[limit]=25
```

#### Response Metadata

It's recommended to include pagination metadata in responses:

```json
{
  "data": [...],
  "pagination": {
    "offset": 0,
    "limit": 25,
    "total": 150,
    "hasMore": true,
    "nextOffset": 25
  }
}
```

### Combined Operations

Combine filtering, sorting, and pagination in a single request:

```csharp
[HttpGet]
public IActionResult GetProducts(
    [FromQuery(Name = "filter")] FilterInfoCollection? filters = null,
    [FromQuery(Name = "sort")] IEnumerable<SortInfo>? sortInfos = null,
    [FromQuery(Name = "page")] PageInfo? pageInfo = null)
{
    IEnumerable<Product> products = _repository.GetAll();

    // IMPORTANT: Apply operations in this order for best results
    var filtered = filters?.Apply(products) ?? products;
    var sorted = sortInfos?.Apply(filtered) ?? filtered;
    var paged = pageInfo?.Apply(sorted) ?? sorted;

    return Ok(paged);
}
```

## API Reference

### FilterExpressionBuilder

The `FilterExpressionBuilder` class provides methods for building LINQ expression trees from `FilterInfoCollection` objects. This is particularly useful for Entity Framework Core scenarios where you need expressions that can be translated to SQL.

```csharp
public static class FilterExpressionBuilder
{
    // Builds a LINQ expression tree that represents a filter predicate
    public static Expression<Func<T, bool>> BuildExpression<T>(IEnumerable<IFilterInfo> filters);
    
    // Builds a compiled lambda function that evaluates filters
    public static Func<T, bool> BuildLambda<T>(IEnumerable<IFilterInfo> filters);
}
```

**Usage with Entity Framework Core:**

```csharp
using Karma.Extensions.AspNetCore;
using Microsoft.EntityFrameworkCore;

[HttpGet]
public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
    [FromQuery(Name = "filter")] FilterInfoCollection? filters = null,
    [FromQuery(Name = "page")] PageInfo? pageInfo = null,
    CancellationToken cancellationToken = default)
{
    IQueryable<Product> query = _context.Products.AsQueryable();

    // Apply filters using compiled expression
    if (filters?.Any() == true)
    {
        var predicate = FilterExpressionBuilder.BuildLambda<Product>(filters);
        query = query.Where(predicate);
    }

    // Apply pagination
    if (pageInfo != null)
    {
        query = query.Skip((int)pageInfo.Offset).Take((int)pageInfo.Limit);
    }

    var results = await query.ToListAsync(cancellationToken);
    return Ok(results);
}
```

**Key Features:**
- **Type Safety** - Compile-time type checking ensures filter paths match entity properties
- **NULL Safety** - Automatically handles null property navigation with safe null checks
- **Type Conversion** - Automatically converts filter values to match property types
- **Nested Properties** - Supports dotted property paths (e.g., "Address.City")

**When you need the expression tree itself (not compiled)**
```csharp
Expression<Func<Product, bool>> filterExpression = FilterExpressionBuilder.BuildExpression<Product>(filters);

// Useful for inspection, debugging, or combining with other expressions
Console.WriteLine($"Filter expression: {filterExpression}");

// Can still be compiled later if needed
Func<Product, bool> compiled = filterExpression.Compile();
```

### Extension Methods

#### IMvcBuilder Extensions

Located in `Microsoft.AspNetCore.Builder` namespace:

```csharp
// Add all parameter bindings with default configuration
public static IMvcBuilder AddQueryStringInfoParameterBinding(
    this IMvcBuilder builder, 
    Action<QueryStringParameterBindingOptions>? configureOptions = null)

// Add filter parameter binding
public static IMvcBuilder AddFilterInfoParameterBinding(
    this IMvcBuilder builder,
    string? parameterKey = "filter")

// Add filter parameter binding with custom pattern provider
public static IMvcBuilder AddFilterInfoParameterBinding(
    this IMvcBuilder builder,
    FilterPatternProvider? filterPatternProvider,
    string? parameterKey = "filter")

// Add pagination parameter binding
public static IMvcBuilder AddPagingInfoParameterBinding(
    this IMvcBuilder builder,
    string? parameterKey = "page")

// Add pagination parameter binding with custom pattern provider
public static IMvcBuilder AddPagingInfoParameterBinding(
    this IMvcBuilder builder,
    PageInfoPatternProvider pageInfoPatternProvider,
    string? parameterKey = "page")

// Add sort parameter binding
public static IMvcBuilder AddSortInfoParameterBinding(
    this IMvcBuilder builder,
    string? parameterKey = "sort")
```

#### IEnumerable<T> Extension Methods

Apply filter, sort, and pagination operations to collections:

```csharp
// Apply filters to a collection (primary method)
public static IEnumerable<T>? Apply<T>(
    this FilterInfoCollection? filters,
    IEnumerable<T> source)

// Apply filters to a collection (alternative)
public static IEnumerable<T>? Filter<T>(
    this IEnumerable<T> source,
    FilterInfoCollection? filters)

// Apply sorting to a collection (primary method)
public static IEnumerable<T>? Apply<T>(
    this IEnumerable<SortInfo>? sortInfos,
    IEnumerable<T> source)

// Apply sorting to a collection (alternative)
public static IEnumerable<T>? Sort<T>(
    this IEnumerable<T> source,
    IEnumerable<SortInfo>? sortInfos)

// Apply pagination to a collection (primary method)
public static IEnumerable<T>? Apply<T>(
    this PageInfo? pageInfo,
    IEnumerable<T> source)

// Apply pagination to a collection (alternative)
public static IEnumerable<T>? Page<T>(
    this IEnumerable<T> source,
    PageInfo? pageInfo)

// Apply pagination with page number and page size
public static IEnumerable<T>? Page<T>(
    this IEnumerable<T>? source,
    int pageNumber,
    int pageSize)

// Apply cursor-based pagination with custom cursor property (nullable reference types)
public static IEnumerable<T>? Apply<T, TValue>(
    this PageInfo pageInfo,
    IEnumerable<T>? source,
    Func<T, TValue?> cursorProperty)
    where TValue : IComparable<TValue>, IParsable<TValue>

// Apply cursor-based pagination with custom cursor property (nullable value types)
public static IEnumerable<T>? Apply<T, TValue>(
    this PageInfo pageInfo,
    IEnumerable<T>? source,
    Func<T, TValue?> cursorProperty)
    where TValue : struct, IComparable<TValue>, IParsable<TValue>

// Apply cursor-based pagination (alternative, nullable reference types)
public static IEnumerable<T>? Page<T, TValue>(
    this IEnumerable<T>? source,
    PageInfo? pageInfo,
    Func<T, TValue?> cursorProperty)
    where TValue : IComparable<TValue>, IParsable<TValue>

// Apply cursor-based pagination (alternative, nullable value types)
public static IEnumerable<T>? Page<T, TValue>(
    this IEnumerable<T>? source,
    PageInfo? pageInfo,
    Func<T, TValue?> cursorProperty)
    where TValue : struct, IComparable<TValue>, IParsable<TValue>
```

**Null-Safety Behavior:**
- All `Apply()` and alternative extension methods handle `null` input gracefully
- When the parameter (filters, sortInfos, or pageInfo) is `null`, the original `source` enumerable is returned unchanged
- This allows safe chaining without explicit null checks

**Method Naming:**
- **`Apply()`** - Primary fluent-style methods where the operation parameter calls the method
- **`Filter()`, `Sort()`, `Page()`** - Alternative methods where the collection calls the method

**Usage Examples:**
```csharp
using Karma.Extensions.AspNetCore;

// Applying filters - both methods are equivalent
IEnumerable<Product> products = repository.GetAll();
var filtered1 = filters?.Apply(products) ?? products;
var filtered2 = products.Filter(filters);

// Applying sorting - both methods are equivalent
var sorted1 = sortInfos?.Apply(filtered1) ?? filtered1;
var sorted2 = filtered1.Sort(sortInfos);

// Applying pagination - multiple options
var paged1 = pageInfo?.Apply(sorted1) ?? sorted1;
var paged2 = sorted1.Page(pageInfo);
var paged3 = sorted1.Page(pageNumber: 1, pageSize: 10);

// Cursor-based pagination with custom property
var pagedWithCursor = pageInfo?.Apply(sorted1, p => p.Id) ?? sorted1;

// Chaining operations - null-safe fluent API
var result = products
    .Filter(filters)      // Returns products if filters is null
    .Sort(sortInfos)    // Returns input if sortInfos is null
    .Page(pageInfo);    // Returns input if pageInfo is null
```

**Cursor-Based Pagination Examples:**
```csharp
// Using integer ID as cursor
var products = repository.GetAll();
var cursorPaged = pageInfo?.Apply(products, p => p.Id) ?? products;

// Using Guid as cursor
var orders = repository.GetOrders();
var guidPaged = pageInfo?.Apply(orders, o => o.OrderId) ?? orders;

// Using DateTime as cursor
var logs = repository.GetLogs();
var datePaged = pageInfo?.Apply(logs, l => l.CreatedAt) ?? logs;
```

### Entity Framework Core Integration

For database queries with Entity Framework Core, use `IQueryable<T>` methods to ensure all operations are translated to SQL:

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<Product>>> GetProducts(
    [FromQuery(Name = "filter")] FilterInfoCollection? filters = null,
    [FromQuery(Name = "sort")] IEnumerable<SortInfo>? sortInfos = null,
    [FromQuery(Name = "page")] PageInfo? pageInfo = null,
    CancellationToken cancellationToken = default)
{
    // Build query - each operation returns IQueryable<T>
    IQueryable<Product> query = _context.Products.Filter(filters);
    query = sortInfos?.Apply(query) ?? query;
    query = pageInfo?.Apply(query) ?? query;

    // Get total count before pagination
    int totalCount = await _context.Products.Filter(filters).CountAsync(cancellationToken);

    // Materialize the query (executes SELECT query with filters, sorting, and pagination)
    List<Product> results = await query.ToListAsync(cancellationToken);

    return Ok(new PagedResult<Product>
    {
        Data = results,
        TotalCount = totalCount,
        Offset = pageInfo?.Offset ?? 0,
        Limit = pageInfo?.Limit ?? (uint)results.Count
    });
}
```

**Key Points:**
- ✅ Use `IQueryable<T>` methods to build the query
- ✅ All operations (`Filter()`, `Apply()` for sort/page) return `IQueryable<T>`
- ✅ Use the pattern: `query = operation?.Apply(query) ?? query`
- ✅ Get the count before applying pagination
- ✅ Use `CancellationToken` for async operations
- ❌ Don't convert to `IEnumerable<T>` before applying operations

**Alternative Syntax Options:**

```csharp
// Option 1: Explicit null-coalescing (recommended)
IQueryable<Product> query = _context.Products.Filter(filters);
query = sortInfos?.Apply(query) ?? query;
query = pageInfo?.Apply(query) ?? query;

// Option 2: Inline application
var query = pageInfo?.Apply(
    sortInfos?.Apply(
        _context.Products.Filter(filters)
    ) ?? _context.Products.Filter(filters)
) ?? sortInfos?.Apply(_context.Products.Filter(filters)) ?? _context.Products.Filter(filters);

// Option 3: Traditional LINQ (most verbose)
IQueryable<Product> query = _context.Products.Filter(filters);
if (sortInfos != null && sortInfos.Any())
{
    // Manual sorting with Apply
    query = sortInfos.Apply(query) ?? query;
}
if (pageInfo != null)
{
    query = pageInfo.Apply(query) ?? query;
}
```

### Core Types

#### PageInfo

```csharp
public record PageInfo
{
    // Constructors
    public PageInfo();
    public PageInfo(string after, uint limit = uint.MaxValue);
    public PageInfo(uint offset, uint limit = uint.MaxValue);

    // Properties
    public string? After { get; init; }
    public string? Before { get; init; }
    public uint Limit { get; init; }
    public uint Offset { get; init; }
}
```

#### FilterInfo

```csharp
public record FilterInfo : IFilterInfo
{
    // Constructors
    public FilterInfo(string name, string path, params object[] values);
    public FilterInfo(string name, string path, Operator @operator, params object[] values);
    public FilterInfo(string memberOf, string name, string path, Operator @operator, params object[] values);

    // Properties
    public string Name { get; }
    public string? MemberOf { get; }
    public Operator Operator { get; }
    public string? Path { get; }
    public IReadOnlyCollection<object> Values { get; }
}
```

#### FilterInfoCollection

```csharp
public sealed record FilterInfoCollection : IFilterInfo, IReadOnlyCollection<IFilterInfo>
{
    // Constructors
    public FilterInfoCollection();
    public FilterInfoCollection(string name, IEnumerable<IFilterInfo>? filters = null);
    public FilterInfoCollection(string name, Conjunction conjunction, IEnumerable<IFilterInfo>? filters = null);
    public FilterInfoCollection(string? memberOf, string name, Conjunction conjunction = Conjunction.And, IEnumerable<IFilterInfo>? filters = null);

    // Properties
    public int Count { get; }
    public Conjunction Conjunction { get; }
    public string Name { get; }
    public string? MemberOf { get; }

    // Methods
    public IEnumerator<IFilterInfo> GetEnumerator();
}
```

#### SortInfo

```csharp
public sealed record SortInfo
{
    // Constructor
    public SortInfo(string fieldName, ListSortDirection direction = ListSortDirection.Ascending);

    // Properties
    public ListSortDirection Direction { get; }
    public string FieldName { get; }
    public string OriginalFieldName { get; }

    // Operators
    public static implicit operator string(SortInfo? sortParameter);
    public static implicit operator SortInfo(string value);

    // Static Methods
    public static ICollection<SortInfo> CreateCollection(IEnumerable<SortInfo>? sortInfo = null);
    
    // Override Methods
    public override string ToString();
}
```

### Enums

#### Operator

```csharp
public enum Operator
{
    None,
    EqualTo,              // eq
    NotEqualTo,           // ne
    LessThan,             // lt
    LessThanOrEqualTo,    // le, lte
    GreaterThan,          // gt
    GreaterThanOrEqualTo, // ge, gte
    Between,              // between
    NotBetween,           // notbetween
    Contains,             // contains
    NotContains,          // notcontains
    EndsWith,             // endswith
    In,                   // in
    NotIn,                // notin
    IsNull,               // null
    IsNotNull,            // notnull
    StartsWith,           // startswith
    Regex                 // regex
}
```

#### Conjunction

```csharp
public enum Conjunction
{
    And,  // Combines filters where ALL conditions must be true
    Or    // Combines filters where ANY condition can be true
}
```

### Configuration Options

#### QueryStringParameterBindingOptions

```csharp
public class QueryStringParameterBindingOptions
{
    public FilterBindingOptions FilterBindingOptions { get; set; }
    public PageBindingOptions PageBindingOptions { get; set; }
    public SortBindingOptions SortBindingOptions { get; set; }
}
```

#### FilterBindingOptions

```csharp
public class FilterBindingOptions
{
    public string ParameterKey { get; set; } = "filter";
    public FilterPatternProvider? PatternProvider { get; set; }
}
```

#### PageBindingOptions

```csharp
public class PageBindingOptions
{
    public string ParameterKey { get; set; } = "page";
    public PageInfoPatternProvider? PatternProvider { get; set; }
}
```

#### SortBindingOptions

```csharp
public class SortBindingOptions
{
    public string ParameterKey { get; set; } = "sort";
}
```

### Pattern Providers

#### FilterPatternProvider

```csharp
public sealed partial class FilterPatternProvider : PatternProviderBase
{
    // Static Property
    public static FilterPatternProvider Default { get; }

    // Constructors
    public FilterPatternProvider();
    public FilterPatternProvider(string pattern, 
        string pathGroupName = "path",
        string valueGroupName = "value",
        string operatorGroupName = "operator",
        string typeGroupName = "type",
        string conjunctionGroupName = "conjunction",
        string groupIndexGroupName = "groupIndex",
        string memberOfGroupName = "memberOf");

    // Properties
    public string ConjunctionGroupName { get; init; }
    public string GroupIndexGroupName { get; init; }
    public string MemberOfGroupName { get; init; }
    public string OperatorGroupName { get; init; }
    public string PathGroupName { get; init; }
    public string TypeGroupName { get; init; }
    public override Regex RegularExpression { get; }
}
```

#### PageInfoPatternProvider

```csharp
public sealed partial class PageInfoPatternProvider : PatternProviderBase
{
    // Static Property
    public static PageInfoPatternProvider Default { get; }

    // Constructors
    public PageInfoPatternProvider();
    public PageInfoPatternProvider(string pattern,
        string valueGroupName = "value",
        string keyGroupName = "key");

    // Properties
    public string KeyGroupName { get; init; }
    public override Regex RegularExpression { get; }
}
```

### Interfaces

#### IFilterInfo

```csharp
public interface IFilterInfo
{
    string Name { get; }
    string? MemberOf { get; }
}
```

#### IParseStrategy<T>

```csharp
public interface IParseStrategy<T> : IParseStrategy
{
    T Parse(string input);
    bool TryParse(string input, [NotNullWhen(true)] out T? parsed);
}

public interface IParseStrategy
{
    string ParameterKey { get; }
    object? Parse(string input);
    bool TryParse(string input, [NotNullWhen(true)] out object? parsed);
}
```

### Model Binders

The library provides custom model binders for automatic parameter binding:

- **`FilterInfoModelBinder`** - Binds `FilterInfoCollection` from query strings
- **`PageInfoModelBinder`** - Binds `PageInfo` from query strings
- **`SortInfoModelBinder`** - Binds `IEnumerable<SortInfo>` from query strings

These are registered automatically when using `AddQueryStringInfoParameterBinding()`.

### Value Provider Factories

- **`CompleteKeyedQueryStringValueProviderFactory`** - Provides complete keyed query string values for filter and page parameters
- **`DelimitedQueryStringValueProviderFactory`** - Provides delimited query string values for sort parameters

## License

Copyright © Karma, LLC 2023. All rights reserved.

---

**Author:** Sean M. Williams  
**Company:** Karma, LLC  
**NuGet Package:** [Karma.Extensions.AspNetCore](https://www.nuget.org/packages/Karma.Extensions.AspNetCore)