# Karma.Extensions.AspNetCore

A powerful ASP.NET Core library that provides advanced query string parsing capabilities for filtering, sorting, and pagination. This library enables robust data querying through URL parameters with support for complex filter expressions, multiple operators, and logical grouping.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Features](#features)
- [Filter Functionality](#filter-functionality)
  - [Basic Filtering](#basic-filtering)
  - [Filter Operators Reference](#filter-operators-reference)
  - [Logical Grouping](#logical-grouping)
  - [Nested Properties](#nested-properties)
  - [Advanced Examples](#advanced-examples)
- [Sorting](#sorting)
- [Pagination](#pagination)
- [Configuration](#configuration)
- [API Reference](#api-reference)
- [Examples](#examples)
- [Contributing](#contributing)

## Installation

```bash
dotnet add package Karma.Extensions.AspNetCore
```

## Quick Start

### 1. Configure the Middleware

Add the query string parsing middleware to your ASP.NET Core application:

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

var app = builder.Build();

// Configure middleware - order matters!
app.ParseQueryStringFilters()     // Parse filter parameters
   .ParseQueryStringSortingInfo() // Parse sort parameters  
   .ParseQueryStringPagingInfo(); // Parse pagination parameters

app.MapControllers();
app.Run();
```

### 2. Access Parsed Information

The parsed information is available in your controllers through `HttpContext.Items`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        // Access parsed filter information
        var filters = HttpContext.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        
        // Build and apply filters to your data
        var lambda = FilterExpressionBuilder.BuildLambda<Product>(filters);
        var filteredProducts = products.Where(lambda);
        
        return Ok(filteredProducts);
    }
}
```

## Features

- **🔍 Advanced Filtering**: Support for complex filter expressions with multiple operators
- **🔗 Logical Grouping**: Combine filters using AND/OR logic with nested grouping
- **📊 Multiple Operators**: Equality, comparison, string matching, range, and null checks
- **📈 Sorting**: Multi-field sorting with ascending/descending order
- **📑 Pagination**: Offset and limit-based pagination
- **🏗️ Expression Building**: Automatic LINQ expression generation from filter definitions
- **🎯 Type Safety**: Strongly-typed filter and sort information
- **⚡ Performance**: Optimized parsing with minimal allocations

## Filter Functionality

### Basic Filtering

Filter query parameters follow the pattern: `filter[property]=value`

```
GET /api/products?filter[name]=iPhone
GET /api/products?filter[price]=999
GET /api/products?filter[category]=Electronics
```

### Filter Operators Reference

The library supports a comprehensive set of operators for different comparison types. All operators can be used in the format `filter[property][$operator]=value`.

| Operator | Query Parameter | Description | Example | Use Case |
|----------|----------------|-------------|---------|----------|
| **Equality Operators** |
| `EqualTo` (default) | `$eq` or none | Tests if property equals the specified value | `filter[status][$eq]=active`<br/>`filter[status]=active` | Exact matching for enums, IDs, simple values |
| `NotEqualTo` | `$ne` | Tests if property does not equal the specified value | `filter[status][$ne]=inactive` | Excluding specific values |
| **Comparison Operators** |
| `GreaterThan` | `$gt` | Tests if property is greater than the specified value | `filter[price][$gt]=100` | Numeric ranges, date comparisons |
| `GreaterThanOrEqualTo` | `$ge` or `$gte` | Tests if property is greater than or equal to the specified value | `filter[rating][$gte]=4`<br/>`filter[rating][$ge]=4` | Minimum thresholds |
| `LessThan` | `$lt` | Tests if property is less than the specified value | `filter[price][$lt]=1000` | Upper bounds, date filters |
| `LessThanOrEqualTo` | `$le` or `$lte` | Tests if property is less than or equal to the specified value | `filter[price][$lte]=500`<br/>`filter[price][$le]=500` | Maximum limits |
| **String Operators** |
| `Contains` | `$contains` | Tests if string property contains the specified substring | `filter[name][$contains]=phone` | Search functionality, partial matching |
| `NotContains` | `$notcontains` | Tests if string property does not contain the specified substring | `filter[description][$notcontains]=deprecated` | Excluding content with specific terms |
| `StartsWith` | `$startswith` | Tests if string property starts with the specified value | `filter[email][$startswith]=admin` | Prefix matching, categorization |
| `EndsWith` | `$endswith` | Tests if string property ends with the specified value | `filter[email][$endswith]=@company.com` | Suffix matching, domain filtering |
| **Pattern Matching Operators** |
| `Regex` | `$regex` | Tests if string property matches the specified regular expression pattern | `filter[email][$regex]=^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$` | Complex pattern matching, validation, advanced text search |
| **Range Operators** |
| `Between` | `$between` | Tests if property value is between two bounds (inclusive) | `filter[price][$between]=100,500` | Price ranges, date ranges |
| `NotBetween` | `$notbetween` | Tests if property value is not between two bounds | `filter[date][$notbetween]=2023-01-01,2023-12-31` | Excluding specific ranges |
| **Membership Operators** |
| `In` | `$in` | Tests if property value is in the specified list | `filter[category][$in]=Electronics,Books,Clothing` | Multiple choice selections |
| `NotIn` | `$notin` | Tests if property value is not in the specified list | `filter[status][$notin]=deleted,archived` | Excluding multiple values |
| **Null Operators** |
| `IsNull` | `$null` or `$isnull` | Tests if property is null or empty | `filter[deletedAt][$null]=`<br/>`filter[deletedAt][$isnull]=` | Finding records with missing data |
| `IsNotNull` | `$notnull` or `$isnotnull` | Tests if property is not null and not empty | `filter[description][$notnull]=`<br/>`filter[description][$isnotnull]=` | Finding records with data present |

#### Operator Examples by Data Type

**Numeric Properties:**
```
filter[price][$gt]=50          // Price greater than 50
filter[quantity][$between]=1,10 // Quantity between 1 and 10
filter[rating][$in]=4,5        // Rating is 4 or 5 stars
```

**String Properties:**
```
filter[name][$contains]=phone     // Name contains "phone"
filter[email][$endswith]=@test.com // Email ends with "@test.com"
filter[title][$startswith]=Mr     // Title starts with "Mr"
```

**Regular Expression Pattern Matching:**
```
// Email validation
filter[email][$regex]=^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$

// Phone number validation (US format)
filter[phone][$regex]=^\(\d{3}\)\s\d{3}-\d{4}$

// Product codes (3 letters followed by 3 numbers)
filter[productCode][$regex]=^[A-Z]{3}-\d{3}$

// Alphanumeric strings only
filter[username][$regex]=^[a-zA-Z0-9]+$

// Case-sensitive pattern matching
filter[name][$regex]=^Test.*

// Flexible date formats (YYYY-MM-DD or MM/DD/YYYY)
filter[dateString][$regex]=^\d{4}-\d{2}-\d{2}|\d{2}\/\d{2}\/\d{4}$

// Content validation (no special characters except spaces and hyphens)
filter[description][$regex]=^[a-zA-Z0-9\s\-]+$

// URL validation
filter[website][$regex]=^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$
```

**Date Properties:**
```
filter[createdAt][$gte]=2024-01-01          // Created after Jan 1, 2024
filter[updatedAt][$between]=2024-01-01,2024-12-31 // Updated in 2024
filter[deletedAt][$null]=                  // Not deleted (null deleted date)
```

**Boolean Properties:**
```
filter[isActive][$eq]=true    // Active records
filter[isActive][$ne]=false   // Active records (alternative)
```

### Logical Grouping

Combine multiple filters using logical operators:

#### AND Grouping (default)
Multiple filters are combined with AND by default:

```
filter[category]=Electronics&filter[price][$lt]=1000
// category = "Electronics" AND price < 1000
```

#### Explicit AND Grouping
```
filter[$and][0][category]=Electronics&filter[$and][1][price][$lt]=1000
```

#### OR Grouping
```
filter[$or][0][category]=Electronics&filter[$or][1][category]=Books
// category = "Electronics" OR category = "Books"
```

#### Named Groups
Create reusable filter groups:

```
filter[group]=electronics&filter[electronics][0][category]=Electronics&filter[electronics][1][brand]=Apple
```

#### Complex Nested Logic
```
filter[$and][0][category]=Electronics&filter[$and][1][$or][0][brand]=Apple&filter[$and][1][$or][1][brand]=Samsung
// category = "Electronics" AND (brand = "Apple" OR brand = "Samsung")
```

### Nested Properties

Filter on nested object properties using dot notation or bracket notation:

```
// Dot notation
filter[address.city]=Seattle

// Bracket notation  
filter[address][city]=Seattle
filter[user][profile][name]=John
```

### Regular Expression Pattern Matching

The `$regex` operator provides powerful pattern matching capabilities using standard .NET regular expressions. This operator is particularly useful for:

- **Data validation** - Ensuring input matches specific formats
- **Complex searches** - Finding records that match sophisticated patterns
- **Content filtering** - Identifying records with specific text patterns
- **Format verification** - Validating standardized formats like emails, phone numbers, etc.

#### Regex Operator Behavior

- **Null Safety**: Returns `false` for null property values
- **Type Conversion**: Non-string values are converted to strings before pattern matching
- **Case Sensitivity**: Patterns are case-sensitive by default (use `(?i)` flag for case-insensitive matching)
- **Empty Patterns**: Empty or null patterns match all non-null strings
- **Multiple Values**: Only the first value is used as the pattern when multiple values are provided

#### Common Regex Patterns

**Email Validation:**
```
filter[email][$regex]=^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
```

**Phone Number Formats:**
```
// US Phone: (123) 456-7890
filter[phone][$regex]=^\(\d{3}\)\s\d{3}-\d{4}$

// International: +1-123-456-7890
filter[phone][$regex]=^\+\d{1,3}-\d{3}-\d{3}-\d{4}$

// Flexible: Accept various formats
filter[phone][$regex]=^[\+]?[\d\s\-\(\)]{10,}$
```

**Alphanumeric Patterns:**
```
// Product codes: ABC-123
filter[productCode][$regex]=^[A-Z]{3}-\d{3}$

// User IDs: letters and numbers only
filter[userId][$regex]=^[a-zA-Z0-9]+$

// License plates: 3 letters, 4 numbers
filter[licensePlate][$regex]=^[A-Z]{3}\d{4}$
```

**Date and Time Patterns:**
```
// ISO Date: YYYY-MM-DD
filter[dateString][$regex]=^\d{4}-\d{2}-\d{2}$

// US Date: MM/DD/YYYY
filter[dateString][$regex]=^\d{2}\/\d{2}\/\d{4}$

// Time: HH:MM (24-hour)
filter[timeString][$regex]=^([01]?[0-9]|2[0-3]):[0-5][0-9]$
```

**Content Validation:**
```
// No special characters (alphanumeric, spaces, hyphens only)
filter[name][$regex]=^[a-zA-Z0-9\s\-]+$

// Strong password requirements
filter[password][$regex]=^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$

// URL validation
filter[url][$regex]=^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$
```

#### Advanced Regex Features

**Case-Insensitive Matching:**
```
// Case-insensitive email search
filter[email][$regex]=(?i)gmail\.com$
```

**Word Boundaries:**
```
// Exact word match
filter[content][$regex]=\bimportant\b
```

**Quantifiers:**
```
// 2-4 digits
filter[code][$regex]=^\d{2,4}$

// At least 3 characters
filter[name][$regex]=^.{3,}$
```

**Character Classes:**
```
// Letters only
filter[name][$regex]=^[a-zA-Z]+$

// No digits
filter[description][$regex]=^[^0-9]+$
```

#### Important Notes

⚠️ **Performance Consideration**: Complex regex patterns can impact query performance. Use simpler operators when possible.

⚠️ **Pattern Validation**: Invalid regex patterns will throw exceptions during query execution.

⚠️ **Escaping**: Remember to properly escape special characters in URLs: `\` becomes `%5C`, `+` becomes `%2B`, etc.

### Advanced Examples

#### Complex Product Search
```
GET /api/products?
  filter[$and][0][category]=Electronics&
  filter[$and][1][$or][0][brand]=Apple&
  filter[$and][1][$or][1][brand]=Samsung&
  filter[$and][2][price][$between]=500,2000&
  filter[$and][3][rating][$gte]=4&
  filter[$and][4][description][$contains]=wireless
```

This creates the following logic:
```
category = "Electronics" AND 
(brand = "Apple" OR brand = "Samsung") AND 
price BETWEEN 500 AND 2000 AND 
rating >= 4 AND 
description CONTAINS "wireless"
```

#### User Search with Multiple Criteria
```
GET /api/users?
  filter[group][$or]=activeUsers&
  filter[activeUsers][0][status]=active&
  filter[activeUsers][1][lastLogin][$gte]=2024-01-01&
  filter[email][$endswith]=@company.com
```

#### Advanced Pattern Matching with Regex
```
GET /api/users?
  filter[$and][0][email][$regex]=^[a-zA-Z0-9._%+-]+@company\.com$&
  filter[$and][1][phone][$regex]=^\(\d{3}\)\s\d{3}-\d{4}$&
  filter[$and][2][employeeId][$regex]=^EMP-\d{4}$&
  filter[$and][3][status]=active
```

This searches for:
```
email MATCHES company email pattern AND
phone MATCHES US phone format AND  
employeeId MATCHES "EMP-" followed by 4 digits AND
status = "active"
```

#### Product Validation and Search
```
GET /api/products?
  filter[$and][0][sku][$regex]=^[A-Z]{2}-\d{4}-[A-Z]{2}$&
  filter[$and][1][$or][0][description][$regex]=(?i)\b(premium|deluxe|pro)\b&
  filter[$and][1][$or][1][category]=Premium&
  filter[$and][2][price][$gte]=100
```

This finds products where:
```
SKU MATCHES pattern "XX-9999-XX" AND
(description CONTAINS words "premium", "deluxe", or "pro" (case-insensitive) OR category = "Premium") AND
price >= 100
```

## Sorting

Sort results using the `sort` parameter:

```
GET /api/products?sort=name              // Ascending by name
GET /api/products?sort=-price            // Descending by price  
GET /api/products?sort=category,name     // Multiple fields
GET /api/products?sort=category,-price   // Mixed order
```

Access sort information in controllers:

```csharp
var sortInfo = HttpContext.Items[ContextItemKeys.Sorts] as IEnumerable<SortInfo>;
```

## Pagination

Control result pagination:

```
GET /api/products?page[offset]=20&page[limit]=10
GET /api/products?page[number]=3&page[size]=25
```

Access pagination information:

```csharp
var pageInfo = HttpContext.Items[ContextItemKeys.Paging] as PageInfo;
```

## Configuration

### Custom Parse Strategies

Implement custom parsing logic:

```csharp
public class CustomFilterParser : IParseStrategy<FilterInfoCollection>
{
    public string ParameterKey => "filter";
    
    public FilterInfoCollection Parse(string input)
    {
        // Custom parsing logic
        return new FilterInfoCollection("custom");
    }
}

// Register custom parser
app.ParseQueryStringFilters(new CustomFilterParser());
```

### Middleware Options

Configure middleware behavior:

```csharp
// Configure individual middleware
app.UseMiddleware<AddFilterInfoMiddleware>(customParseStrategy);
app.UseMiddleware<AddSortInfoMiddleware>();
app.UseMiddleware<AddPagingInfoMiddleware>();
```

## API Reference

### Core Types

- **`FilterInfo`** - Represents a single filter condition
- **`FilterInfoCollection`** - Collection of filters with logical grouping
- **`Operator`** - Enumeration of supported filter operators
- **`Conjunction`** - AND/OR logical operators
- **`SortInfo`** - Sorting information for a field
- **`PageInfo`** - Pagination parameters

### Expression Building

- **`FilterExpressionBuilder.BuildLambda<T>(filters)`** - Creates compiled lambda expressions

### Context Keys

- **`ContextItemKeys.Filters`** - Access parsed filters
- **`ContextItemKeys.Sorts`** - Access parsed sort information  
- **`ContextItemKeys.Paging`** - Access parsed pagination

## Examples

### Entity Framework Integration

```csharp
[HttpGet]
public async Task<IActionResult> GetProducts()
{
    var filters = HttpContext.Items[ContextItemKeys.Filters] as FilterInfoCollection;
    var sorts = HttpContext.Items[ContextItemKeys.Sorts] as IEnumerable<SortInfo>;
    var paging = HttpContext.Items[ContextItemKeys.Paging] as PageInfo;

    var query = _context.Products.AsQueryable();

    // Apply filters
    if (filters?.Any() == true)
    {
        var predicate = FilterExpressionBuilder.BuildLambda<Product>(filters);
        query = query.Where(predicate);
    }

    // It is also possible to apply filters directly from HttpContext
    query = query.FilterByQuery(HttpContext);

    // Apply sorting
    if (sorts?.Any() == true)
    {
        query = query.ApplySorting(sorts);
    }

    // Apply pagination
    if (paging != null)
    {
        query = query.Skip(paging.Offset).Take(paging.Limit);
    }

    var products = await query.ToListAsync();
    return Ok(products);
}
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for your changes
4. Ensure all tests pass
5. Submit a pull request

## License

Copyright (c) Karma, LLC. All rights reserved.

