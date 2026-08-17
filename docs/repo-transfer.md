# Moving the repository to a different owner

Everything coupled to the owner's name, in the order it has to be done.

The repository name stays `olympiad-quizzer-net`, so the Pages base href
(`/olympiad-quizzer-net/`) is unaffected. What changes is the **owner**, and with it the Pages
origin, which the API validates.

**Read this first:** the frontend will be broken between the transfer and the backend redeploy.
The API only accepts requests from the old Pages origin until step 4 lands. Pick a moment when
that gap does not matter.

---

## 1. Before touching anything

- [ ] Note whether the self-hosted runner is registered at the **organisation** or the
      **repository**. Organisation-level registration does not survive the transfer, and every CI
      job blocks until it is re-registered. Settings -> Actions -> Runners.
- [ ] Note whether `RENDER_API_URL` and `RENDER_DEPLOY_HOOK` are **repository** secrets or
      **organisation** secrets. Repository secrets move with the repository; organisation secrets
      do not and must be recreated.
- [ ] Confirm no deploy is in flight.

## 2. Transfer

- [ ] GitHub: Settings -> General -> Danger Zone -> Transfer ownership.
- [ ] Confirm branch protection survived (Settings -> Branches). Recreate if not.
- [ ] Re-enable GitHub Pages if it did not carry over: Settings -> Pages.

## 3. Update the local clone

The push prompt asking which account to use comes from Git Credential Manager holding more than
one GitHub account with no hint for this remote. The transfer changes the remote, so the cached
credential no longer matches either.

- [ ] Point the remote at the new owner:

      git remote set-url origin https://github.com/<new-owner>/olympiad-quizzer-net.git
      git remote -v

- [ ] Stop the account picker by naming the account for GitHub:

      git config --global credential.https://github.com.username <github-username>

      If the wrong account is already cached, clear it first: Windows Credential Manager ->
      Windows Credentials -> remove entries under `git:https://github.com`.

- [ ] Confirm the commit identity is the personal one and not an organisation address:

      git config user.name
      git config user.email

- [ ] Prove it end to end with a throwaway push (a branch you delete afterwards), so the first
      real push is not the one that discovers a credential problem.

## 4. Update the allowed origin - this is the one that breaks the app

The API reads the allowed Pages origin from a Render environment variable. No code change or PR
needed - this is a dashboard change followed by a redeploy.

- [ ] Render dashboard → the API service → Environment → find `Cors__AllowedOrigin` → set it to
      `https://<new-owner>.github.io`.
- [ ] Deploy the backend (trigger `deploy-backend.yml` or deploy from the Render dashboard).

**Until the new backend is live**, the frontend gets the generic error screen on every request and
the browser console shows a CORS rejection - the same symptom as an API that is down.

## 5. Update the documentation

Live URLs only; none of these affect behaviour.

- [ ] `docs/adl/ADR-004-github-pages-hosting.md`
- [ ] `docs/development.md`
- [ ] `docs/integrations/github-pages.md` - including the root `robots.txt` repository, which is
      named after the owner (`<owner>.github.io`) and has to be created under the new account for
      ADR-028 to hold.
- [ ] `docs/integrations/render-com.md`

`docs/pocs/` is deliberately left alone. A proof-of-concept document records what was true at the
time; updating it would falsify the record rather than maintain it.

## 6. Render

- [ ] Re-authorise the repository connection. The service points at a repository that has moved,
      and Render's GitHub authorisation is granted per account or organisation.
- [ ] Confirm the settings survived, particularly **Dockerfile Path** and **Docker Build Context
      Directory** - see `integrations/render-com.md`. These live only in the dashboard and nothing
      in the repository validates them.
- [ ] Deploy, and check the run fails loudly if the build fails. The deploy verifies the live
      commit rather than a bare 200, so a failed build no longer reports success.

## 7. Verify

- [ ] `https://<new-owner>.github.io/olympiad-quizzer-net/` loads.
- [ ] The quiz draws questions - this is the real CORS test, because the filter request is the
      first cross-origin call.
- [ ] `/healthz` reports the commit you deployed.

---

## Why the origin is in configuration

The allowed Pages origin is set as a Render environment variable (`Cors__AllowedOrigin`) rather
than compiled into the API. A future owner change requires only a dashboard update and a redeploy,
not a PR. The trade-off - configuration can be wrong in a place nothing validates - is accepted here
because the value is short, has one consumer, and the deploy verification step catches a missing or
wrong value immediately (CORS rejections appear in the browser console on the first quiz request).
