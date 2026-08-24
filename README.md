# SFGame Guild Reporter  
A small utility tool for Shakes & Fidget guild officers to parse guild fight reports, track participation, and send formatted summaries to Discord.

The tool reads exported fight-report `.txt` files from the game, extracts:
- who signed up,
- who did not sign up,
- player levels,
- and generates Discord-friendly output.

It also stores historical participation data and can warn about weekly offenders (players who repeatedly miss fights).

---

## 📦 Features

### ✔ Parse SFGame guild fight reports  
Supports **English** and **Hungarian** report formats:
- “Members that did not sign up”
- “Members that signed up”
- “Tagok, akik nem jelentkeztek”
- “Tagok, akik jelentkeztek”

### ✔ Extract player names and levels  
Supports both formats:
- `(Level 250)`
- `(250. szint)`

### ✔ Discord notifications  
Sends a formatted message to a Discord webhook.

### ✔ Weekly offender tracking  
Warns if a player missed fights repeatedly within the last 7 days.  
Threshold is configurable.

### ✔ History storage  
Every parsed report is saved as a JSON file in the `history/` folder.

### ✔ Configurable settings  
All settings are stored in `Config/config.json`.

---

## 📁 Folder Structure

Your release ZIP should contain:

SFGameGuildReporter.exe
Config/
config.json
history/
reports/

### `Config/config.json`
Contains:
```json
{
  "Webhook": "YOUR_DISCORD_WEBHOOK_URL",
  "ReportsFolder": "reports",
  "AutoPickNewestReport": false,
  "WeeklyOffenderThreshold": 3
}

reports/
Place your .txt fight reports here.

history/
The tool automatically stores parsed reports here as JSON.


### `▶ How to Use (English)`
Export a guild fight report from SFGame as a .txt file (select all text, copy and paste into a new .txt file).
(It's only working from a PC browser/Steam app, not mobile.)

Place the file into the reports/ folder.

Run SFGameGuildReporter.exe.

When prompted, enter the path to the .txt file
(or enable AutoPickNewestReport in config).

The tool:

parses the report,

saves it to history/,

sends a formatted message to Discord,

checks weekly offenders and sends warnings.

That’s it — simple and fast.

### `▶ Hogyan kell használni (Hungarian)`
Exportáld a céhes harcjelentést .txt fájlba a játékból.

Másold be a fájlt a reports/ mappába.

Indítsd el a SFGameGuildReporter.exe programot.

Add meg a .txt fájl elérési útját
(vagy kapcsold be az AutoPickNewestReport opciót a configban).

A program:

feldolgozza a jelentést,

elmenti a history/ mappába,

elküldi Discordra a formázott üzenetet,

ellenőrzi a heti hiányzókat és figyelmeztetést küld.

Ennyi — gyors, egyszerű, automatikus.


⚙ Configuration
All settings are in Config/config.json.

Discord webhook
Set your guild’s webhook URL.

Weekly offender threshold
Number of missed fights required to trigger a warning.

Auto-pick newest report
If enabled, the tool automatically selects the newest .txt file in reports/.

📝 Notes
The tool supports both English and Hungarian SFGame report formats.

Fight-type separation (Raid / Attack / Defense / Pet Battle / Portal) is planned but not yet implemented.

No GUI, no autorun — intentionally simple and lightweight.