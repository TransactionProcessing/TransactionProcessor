# Codex automated approvals

This workflow **never merges PRs**. It does not check out or execute PR code.

## Enable only after validation
1. Create a separate GitHub App (not the PR author's identity) installed on this repository. Grant **Pull requests: read/write**, **Checks: read**, **Issues: read**, **Contents: read** and Metadata read.
2. Store its App ID and PEM private key as repository Actions secrets `PR_REVIEW_APP_ID` and `PR_REVIEW_APP_PRIVATE_KEY`.
3. Verify GitHub permits this App to submit an APPROVE review and whether that review counts toward your branch protection requirements. GitHub may not count bot approvals as a required human review.
4. Confirm the Codex summary format and exact required check names / GitHub App slugs still match PR #2039. Update the workflow if any check is renamed or removed.
5. Set repository Actions variable `CODEX_AUTO_APPROVAL_ENABLED` to `true` **only after testing**. Without it, the workflow is inert.

## Behaviour
- Runs when the Codex bot creates/edits its PR summary and every 15 minutes as a fallback for CI finishing later; also supports manual dispatch.
- Requires the latest summary to have `code_review.status=completed`, matching head SHA, and both findings flags explicitly false.
- Requires the six named CI checks to have completed successfully on the same head SHA.
- Fails closed if a requested security review is incomplete or has findings.
- Only reviews open, non-draft, same-repository PRs targeting `main`; ignores forks.
- Does not auto-merge, enable auto-merge, push commits, or change branch protection.
- Does not revoke prior approvals. Configure branch protection to dismiss stale approvals on new commits.
- Review summaries are bot-authored PR comments, not a signed contract. This is a best-effort automation gate, **not** a security boundary or a substitute for human review of sensitive changes.

## Test
1. With the enable variable unset, merge this workflow PR manually.
2. Create a test PR and observe the workflow without approvals.
3. Configure the App and enable the variable.
4. Test clean review + all checks passing; Codex findings; failed checks; and a new commit after approval.
5. Confirm the App's approval counts as expected. If it does not, keep formal human review mandatory.
