# Copilot Instructions

## Table of Contents

1. [Description](#description)
2. [Prompt File Precedence](#prompt-file-precedence)
3. [Instruction File Loading](#instruction-file-loading)
   - [Required Instruction Files to Load](#required-instruction-files-to-load)
   - [Loading Process](#loading-process)
   - [Verification](#verification)
   - [Error Recovery](#error-recovery)
4. [Terminal Use](#terminal-use)
5. [Related Guidelines](#related-guidelines)

## Description:
This document outlines the mandatory requirements for using Copilot in this repository.

## Prompt File Precedence
- Always reference and apply the latest prompt files from the `.github/prompts` directory. when generating code, documentation, or tests.
  - This is necessary to ensure that the generated content adheres to the latest standards and requirements.
  - **DO NOT** skip this step, as it is critical for maintaining consistency and quality in the generated content.
- Before generating any output, ensure that the latest prompt files are loaded and applied.
- When performing a code review, always load and use the `.github/prompts/code-review.prompt.md` file to guide the review process.

## Instruction File Loading
**MANDATORY REQUIREMENT**: Always load the instruction file `copilot-instructions.md` before generating any code, documentation, or tests. You **MUST** load and apply all instruction files from the `.github/instructions` directory. This is not optional.

**MANDATORY REQUIREMENT**: Always reload instruction files as they may change between prompts.  Inform the user of which instruction files are being used. This is not optional.
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
- Use the `text_search` or `get_file` tools to load these instruction files at the beginning of any request
- **Always** verify successful loading by checking for content presence
- If a file fails to load, use `text_search` as a fallback to locate the content
- Apply the loaded instructions consistently throughout your response
- If there are conflicts between the instructions files, resolve them in this order of precedence:
  1. `.github/prompts/*.md` files (highest precedence)
  2. `.github/instructions/*.md` files
  3. General best practices and guidelines (lowest priority)

### Verification:
- After loading the instruction files, verify that they have been successfully loaded by checking for their content.  Inform the user of the files loaded and any issues before responding.
- Reference the loaded instruction files in your responses to ensure compliance with the guidelines.
- Ensure that all generated code, documentation, and tests adhere to the guidelines specified in these instruction
- If any instruction file is not found, explicitly state that the file is missing and cannot be applied.

### Error Recovery:
- If any required instruction file is missing, continue with available files
- Document which files could not be loaded in your response
- Use fallback to general best practices for missing guidance

## Terminal Use:
- When using the terminal, it may take up to 2 minutes to respond.  Please wait up to 2 minutes for the terminal to respond.
- Always execute `dotnet --version` to confirm the version of `dotnet` cli being used.
  - Adjust the expected output based on the version of `dotnet` being used.
- Confirm the current location before assuming the terminal is having a problem when running a command, especially `dotnet` commands.
  - Reset the current location to the root of the solution and try the command again
- **Mandatory Requirement** Always inform the user the exact text output you are seeking in the terminal that indicates a problem. This is not optional.
  - The user needs the **exact** text output to determine if there is a problem with the command or if it is simply taking a long time to respond.
  - **Always** Provide a comparison of the expected output and the actual output, if available.

## Related Guidelines
This document should be used in conjunction with the following guidelines:
- [Coding Guidelines](.github/instructions/coding-guidelines.instructions.md)
- [Coding Style](.github/instructions/coding-style.instructions.md)
- [Testing Guidelines](.github/instructions/testing-mstest.instructions.md)
- [Editor Config](../../.editorconfig)