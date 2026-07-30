# OpenEdge Bridge — Documentation

**Version:** 1.0  
**Date:** July 23, 2026  
**Purpose:** Sync bridge between Progress OpenEdge and MySQL for Entrust Instant ID card printing. Reads `"id-print"` from OpenEdge and mirrors it to MySQL's `id_print` table.

---

## Architecture Overview

```
┌──────────────────────────────┐
│  Progress OpenEdge SQL Server │  ← Source of truth (remote, provided by client)
└──────────────┬───────────────┘
               │ ODBC
┌──────────────▼───────────────┐
│  OpenEdge ODBC Driver         │  ← PROGRESS_DATADIRECT_ODBC_8.0_WIN_64_INSTALL.exe
│  (DataDirect 8.0, 64-bit)    │
└──────────────┬───────────────┘
               │
┌──────────────▼───────────────┐
│  C# Windows Service           │  ← OpenEdgeBridge.exe (this application)
│  (.NET 8, self-contained)    │
└──────────────┬───────────────┘
               │ MySQL Connector
┌──────────────▼───────────────┐
│  MySQL Server (XAMPP)         │  ← Local database, runs on the same PC
└──────────────┬───────────────┘
               │ JDBC (mysql-connector-j-8.0.33.jar)
┌──────────────▼───────────────┐
│  Entrust Instant ID           │  ← Card issuance system
└──────────────────────────────┘
```

---

## Data Flow

### OpenEdge → MySQL (One-Way Sync)

```
OpenEdge "id-print" ──► C# Service ──► MySQL id_print table
       (read)               (sync loop)         (upsert)
```

All 24 fields from the `id-print` table are synced, including:
- `batch-no`, `batch-date`, `app-no`, `dob`
- `cp-no`, `fullname`, `hcp-fullname`, `address`, `hcp-cpno`
- `lname`, `fname`, `mname`
- `spa-docno`, `page-no`, `book-no`, `series-no`, `spa-atty`, `spa-dt-issue`
- `printed`, `printed-dt`, `printed-tm`
- `id-type`, `image-data`, `id-code`

Runs every N seconds (configurable in `appsettings.json`). Uses **INSERT ON DUPLICATE KEY UPDATE** to avoid duplicates. `print_count` is only inserted on first sync; subsequent updates do NOT overwrite it (Instant ID manages `print_count`).

### MySQL → OpenEdge (Printed Back-Sync)

```
MySQL id_print ──────► C# Service ──────► OpenEdge "id-print"
 (print_count > 0)    (check + mark)       (SET printed = yes)
```

When Instant ID increments `print_count > 0` on a record, the bridge detects it and sets `printed = yes` in the corresponding OpenEdge record.

---

## Prerequisites

### On the Target Windows PC

| # | Component | Version / File | Notes |
|---|-----------|---------------|-------|
| 1 | **Windows OS** | Windows 10/11 or Windows Server 2016+ | 64-bit required |
| 2 | **OpenEdge ODBC Driver** | `PROGRESS_DATADIRECT_ODBC_8.0_WIN_64_INSTALL.exe` | Install first; configure a System DSN |
| 3 | **XAMPP** | Latest (8.x) | Provides local MySQL server |
| 4 | **MySQL Connector/J** | `mysql-connector-j-8.0.33.jar` | For Entrust Instant ID to connect to MySQL |
| 5 | **OpenEdgeBridge** | Published `publish/` folder | Self-contained, no .NET runtime needed |

---

## Local Development — Installing Progress OpenEdge Database

> **Use this section when no remote OpenEdge server is available.**  
> You can install Progress OpenEdge locally, create a database, and run the SQL below.

### Step 0-A: Download & Install Progress OpenEdge

1. Go to https://www.progress.com/openedge/downloads
2. Download **Progress OpenEdge Developer Studio** or **Progress OpenEdge Personal Edition** (free for development)
3. Run the installer (typically `PROGRESS_OE_12.x_WIN_64.exe`) as Administrator
4. Choose **Typical** or **Complete** installation
5. Default install path: `C:\Progress\OpenEdge\`

After installation, a **"Proenv"** shortcut will appear in your Start Menu.  
**Proenv** is a command prompt pre-configured with all OpenEdge environment variables.

### Step 0-B: Create a Local OpenEdge Database

Open **Proenv** as Administrator and run:

```bat
:: Create a folder for your database
mkdir C:\OpenEdgeDB
cd C:\OpenEdgeDB

:: Create an empty database called 'instantid'
prodb instantid empty
```

This creates `instantid.db` and related files in `C:\OpenEdgeDB\`.

### Step 0-C: Start the Database with SQL Broker

In **Proenv**, run:

```bat
:: Start the database for multi-user access with SQL enabled
proserve C:\OpenEdgeDB\instantid -S 20000 -n 100 -ServerType SQL
```

| Parameter | Meaning |
|-----------|---------|
| `-S 20000` | Service port (SQL and 4GL clients connect on this port) |
| `-n 100`  | Maximum concurrent users |
| `-ServerType SQL` | **Required** — enables the SQL-92 engine |

The database is now running. Keep this Proenv window open.

### Step 0-D: Configure the ODBC System DSN

1. Press `Win + R`, type `odbcad32.exe`, press Enter
2. Go to the **System DSN** tab, click **Add...**
3. Select **Progress OpenEdge 12.x Driver** (or "DataDirect OpenEdge SQL Server")
4. Configure:
   - **Data Source Name:** `OpenEdgeInstantID`
   - **Host:** `localhost`
   - **Port:** `20000`
   - **Database:** `instantid`
   - **User ID:** `admin`
   - **Password:** `admin`
5. Click **Test Connect** → should succeed
6. Click **OK**

### Step 0-E: Run the CREATE TABLE SQL

Connect to your local OpenEdge database using any ODBC-compatible SQL client (such as **DBeaver**, **HeidiSQL**, or the built-in **OpenEdge SQL Explorer**), then paste and run the SQL in the sections below.

---

## Database Tables

### `id-print` / `id_print`

> Converted from the provided `.df` schema.  
> **Paste each CREATE TABLE directly into your SQL editor and run it.**

#### OpenEdge SQL — CREATE TABLE

```sql
CREATE TABLE "id-print" (
    "batch-no"      INTEGER       NOT NULL DEFAULT 0,
    "batch-date"    DATE          NULL,
    "app-no"        INTEGER       NOT NULL DEFAULT 0,
    "dob"           DATE          NULL,
    "cp-no"         VARCHAR(15)   NOT NULL DEFAULT '',
    "fullname"      VARCHAR(65)   NOT NULL DEFAULT '',
    "hcp-fullname"  VARCHAR(65)   NOT NULL DEFAULT '',
    "address"       VARCHAR(65)   NOT NULL DEFAULT '',
    "hcp-cpno"      VARCHAR(15)   NOT NULL DEFAULT '',
    "lname"         VARCHAR(55)   NOT NULL DEFAULT '',
    "fname"         VARCHAR(55)   NOT NULL DEFAULT '',
    "mname"         VARCHAR(55)   NOT NULL DEFAULT '',
    "spa-docno"     VARCHAR(25)   NOT NULL DEFAULT '',
    "page-no"       VARCHAR(20)   NOT NULL DEFAULT '',
    "book-no"       VARCHAR(20)   NOT NULL DEFAULT '',
    "series-no"     VARCHAR(20)   NOT NULL DEFAULT '',
    "spa-atty"      VARCHAR(65)   NOT NULL DEFAULT '',
    "spa-dt-issue"  DATE          NULL,
    "printed"       BIT           NOT NULL DEFAULT 0,
    "printed-dt"    DATE          NULL,
    "printed-tm"    VARCHAR(35)   NOT NULL DEFAULT '',
    "id-type"       INTEGER       NOT NULL DEFAULT 0,
    "image-data"    VARBINARY(110) NULL,
    "id-code"       INTEGER       NOT NULL DEFAULT 0
);
```

> **Note:** OpenEdge requires double-quotes around field names containing hyphens (`-`).  
> The schema resides in the `PUB` area (default).  
> `BIT` represents the OpenEdge `LOGICAL` type (yes/no → 1/0).  
> `VARBINARY(110)` stores the raw image binary data (MAX-WIDTH 110 from the schema).

#### MySQL — CREATE TABLE

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
    id_code        INT           NULL,
    print_count    INT           NULL        COMMENT 'Incremented by Instant ID; triggers printed=yes back-sync',
    PRIMARY KEY (id),
    UNIQUE INDEX idx_app_no (app_no)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;
```

> **Note:** `id` is an internal auto-increment primary key. `app_no` is the unique business key — the bridge uses it for deduplication (`ON DUPLICATE KEY UPDATE` fires when the same `app_no` is synced again).

---

## Step-by-Step Setup (Remote Server)

> **If you already have a remote OpenEdge server**, follow these steps.  
> **If no server is available**, go back to [Local Development — Installing Progress OpenEdge Database](#local-development--installing-progress-openedge-database) first.

### 1. Install OpenEdge ODBC Driver

Run the installer as Administrator:

```
PROGRESS_DATADIRECT_ODBC_8.0_WIN_64_INSTALL.exe
```

After installation, open **ODBC Data Sources (64-bit)** and create a **System DSN**:

1. Press `Win + R`, type `odbcad32.exe`, press Enter
2. Go to the **System DSN** tab
3. Click **Add...**
4. Select **Progress DataDirect OpenEdge SQL Server** (or similar)
5. Configure:
   - **Data Source Name:** e.g., `OpenEdgeInstantID`
   - **Host:** (provided by your OpenEdge team, or `localhost` if local)
   - **Port:** (typically `20000` or as provided)
   - **Database:** (provided by your OpenEdge team, or `instantid` if local)
   - **User ID / Password:** (provided by your OpenEdge team)
6. Click **Test Connect** to verify, then **OK**

### 2. Create the OpenEdge Tables

Connect to your OpenEdge database using any ODBC-compatible SQL client (DBeaver, HeidiSQL, etc.) and paste the SQL from the [Database Tables](#database-tables) section above:

- Run the **`"id-print"`** CREATE TABLE

### 3. Install XAMPP

1. Download from https://www.apachefriends.org/
2. Run the installer with default settings
3. Open the **XAMPP Control Panel**
4. Click **Start** next to **MySQL**
5. Click **Admin** to open phpMyAdmin

### 4. Create the MySQL Database & Tables

In phpMyAdmin (or any MySQL client), run:

```sql
CREATE DATABASE IF NOT EXISTS instantid_bridge
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE instantid_bridge;
```

Then paste and run the MySQL CREATE TABLE statements from the [Database Tables](#database-tables) section above:

- Run the **`id_print`** CREATE TABLE

### 5. Configure Entrust Instant ID (MySQL Connector/J)

1. Copy `mysql-connector-j-8.0.33.jar` to Entrust's `lib/` folder
2. Configure Entrust to point to the local MySQL database:
   - **Host:** `localhost`
   - **Port:** `3306`
   - **Database:** `instantid_bridge`
   - **User:** `root` (or a dedicated user)
   - **Table:** `id_print`

### 6. Deploy the Bridge Service

Copy the entire `publish/` folder to the target PC, e.g. `C:\OpenEdgeBridge\`.

#### Edit `appsettings.json`

```json
{
  "ConnectionStrings": {
    "OpenEdge": "DSN=OpenEdgeInstantID;Uid=your_user;Pwd=your_password;",
    "MySql": "Server=localhost;Database=instantid_bridge;Uid=root;Pwd=your_mysql_password;"
  },
  "Sync": {
    "IntervalSeconds": 10
  }
}
```

| Setting | Description |
|---------|-------------|
| `OpenEdge` | ODBC DSN name, user, and password |
| `MySql` | MySQL server, database, user, and password |
| `Sync:IntervalSeconds` | How often the sync loop runs (default: 10s) |

### 7. Install as a Windows Service

Open **PowerShell as Administrator** and run:

```powershell
New-Service -Name "OpenEdgeBridge" `
            -BinaryPathName "C:\OpenEdgeBridge\OpenEdgeBridge.exe" `
            -DisplayName "OpenEdge Bridge Service" `
            -Description "Syncs id-print data between OpenEdge and MySQL for Entrust Instant ID" `
            -StartupType Automatic

Start-Service OpenEdgeBridge
```

Verify:
```powershell
Get-Service OpenEdgeBridge
# Status should be: Running
```

---

## Service Management Commands

```powershell
Start-Service OpenEdgeBridge      # Start
Stop-Service OpenEdgeBridge       # Stop
Restart-Service OpenEdgeBridge    # Restart (after config changes)
Get-Service OpenEdgeBridge        # Check status

# Uninstall
Stop-Service OpenEdgeBridge
sc delete OpenEdgeBridge
```

---

## Logging & Troubleshooting

### Log Locations

| Source | Location |
|--------|----------|
| File logs | `C:\OpenEdgeBridge\Logs\` (dated log files) |
| Windows Event Viewer | *Windows Logs → Application*, Source: `OpenEdgeBridge` |

### Common Issues

| Problem | Likely Cause | Fix |
|---------|-------------|-----|
| Service starts then stops immediately | Wrong DSN name or credentials | Test the ODBC DSN in ODBC Administrator with **Test Connect** |
| "MySQL connection string not found" | `appsettings.json` not in same folder as `.exe` | Ensure `appsettings.json` is in `C:\OpenEdgeBridge\` |
| Duplicate rows in MySQL | Missing UNIQUE index | Run the `ALTER TABLE ... ADD UNIQUE INDEX` SQL above |
| OpenEdge ODBC driver not found | Wrong bitness (32 vs 64) | Use `odbcad32.exe` (64-bit), not the 32-bit one in SysWOW64 |
| MySQL not accessible from Entrust | MySQL not running or wrong port | Check XAMPP Control Panel; MySQL must be **Started** |
| OpenEdge SQL CREATE TABLE fails with "Invalid table name" | Hyphens in table/field names not quoted | Wrap names containing hyphens in double quotes: `"id-print"`, `"batch-no"` |
| `image-data` field too small | VARBINARY length insufficient | The OpenEdge schema specifies MAX-WIDTH 110; adjust as needed |

---

## Sync Interval Tuning

The default sync interval is **10 seconds**. Adjust in `appsettings.json`:

```json
"Sync": {
  "IntervalSeconds": 30
}
```

- **Lower** (e.g., 5s) = faster sync, more database load
- **Higher** (e.g., 60s) = slower sync, less load
- Recommended: 10–30 seconds for production

---

## Port Summary

| Component | Port | Direction |
|-----------|------|-----------|
| OpenEdge SQL Server | Provided by OpenEdge team | Outbound |
| MySQL (XAMPP) | 3306 | Local |
| Entrust → MySQL | 3306 | Local via JDBC |

No inbound ports are opened by the bridge itself.

---

## Upgrade Procedure

1. Stop the service: `Stop-Service OpenEdgeBridge`
2. Replace all files in `C:\OpenEdgeBridge\` with the new `publish/` output
3. Preserve your `appsettings.json` (do not overwrite with default)
4. Start the service: `Start-Service OpenEdgeBridge`

---

## Checklist for Target PC

- [ ] Windows 10/11 or Server 2016+ (64-bit)
- [ ] OpenEdge ODBC driver installed and DSN configured
- [ ] XAMPP installed and MySQL running
- [ ] `instantid_bridge` database created in MySQL
- [ ] `id-print` / `id_print` table created in **both** OpenEdge and MySQL
- [ ] `mysql-connector-j-8.0.33.jar` placed in Entrust's lib folder
- [ ] Bridge `publish/` folder copied to `C:\OpenEdgeBridge\`
- [ ] `appsettings.json` updated with correct connection strings
- [ ] Windows Service installed and started
- [ ] Logs checked for errors
