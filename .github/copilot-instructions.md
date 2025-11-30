# Copilot Instructions

## Table of Contents

1. [Description](#description)
2. [Prompt File Precedence](#prompt-file-precedence)
3. [Instruction File Loading](#instruction-file-loading)
   - [⚠️ Detecting Code Review Requests](#detecting-code-review-requests)
   - [Required Instruction Files to Load](#required-instruction-files-to-load)
   - [Loading Process](#loading-process)
   - [Verification](#verification)
   - [Error Recovery](#error-recovery)
4. [Response Format Requirements](#response-format-requirements)
5. [Code File Handling](#code-file-handling)
   - [Code File Reloading Requirements](#code-file-reloading-requirements)
   - [Verification](#verification-1)
   - [Rationale](#rationale)
   - [Process](#process)
6. [Documentation File Handling](#documentation-file-handling)
   - [Documentation File Reloading Requirements](#documentation-file-reloading-requirements)
   - [Verification](#verification-2)
   - [Rationale](#rationale-1)
   - [Process](#process-1)
7. [Minimal Edit Requirements](#minimal-edit-requirements)
8. [Terminal Use](#terminal-use)
   - [Mandatory Terminal Output Handling](#mandatory-terminal-output-handling)
9. [Compliance and Failure Handling](#compliance-and-failure-handling)
10. [Related Guidelines](#related-guidelines)

## Description:
This document outlines the mandatory requirements for using Copilot in this repository.

## Prompt File Precedence
- **Mandatory Requirement** Always reference and apply the latest prompt files from the `.github/prompts` directory. when generating code, documentation, or tests. This is not optional.
  - This is necessary to ensure that the generated content adheres to the latest standards and requirements.
  - **DO NOT** skip this step, as it is critical for maintaining consistency and quality in the generated content.
- Before generating any output, ensure that the latest prompt files are loaded and applied.
- **Mandatory Requirement** When performing a code review, always load and use the `.github/prompts/code-review.prompt.md` file to guide the review process. This is not optional.

## Instruction File Loading
**MANDATORY REQUIREMENT**: Always load the instruction file `copilot-instructions.md` before generating any code, documentation, or tests. You **MUST** load and apply all instruction files from the `.github/instructions` directory. This is not optional.

**MANDATORY REQUIREMENT**: Always reload instruction files as they may change between prompts.  Inform the user of which instruction files are being used. This is not optional.

---

### ⚠️ Detecting Code Review Requests

**MANDATORY**: Determine if the user is requesting a code review by checking for these keywords:
- "review" (review this code, code review, please review)
- "check" (check this code, check for issues)
- "analyze" (analyze this class, analyze for problems)
- "issues" (any issues, find issues, issues following)
- "following the required instructions"
- "adherence to guidelines"
- "compliance with"
-	"validate" (validate this code)
- "evaluate" (evaluate this implementation)
- "assess" (assess the quality)
- "audit" (audit this code)
- "inspect" (inspect this class)

**If ANY of these keywords are detected:**
1. ✅ STOP - Do not proceed with analysis yet
2. ✅ Execute `get_file` for `.github/prompts/code-review.prompt.md`
3. ✅ Display status section showing this file was loaded
4. ✅ THEN proceed with code review using the prompt's guidance

**If code review prompt loading fails:**
- ❌ DO NOT proceed with the review
- ❌ Inform the user the prompt file could not be loaded
- ❌ Ask if they want to proceed without the prompt

### Required Instruction Files to Load:
1. **ALWAYS** load `.github/instructions/testing-mstest.instructions.md` when:
    - Generating tests for C# code
    - Reviewing C# code for test generation
    - Reviewing C# code for test coverage
    - Reviewing test code
    - Discussing test strategies for C# code
    - Working with any files in `tests/**/*.cs`

2. **ALWAYS** load `.github/instructions/coding-guidelines.instructions.md` when:
    - Generating code in C#
    - Reviewing C# code for any reason
    - Creating new classes, methods, properties or other members in C#
    - Working with any `.cs` files
    - Generating documentation for C# code
    - Reviewing C# code for documentation completeness
    - Working with any files that require documentation in C#

3. **ALWAYS** load `.github/instructions/coding-style.instructions.md` when:
    - Generating code in C#
    - Formatting C# code
    - Reviewing C# code for coding style
    - Working with any `.cs` files

### Loading Process:

**EXECUTE THESE STEPS FIRST - BEFORE ANY ANALYSIS:**

**STEP 1 - Load Instruction Files (MANDATORY FIRST ACTION):**
1. Execute `get_file` for `.github/copilot-instructions.md`
2. Execute `get_file` for task-specific instruction files based on the work being performed:
   - `.github/instructions/coding-guidelines.instructions.md` (when working with any C# code)
   - `.github/instructions/coding-style.instructions.md` (when working with any C# code)
   - `.github/instructions/testing-mstest.instructions.md` (when working with tests)
3. **Execute `get_file` for relevant prompt files** based on the task:
   - `.github/prompts/code-review.prompt.md` (when performing code reviews)

**STEP 2 - Reload Target Code Files (MANDATORY):**
1. Execute `get_file` for the target code file(s) being reviewed, modified, or analyzed
2. Verify file content is current and complete

**STEP 2A - Reload Target Documentation Files (MANDATORY):**
1. Execute `get_file` for any documentation file(s) (README.md, CHANGELOG.md, etc.) being reviewed, modified, or analyzed
2. Verify file content is current and complete
3. **CRITICAL**: Never use cached documentation content from earlier in the conversation

**STEP 3 - Report Status to User (MANDATORY):**
- Display the "📋 Instruction Files Status" section at the START of your response (see [Response Format Requirements](#response-format-requirements))
- List all files loaded with their status
- Explicitly confirm readiness to proceed

**STEP 4 - Proceed with Task:**
- Apply loaded guidelines consistently throughout your response
- Reference the loaded instruction files when making recommendations
- Use required formatting (emojis for code reviews, severity levels, etc.)

**NEVER skip steps 1-3. If you cannot load a file, explicitly state this in the status section before proceeding.**

### Verification:
- After loading the instruction files, verify that they have been successfully loaded by checking for their content
- **MANDATORY:** Display the status section showing which files were loaded successfully and which failed
- Reference the loaded instruction files in your responses to ensure compliance with the guidelines
- Ensure that all generated code, documentation, and tests adhere to the guidelines specified in these instructions
- If any instruction file is not found, explicitly state that the file is missing and cannot be applied in the status section

### Error Recovery:
- If any required instruction file is missing, continue with available files
- Document which files could not be loaded in your response
- Use fallback to general best practices for missing guidance

## Response Format Requirements

**MANDATORY**: Every response involving code review, generation, modification, or analysis MUST begin with the following status section:

```markdown
## 📋 Instruction Files Status

✅ **Loaded Instruction Files:**
- `.github/copilot-instructions.md` - ✅ Loaded successfully
- `.github/instructions/coding-guidelines.instructions.md` - [✅ Loaded / ❌ Failed / ⊘ Not Required]
- `.github/instructions/coding-style.instructions.md` - [✅ Loaded / ❌ Failed / ⊘ Not Required]
- `.github/instructions/testing-mstest.instructions.md` - [✅ Loaded / ❌ Failed / ⊘ Not Required]
- `.github/prompts/code-review.prompt.md` - [✅ Loaded / ❌ Failed / ⊘ Not Required]

✅ **Code Files Reloaded:**
- `[filename]` - ✅ Loaded successfully

✅ **Documentation Files Reloaded:**
- `[filename]` - ✅ Loaded successfully

✅ **Ready to proceed with analysis**
```

**This status section MUST appear BEFORE any code analysis, suggestions, or recommendations.**

**Verification Checklist (Internal - Verify Before Responding):**

Before providing any code-related response, verify:

- [ ] **Task type identified** - Is this a code review? (keywords: review, check, analyze, issues, compliance)
- [ ] All required instruction files loaded via `get_file` tool
- [ ] **IF CODE REVIEW:** `.github/prompts/code-review.prompt.md` loaded via `get_file` tool
- [ ] Target code file(s) reloaded via `get_file` tool
- [ ] Target documentation file(s) reloaded via `get_file` tool (if applicable)
- [ ] Status section displayed at the start of response
- [ ] All loaded files explicitly listed in status section
- [ ] Guidelines applied consistently throughout response
- [ ] Required formatting followed (emojis, severity levels, etc. as specified in prompt files)

**⚠️ CRITICAL: If "code review" checkbox is unchecked but user requested a review, STOP IMMEDIATELY and load the prompt file.**
**Example of detection failure:**
- User says: "Please review IQueryableExtensions"
- Keywords detected: "review"
- Action required: ✅ Load `.github/prompts/code-review.prompt.md` BEFORE starting review
-
**If any checkbox is unchecked, STOP and complete that step before continuing.**

## Code File Handling
**MANDATORY REQUIREMENT**: Always reload any code file before reviewing, modifying, generating tests for, or performing any analysis on it. This is not optional.

### Code File Reloading Requirements:
1. **ALWAYS** reload the target code file using `get_file` before:
   - Reviewing code for any reason
   - Making modifications or edits to code
   - Generating tests for the code
   - Analyzing code for refactoring
   - Checking code for compliance with guidelines
   - Generating documentation for the code
   - Any other code-related action

### Verification:
   - After reloading, verify the file content is current
   - Inform the user which code files have been reloaded
   - If a file cannot be loaded, explicitly state this before proceeding

### Rationale:
   - Code files may change between prompts or during development
   - Ensures all analysis and modifications are based on current file state
   - Prevents working with stale or outdated code
   - Avoids introducing errors from outdated context

### Process:
- Use `get_file` tool to reload the code file at the beginning of any code-related request
- Confirm successful reload by checking file content
- Reference the reloaded file in your response
- If multiple files are involved, reload all relevant files before proceeding
- **When editing code files, follow [Minimal Edit Requirements](#minimal-edit-requirements)**

## Documentation File Handling
**MANDATORY REQUIREMENT**: Always reload any documentation files (README.md, CHANGELOG.md, etc.) before generating, modifying, or performing any analysis on it. This is not optional.

### Documentation File Reloading Requirements:
1. **ALWAYS** reload the documentation file using `get_file` before:
   - Reviewing documentation for any reason
   - Making modifications or edits to documentation
   - Adding new sections to documentation
   - Updating existing documentation content
   - Generating new documentation
   - Analyzing documentation for completeness or correctness
   - Verifying accuracy of documentation
   - Any other documentation-related action

### Verification:
   - After reloading, verify the file content is current and complete
   - Inform the user which documentation files have been reloaded
   - If a file cannot be loaded, explicitly state this before proceeding
   - **CRITICAL**: Never rely on cached documentation content from earlier in the conversation

### Rationale:
   - Documentation files (README.md, CHANGELOG.md, API docs, etc.) may change between prompts
   - Documentation is often edited directly by users during conversations
   - Ensures all analysis and modifications are based on current file state
   - Prevents working with stale or outdated documentation
   - Avoids inadvertently deleting or overwriting user changes
   - **Major Risk**: Using `edit_file` with cached content can replace entire files with outdated versions

### Process:
- Use `get_file` tool to reload the documentation file at the beginning of any documentation-related request
- Confirm successful reload by checking file content
- Reference the reloaded file in your response
- If multiple documentation files are involved, reload all relevant files before proceeding
- **Never use `edit_file` without first reloading the target documentation file**
- **When editing documentation files, follow [Minimal Edit Requirements](#minimal-edit-requirements)**

## Minimal Edit Requirements

**MANDATORY**: When editing any file (code, documentation, configuration, etc.), make ONLY the changes directly requested by the user.

1. **Identify exact lines** - Determine which specific lines need modification
2. **Use minimal syntax** - Use `// ...existing code...` for unchanged sections
3. **No extraneous changes** - Do NOT modify formatting, style, or unrelated code
4. **Assume current state is correct** - Do not "fix" other parts of the file
5. **Focus precisely** - Make only the requested change, nothing more

**Example:**
```csharp
// User: "Add null check to the parameter in GetOrCreatePropertySelector"

// ✅ CORRECT:
private static Func<T, object?>? GetOrCreatePropertySelector<T>(string propertyName)
{
    ArgumentNullException.ThrowIfNull(propertyName);
    
    // ...existing code...
}

// ❌ WRONG: Don't also change unrelated methods or formatting
```

**Mandatory Checklist Before Editing:**
- [ ] Identify the exact lines that need to change
- [ ] Verify no other lines are being modified
- [ ] Use concise edit syntax with `// ...existing code...` comments
- [ ] DO NOT update formatting, style, or unrelated code

**See Also:** [Lessons Learned - Lesson 2: Minimal Code Changes Only](.github/lessons-learned.md#lesson-2-minimal-code-changes-only)

## Terminal Use:
- When using the terminal, it may take up to 2 minutes to respond.  Please wait up to 2 minutes for the terminal to respond.

### Mandatory Terminal Output Handling

**MANDATORY REQUIREMENT**: When running any terminal command where the output needs to be analyzed, **ALWAYS** redirect output to a file and then read the file. This is not optional.

**Why This Is Required:**
- The `run_command_in_terminal` tool does not reliably return complete command output
- Terminal output is often truncated, showing only PowerShell scaffolding
- Direct terminal output is unreliable for analysis of test results, build output, or command results

### Required Process for Commands Requiring Output Analysis:

**STEP 0 - Mark Copilot Terminal Session (MANDATORY):**
```powershell
# Create sentinel file to trigger minimal profile
New-Item -ItemType File -Path "$env:TEMP\.copilot-terminal-active" -Force | Out-Null
```

**STEP 1 - Write Output to File:**
```powershell
# For any command where you need to analyze the output
<command> 2>&1 | Tee-Object -FilePath <output-filename>.txt
```

**Examples:**
```powershell
# Test execution - complete workflow
New-Item -ItemType File -Path "$env:TEMP\.copilot-terminal-active" -Force | Out-Null; dotnet test 2>&1 | Tee-Object -FilePath test-results.txt

# Build output - complete workflow
New-Item -ItemType File -Path "$env:TEMP\.copilot-terminal-active" -Force | Out-Null; dotnet build 2>&1 | Tee-Object -FilePath build-results.txt

# Custom commands - complete workflow
New-Item -ItemType File -Path "$env:TEMP\.copilot-terminal-active" -Force | Out-Null; dotnet --version 2>&1 | Tee-Object -FilePath version-info.txt
```

**STEP 2 - Read the Output File:**
```
get_file with filename: <output-filename>.txt
```

**STEP 3 - Analyze and Report:**
- Read the complete output from the file
- Provide analysis based on the actual output
- Report findings to the user with verbatim excerpts from the file

### Commands That Require File-Based Output:

**ALWAYS use file-based output for:**
- `dotnet test` - Test execution results
- `dotnet build` - Build output and errors
- `dotnet run` - Application execution output
- `dotnet pack` - Package creation results
- Any command where you need to check for errors, warnings, or success status
- Any command where you need to analyze or report specific output

### Commands That May Use Direct Terminal:

**Direct terminal may be acceptable for:**
- `dotnet --version` - Simple version check (but file-based is preferred)
- `cd` or `pwd` - Directory navigation (no output analysis needed)
- File operations that don't produce analyzed output

### Verification Checklist:

Before running a terminal command, verify:
- [ ] Does this command produce output that needs analysis?
- [ ] If YES: Use `2>&1 | Tee-Object -FilePath <filename>.txt`
- [ ] After command completes: Use `get_file` to read the output file
- [ ] Analyze the file content and report findings

### Example Workflow:

```markdown
**Running Tests:**

1. Execute command with output redirect:
   `dotnet test 2>&1 | Tee-Object -FilePath test-results.txt`

2. Read the output file:
   `get_file` with filename: `test-results.txt`

3. Analyze the results:
   - Total tests: [X]
   - Failed: [Y]
   - Succeeded: [Z]
   - Duration: [D]

4. Report specific failures if any:
   [Verbatim error messages from file]
```

### Error Handling:

**If file creation fails:**
- Inform the user that output redirection failed
- Request they run the command manually and provide output
- Do NOT rely on direct terminal output alone

**If file reading fails:**
- Retry the `get_file` command once
- If still fails, request user provide the file content manually
- Explain what information is needed from the output

### Legacy Instructions (Still Applicable):

- **Mandatory Requirement** Always execute `dotnet --version` to confirm the version of `dotnet` cli being used. This is not optional.
  - Adjust the expected output based on the version of `dotnet` being used.
- Confirm the current location before assuming the terminal is having a problem when running a command, especially `dotnet` commands.
  - Reset the current location to the root of the solution and try the command again
- **Mandatory Requirement** Always inform the user the exact text output you are seeking in the terminal that indicates a problem. This is not optional.
  - The user needs the **exact** text output to determine if there is a problem with the command or if it is simply taking a long time to respond.
  - **Always** Provide a comparison of the expected output and the actual output, if available.

## Compliance and Failure Handling

### User Challenge Process

If mandatory requirements are not followed, the user may challenge the response. When challenged:

1. **Acknowledge the failure** - Explicitly confirm which requirement was not met
2. **Apologize** - Take responsibility for the oversight
3. **Restart properly** - Execute steps 1-4 of the Loading Process correctly
4. **Display status section** - Show all loaded files
5. **Provide corrected analysis** - Complete the task with proper adherence to guidelines

**Example User Challenge:**
> "You did not reload the instruction files as required. Please restart following the mandatory process."

**Required AI Response Format:**
```markdown
You are correct. I apologize for not following the mandatory requirements. Let me restart properly:

## 📋 Instruction Files Status

✅ **Loaded Instruction Files:**
[List all loaded files]

✅ **Code Files Reloaded:**
[List reloaded code files]

✅ **Documentation Files Reloaded:**
[List reloaded documentation files]

✅ **Ready to proceed with analysis**

[Proceed with proper analysis following all loaded guidelines]
```

### Consequences of Non-Compliance

- **Response may be invalid** - Analysis based on stale context or missing guidelines is unreliable
- **User trust is diminished** - Failure to follow documented requirements undermines confidence
- **Rework required** - The entire analysis must be restarted, wasting time
- **Guidelines not applied** - Code quality, style, and testing standards may be violated

**Remember:** The mandatory requirements exist to ensure consistency, quality, and reliability. They are not optional suggestions.

## Related Guidelines
This document should be used in conjunction with the following guidelines:
- [Coding Guidelines](.github/instructions/coding-guidelines.instructions.md)
- [Coding Style](.github/instructions/coding-style.instructions.md)
- [Testing Guidelines](.github/instructions/testing-mstest.instructions.md)
- [Editor Config](../../.editorconfig)
- [Lessons Learned](.github/lessons-learned.md)