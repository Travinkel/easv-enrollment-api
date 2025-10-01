# Contributing & Branch Workflow

We follow a simple **GitHub Flow** style with semantic commit messages.

## Branches
- `main` → production (auto-deploys to Fly.io).
- `feature/*` → new features (merged via PR).
- `fix/*` → bug fixes.
- `chore/*` → CI/CD, tooling, docs.

## Workflow
1. **Create branch**
   ```bash
   git checkout -b feature/enrollment-confirm

2. **Commit with semantic style**

feat(api): add confirm enrollment endpoint

fix(api): correct CORS policy

chore(ci): add GitHub Actions pipeline

3. **PR to main**

CI will build & test automatically.

Reviewer (your future self or teammates) checks ADR/doc update.

4. **Merge to main**

Auto-deploys to Fly.io.

### Documentation

All significant design choices captured in docs/adr/*.md.

Each feature branch should include an ADR or doc update.

### Semantic Commit Types

- feat: new endpoint, domain logic

- fix: bug fix

- chore: infra, tooling, no user impact

- docs: documentation

- test: add or improve tests

- refactor: internal code change without new features