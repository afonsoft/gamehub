---
name: dotnet-github-actions
description: 'Use when creating, updating, or standardizing GitHub Actions workflows for a .NET repository, especially C# libraries, NuGet packages, ABP/EAF web apps, or Angular frontends.'
---

# .NET GitHub Actions Playbook

Playbook for generating consistent CI/CD, security, quality, and publishing workflows for afonsoft .NET repositories. Patterns are distilled from `metar-decoder`, `QRCoder.Core`, and `EAF`.

## When to Use

- Bootstrapping CI/CD for a new .NET repo.
- Refactoring existing `.github/workflows` to match org conventions.
- Adding security scanning, code quality, or NuGet publishing to a .NET project.
- Repositories with Angular frontends, ABP/EAF templates, or multi-target class libraries.

## Repository Archetypes

| Archetype | Examples | Primary artifacts | Templates to start |
|---|---|---|---|
| .NET class library / NuGet | metar-decoder, QRCoder.Core | `*.sln`, `src/**`, `tests/**` | `ci-dotnet-library.yml`, `code-quality.yml`, `security-scan.yml`, `publish-nuget.yml` |
| .NET web + Angular (ABP/EAF) | EAF | `*.sln`, `Templates/Api/**`, `Templates/Angular/**/package.json` | `ci-dotnet-web-angular.yml`, `code-quality.yml`, `security-scan.yml`, `publish-nuget.yml`, `release-pipeline.yml` |
| Multi-target with native deps | QRCoder.Core | SkiaSharp, `libSkiaSharp.so` | Add SkiaSharp native-library steps from the QRCoder.Core pattern |

## Workflow File Map

| Workflow | Purpose | Trigger |
|---|---|---|
| `ci-dotnet-library.yml` | Build, test, coverage, artifacts | push to `feature/*`, `bug/*`, `hotfix/*`; PR to `main` |
| `ci-dotnet-web-angular.yml` | Build .NET API + Node/Angular frontend | push to `develop`, `feature/*`, `release/*`; PR to `main`/`develop` |
| `code-quality.yml` | Qodana, SonarQube, Snyk, metrics | PR to `main`/`develop`, push to `main`/`releases/*` |
| `security-scan.yml` | CodeQL, Snyk, SonarQube | push/PR to `main`/`develop`, weekly schedule |
| `publish-nuget.yml` | Validate, pack, push to GitHub Packages + NuGet.org, GitHub Release | tags `v*`, release published, `workflow_dispatch` |
| `release-pipeline.yml` | Production build, staging deploy, production deploy | push to `main`/`release/*`, release published |
| `auto-pr-dependency-update.yml` | Check outdated NuGet packages and open update PRs | push to `main`, weekly, `workflow_dispatch` |
| `auto-pr-branch-sync.yml` | Sync `main` into active branches | push to `main`, `workflow_dispatch` |
| `opencode.yml` | Trigger OpenCode agent on `/oc` comment | issue / PR review comment |
| `opencode-auto-fix.yml` | Trigger OpenCode on `fix-me` label | issue / PR labeled |
| `cleanup-artifacts.yml` | Delete workflow artifacts older than 30 days | daily schedule |

## Common Environment & Versions

Use these defaults unless `global.json`, `package.json`, or `Directory.Build.props` say otherwise.

| Tool | Version / Action |
|---|---|
| .NET SDK | `10.0.x` |
| Node.js | `20` |
| Java (SonarQube) | `17` (Zulu) |
| `actions/checkout` | `v7` |
| `actions/setup-dotnet` | `v5` |
| `actions/setup-node` | `v4` |
| `actions/cache` | `v6` |
| `actions/upload-artifact` | `v7` |
| `actions/download-artifact` | `v8` |
| `NuGet/setup-nuget` | `v4` |
| `actions/setup-java` | `v5` |
| `github/codeql-action/*` | `v4` |
| `JetBrains/qodana-action` | `v2026.1.3` |
| `codecov/codecov-action` | `v7` |
| `peter-evans/create-pull-request` | `v8` |
| `softprops/action-gh-release` | `v1` |
| `anomalyco/opencode/github` | `b1fc8113948b518835c2a39ece49553cffe9b30c` |

## Required Secrets

| Secret | Used in | Notes |
|---|---|---|
| `GITHUB_TOKEN` | all workflows | auto-injected |
| `NUGET_TOKEN` | publish-nuget | NuGet.org API key |
| `SNYK_TOKEN` | security-scan, code-quality | optional; skip gracefully if missing |
| `SONAR_TOKEN` | code-quality | SonarCloud token; use `sonar.token` or `sonar.login` |
| `QODANA_TOKEN` | code-quality | JetBrains Qodana token |
| `CODECOV_TOKEN` | ci-dotnet-library | optional; upload coverage |
| `OMNIROUTE_API_KEY` | opencode | OpenCode agent API key |
| `FTP_PUBLISH_USER` | release-pipeline | optional FTP deploy |
| `FTP_PUBLISH_PASSWORD` | release-pipeline | optional FTP deploy |
| `FTP_PUBLISH_URL` | release-pipeline | optional FTP deploy |
| `SLACK_WEBHOOK_URL` | release-pipeline | optional deploy notification |

## Per-Repo Discovery Checklist

Before generating workflows, identify:

1. Main solution file(s): `*.sln` (e.g., `MetarDecoder.sln`, `QRCoder.Core.sln`, `Eaf.sln`).
2. Test project(s): directory or `*.Tests.csproj`.
3. Target frameworks: `<TargetFrameworks>` in csproj (e.g., `netstandard2.0;net8.0;net10.0;net48`).
4. Frontend: existence of `package.json` and Angular/React scripts.
5. Native dependencies: e.g., SkiaSharp, libfontconfig, `libSkiaSharp.so`.
6. Branching model: `main` only, or `develop` + `main` + `release/*`.
7. Publish output: NuGet packages, FTP deploy, Docker image.
8. Existing secrets: which of the above secrets are configured.

## Customization Variables

Every template uses these `env` variables. Replace the placeholder values at the top of each workflow:

- `DOTNET_VERSION` — .NET SDK version (`10.0.x`).
- `SOLUTION_FILE` — primary `.sln` path.
- `TEST_PROJECTS` — glob or paths for test csproj; leave empty to run solution tests.
- `NODE_VERSION` — Node.js version (`20`).
- `ANGULAR_WORKDIR` — path to Angular app (e.g., `Templates/Angular/Eaf.ProjectName.UI`).
- `SONAR_PROJECT_KEY` — SonarCloud project key, usually `owner_repo`.
- `NUGET_SOURCE_GH` — `https://nuget.pkg.github.com/afonsoft/index.json`.
- `NUGET_SOURCE_NUGET` — `https://api.nuget.org/v3/index.json`.

## Template Index

Templates are in `templates/`. Copy the ones that match your archetype into `.github/workflows/`.

- `templates/ci-dotnet-library.yml`
- `templates/ci-dotnet-web-angular.yml`
- `templates/code-quality.yml`
- `templates/security-scan.yml`
- `templates/publish-nuget.yml`
- `templates/release-pipeline.yml`
- `templates/auto-pr-dependency-update.yml`
- `templates/auto-pr-branch-sync.yml`
- `templates/opencode.yml`
- `templates/opencode-auto-fix.yml`
- `templates/cleanup-artifacts.yml`

## LLM Instructions

When asked to create GitHub Actions for a .NET repo:

1. Run the discovery checklist above.
2. Choose the matching archetype.
3. Copy the relevant templates into `.github/workflows/`.
4. Replace `env` placeholders with real values.
5. Standardize on `SONAR_TOKEN` and `SNYK_TOKEN` unless the repo already uses a different spelling (e.g., older QRCoder.Core workflows use `SONNAR_TOKEN`).
6. Add repository-specific native-dependency steps (SkiaSharp, libfontconfig) only when the project needs them.
7. Ensure CodeQL and OpenCode jobs have correct `permissions`.
8. Keep secrets out of plaintext; always use `${{ secrets.NAME }}`.
9. Validate YAML syntax and run a dry-run if possible.
10. Do not rename `main` to `master` unless the repo already uses `master`.

## Quality Checklist

- [ ] All `env` placeholders replaced.
- [ ] Solution/project paths are correct.
- [ ] Branch triggers match the repo branching model.
- [ ] `permissions` set on CodeQL and OpenCode jobs.
- [ ] Secrets referenced with `${{ secrets.NAME }}`.
- [ ] Action versions are pinned or use a trusted release tag.
- [ ] `continue-on-error` only used for optional quality/security tools, never for build/test.
- [ ] Test artifacts uploaded with `if: always()`.

## References

- [GitHub Actions documentation](https://docs.github.com/en/actions)
- [Agent Skills specification](https://agentskills.io)
- Repository conventions: `AGENTS.md` in this repo.
