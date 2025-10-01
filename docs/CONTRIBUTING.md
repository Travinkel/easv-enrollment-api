## Branching Strategy

We follow a **Git Flow-inspired workflow** with semantic commit messages.

### Permanent branches
- `main` → production branch, auto-deploys to Fly.io.
- `develop` → integration branch, accumulates features for the next release.

### Supporting branches
- `feature/*` → new features, branched from `develop`, merged back into `develop`.
- `fix/*` → bug fixes, branched from `main`, merged into both `main` and `develop`.
- `release/*` → optional, stabilize `develop` into a release before merging to `main`.
- `chore/*` → CI/CD, tooling, docs improvements.

### Workflow
1. Start new work from `develop`:
   ```bash
   git checkout develop
   git pull
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