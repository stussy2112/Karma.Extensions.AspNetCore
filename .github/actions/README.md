# GitHub Actions - Composite Actions

This directory contains reusable composite actions for building, testing, and analyzing the Karma.Extensions.AspNetCore solution.

## Available Actions

### 1. `setup-environment`

Sets up the build environment including .NET SDK, Java (for SonarCloud), and required tools.

**Inputs:**
- `dotnet-version` (required): .NET SDK version to install (default: `10.0.x`)
- `java-version` (optional): Java version for SonarCloud (default: `17`)

**Outputs:**
- `dotnet-version`: The installed .NET SDK version

**Example:**
```yaml
- uses: ./.github/actions/setup-environment
  with:
    dotnet-version: '10.0.x'
    java-version: '17'
```

---

### 2. `build-solution`

Restores NuGet packages, begins SonarCloud analysis, and builds the solution.

**Inputs:**
- `solution` (required): Path to the solution file
- `configuration` (required): Build configuration (e.g., `Release`)
- `sonar-token` (required): SonarCloud authentication token
- `sonar-project-key` (required): SonarCloud project key
- `sonar-organization` (required): SonarCloud organization
- `test-results-path` (required): Path where test results will be stored

**Outputs:**
- `build-configuration`: The build configuration used

**Example:**
```yaml
- uses: ./.github/actions/build-solution
  with:
    solution: 'MyProject.sln'
    configuration: 'Release'
    sonar-token: ${{ secrets.SONAR_TOKEN }}
    sonar-project-key: 'my-project-key'
    sonar-organization: 'my-org'
    test-results-path: './TestResults'
```

---

### 3. `run-tests`

Executes unit tests with code coverage collection.

**Inputs:**
- `solution` (required): Path to the solution file
- `configuration` (required): Build configuration (must match build configuration)
- `test-results-path` (required): Path to store test results
- `verbosity` (optional): Test output verbosity (default: `normal`)

**Outputs:**
- `test-results-path`: Path where test results were stored

**Example:**
```yaml
- uses: ./.github/actions/run-tests
  with:
    solution: 'MyProject.sln'
    configuration: 'Release'
    test-results-path: './TestResults'
    verbosity: 'minimal'
```

---

### 4. `publish-results`

Completes SonarCloud analysis and publishes test results and code coverage.

**Inputs:**
- `test-results-path` (required): Path to test results
- `sonar-token` (required): SonarCloud authentication token
- `codecov-token` (optional): Codecov authentication token
- `fail-on-coverage-error` (optional): Fail build if coverage upload fails (default: `false`)

**Example:**
```yaml
- uses: ./.github/actions/publish-results
  with:
    test-results-path: './TestResults'
    sonar-token: ${{ secrets.SONAR_TOKEN }}
    codecov-token: ${{ secrets.CODECOV_TOKEN }}
    fail-on-coverage-error: 'false'
```

---

## Complete Workflow Example

```yaml
name: Build, Test, and Analyze

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

permissions:
  checks: write
  contents: read

env:
  SOLUTION: 'Karma.Extensions.AspNetCore.sln'
  BUILD_CONFIGURATION: 'Release'
  DOTNET_VERSION: '10.0.x'
  TEST_RESULTS: './TestResults'

jobs:
  build-test-analyze:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
      with:
        fetch-depth: 0
        
    - uses: ./.github/actions/setup-environment
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
        
    - uses: ./.github/actions/build-solution
      with:
        solution: ${{ env.SOLUTION }}
        configuration: ${{ env.BUILD_CONFIGURATION }}
        sonar-token: ${{ secrets.SONAR_TOKEN }}
        sonar-project-key: 'my-project-key'
        sonar-organization: 'my-org'
        test-results-path: ${{ env.TEST_RESULTS }}
        
    - uses: ./.github/actions/run-tests
      with:
        solution: ${{ env.SOLUTION }}
        configuration: ${{ env.BUILD_CONFIGURATION }}
        test-results-path: ${{ env.TEST_RESULTS }}
        
    - uses: ./.github/actions/publish-results
      if: always()
      with:
        test-results-path: ${{ env.TEST_RESULTS }}
        sonar-token: ${{ secrets.SONAR_TOKEN }}
        codecov-token: ${{ secrets.CODECOV_TOKEN }}
```

---

## Required Secrets

Configure these secrets in your repository settings:

1. **SONAR_TOKEN**: SonarCloud authentication token
   - Get from: https://sonarcloud.io/account/security
   
2. **CODECOV_TOKEN** (optional): Codecov upload token
   - Get from: https://codecov.io (after linking your repository)

---

## Benefits of Composite Actions

✅ **Reusability**: Use the same actions across multiple workflows  
✅ **Maintainability**: Update logic in one place  
✅ **Clarity**: Each action has a single, well-defined purpose  
✅ **Testability**: Actions can be tested independently  
✅ **Documentation**: Each action is self-documenting with inputs/outputs

---

## Customization

Each action can be customized by modifying its `action.yml` file. Common customizations:

- **Add more inputs**: Expose additional configuration options
- **Add validation**: Check input parameters before execution
- **Add outputs**: Return values for use in subsequent steps
- **Add conditional logic**: Skip steps based on conditions
- **Add error handling**: Improve resilience and debugging

---

## Troubleshooting

### Action not found
Ensure the workflow is checked out with sufficient depth:
```yaml
- uses: actions/checkout@v4
  with:
    fetch-depth: 0
```

### Permissions issues
Ensure the workflow has required permissions:
```yaml
permissions:
  checks: write
  contents: read
```

### Cache not working
Clear the cache from GitHub Actions UI or update cache keys in `setup-environment/action.yml`.

---

## Contributing

When updating actions, ensure:
1. Input/output documentation is up-to-date
2. Default values are sensible
3. Actions remain platform-agnostic (use `shell: bash`)
4. Error messages are clear and actionable
