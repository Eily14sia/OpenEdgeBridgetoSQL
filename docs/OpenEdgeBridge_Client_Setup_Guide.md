# OpenEdge Bridge — Client-Side Setup Guide

**Version:** 1.0
**Date:** August 7, 2026
**Audience:** Client IT / personnel deploying the ID-printing system on their Windows PC.

---

## What This Guide Covers

This guide walks you through setting up everything **on your PC** so that ID-card data flows from the **central OpenEdge server** (managed by the system provider) into a **local MySQL database** (run via XAMPP), which is then read by the **Entrust Instant ID** card-printing machine.

<table style="width:100%; max-width:480px; margin:14px auto; border-collapse:collapse; font-size:9pt; font-family:Helvetica,Arial,sans-serif;">
<tr><td style="border:2pt solid #0f3d6e; padding:8px 12px; text-align:center; background:#eef4fb;">
<strong style="color:#0f3d6e;">OpenEdge Server (REMOTE)</strong><br/>
<span style="font-size:8pt;">Hosted at the provider's site</span><br/>
<em style="font-size:7.5pt; color:#666;">&larr; Provided &amp; managed by the system provider</em>
</td></tr>
<tr><td style="text-align:center; padding:7px 0; color:#0f3d6e; font-weight:bold;">
&#9660; OpenEdge ODBC Driver <em style="font-weight:normal;">(you install this)</em>
</td></tr>
<tr><td style="border:2pt solid #0f3d6e; padding:8px 12px; text-align:center; background:#eef4fb;">
<strong style="color:#0f3d6e;">OpenEdge Bridge Service</strong><br/>
<span style="font-size:8pt;">(OpenEdgeBridge.exe)</span><br/>
<em style="font-size:7.5pt; color:#666;">&larr; Runs on YOUR PC</em>
</td></tr>
<tr><td style="text-align:center; padding:7px 0;">
&#9660;
</td></tr>
<tr><td style="border:2pt solid #0f3d6e; padding:8px 12px; text-align:center; background:#eef4fb;">
<strong style="color:#0f3d6e;">MySQL (XAMPP) &mdash; LOCAL</strong><br/>
<span style="font-size:8pt;">database: instantid_bridge</span><br/>
<em style="font-size:7.5pt; color:#666;">&larr; Runs on YOUR PC</em>
</td></tr>
<tr><td style="text-align:center; padding:7px 0; color:#0f3d6e; font-weight:bold;">
&#9660; MySQL Connector/J (JDBC)
</td></tr>
<tr><td style="border:2pt solid #0f3d6e; padding:8px 12px; text-align:center; background:#eef4fb;">
<strong style="color:#0f3d6e;">Entrust Instant ID</strong><br/>
<em style="font-size:7.5pt; color:#666;">&larr; Card issuance / printing machine</em>
</td></tr>
</table>

**In short, you need to install 4 things on your PC:**

| # | What | Why |
|---|------|-----|
| 1 | **OpenEdge ODBC Driver** | Lets your PC talk to the remote OpenEdge server |
| 2 | **XAMPP** (MySQL) | Local database that Entrust reads from |
| 3 | **OpenEdgeBridge** (published folder) | The service that copies data from OpenEdge → MySQL |
| 4 | **MySQL Connector/J** (`.jar`) | Lets Entrust connect to MySQL |

> ⚠️ **Important:** The OpenEdge server itself is **remote** — you do **not** install or configure OpenEdge on your PC. You only install the **driver** that connects to it.

---

## Before You Start — Information You Need From the Provider

Ask the system provider for the following details and keep them handy:

| Item | Example | Where Used |
|------|---------|------------|
| OpenEdge server **Host** (IP or name) | `10.0.0.25` | ODBC DSN setup (Step 2) |
| OpenEdge **Port** | `20940` | ODBC DSN setup (Step 2) |
| OpenEdge **Database name** | `instantID` | ODBC DSN setup (Step 2) |
| OpenEdge **User ID / Password** | `admin` / `******` | ODBC DSN setup (Step 2) |
| OpenEdge **DSN name** to use | `OpenEdgeInstantID` | `appsettings.json` (Step 7) |
| MySQL **password** you want to set | — | XAMPP/phpMyAdmin setup (Step 4) |
| The **`publish/` folder** of the bridge | — | Deployment (Step 7) |

---

## Step 1 — Install the OpenEdge ODBC Driver

1. Get the installer file: `PROGRESS_DATADIRECT_ODBC_8.0_WIN_64_INSTALL.exe` (provided by the system provider).
2. **Right-click → Run as Administrator** and follow the installer defaults.
3. When finished, verify it is installed by opening **ODBC Data Source Administrator** (64-bit):
   - Press `Win + R`, type `odbcad32.exe`, press **Enter**.
   - You should see **Progress DataDirect OpenEdge SQL Server** (or similar) in the list of available drivers.
4. **Restart your PC** if the driver was just installed (recommended so the driver registers properly).

> ✅ **Done when:** `odbcad32.exe` shows the Progress DataDirect driver in the *Drivers* tab.

---

## Step 2 — Create the ODBC System DSN (Connection to Remote OpenEdge)

A **DSN** (Data Source Name) is a saved "connection profile" that the bridge uses to reach the remote OpenEdge server.

1. Press `Win + R`, type `odbcad32.exe`, press **Enter**.
2. Go to the **System DSN** tab → click **Add...**.
3. Select **Progress DataDirect OpenEdge SQL Server** → click **Finish**.
4. Fill in the details (from the *Before You Start* table):

   | Field | Value |
   |-------|-------|
   | **Data Source Name** | `OpenEdgeInstantID` (use exactly this name) |
   | **Description** | `OpenEdge Instant ID Database` |
   | **Host Name** | *(provided by provider — e.g., `10.0.0.25`)* |
   | **Port Number** | *(provided by provider — e.g., `20940`)* |
   | **Database Name** | *(provided by provider — e.g., `instantID`)* |
   | **User ID** | *(provided by provider — e.g., `admin`)* |
   | **Password** | *(provided by provider)* |

5. Click **Test Connect** — you should see a success message. If it fails, check the host/port/user/password with the provider.
6. Click **OK** to save the DSN.

> ✅ **Done when:** **Test Connect** succeeds, and `OpenEdgeInstantID` appears under the **System DSN** tab.

> ⚠️ **Permissions on the remote OpenEdge server:** The user you entered in the DSN must be able to read and update the `pub."id-print"` table. If your login user is **not** `admin`, ask the system provider (or run yourself from a DBA account) to execute this on the OpenEdge server, replacing `your_login_user` with your actual DSN user:
>
> ```sql
> GRANT SELECT, INSERT, UPDATE, DELETE ON pub."id-print" TO your_login_user;
> ```
>
> Without this permission, the bridge will fail with an access-denied error when it starts.

---

## Step 3 — Install XAMPP (Local MySQL)

1. Download XAMPP from **https://www.apachefriends.org/** (latest version).
2. Run the installer and accept the defaults. Install to the default location (e.g., `C:\xampp`).
3. Open the **XAMPP Control Panel** (from Start Menu or desktop shortcut).
4. Click **Start** next to **MySQL** — the row should turn green.
5. Click **Admin** next to MySQL — this opens **phpMyAdmin** in your browser (http://localhost/phpmyadmin).
6. **Set a password for the MySQL `root` user** (recommended for security):
   - In phpMyAdmin, click the **User accounts** tab.
   - Click **Edit privileges** on `root@localhost` → **Change password** → set a password you will remember.
   - Write this password down — you will need it in Step 7.
7. Keep MySQL **running** (do not close the XAMPP Control Panel, or minimize it).

> 💡 **Tip:** For the machine to work after a reboot, add XAMPP's MySQL to auto-start, or simply start MySQL each time the PC boots.

> ✅ **Done when:** MySQL shows a green **Running** status in the XAMPP Control Panel and phpMyAdmin opens.

---

## Step 4 — Create the Local Database & Table in MySQL

1. Open **phpMyAdmin** (from XAMPP Control Panel → **Admin** next to MySQL).
2. Click the **SQL** tab and paste the following:

```sql
CREATE DATABASE IF NOT EXISTS instantid_bridge
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE instantid_bridge;
```

3. Click **Go**.
4. Still in the **SQL** tab (make sure `instantid_bridge` is selected in the left panel), paste and run:

```sql
CREATE TABLE IF NOT EXISTS id_print (
    id             INT           NOT NULL AUTO_INCREMENT,
    batch_no       INT           NULL,
    batch_date     DATE          NULL,
    app_no         INT           NULL,
    dob            DATE          NULL,
    cp_no          VARCHAR(15)   NULL,
    fullname       VARCHAR(65)   NULL,
    hcp_fullname   VARCHAR(65)   NULL,
    address        VARCHAR(65)   NULL,
    hcp_cpno       VARCHAR(15)   NULL,
    lname          VARCHAR(55)   NULL,
    fname          VARCHAR(55)   NULL,
    mname          VARCHAR(55)   NULL,
    spa_docno      VARCHAR(25)   NULL,
    page_no        VARCHAR(20)   NULL,
    book_no        VARCHAR(20)   NULL,
    series_no      VARCHAR(20)   NULL,
    spa_atty       VARCHAR(65)   NULL,
    spa_dt_issue   DATE          NULL,
    printed        VARCHAR(3)    NULL        COMMENT 'yes / no (matches OpenEdge LOGICAL FORMAT)',
    printed_dt     DATE          NULL,
    printed_tm     VARCHAR(35)   NULL,
    id_type        INT           NULL        COMMENT '1 = for RTC, 2 = senior',
    image_data     LONGBLOB      NULL,
    signature_data LONGBLOB      NULL        COMMENT 'Signature image; written by Entrust Instant ID (MySQL only)',
    qr_data        LONGBLOB      NULL        COMMENT 'QR code image; written by Entrust Instant ID (MySQL only)',
    id_code        INT           NULL,
    print_count    INT           NULL        COMMENT 'Incremented by Instant ID; triggers printed=yes back-sync',
    PRIMARY KEY (id),
    UNIQUE INDEX idx_app_no (app_no)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;
```

5. Click **Go**.
6. Verify: in the left panel, expand `instantid_bridge` → you should see the `id_print` table.

> ✅ **Done when:** `instantid_bridge` database and `id_print` table appear in phpMyAdmin.

---

## Step 5 — Configure Entrust Instant ID (MySQL Connector/J)

1. Copy the file **`mysql-connector-j-8.0.33.jar`** (provided by the system provider) into Entrust Instant ID's `lib/` folder.
2. In **Entrust Instant ID**, configure the database connection to point to your **local** MySQL:

   | Setting | Value |
   |---------|-------|
   | **Host** | `localhost` |
   | **Port** | `3306` |
   | **Database** | `instantid_bridge` |
   | **User** | `root` (or a dedicated user you created) |
   | **Password** | *(the MySQL password you set in Step 3)* |
   | **Table** | `id_print` |

3. Save the settings and restart Entrust Instant ID.

> ⚠️ Entrust connects to MySQL **on your PC** (`localhost`) — not to OpenEdge. The bridge is what talks to OpenEdge.

> ✅ **Done when:** Entrust Instant ID can open/query the `id_print` table without errors.

---

## Step 6 — Deploy the Bridge Service

1. Copy the entire **`publish/`** folder (provided by the system provider) to your PC, e.g. to **`C:\OpenEdgeBridge\`**.
   - The folder must contain at least: `OpenEdgeBridge.exe`, `appsettings.json`, and the `runtimes/` subfolder.
2. **Edit `appsettings.json`** (right-click → Open with Notepad) and fill in your real values:

```json
{
  "ConnectionStrings": {
    "OpenEdge": "DSN=OpenEdgeInstantID;Uid=admin;Pwd=YOUR_OPENEDGE_PASSWORD;DefaultIsolationLevel=1;",
    "OpenEdgeRead": "DSN=OpenEdgeInstantID;Uid=admin;Pwd=YOUR_OPENEDGE_PASSWORD;DefaultIsolationLevel=1;",
    "MySql": "Server=127.0.0.1;Database=instantid_bridge;Uid=root;Pwd=YOUR_MYSQL_PASSWORD;"
  },
  "Sync": {
    "IntervalSeconds": 10
  }
}
```

| Setting | What to put |
|---------|-------------|
| `OpenEdge` / `OpenEdgeRead` | `DSN` must be **exactly** the DSN name from Step 2 (`OpenEdgeInstantID`); `Uid`/`Pwd` are the OpenEdge credentials from the provider |
| `MySql` | `Server=127.0.0.1` (your local XAMPP); `Uid=root`; `Pwd` = the MySQL password you set in Step 3 |
| `Sync:IntervalSeconds` | How often the bridge checks for new data (10 = every 10 seconds; 10–30 is recommended) |

3. Save the file.

> ⚠️ **Do not** rename or move `appsettings.json` — it must sit in the same folder as `OpenEdgeBridge.exe`.

> ✅ **Done when:** `C:\OpenEdgeBridge\` contains `OpenEdgeBridge.exe` and an edited `appsettings.json`.

---

## Step 7 — Install the Bridge as a Windows Service

1. Open **PowerShell as Administrator** (right-click Start → *Windows PowerShell (Admin)* / *Terminal (Admin)*).
2. Run (adjust the path if you copied the folder elsewhere):

```powershell
New-Service -Name "OpenEdgeBridge" `
            -BinaryPathName "C:\OpenEdgeBridge\OpenEdgeBridge.exe" `
            -DisplayName "OpenEdge Bridge Service" `
            -Description "Syncs id-print data between OpenEdge and MySQL for Entrust Instant ID" `
            -StartupType Automatic

Start-Service OpenEdgeBridge
```

3. Verify it is running:

```powershell
Get-Service OpenEdgeBridge
# Status should be: Running
```

4. **Confirm data is syncing:** open phpMyAdmin → `instantid_bridge` → `id_print` → **Browse**. After a few seconds, rows should appear that match the records in OpenEdge.

> ✅ **Done when:** Service status = **Running**, and rows appear in `id_print` automatically.

---

## Everyday Operations

### Start / Stop / Restart the service

```powershell
Start-Service OpenEdgeBridge
Stop-Service OpenEdgeBridge
Restart-Service OpenEdgeBridge      # use this after changing appsettings.json
Get-Service OpenEdgeBridge          # check status
```

### Daily startup order (after a PC reboot)

1. Open **XAMPP Control Panel** → click **Start** next to **MySQL**.
2. The **OpenEdge Bridge** service starts automatically (StartupType = Automatic). If not, run `Start-Service OpenEdgeBridge`.
3. Check that `id_print` in phpMyAdmin is receiving rows.

### Checking the logs

- Log files: `C:\OpenEdgeBridge\Logs\` (one dated file per day)
- Windows Event Viewer: *Windows Logs → Application*, source: `OpenEdgeBridge`

---

## Troubleshooting (Common Issues)

| Problem | Likely Cause | Fix |
|---------|--------------|-----|
| Service starts then stops immediately | Wrong DSN name or wrong OpenEdge credentials | Open `odbcad32.exe` → select `OpenEdgeInstantID` → **Configure → Test Connect**. Then `Restart-Service OpenEdgeBridge` |
| "Driver not found" / no DSN listed | Wrong bitness (32-bit vs 64-bit) | Always use **`odbcad32.exe`** (64-bit) — NOT the 32-bit one in `C:\Windows\SysWOW64` |
| No rows in `id_print` | MySQL not running, or bridge not running | Check XAMPP Control Panel → MySQL must be **green**; check `Get-Service OpenEdgeBridge` |
| "MySQL connection string not found" | `appsettings.json` missing or not beside the `.exe` | Confirm `appsettings.json` is in `C:\OpenEdgeBridge\` and has valid JSON |
| Duplicate rows in MySQL | Missing UNIQUE index on `app_no` | Run: `ALTER TABLE id_print ADD UNIQUE INDEX idx_app_no (app_no);` |
| Entrust can't connect to MySQL | MySQL not started, or wrong credentials in Entrust | Check XAMPP (MySQL green); re-check Entrust host `localhost`, port `3306`, database `instantid_bridge` |
| `printed` never becomes `yes` in OpenEdge | Entrust isn't incrementing `print_count` | Check Entrust is configured to the `id_print` table and is incrementing `print_count` after printing |
| Bridge can't reach remote OpenEdge | Network/firewall blocking, or wrong host/port | Verify you can ping the host and the port is reachable; confirm details with the provider |

---

## Checklist for Your PC

- [ ] OpenEdge ODBC Driver installed (64-bit)
- [ ] System DSN `OpenEdgeInstantID` created and **Test Connect** succeeds
- [ ] XAMPP installed, MySQL **running**, `root` password set
- [ ] `instantid_bridge` database created
- [ ] `id_print` table created (with `signature_data`, `qr_data`, `print_count`, and UNIQUE `app_no`)
- [ ] `mysql-connector-j-8.0.33.jar` placed in Entrust's `lib/` folder
- [ ] Entrust configured → `localhost:3306` / `instantid_bridge` / `id_print`
- [ ] `publish/` folder copied to `C:\OpenEdgeBridge\`
- [ ] `appsettings.json` edited with correct DSN name, OpenEdge user/pass, MySQL user/pass
- [ ] Windows Service installed and **Running**
- [ ] Rows visible in `id_print` (phpMyAdmin) confirming sync works
- [ ] `C:\OpenEdgeBridge\Logs\` has today's log file with no errors

---

## Contact / Escalation

If any step fails after following this guide:

1. Check the **Logs** folder (`C:\OpenEdgeBridge\Logs\`) for error messages.
2. Send the log file and the output of `Get-Service OpenEdgeBridge` to the system provider.
3. Provide the result of the ODBC **Test Connect** (screenshot) — this isolates whether the issue is the driver/network or the bridge itself.
