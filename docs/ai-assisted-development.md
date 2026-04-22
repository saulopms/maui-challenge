# AI Assisted Development Notes

## Prompt history summary

### User intent

- Build this project
- Prefer good practices such as `MVVM`, `SOLID`, and `clean code`
- Keep comments in English when needed
- Improve `README` and `.gitignore` if necessary

### AI execution summary

The implementation work followed these steps:

1. Read the challenge statement and the two DEX files provided by the user.
2. Scaffold a solution with separate MAUI, API, and shared projects.
3. Implement a shared DEX parser focused on the required `ID`, `VA`, and `PA` segments.
4. Build the Minimal API with Basic Authentication and SQL persistence through stored procedures.
5. Build the MAUI UI with MVVM, asynchronous commands, and configurable endpoint handling.
6. Improve repository documentation and delivery notes.

## Commentary on code style and output

### What is strong in the generated solution

- Responsibilities are separated clearly across projects.
- The MAUI layer avoids business logic in code-behind.
- The parser is centralized and reusable.
- The API uses dependency injection and small focused services.
- SQL persistence matches the challenge requirement for stored procedures.
- Naming is explicit and interview-friendly.

### Trade-offs and limitations

- The DEX payloads are large because the challenge explicitly asked for hardcoded text values.
- The current implementation parses only the segments needed by the assignment, not the entire DEX specification.
- End-to-end runtime verification was partially constrained by the local shell/sandbox environment, even though both target projects compiled successfully.
- The `.bak` file was not generated automatically in this environment because LocalDB tooling connectivity presented an encryption/connection issue.

### Why this output is appropriate for the challenge

- It satisfies the requested stack and architecture.
- It demonstrates practical engineering choices instead of overengineering.
- It keeps the code readable enough to discuss in an interview.
- It leaves obvious extension points for tests, more endpoints, richer diagnostics, and broader DEX support.
