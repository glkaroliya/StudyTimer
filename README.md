# StudyTimer

StudyTimer Phase 1 core implementation delivered as a professional .NET 8 solution with test coverage.

## Delivered Phase 1 scope

- Role-based authentication (`Admin`, `Student`) with secure password hashing
- Student CRUD + search
- Subject CRUD + search
- Timetable slot CRUD + search, completion status, overlap validation
- Daily review note creation and retrieval
- Student dashboard aggregation (today slots, completion counters, review notes)
- Timer workflow service (countdown tick, auto-next slot, completion alert flag)
- Printable timetable export and simple PDF byte generation
- Domain-level validation and typed exceptions (`Validation`, `NotFound`, `Unauthorized`)

## Solution layout

- `/home/runner/work/StudyTimer/StudyTimer/StudyTimer.Core` - production domain/services
- `/home/runner/work/StudyTimer/StudyTimer/StudyTimer.Tests` - xUnit tests
- `/home/runner/work/StudyTimer/StudyTimer/StudyTimer.slnx` - solution file

## Build and test

```bash
cd /home/runner/work/StudyTimer/StudyTimer
dotnet build StudyTimer.slnx
dotnet test StudyTimer.slnx
```
