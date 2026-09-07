# Development — 0.2.0

Build with Source/Build.ps1 against locally installed RimWorld 1.6 and RimTalk assemblies. No dependency binaries are redistributed.

The core CampfireStories.dll references only game/framework assemblies. LoadFolders.xml loads Integrations/RimTalk conditionally. CampfireStories.RimTalk.dll implements IStoryService and delegates networking to RimTalk. Expand Memory is read via public reflection, including derived component types. No separate credential storage exists.

Source/Validate.ps1 validates localization and XML. Source/Localize.ps1 regenerates four-language translations. Game diagnostics require explicit -campfire-selftest; the disposable ritual test additionally requires -campfire-playtest and -quicktest. Tests refuse an API-configured environment. They are not activated by ordinary play.

Preserve packageId, defNames and PublishedFileId across releases. Never bundle copied test dependencies, decompiled game sources, save folders or API settings. PrivateWorkshopReceipt.txt records upload visibility verification and is local bookkeeping.
