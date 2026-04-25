---
trigger: always_on
---
You're an expert software engineer.
# Language to use: C# with ASP.NET

# Architecture Constraints

- Controllers: request/response only (no business logic)
- Services: all business logic lives here
- Repositories/DB layer: data access only
- Models: schema definitions only

- State machine logic MUST live in service layer
- Controllers must NOT enforce business rules

# Build Order (MANDATORY)

1. Data models / schema
2. Repository / data access layer
3. Service layer (business logic)
4. Controllers / routes
5. Tests

Do not skip or reorder.

# Task Execution Rules

- Only modify files relevant to the current task
- Do not refactor unrelated code
- Do not introduce abstractions unless required
- Prefer simple solutions over extensible ones

# Domain Rules

# Testing Rules
write test for every change/implementation task

# Behavior

- If requirements are unclear: stop and ask
- If a rule conflicts with implementation: follow rules

# Code Documentation and Comments
- Add clear and concise comments to explain complex logic
- Include docstrings for functions, classes, and modules
- Structure comments to explain:
  - Purpose of the code
  - Input parameters and return values
  - Any important assumptions or limitations
  - Examples where helpful
- Use consistent comment formatting

# Code Quality Best Practices
- Maintain consistent code formatting and style
- Write modular and reusable code
- Keep functions focused and single-purpose
- Include input validation where appropriate
- Add logging statements for debugging purposes
- Implement proper error handling
- Include version compatibility information when relevant

# Security Considerations
- Never expose sensitive information in comments or logs
- Include security-related warnings where applicable
- Document any security-related configurations
- Highlight potential security risks in the implementation
- Implement secure coding practices
- Follow security best practices for the language/framework

# Performance Considerations
- Add comments about performance implications for complex operations
- Document any performance optimization techniques used
- Include resource usage considerations
- Optimize code where necessary
- Consider scalability implications

# Code Maintenance
- Keep code documentation up-to-date
- Include deprecation notices when needed
- Document dependencies and requirements
- Add change logs for significant updates
- Follow versioning conventions
- Maintain backwards compatibility where possible
