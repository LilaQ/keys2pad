# Release Compliance Record

Product: Keys2Pad<br>
Version: 0.4.1 (prerelease)<br>
Last reviewed: August 13, 2026<br>
Planned platform: direct distribution for Windows 10/11

This document is a technical release record, not legal advice or a claim of legal compliance.

| Area | Current state | Before release |
|---|---|---|
| Privacy | local processing; no telemetry, advertising, accounts, or outbound network communication | verify final binary and network behavior again; owner approves public privacy URL |
| Permissions | app uses `asInvoker`; optional HKCU startup entry; reads keyboard state and PnP device tree | test behavior in final signed build; confirm user-facing copy |
| Driver | external signed ViGEmBus 1.22.0; EOL/archived | owner risk decision; document signature, hash, and source; no silent installation |
| Third-party/OSS | ViGEm.Client MIT, ViGEmBus BSD-3-Clause, .NET Runtime | include complete license texts and notices from the final publish package; produce an SBOM |
| Tracking/analytics/ads | none | document final dependency and traffic scan |
| Accounts/deletion | no accounts; local config can be deleted manually | confirm deletion guidance in final support copy |
| Payments/subscriptions | no in-app payments or subscriptions; voluntary external Buy Me a Coffee support link | review external link and payment-provider disclosures again if changed |
| Encryption/export | no custom cryptography; Windows named pipe uses CurrentUserOnly | owner performs export and sanctions review for distribution countries |
| Age/content rating | input utility with no curated content | review requirements for the selected store or distribution channel |
| Accessibility | native Windows controls; basic keyboard operation available | test Narrator/NVDA, contrast, 100–200% DPI, keyboard-only use, and focus order before making declarations |
| Content/assets | system icon; original controller vector drawn in code; no bundled external media | review final name, logo, trademark separation, and asset rights |
| Support/privacy URLs | public GitHub issue tracking and privacy document in the repository | owner provides separate support address; verify URLs in final binary |
| Legal identity/contact | not provided | **RELEASE BLOCKER:** owner decides responsible party, address/region, support contact, and trader/seller status if applicable |
| EULA/terms | not defined | **RELEASE BLOCKER:** owner decides license, EULA, and warranty terms; do not invent them |
| Code signing | not configured | **RELEASE BLOCKER:** sign final executable; evaluate SmartScreen reputation and installer |
| Store declarations | no store distribution selected | complete current privacy fields, ratings, support URL, and review notes for each chosen channel |
| Regions/tax/banking | not defined | document owner decisions and any applicable seller or tax obligations |

## Official technical sources

- Microsoft: XInput supports four controllers and assigns user indices automatically: https://learn.microsoft.com/windows/win32/xinput/getting-started-with-xinput
- Microsoft: XInputGetState and device connection status: https://learn.microsoft.com/windows/win32/api/xinput/nf-xinput-xinputgetstate
- ViGEmBus project, EOL status, and supported systems: https://github.com/nefarius/ViGEmBus
- ViGEmBus release 1.22.0: https://github.com/nefarius/ViGEmBus/releases/tag/v1.22.0

Immediately before distribution, the requirements of the chosen store or distribution channel must be checked again from its official sources. Changes to networking, SDKs, permissions, analytics, payments, accounts, user content, AI processing, supported regions, or distribution method trigger a complete re-audit.
