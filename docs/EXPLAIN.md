# How one request flows (Jobs example)

`POST /api/jobs/5/approve`

1. **JobsController.Approve** reads the user id and role from the JWT and calls the service. No rules here.
2. **JobService.ApproveAsync** checks the rules: role must be HiringManager or Admin, the job must be PendingApproval.
   It sets the status to Open, saves, and writes an activity log entry.
3. **JobRepository** only talks to the database (`GetByIdAsync`, `SaveChangesAsync`).
4. **ExceptionHandlingMiddleware** turns rule errors into clean HTTP answers (404, 409, 403, 401, safe 500).

Why this layout: rules live in one place (services), are unit tested with fake repositories, and the database can change
without touching business rules.

## Explain in 30 seconds
"HireFlow is a hiring platform. Candidates apply with a CV, get a skills match score, recruiters move them through
stages, managers approve offers, and an accepted offer creates an onboarding checklist. The API uses clean layers:
controller, service, repository, with every business rule unit tested."
