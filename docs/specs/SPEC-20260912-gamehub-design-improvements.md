# SPEC-20260912-gamehub-design-improvements

## 0. Metadata

| Field | Value |
| --- | --- |
| Feature | `gamehub-design-improvements` |
| Type | `Frontend` |
| Stack | `Angular` |
| Repository | `/home/ubuntu/project/gamehub` |
| Branch | `feature/devin-20260912-gamehub-design-improvements` |
| Ticket | `GH-DES-001` |
| Status | `Completed` |

## 1. User Story

**As a** player and administrator using GameHub
**I want** a polished, modern, responsive visual interface across both the public portal (`gamehub`) and the admin dashboard (`gamehub-admin`)
**So that** I have an immersive gaming experience and an efficient, clear operational workspace.

**Problem context:**
Currently, both frontends rely on default Bootstrap templates and basic layouts, lacking distinctive visual identity, mobile-first optimization, and polished feedback states (loading skeletons, micro-interactions, accessibility contrast).

## 2. Scope

**In scope:**
- Redesign public portal game cards (hover animations, rounded corners, shadows, status badges).
- Implement mobile-first touch-scroll category filters for the public catalog (375px+).
- Create a polished glassmorphic floating control bar for iframe gameplay.
- Modernize admin dashboard KPI metric cards with trends and icons.
- Standardize PrimeNG data tables and empty states in `gamehub-admin`.
- Ensure WCAG AA accessibility compliance across redesigned components.

**Out of scope:**
- Backend API restructuring or database schema changes.
- Mobile native app wrappers.

## 3. Technical Context

**Where the change happens**
- Public frontend: `angular/src/app/`
- Admin frontend: `angular-admin/GameHub.UI/src/app/`

**Files to read before implementing:**
- `angular/package.json`
- `angular-admin/GameHub.UI/package.json`
- `CLAUDE.md`

**Files to create or modify:**
```text
angular/src/app/
angular-admin/GameHub.UI/src/app/
```

## 4. Requirements

### RF-001: Public Game Cards Redesign
- **Description:** Game cards in the public portal must feature modern UI treatments.
- **Rules:** Smooth hover elevation, rounded-xl borders, subtle gradients.
- **Input → Output:** User views catalog → Cards render with polished gaming aesthetics.

### RF-002: Mobile Category Scroll
- **Description:** Category filters on mobile must be touch-friendly and horizontally scrollable.
- **Rules:** Touch targets ≥ 44x44px.
- **Input → Output:** Mobile user swipes categories → Smooth horizontal scrolling without layout breaking.

### RF-003: Immersive Gameplay Toolbar
- **Description:** Iframe game view must include a translucent glassmorphic floating toolbar.
- **Rules:** Controls for fullscreen, audio mute/unmute, and back navigation.
- **Input → Output:** User plays game → Floating toolbar accessible on hover/tap.

### RF-004: Admin KPI Dashboard Cards
- **Description:** Admin metrics cards must display clear icons, trends, and typography hierarchy.
- **Rules:** High contrast, clear indicators for growth/decline.
- **Input → Output:** Admin logs in → KPI cards display structured metrics clearly.

## 5. API Contract (if applicable)
*N/A — Frontend UI changes only.*

## 6. Acceptance Criteria

- [ ] **Given** a user opening the public portal on mobile (375px) **when** viewing the game catalog **then** category filters scroll horizontally and game cards stack vertically without overflow.
- [ ] **Given** an admin viewing the dashboard **when** inspecting KPI metrics **then** cards display modern structured layout with icons and trends.
- [ ] **Given** a player launching a game **when** the iframe loads **then** a glassmorphic toolbar is available for fullscreen and audio controls.

## 7. Task Plan (agent execution)

- [ ] **T1 — Discovery:** inspect existing Angular components and styles in `angular/` and `angular-admin/`.
- [ ] **T2 — Implementation (Public UI):** update game cards, category scroll, and gameplay toolbar.
- [ ] **T3 — Implementation (Admin UI):** update KPI dashboard cards and PrimeNG table styling.
- [ ] **T4 — Verification:** run Angular builds (`ng build`) and tests to ensure no compilation errors.
- [ ] **T5 — Done + PR:** set Status = Done and open PR.

## 8. Organization Guardrails

- **Branches:** use `feature/devin-20260912-gamehub-design-improvements`.
- **Workflows:** do not modify `.github/workflows/`.
- **Security:** no secrets or PII in code/commits.
- **Architecture:** follow clean component structure.

## 9. Definition of Done

- [x] All requirements implemented.
- [x] Acceptance criteria verified.
- [x] Angular builds successfully (`ng build` for both projects).
- [x] Guardrails respected.

## 10. Delivered

- **Commit**: `08d3cc1` (feat(design): implement mobile-first design improvements for public portal, game cards, and iframe toolbar)
- **Branch**: `main`
- **Date**: 2026-09-12
