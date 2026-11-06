# HireFlow – Hiring & Onboarding Platform (ASP.NET Core Web Api Microservices, SQL Server, Angular,  Next.js)

A hiring and onboarding platform. Companies post jobs, candidates apply with a CV, the system gives a
CV match score, recruiters move candidates through interview stages, and an accepted offer starts the
onboarding checklist.

HireFlow is a full-stack hiring platform a real recruitment

How it works: a recruiter creates a job, a hiring manager approves it, and it appears on a public careers site. Candidates apply with a CV and get a skills match score. Recruiters move them across a Kanban board (Applied, Screening, Interview, Offer, Hired, Rejected). Interviewers submit scorecards, a manager approves the offer, the candidate accepts through a secure one-time link, and an onboarding checklist is created automatically.

Roles: Admin, Recruiter and Hiring Manager, each with different permissions (JWT authentication and role-based access).

Business rules I enforced and unit tested:
• A job needs approval before it opens
• A candidate cannot move to Offer without an interview scorecard
• Rejecting needs a reason
• An interviewer cannot be double-booked
• Offer links work only once

Screens: login, dashboard, jobs, Kanban pipeline, candidate profile with timeline, interviews, offers, activity log, and a public careers site with job search, apply form with CV upload, and application status page.

Tech: ASP.NET Core Web API (.NET 10), Microservices, clean layers (Controller → Service → Repository), Entity Framework Core, SQL Server and SQLite, Angular, Next.js (TypeScript), xUnit, Git/GitHub, Vercel Cloud, Cloudflare Cloud .

The CV match score.

Live preview;- Carrier Site Live: https://hireflow-careers-fawn.vercel.app/ | Staff App Live: https://hireflow-admin-1u8.pages.dev/

## Screenshots

![Home](docs/screenshots/1.png)
![Dashboard](docs/screenshots/2.png)
![Pipeline](docs/screenshots/3.png)
