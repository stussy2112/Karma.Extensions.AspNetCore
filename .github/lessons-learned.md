# Lessons Learned

This document records critical lessons from real-world Copilot usage to prevent recurring issues.

## Lesson 1: Always Reload Documentation Files Before Editing

**Date Learned**: 2025-01-14

**Issue**: 
When making edits to documentation files (README.md, CHANGELOG.md, etc.) using `edit_file`, the AI worked from cached context instead of reloading the current file state. This resulted in:
- Overwriting user changes made during the conversation
- Deleting large sections of content that had been restored by the user
- Working with stale documentation from earlier in the conversation

**Root Cause**:
- The mandatory requirements only explicitly covered reloading **instruction files** and **code files**
- Documentation files were not explicitly mentioned in the reloading requirements
- The AI assumed cached context was sufficient for documentation edits

**Solution Implemented**:
- Added **Section 6: Documentation File Handling** to `copilot-instructions.md` with mandatory reloading requirements
- Updated **STEP 2A** in the Loading Process to explicitly require documentation file reloading
- Added documentation files to the **Verification Checklist**
- Updated the **Response Format Requirements** to include documentation files in status reporting

**Prevention**:
- **ALWAYS** execute `get_file` for documentation files before any edit operation
- **NEVER** rely on cached documentation content from earlier in the conversation
- **VERIFY** file content is current before applying changes
- **REPORT** documentation files loaded in the status section

**Impact**:
- **HIGH RISK**: Using stale documentation can result in data loss
- **USER TRUST**: Violating file integrity severely damages user confidence
- **REWORK REQUIRED**: All documentation changes must be redone after detecting the issue

**Key Takeaway**:
> Documentation files are just as important as code files and must be treated with the same mandatory reload requirements. The `edit_file` tool replaces content based on what the AI has in context, making stale context extremely dangerous for documentation.

---

## Lesson 2: Minimal Code Changes Only

**Date Learned**: 2025-01-14

**Issue**: 
When making edits to code files using `edit_file`, the AI modified many more lines than necessary, including unrelated code that didn't need to change. For example, when asked to implement a single-cache solution, the AI also modified an unrelated `CreateGroupDictionary` method and other nearby code.

**Root Cause**:
- File content had changed between conversations (user reverted previous changes)
- AI saw differences from its previous understanding of the file
- Without explicit guidance to only modify requested lines, AI "helpfully" updated unrelated code
- No instruction to assume the current file state is correct

**Solution Implemented**:
- Added **Minimal Edit Requirements** section to `copilot-instructions.md` under "Code File Handling"
- Created mandatory checklist before editing with 5 specific rules
- Added example showing correct vs. wrong approach
- Emphasized using `// ...existing code...` comments liberally

**Prevention**:
- **ALWAYS** identify exact lines that need modification before editing
- **NEVER** "fix" or update unrelated code even if it differs from expectations
- **ASSUME** the current file state is correct after reloading and only modify requested lines
- **USE** `// ...existing code...` comments to indicate unchanged sections
- **FOCUS** laser-precisely on the requested change only

**Impact**:
- **MODERATE RISK**: Unintended changes can introduce bugs or break functionality
- **USER FRUSTRATION**: Extra changes require rollback and rework
- **TIME WASTE**: User must identify and revert unrelated modifications
- **TRUST EROSION**: Making changes beyond scope reduces confidence in AI assistance

**Key Takeaway**:
> When editing code files, make ONLY the changes directly requested by the user. Assume the current file state is correct only after reloading and do not modify formatting, style, or unrelated code. Use `// ...existing code...` comments to clearly indicate unchanged sections.

---

## Lesson 3: Always Load Code Review Prompt Before Reviewing Code

**Date Learned**: 2025-01-14

**Issue**: 
When asked to review code, the AI loaded instruction files (coding-guidelines.md, coding-style.md) but completely skipped loading the required code review prompt (`.github/prompts/code-review.prompt.md`). This resulted in:
- Performing a review without following the prompt's specific structure and requirements
- Missing required formatting (emojis, severity indicators, structured sections)
- Not applying the prompt's review methodology
- Violating the mandatory requirement to always load prompts for specific tasks

**Root Cause**:
- The AI selectively followed STEP 1 instructions, loading some files but not others
- No explicit "detection rule" to identify when a code review is being requested
- No forcing mechanism to prevent proceeding without the prompt
- The verification checklist didn't explicitly call out code review prompt loading as a separate, critical step

**Solution Implemented**:
- Added **"Detecting Code Review Requests"** section to `copilot-instructions.md` with keyword detection
- Updated **Verification Checklist** to include explicit code review prompt check
- Made code review prompt loading a blocking requirement (STOP if not loaded)
- Added this lesson to `lessons-learned.md` for future reference

**Prevention**:
- **ALWAYS** detect task type first (code review vs. code generation vs. documentation)
- **ALWAYS** check for code review keywords: "review", "check", "analyze", "issues", "compliance", "adherence"
- **STOP IMMEDIATELY** if code review detected and prompt not loaded
- **NEVER** proceed with a code review without loading `.github/prompts/code-review.prompt.md`
- **VERIFY** the code review prompt is loaded BEFORE starting analysis
- **DISPLAY** prompt loading status in the status section

**Impact**:
- **HIGH RISK**: Reviews without prompts miss required structure and methodology
- **QUALITY DEGRADATION**: Output doesn't match expected format and rigor
- **COMPLIANCE FAILURE**: Violates mandatory instructions explicitly stated in copilot-instructions.md
- **USER FRUSTRATION**: User must challenge and request restart of review process
- **TIME WASTE**: Review must be completely redone with proper prompt

**Key Takeaway**:
> Code review prompts are **mandatory tools** that define how reviews should be conducted. Just like a surgeon must use their checklist before surgery, an AI must load and apply the code review prompt before reviewing code. Skipping this step invalidates the entire review process.

**Detection Keywords for Code Reviews**:
- "review" (review this code, code review)
- "check" (check this code, check for issues)
- "analyze" (analyze this class, analyze for problems)
- "issues" (any issues, find issues)
- "following the required instructions"
- "adherence to guidelines"
- "compliance with"
- "inform me of any issue following"

**Before Starting ANY Response:**
1. ✅ Read the user's request completely
2. ✅ Identify task type (review/generate/document)
3. ✅ Load ALL required files for that task type (including prompts)
4. ✅ Display status showing what was loaded
5. ✅ THEN proceed with the task

---

## Template for Future Lessons

When new issues are discovered, document them following this format:

### Lesson N: [Title]

**Date Learned**: YYYY-MM-DD

**Issue**: 
[What went wrong]

**Root Cause**:
[Why it happened]

**Solution Implemented**:
[What was added/changed to prevent it]

**Prevention**:
[How to avoid the issue in the future]

**Impact**:
[Why this matters]

**Key Takeaway**:
> [Core principle to remember]

---

## How to Use This Document

1. **Before Starting Work**: Review recent lessons learned
2. **When Issues Occur**: Document them immediately with full context
3. **Regular Reviews**: Periodically review lessons to reinforce learning
4. **Share Knowledge**: Reference specific lessons when training or onboarding
5. **Update Instructions**: Ensure `copilot-instructions.md` reflects all lessons learned
