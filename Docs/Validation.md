# Validation — 0.2.0

Tested 2026-09-06 with RimWorld 1.6.4871 rev591 in isolated quicktest save directories. User saves, API credentials and active ModsConfig were not used or changed.

Passed: core and optional adapter compile; 27 XML files parse; four languages each have 39 matching translated fields, valid targets and placeholders. Nine game definition checks pass.

All three installations (Core + Ideology + addon; plus RimTalk; plus RimTalk + Expand Memory) complete a three-storyteller ritual without any API configuration. Each storyteller was observed standing facing the actual audience. No generated fallback lines were displayed; the ordinary mood outcome was applied.

RimTalk-only test selects the first pawn's built-in conversation history and excludes user prompts and the second pawn's history. Expand Memory test selects the first pawn's expanded memory and excludes RimTalk history and other pawn memories. The adapter recognizes derived memory components.

Author-reported verification, 2026-09-07: live API dialogue, complete save/load cycles, custom races, large mod lists and audience-facing behavior passed in actual play. These are the author's tests, separate from the automated quicktests above. No new runtime change was needed for this documentation update.

RimTalk history becoming empty after restarting is normal behavior. Present-day topics without memories and model-dependent text length are expected behavior, not pending defects. Other scenario-specific regression checks remain in the development walkthrough; the author's successful tests do not imply compatibility with every possible mod combination.

Test logs are retained outside the release package under the isolated test-base, test-rimtalk and test-expanded directories. No paid API request was sent.
