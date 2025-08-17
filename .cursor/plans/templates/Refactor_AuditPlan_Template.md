## Refactor Batch Audit Plan (Template)

> Planning and auditing only. Do not execute commands in this document.

### 1. Batch Metadata
- **Batch ID**: <e.g., R130-Regex-2025-08-07-A>
- **Owner**: <name>
- **Date**: <yyyy-mm-dd>
- **Project/Path**: <e.g., vs/Core/ExxerAI.Domain>

### 2. Objective (No Behavior Change)
- **Goal**: <concise, semantics-preserving objective>
- **Taxonomy Targets**: <R-100|R-110|R-120|R-130|R-140|R-150|R-160>
- **Templates to Apply**: <T-100|T-130|T-150|T-160>

### 3. Discovery (Serena MCP Evidence)
- Activate project:
  - mcp_serena_activate_project("ExxerAI")
- Pattern findings:
  - search: <substring_pattern>, path: <relative_path>
  - results summary: <files, counts>
- Structure inspection:
  - get_symbols_overview: <file or directory>
- Selected files for this batch (1–3 files):
  - <file 1>
  - <file 2>
  - <file 3>

### 4. Current State Snapshot
- For each file, list:
  - Public APIs affected (if any): <none|list>
  - Tests covering the area: <test files>
  - Known smells:
    - <e.g., RegexOptions.Compiled, EXXER200 manual null checks, #region>

### 5. Planned Changes (Design Only)
- For each file, map smell -> template action:
  - <file>: <R-xxx> via <T-xxx>: <brief change description>
- Logging and cancellations: <confirm structured logging + token propagation>
- XML docs: <confirm docs update if public API touched>

### 6. RefactorMCP Commands (Draft; Do Not Run Here)
- Command 1: <tool and parameters>
- Command 2: <tool and parameters>
- Notes: Ensure integer types for line ranges; avoid long-running operations without timeouts.

### 7. Risk Assessment
- **Behavior change risk**: <low/medium/high + rationale>
- **API surface impact**: <none|describe>
- **Ripple risk (cross-project)**: <none|describe>
- **Mitigations**:
  - Small batch size
  - Build + test after batch
  - Rollback plan per file

### 8. Test Strategy (Pre/Post)
- Baseline tests (xUnit v3): <list>
- Additional/updated tests needed: <list or none>
- Cancellation use in tests: ensure TestContext.Current.CancellationToken is propagated

### 9. Rollback Strategy
- One-commit per file or cohesive set
- Revert strategy: `git revert <commit>`; no mixed concerns in a single commit

### 10. Approval Gate
- Reviewer(s): <name(s)>
- Criteria:
  - [ ] Objectives and taxonomy align with standards
  - [ ] No behavior change expected
  - [ ] Commands reviewed
  - [ ] Risks acceptable and mitigations adequate

### 11. Post-Approval Execution Checklist (For Later)
- [ ] Run planned RefactorMCP commands
- [ ] dotnet build (project-specific)
- [ ] dotnet test (xUnit v3)
- [ ] Lints and XML docs validated
- [ ] Commit with descriptive message referencing batch ID

### 12. References
- `.cursor/rules/1034_GeneralRefactoringStandards.mdc`
- `.cursor/rules/1033_EXXER200NullParameterRefactoring.mdc`
- `.cursor/rules/1032_SourceGeneratedRegexBestPractices.mdc`
- `.cursor/rules/1029_ExxerAITestingStandards.mdc`
- `.cursor/rules/1030_ExxerAIModernCSharp.mdc`