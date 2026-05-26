# Student Management SaaS Platform
## High-Level Architecture & Development Plan

> **Superseded by:** [`student_management_saas_complete_module_plan.md`](student_management_saas_complete_module_plan.md) — the master plan with all 16 modules, phases, APIs, permissions, and dependencies. This file is kept for historical reference.

> **Implementation note (May 2026):** The live API uses **MySQL 8**, **database-per-tenant** (`ss_t_{slug}`), and a **layered** layout under `src/SchoolSaaS.*` — not PostgreSQL shared-schema or `src/Modules/*` as described in older sections below. CQRS code lives in `SchoolSaaS.Application/Commands/{Area}/{Endpoint}/` and `Queries/{Area}/{Endpoint}/`. See [`README.md`](README.md), [`AGENTS.md`](AGENTS.md), and the “As implemented” section in the master plan.

---

# Vision

Build a multi-tenant SaaS-based Student Management Platform supporting:

- Student Information System (SIS)
- Attendance Management
- Fees & Accounting
- LMS
- Parent & Student Mobile Apps
- Internal Social Networking
- Institution Customization
- Enterprise-grade RBAC & Auditability

---

# Core Product Goals

## Functional Goals

- Multi-institution SaaS platform
- Mobile-first experience
- Enterprise-grade security
- Full auditability
- Modular architecture
- Scalable deployment model
- AI-assisted development workflow

---

# Core Modules

# 1. Identity & Access Management

## Features

- Authentication
- JWT + Refresh Tokens
- Session Management
- Role-Based Access Control (RBAC)
- Permission Engine
- Multi-Tenant Authorization
- Device Management
- Audit Logging

## Roles

- Super Admin
- Institution Admin
- Principal
- Teacher
- Accountant
- Parent
- Student

---

# 2. Institution Management

## Features

- Institution onboarding
- Academic years
- Branches/campuses
- Departments
- Classes & Sections
- Subscription plans
- White-label settings
- Branding

---

# 3. Student Management

## Features

- Admissions
- Student Profiles
- Guardian Management
- Student Documents
- Transfers
- Academic History
- ID Cards
- Student Promotion

## Dynamic Custom Fields

Each institution can define:

- Custom student fields
- Validation rules
- Dropdown options
- Dynamic forms

---

# 4. Attendance Management

## Features

- Daily Attendance
- Subject-wise Attendance
- Staff Attendance
- Attendance Analytics
- Parent Notifications
- Leave Management

## Integrations (Future)

- Biometric Devices
- RFID
- QR Attendance
- Geo Attendance

---

# 5. Fees Management

## Features

- Fee Structures
- Installments
- Discounts
- Fine Rules
- Fee Collection
- Online Payments
- Receipts
- Refunds
- Due Tracking

## Payment Gateway Layer

Abstract provider model supporting:

- Razorpay
- Cashfree
- PhonePe

---

# 6. Finance & Accounting

## Features

- Double Entry Accounting
- Chart of Accounts
- Journals
- Trial Balance
- Balance Sheet
- Profit & Loss
- Expense Management
- Bank Reconciliation

## Important Rules

- Immutable financial records
- Reversal entries only
- Full financial audit trail

---

# 7. LMS (Learning Management System)

## Features

- Course Management
- Video Lessons
- Assignments
- Quizzes
- Exams
- Progress Tracking
- Certificates
- Discussion Boards

---

# 8. Social Networking

## Features

- Institution Feed
- Classroom Groups
- Announcements
- Student Interactions
- Events
- Comments & Reactions

## Moderation

- Reporting
- Role restrictions
- Content moderation
- Approval workflows

---

# Mobile Applications

# Student App

## Features

- Attendance
- Timetable
- LMS Access
- Assignments
- Notifications
- Fee Payments
- Student Feed

---

# Parent App

## Features

- Child Tracking
- Attendance Monitoring
- Fee Payments
- Notifications
- Leave Requests
- Parent Communication

---

# Recommended Technology Stack

# Backend

- ASP.NET Core 9
- Clean Architecture
- CQRS + MediatR
- Entity Framework Core
- FluentValidation
- Serilog

---

# Frontend

## Admin Portal

- Angular
- Angular Material
- AG Grid
- RxJS

---

# Mobile

- Flutter
- Riverpod
- Dio

---

# Database

- MySQL 8 *(implemented; was planned as PostgreSQL)*
- Redis
- RabbitMQ

---

# Storage

## Initial

- Local Storage

## Later

- MinIO
- S3-compatible object storage

---

# Architecture Strategy

# Recommended Architecture

## Initial

- Modular Monolith

## Later Scale

- Microservices

---

# Multi-Tenant Strategy

## Initial

Shared database with:

- TenantId
- InstitutionId

on all entities.

---

# Core Architectural Principles

## Principles

- Multi-tenant first
- Fully auditable
- Event-driven workflows
- Strong RBAC
- API-first design
- Modular domain boundaries
- Async processing
- Production-ready deployment

---

# RBAC Strategy

# Recommended Authorization Model

RBAC + Permission Claims + Scope Validation

## Permission Examples

- student.read
- student.create
- attendance.mark
- fees.collect
- finance.journal.post

## Scope Examples

- Institution
- Campus
- Class
- Own Children
- Assigned Classes

---

# Audit Strategy

# Everything Important Must Be Auditable

## Audit Coverage

- Student updates
- Attendance edits
- Fee collection
- Refunds
- Role changes
- Permission changes
- Financial postings
- Login activity

## Audit Principles

- Immutable logs
- Append-only history
- Tenant-aware auditing
- Correlation IDs

---

# Deployment Strategy

# Phase 1

Single VPS Deployment:

- Ubuntu
- Docker
- Docker Compose
- Nginx
- MySQL 8 *(implemented; was planned as PostgreSQL)*
- Redis
- RabbitMQ

---

# Phase 2

Split infrastructure:

- Dedicated DB server
- Multiple API nodes
- Shared Redis

---

# Phase 3

Container orchestration:

- Kubernetes
- Helm
- ArgoCD

---

# Development Strategy

# AI-Assisted Development

Use Cursor for:

- CRUD generation
- Angular pages
- API scaffolding
- DTOs
- Validators
- Docker configs
- Test generation

Human ownership required for:

- Architecture
- Security
- Finance logic
- RBAC
- SaaS boundaries

---

# Recommended Repository Structure

```text
/backend
/frontend-admin
/mobile
/devops
/prompts
/templates
/docs
```

---

# Cursor Prompt Infrastructure

## Prompt Categories

- Architecture prompts
- CRUD generation prompts
- Angular UI prompts
- Flutter feature prompts
- Deployment prompts
- Audit prompts
- RBAC prompts

---

# Task Splitting Strategy

# Development Hierarchy

## Level 1

Domains:

- Identity
- Students
- Attendance
- Fees
- Finance
- LMS

## Level 2

Modules:

- Student Profile
- Attendance Analytics
- Fee Collection

## Level 3

Features:

- Create Student
- Mark Attendance
- Collect Fees

## Level 4

Technical Tasks:

- Entity
- DTO
- Validator
- CQRS
- API
- UI
- Tests
- Audit
- RBAC

---

# Recommended Development Phases

# Phase 1 — Foundation

- Clean Architecture
- Auth
- RBAC
- Tenant Middleware
- Audit Infrastructure
- Shared Services

---

# Phase 2 — Core ERP

- Institutions
- Students
- Attendance
- Fees

---

# Phase 3 — Finance

- Double-entry accounting
- Reports
- Ledger system

---

# Phase 4 — Mobile Apps

- Student app
- Parent app

---

# Phase 5 — LMS

- Courses
- Assignments
- Quiz Engine

---

# Phase 6 — Social Networking

- Feed
- Comments
- Moderation

---

# Security Principles

## Mandatory Controls

- RBAC
- Tenant isolation
- Audit logging
- Rate limiting
- File validation
- JWT validation
- Session tracking
- Encrypted secrets

---

# Monitoring & Observability

## Recommended Stack

- Serilog
- Seq
- Prometheus
- Grafana

---

# CI/CD Strategy

## Pipeline

```text
Commit
 → Build
 → Test
 → Docker Build
 → Deploy
 → Health Check
```

---

# MVP Recommendation

## Initial MVP Scope

Include:

- Student Management
- Attendance
- Fees
- Parent App
- Notifications

Exclude:

- Advanced LMS
- Social networking
- AI features
- Complex analytics

---

# Long-Term Vision

## Future Enhancements

- AI analytics
- Predictive attendance
- Advanced reporting
- Multi-region deployment
- Dedicated tenant infrastructure
- Enterprise integrations
- Offline-first mobile support

---

# Final Engineering Principles

## Key Principles

- Build modularly
- Keep domains isolated
- Audit everything important
- Design for SaaS from day one
- Use AI for acceleration, not architecture ownership
- Avoid premature microservices
- Keep infrastructure simple initially
- Focus on institution workflows first

