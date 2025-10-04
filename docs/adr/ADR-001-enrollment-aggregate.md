# ADR-001: Enrollment Aggregate & Invariants

## Context
The Enrollment API manages student enrollments into courses. Without explicit domain rules, the system could allow inconsistent states such as:
- Duplicate active enrollments for the same student in a course
- Confirming an enrollment multiple times
- Cancelling after completion
- Rolling back a completed enrollment to pending

We need a design that enforces invariants both in code and persistence to ensure data integrity.

## Decision
We model **Enrollment** as an aggregate root with explicit invariants:

- Valid states: `Pending`, `Confirmed`, `Completed`, `Cancelled`
- Allowed transitions:
    - Pending → Confirmed
    - Pending → Cancelled
    - Confirmed → Completed
    - Confirmed → Cancelled
- Illegal transitions (e.g., Completed → Pending) return `409 Conflict`.

Additionally:
- EF Core enforces a unique index on `(StudentId, CourseId)` to prevent duplicate active enrollments.
- The API returns validation problems for invalid GUIDs and business rule violations.

## Alternatives
- **Pure CRUD:** Faster to implement, but risks invalid business states.
- **DB constraints only:** Covers uniqueness but not transition logic.
- **CQRS/MediatR:** Provides full separation and scalability, but adds unnecessary complexity for this slice.

## Consequences
- ✅ Strong domain integrity: only valid states exist in the system.
- ✅ Rules are enforced at the API boundary and in persistence.
- ❌ Slightly more complexity in endpoint code.
- ❌ Requires test coverage for state transitions and conflicts.
