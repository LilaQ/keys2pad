# Release-Compliance-Record

Produkt: IPAC XInput Bridge<br>
Version: 0.1.0 (Entwicklungsfassung)<br>
Letzte Prüfung: 12. August 2026<br>
Geplante Plattform: direkte Distribution für Windows 10/11

Dieses Dokument ist eine technische Release-Akte, keine Rechtsberatung und keine Behauptung rechtlicher Konformität.

| Bereich | Aktueller Stand | Vor Release |
|---|---|---|
| Datenschutz | lokale Verarbeitung; keine Telemetrie, Werbung, Konten oder Netzwerkübertragung | Binary/Netzwerkverkehr erneut verifizieren; öffentliche Policy-URL vom Eigentümer freigeben |
| Berechtigungen | App `asInvoker`; HKCU-Autostart optional; Tastaturzustand und PnP-Gerätebaum | Verhalten auf final signiertem Build prüfen; Nutzertext bestätigen |
| Treiber | externer, signierter ViGEmBus 1.22.0; EOL/archiviert | Risikoentscheidung des Eigentümers; Signatur/Hash/Quelle dokumentieren; keine stille Installation |
| Drittanbieter/OSS | ViGEm.Client MIT, ViGEmBus BSD-3-Clause, .NET Runtime | vollständige Lizenztexte/Notices aus finalem Publish-Paket beilegen und SBOM erzeugen |
| Tracking/Analytics/Ads | keine | finalen Dependency- und Traffic-Scan dokumentieren |
| Konten/Löschung | keine Konten; lokale Config manuell löschbar | Löschhinweis im finalen Supporttext bestätigen |
| Zahlungen/Abos | keine | bei Änderung neu prüfen |
| Verschlüsselung/Export | keine eigene Kryptografie; Windows Named Pipe CurrentUserOnly | Export-/Sanktionsprüfung entsprechend Distributionsländern durch Eigentümer |
| Alters-/Inhaltsrating | reines Eingabewerkzeug, kein kuratierter Inhalt | Anforderungen des gewählten Stores/Vertriebskanals prüfen |
| Barrierefreiheit | native Windows-Controls, Tastaturbedienung grundsätzlich möglich | NVDA/Narrator, Kontrast, DPI 100–200 %, Keyboard-only und Fokusreihenfolge testen; keine Erklärung vor Test abgeben |
| Inhalte/Assets | System-Icon; keine externen Medien | finalen Namen/Logo/Rechte prüfen |
| Support/Privacy URLs | nicht vorhanden | **RELEASE-BLOCKER:** echte eigentümerfreigegebene HTTPS-URLs bereitstellen |
| Rechtliche Identität/Kontakt | nicht vorhanden | **RELEASE-BLOCKER:** Verantwortlicher, Anschrift/Region, Supportkontakt und ggf. Trader-/Seller-Status entscheiden |
| EULA/Nutzungsbedingungen | nicht festgelegt | **RELEASE-BLOCKER:** Eigentümer entscheidet Lizenz/EULA/Gewährleistung; nichts erfinden |
| Codesignierung | nicht eingerichtet | **RELEASE-BLOCKER:** finalen EXE-Build signieren, SmartScreen-Reputation/Installer prüfen |
| Store-Angaben | keine Store-Distribution festgelegt | je Kanal aktuelle offizielle Anforderungen, Datenschutzfelder, Ratings, Support-URL und Review Notes ausfüllen |
| Regionen/Steuer/Banking | nicht festgelegt | Eigentümerentscheidung und ggf. Händler-/Steuerpflichten dokumentieren |

## Offizielle technische Quellen

- Microsoft: XInput unterstützt vier Controller; Benutzerindizes werden automatisch gesetzt: https://learn.microsoft.com/windows/win32/xinput/getting-started-with-xinput
- Microsoft: XInputGetState und Geräteverbindungsstatus: https://learn.microsoft.com/windows/win32/api/xinput/nf-xinput-xinputgetstate
- ViGEmBus-Projekt/EOL und unterstützte Systeme: https://github.com/nefarius/ViGEmBus
- ViGEmBus Release 1.22.0: https://github.com/nefarius/ViGEmBus/releases/tag/v1.22.0

Unmittelbar vor einer Veröffentlichung müssen die Anforderungen des tatsächlich gewählten Stores/Vertriebskanals erneut aus dessen offiziellen Quellen geprüft werden. Änderungen an Netzwerk, SDKs, Berechtigungen, Analyse, Zahlungen, Konten, Inhalten, KI-Verarbeitung, Regionen oder Vertrieb lösen eine vollständige Neuprüfung aus.
