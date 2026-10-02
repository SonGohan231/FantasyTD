# Higgsfield API bridge — separate prepaid balance

This tooling intentionally uses the **Higgsfield developer API** at `https://api.higgsfield.ai`.
It does **not** use the ChatGPT @Higgsfield MCP workspace or its credits.

## Secret

Create a GitHub Actions secret named:

`HF_API_KEY`

Paste the complete API key copied from the Higgsfield API console into that GitHub secret.
Never commit the value to this repository and never put it in `.higgsfield/request.json`.

Official REST authentication used by the client:

`Authorization: Key <complete copied API key>`

## Safety

- Paid generation runs only on branch `visual/higgsfield-api-bridge`.
- Only a commit changing `.higgsfield/request.json` can trigger the paid job.
- `enabled` must be the JSON boolean `true`.
- Manual reruns are blocked by `github.run_attempt == 1`.
- The API host is hard-coded.
- Only allowlisted model paths are accepted.
- Video is capped at 4-8 seconds and 480p/720p.
- Game videos have audio generation disabled to avoid unnecessary spend.
- Workflow permissions are read-only and checkout does not retain GitHub credentials.

Do not leave `enabled:true` in the request file after a generation request unless you intend the next request-file commit to spend again.

## Current allowlist

- `higgsfield-ai/soul/v2/standard`
- `z-image/turbo`
- `recraft/v4.1/text-to-image`
- `bytedance/seedance-2.5/text-to-video`
- `bytedance/seedance-2.5/image-to-video`

## How ChatGPT/Dyrygent can use this

Once the GitHub secret is configured, ChatGPT can write a new request to
`.higgsfield/request.json` on this branch. That push starts GitHub Actions.
The workflow calls the **separate Higgsfield API prepaid account**, waits for completion,
downloads the generated media and stores it as a GitHub Actions artifact.

This makes the prepaid API balance controllable through the same GitHub workflow pattern
already used by the Unity Executor, without exposing the Higgsfield credential to ChatGPT,
the repository, or the generated artifact.
