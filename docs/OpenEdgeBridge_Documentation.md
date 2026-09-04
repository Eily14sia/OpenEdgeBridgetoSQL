# OpenEdge Bridge — Documentation

---

## Sample INSERT Data

### MySQL — `id_print` (11 records)

```sql
INSERT INTO id_print (batch_no, batch_date, app_no, dob, cp_no, fullname, hcp_fullname, address, hcp_cpno, lname, fname, mname, spa_docno, page_no, book_no, series_no, spa_atty, spa_dt_issue, printed, printed_dt, printed_tm, tran_type, id_code, sfx, photo, gender, date_issued, id_no, year_applied, control_no, print_count)
VALUES
(1, '2026-01-15', 1001, '1990-05-20', 'CP-001', 'Juan Dela Cruz', 'Dr. Maria Santos', '123 Rizal St, Manila', 'HCP-001', 'Dela Cruz', 'Juan', 'Santos', 'SPA-001', 'P001', 'BK001', 'SR001', 'Atty. Reyes', '2025-12-01', 'no', NULL, NULL, 1, 5001, '', NULL, 1, '2026-01-15', 10001, 2026, 1, 0),
(1, '2026-01-15', 1002, '1985-08-12', 'CP-002', 'Maria Clara Reyes', 'Dr. Jose Rizal', '456 Mabini St, Quezon City', 'HCP-002', 'Reyes', 'Maria', 'Clara', 'SPA-002', 'P002', 'BK001', 'SR002', 'Atty. Cruz', '2025-12-02', 'no', NULL, NULL, 1, 5002, '', NULL, 2, '2026-01-15', 10002, 2026, 2, 0),
(1, '2026-01-16', 1003, '1992-03-25', 'CP-003', 'Pedro Penduko', 'Dr. Ana Lopez', '789 Bonifacio Ave, Cebu', 'HCP-003', 'Penduko', 'Pedro', 'Garcia', 'SPA-003', 'P003', 'BK002', 'SR003', 'Atty. Santos', '2025-12-03', 'no', NULL, NULL, 2, 5003, 'JR', NULL, 1, '2026-01-16', 10003, 2026, 3, 1),
(2, '2026-02-10', 1004, '1988-11-30', 'CP-004', 'Ana Patricia Gomez', 'Dr. Luis Torres', '321 Aguinaldo Rd, Davao', 'HCP-004', 'Gomez', 'Ana', 'Patricia', 'SPA-004', 'P004', 'BK002', 'SR004', 'Atty. Reyes', '2025-12-10', 'yes', '2026-02-11', '10:30:00', 1, 5004, '', NULL, 2, '2026-02-10', 10004, 2026, 4, 1),
(2, '2026-02-10', 1005, '1995-07-14', 'CP-005', 'Carlos Miguel Tan', 'Dr. Elena Villanueva', '654 Luna St, Makati', 'HCP-005', 'Tan', 'Carlos', 'Miguel', 'SPA-005', 'P005', 'BK003', 'SR005', 'Atty. Dela Rosa', '2025-12-15', 'no', NULL, NULL, 3, 5005, '', NULL, 1, '2026-02-10', 10005, 2026, 5, 2),
(2, '2026-02-11', 1006, '1982-01-08', 'CP-006', 'Sofia Isabelle Lim', 'Dr. Ramon Bautista', '987 Silang Ave, Pasig', 'HCP-006', 'Lim', 'Sofia', 'Isabelle', 'SPA-006', 'P006', 'BK003', 'SR006', 'Atty. Mercado', '2025-12-20', 'no', NULL, NULL, 1, 5006, '', NULL, 2, '2026-02-11', 10006, 2026, 6, 0),
(3, '2026-03-05', 1007, '1998-09-18', 'CP-007', 'Rafael Antonio Uy', 'Dr. Patricia Ong', '159 Roxas Blvd, Paranaque', 'HCP-007', 'Uy', 'Rafael', 'Antonio', 'SPA-007', 'P007', 'BK004', 'SR007', 'Atty. Garcia', '2026-01-05', 'yes', '2026-03-06', '14:15:00', 2, 5007, '', NULL, 1, '2026-03-05', 10007, 2026, 7, 1),
(3, '2026-03-05', 1008, '1991-12-03', 'CP-008', 'Gabriela Nicole Sy', 'Dr. Fernando Aquino', '753 Quezon Blvd, Taguig', 'HCP-008', 'Sy', 'Gabriela', 'Nicole', 'SPA-008', 'P008', 'BK004', 'SR008', 'Atty. Ramos', '2026-01-10', 'no', NULL, NULL, 1, 5008, '', NULL, 2, '2026-03-05', 10008, 2026, 8, 0),
(3, '2026-03-06', 1009, '1987-04-22', 'CP-009', 'Miguel Alfonso Co', 'Dr. Teresa Javier', '246 Osmena St, Baguio', 'HCP-009', 'Co', 'Miguel', 'Alfonso', 'SPA-009', 'P009', 'BK005', 'SR009', 'Atty. Villar', '2026-01-15', 'no', NULL, NULL, 3, 5009, 'III', NULL, 1, '2026-03-06', 10009, 2026, 9, 3),
(4, '2026-04-20', 1010, '1993-06-11', 'CP-010', 'Isabella Marie Chua', 'Dr. Roberto Domingo', '135 Kalayaan Ave, Pampanga', 'HCP-010', 'Chua', 'Isabella', 'Marie', 'SPA-010', 'P010', 'BK005', 'SR010', 'Atty. Pineda', '2026-02-01', 'no', NULL, NULL, 1, 5010, '', NULL, 2, '2026-04-20', 10010, 2026, 10, 0),
(4, '2026-04-20', 1011, '1980-10-05', 'CP-011', 'Antonio Luis Pangilinan', 'Dr. Carmen Salazar', '864 EDSA, Mandaluyong', 'HCP-011', 'Pangilinan', 'Antonio', 'Luis', 'SPA-011', 'P011', 'BK006', 'SR011', 'Atty. Hernandez', '2026-02-10', 'yes', '2026-04-21', '09:00:00', 2, 5011, '', NULL, 1, '2026-04-20', 10011, 2026, 11, 2);
```

### OpenEdge (Progress) — `pub."id-print"` (11 records)

```sql
INSERT INTO pub."id-print" ("batch-no", "batch-date", "app-no", "dob", "cp-no", "fullname", "hcp-fullname", "address", "hcp-cpno", "lname", "fname", "mname", "spa-docno", "page-no", "book-no", "series-no", "spa-atty", "spa-dt-issue", "printed", "printed-dt", "printed-tm", "tran-type", "id-code", "sfx", "photo", "gender", "date-issued", "id-no", "year-applied", "control-no")
VALUES
(1, '2026-01-15', 1001, '1990-05-20', 'CP-001', 'Juan Dela Cruz', 'Dr. Maria Santos', '123 Rizal St, Manila', 'HCP-001', 'Dela Cruz', 'Juan', 'Santos', 'SPA-001', 'P001', 'BK001', 'SR001', 'Atty. Reyes', '2025-12-01', 0, NULL, NULL, 1, 5001, '', NULL, 1, '2026-01-15', 10001, 2026, 1),
(1, '2026-01-15', 1002, '1985-08-12', 'CP-002', 'Maria Clara Reyes', 'Dr. Jose Rizal', '456 Mabini St, Quezon City', 'HCP-002', 'Reyes', 'Maria', 'Clara', 'SPA-002', 'P002', 'BK001', 'SR002', 'Atty. Cruz', '2025-12-02', 0, NULL, NULL, 1, 5002, '', NULL, 2, '2026-01-15', 10002, 2026, 2),
(1, '2026-01-16', 1003, '1992-03-25', 'CP-003', 'Pedro Penduko', 'Dr. Ana Lopez', '789 Bonifacio Ave, Cebu', 'HCP-003', 'Penduko', 'Pedro', 'Garcia', 'SPA-003', 'P003', 'BK002', 'SR003', 'Atty. Santos', '2025-12-03', 0, NULL, NULL, 2, 5003, 'JR', NULL, 1, '2026-01-16', 10003, 2026, 3),
(2, '2026-02-10', 1004, '1988-11-30', 'CP-004', 'Ana Patricia Gomez', 'Dr. Luis Torres', '321 Aguinaldo Rd, Davao', 'HCP-004', 'Gomez', 'Ana', 'Patricia', 'SPA-004', 'P004', 'BK002', 'SR004', 'Atty. Reyes', '2025-12-10', 1, '2026-02-11', '10:30:00', 1, 5004, '', NULL, 2, '2026-02-10', 10004, 2026, 4),
(2, '2026-02-10', 1005, '1995-07-14', 'CP-005', 'Carlos Miguel Tan', 'Dr. Elena Villanueva', '654 Luna St, Makati', 'HCP-005', 'Tan', 'Carlos', 'Miguel', 'SPA-005', 'P005', 'BK003', 'SR005', 'Atty. Dela Rosa', '2025-12-15', 0, NULL, NULL, 3, 5005, '', NULL, 1, '2026-02-10', 10005, 2026, 5),
(2, '2026-02-11', 1006, '1982-01-08', 'CP-006', 'Sofia Isabelle Lim', 'Dr. Ramon Bautista', '987 Silang Ave, Pasig', 'HCP-006', 'Lim', 'Sofia', 'Isabelle', 'SPA-006', 'P006', 'BK003', 'SR006', 'Atty. Mercado', '2025-12-20', 0, NULL, NULL, 1, 5006, '', NULL, 2, '2026-02-11', 10006, 2026, 6),
(3, '2026-03-05', 1007, '1998-09-18', 'CP-007', 'Rafael Antonio Uy', 'Dr. Patricia Ong', '159 Roxas Blvd, Paranaque', 'HCP-007', 'Uy', 'Rafael', 'Antonio', 'SPA-007', 'P007', 'BK004', 'SR007', 'Atty. Garcia', '2026-01-05', 1, '2026-03-06', '14:15:00', 2, 5007, '', NULL, 1, '2026-03-05', 10007, 2026, 7),
(3, '2026-03-05', 1008, '1991-12-03', 'CP-008', 'Gabriela Nicole Sy', 'Dr. Fernando Aquino', '753 Quezon Blvd, Taguig', 'HCP-008', 'Sy', 'Gabriela', 'Nicole', 'SPA-008', 'P008', 'BK004', 'SR008', 'Atty. Ramos', '2026-01-10', 0, NULL, NULL, 1, 5008, '', NULL, 2, '2026-03-05', 10008, 2026, 8),
(3, '2026-03-06', 1009, '1987-04-22', 'CP-009', 'Miguel Alfonso Co', 'Dr. Teresa Javier', '246 Osmena St, Baguio', 'HCP-009', 'Co', 'Miguel', 'Alfonso', 'SPA-009', 'P009', 'BK005', 'SR009', 'Atty. Villar', '2026-01-15', 0, NULL, NULL, 3, 5009, 'III', NULL, 1, '2026-03-06', 10009, 2026, 9),
(4, '2026-04-20', 1010, '1993-06-11', 'CP-010', 'Isabella Marie Chua', 'Dr. Roberto Domingo', '135 Kalayaan Ave, Pampanga', 'HCP-010', 'Chua', 'Isabella', 'Marie', 'SPA-010', 'P010', 'BK005', 'SR010', 'Atty. Pineda', '2026-02-01', 0, NULL, NULL, 1, 5010, '', NULL, 2, '2026-04-20', 10010, 2026, 10),
(4, '2026-04-20', 1011, '1980-10-05', 'CP-011', 'Antonio Luis Pangilinan', 'Dr. Carmen Salazar', '864 EDSA, Mandaluyong', 'HCP-011', 'Pangilinan', 'Antonio', 'Luis', 'SPA-011', 'P011', 'BK006', 'SR011', 'Atty. Hernandez', '2026-02-10', 1, '2026-04-21', '09:00:00', 2, 5011, '', NULL, 1, '2026-04-20', 10011, 2026, 11);
```


**Version:** 1.2  
**Date:** September 4, 2026  
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

All 31 fields from the `id-print` table are synced, including:
- `batch-no`, `batch-date`, `app-no`, `dob`
- `cp-no`, `fullname`, `hcp-fullname`, `address`, `hcp-cpno`
- `lname`, `fname`, `mname`
- `spa-docno`, `page-no`, `book-no`, `series-no`, `spa-atty`, `spa-dt-issue`
- `printed`, `printed-dt`, `printed-tm`
- `tran-type`, `image-data`, `id-code`, `sfx`, `photo`
- `gender`, `date-issued`, `id-no`, `year-applied`, `control-no`

Runs every N seconds (configurable in `appsettings.json`). Uses **check-then-update** upserts (SELECT by `app_no`, then UPDATE or INSERT) to avoid duplicates. `print_count` is only inserted on first sync; subsequent updates do NOT overwrite it (Instant ID manages `print_count`).

#### Full vs. Incremental Sync

The bridge has two read modes, decided automatically on every cycle:

| Mode | Trigger | What it reads from OpenEdge |
|------|---------|------------------------------|
| **FULL** | No `last_full_sync_utc` recorded yet (first run, or fresh MySQL) | All rows in `pub."id-print"` — seeds/refreshes the entire MySQL table |
| **INCREMENTAL** | A full sync has already been recorded | Only rows with `batch-no` greater than the highest `batch_no` already in MySQL |

OpenEdge `id-print` records are **write-once** — new data arrives only as new batches; existing rows are never edited. Therefore, with the default `Sync:FullSyncIntervalMinutes = 0`, the full sync runs **once** (the initial seed) and every later cycle is incremental. Restarting the service or rebooting the PC does **not** cause another full sync — the timestamp is persisted in MySQL's `sync_state` table (see [Database Tables](#database-tables)).

If `Sync:FullSyncIntervalMinutes` is set to a positive value (e.g., `60`), a full sync also runs when that many minutes have elapsed since the last successful full sync — useful only if OpenEdge rows ever start getting edited in place.

> **Note:** `signature_data` and `qr_data` are **not** part of this sync — they are MySQL-only columns populated by Entrust Instant ID and never touched by the bridge (see [Database Tables](#database-tables)).

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
mkdir C:\OpenEdgeDatabases
cd C:\OpenEdgeDatabases

:: Create an empty database called 'iddb'
prodb iddb empty
```

This creates `iddb.db` and related files in `C:\OpenEdgeDatabases\`.

### Step 0-C: Start the Database Server

In **Proenv**, run:

```bat
:: Start the database for multi-user access with both 4GL and SQL enabled
proserve "C:\OpenEdgeDatabases\iddb.db" -S 11918 -ServerType Both -n 51 -L 8192
```

| Parameter | Meaning |
|-----------|---------|
| `-S 11918` | Service port (SQL and 4GL clients connect on this port) |
| `-n 51` | Maximum concurrent users |
| `-L 8192` | Lock table size (entries) |
| `-ServerType Both` | **Required** — enables both 4GL and SQL-92 engine |

The database is now running. Keep this Proenv window open.

### Step 0-D: Configure the ODBC System DSN

1. Press `Win + R`, type `odbcad32.exe`, press Enter
2. Go to the **System DSN** tab, click **Add...**
3. Select **Progress OpenEdge 12.x Driver** (or "DataDirect OpenEdge SQL Server")
4. Configure:
   - **Data Source Name:** `OpenEdgeInstantID`
   - **Description:** `OpenEdge Instant ID Database`
   - **Host Name:** `localhost`
   - **Port Number:** `11918`
   - **Database Name:** `iddb`
   - **User ID:** `admin`
5. Click **Test Connect** → should succeed
6. Click **OK**

### Step 0-E: Create the Database Schema (`.df` File)

The table schema is defined in a `.df` (Data Definition) file. Create a file named `iddb.df` in `C:\OpenEdgeDatabases\` with the following content:

```
ADD TABLE "id-print"
  AREA "Schema Area"
  DUMP-NAME "id-print"

ADD FIELD "batch-no" OF "id-print" AS integer 
  FORMAT "->,>>>,>>9"
  INITIAL "0"
  POSITION 2
  MAX-WIDTH 4
  ORDER 10

ADD FIELD "batch-date" OF "id-print" AS date 
  FORMAT "99/99/9999"
  INITIAL ?
  POSITION 3
  MAX-WIDTH 4
  ORDER 20

ADD FIELD "app-no" OF "id-print" AS integer 
  FORMAT "->,>>>,>>9"
  INITIAL "0"
  POSITION 4
  MAX-WIDTH 4
  ORDER 30

ADD FIELD "dob" OF "id-print" AS date 
  FORMAT "99/99/9999"
  INITIAL ?
  POSITION 5
  MAX-WIDTH 4
  ORDER 40

ADD FIELD "cp-no" OF "id-print" AS character 
  FORMAT "x(15)"
  INITIAL ""
  POSITION 6
  MAX-WIDTH 30
  ORDER 50

ADD FIELD "fullname" OF "id-print" AS character 
  FORMAT "x(65)"
  INITIAL ""
  POSITION 7
  MAX-WIDTH 130
  ORDER 60

ADD FIELD "hcp-fullname" OF "id-print" AS character 
  FORMAT "x(65)"
  INITIAL ""
  POSITION 8
  MAX-WIDTH 130
  ORDER 70

ADD FIELD "address" OF "id-print" AS character 
  FORMAT "x(65)"
  INITIAL ""
  POSITION 9
  MAX-WIDTH 130
  ORDER 80

ADD FIELD "hcp-cpno" OF "id-print" AS character 
  FORMAT "x(15)"
  INITIAL ""
  POSITION 10
  MAX-WIDTH 30
  ORDER 90

ADD FIELD "lname" OF "id-print" AS character 
  FORMAT "x(55)"
  INITIAL ""
  POSITION 11
  MAX-WIDTH 110
  ORDER 100

ADD FIELD "fname" OF "id-print" AS character 
  FORMAT "x(55)"
  INITIAL ""
  POSITION 12
  MAX-WIDTH 110
  ORDER 110

ADD FIELD "mname" OF "id-print" AS character 
  FORMAT "x(55)"
  INITIAL ""
  POSITION 13
  MAX-WIDTH 110
  ORDER 120

ADD FIELD "spa-docno" OF "id-print" AS character 
  FORMAT "x(25)"
  INITIAL ""
  POSITION 14
  MAX-WIDTH 50
  ORDER 130

ADD FIELD "page-no" OF "id-print" AS character 
  FORMAT "x(20)"
  INITIAL ""
  POSITION 15
  MAX-WIDTH 40
  ORDER 140

ADD FIELD "book-no" OF "id-print" AS character 
  FORMAT "x(20)"
  INITIAL ""
  POSITION 16
  MAX-WIDTH 40
  ORDER 150

ADD FIELD "series-no" OF "id-print" AS character 
  FORMAT "x(20)"
  INITIAL ""
  POSITION 17
  MAX-WIDTH 40
  ORDER 160

ADD FIELD "spa-atty" OF "id-print" AS character 
  FORMAT "x(65)"
  INITIAL ""
  POSITION 18
  MAX-WIDTH 130
  ORDER 170

ADD FIELD "spa-dt-issue" OF "id-print" AS date 
  FORMAT "99/99/9999"
  INITIAL ?
  POSITION 19
  MAX-WIDTH 4
  ORDER 180

ADD FIELD "printed" OF "id-print" AS logical 
  FORMAT "yes/no"
  INITIAL "no"
  POSITION 20
  MAX-WIDTH 1
  ORDER 190

ADD FIELD "printed-dt" OF "id-print" AS date 
  FORMAT "99/99/9999"
  INITIAL ?
  POSITION 21
  MAX-WIDTH 4
  ORDER 200

ADD FIELD "printed-tm" OF "id-print" AS character 
  FORMAT "x(35)"
  INITIAL ""
  POSITION 22
  MAX-WIDTH 70
  ORDER 210

ADD FIELD "tran-type" OF "id-print" AS integer 
  DESCRIPTION "1 = for RTC, 2=senior"
  FORMAT "->,>>>,>>9"
  INITIAL "0"
  POSITION 23
  MAX-WIDTH 4
  ORDER 220

ADD FIELD "image-data" OF "id-print" AS raw 
  FORMAT "x(55)"
  INITIAL ""
  POSITION 24
  MAX-WIDTH 110
  ORDER 230

ADD FIELD "id-code" OF "id-print" AS integer 
  FORMAT "->,>>>,>>9"
  INITIAL "0"
  POSITION 25
  MAX-WIDTH 4
  ORDER 240

ADD FIELD "sfx" OF "id-print" AS character 
  FORMAT "x(15)"
  INITIAL ""
  POSITION 26
  MAX-WIDTH 30
  ORDER 250

ADD FIELD "photo" OF "id-print" AS blob 
  FORMAT "x(8)"
  INITIAL ?
  POSITION 27
  LOB-AREA "Schema Area"
  LOB-BYTES 104857600
  LOB-SIZE 100M
  ORDER 260

ADD FIELD "gender" OF "id-print" AS integer 
  DESCRIPTION "1=Female, 2=Male"
  FORMAT "->,>>>,>>9"
  INITIAL "0"
  POSITION 28
  MAX-WIDTH 4
  ORDER 270

ADD FIELD "date-issued" OF "id-print" AS date 
  FORMAT "99/99/9999"
  INITIAL ?
  POSITION 29
  MAX-WIDTH 4
  ORDER 280

ADD FIELD "id-no" OF "id-print" AS integer 
  FORMAT "->,>>>,>>9"
  INITIAL "0"
  POSITION 30
  MAX-WIDTH 4
  ORDER 290

ADD FIELD "year-applied" OF "id-print" AS integer 
  FORMAT ">>>9999"
  INITIAL "0"
  POSITION 31
  MAX-WIDTH 4
  ORDER 300

ADD FIELD "control-no" OF "id-print" AS integer 
  FORMAT "->,>>>,>>9"
  INITIAL "0"
  POSITION 32
  MAX-WIDTH 4
  ORDER 310

.
PSC
cpstream=ISO8859-1
.
0000004144
```

Then, in **Proenv**, load the `.df` file into the database:

```bat
cd C:\OpenEdgeDatabases
prodict iddb
```

In the Data Dictionary menu, select **Admin → Load Data Definitions → .df file**, then choose `iddb.df`.

> **Note:** The database name is `iddb` and the schema resides under the `PUB` area (default).

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
    "tran-type"     INTEGER       NOT NULL DEFAULT 0,
    "image-data"    VARBINARY(110) NULL,
    "id-code"       INTEGER       NOT NULL DEFAULT 0,
    "sfx"           VARCHAR(15)   NOT NULL DEFAULT '',
    "photo"         BLOB          NULL,
    "gender"        INTEGER       NOT NULL DEFAULT 0,
    "date-issued"   DATE          NULL,
    "id-no"         INTEGER       NOT NULL DEFAULT 0,
    "year-applied"  INTEGER       NOT NULL DEFAULT 0,
    "control-no"    INTEGER       NOT NULL DEFAULT 0
);
```

> **Note:** OpenEdge requires double-quotes around field names containing hyphens (`-`).  
> The schema resides in the `PUB` area (default).  
> `BIT` represents the OpenEdge `LOGICAL` type (yes/no → 1/0).  
> `VARBINARY(110)` stores the raw image binary data (MAX-WIDTH 110 from the schema).  
> `BLOB` stores the cardholder photo (LOB-SIZE 100M from the schema).

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
    tran_type      INT           NULL        COMMENT '1 = for RTC, 2 = senior',
    image_data     LONGBLOB      NULL,
    id_code        INT           NULL,
    sfx            VARCHAR(15)   NULL,
    photo          LONGBLOB      NULL,
    gender         INT           NULL        COMMENT '1 = Female, 2 = Male',
    date_issued    DATE          NULL,
    id_no          INT           NULL,
    year_applied   INT           NULL,
    control_no     INT           NULL,
    signature_data LONGBLOB      NULL        COMMENT 'Signature image; written by Entrust Instant ID (MySQL only, NOT synced from OpenEdge)',
    qr_data        LONGBLOB      NULL        COMMENT 'QR code image; written by Entrust Instant ID (MySQL only, NOT synced from OpenEdge)',
    print_count    INT           NULL        COMMENT 'Incremented by Instant ID; triggers printed=yes back-sync',
    PRIMARY KEY (id),
    UNIQUE INDEX idx_app_no (app_no)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;
```

> **Note:** `id` is an internal auto-increment primary key. `app_no` is the unique business key — the bridge uses it for deduplication (it SELECTs by `app_no` first, then UPDATEs or INSERTs as needed).

> **MySQL-only columns (`signature_data`, `qr_data`):** These store the cardholder's signature and QR-code images and are **MySQL only**. They do **not** exist in the OpenEdge `"id-print"` table and are **never read from or written back to OpenEdge**. Entrust Instant ID populates them directly in MySQL. The bridge deliberately leaves them untouched — it does not include them in its INSERT/UPDATE, so syncing from OpenEdge will never overwrite them.

> **Already have the table?** If you created `id_print` before these columns existed, add them with:
> ```sql
> ALTER TABLE id_print
>   CHANGE COLUMN id_type tran_type INT NULL COMMENT '1 = for RTC, 2 = senior',
>   ADD COLUMN sfx          VARCHAR(15) NULL AFTER id_code,
>   ADD COLUMN photo        LONGBLOB    NULL AFTER sfx,
>   ADD COLUMN gender       INT         NULL COMMENT '1 = Female, 2 = Male' AFTER photo,
>   ADD COLUMN date_issued  DATE        NULL AFTER gender,
>   ADD COLUMN id_no        INT         NULL AFTER date_issued,
>   ADD COLUMN year_applied INT         NULL AFTER id_no,
>   ADD COLUMN control_no   INT         NULL AFTER year_applied,
>   ADD COLUMN signature_data LONGBLOB  NULL AFTER control_no,
>   ADD COLUMN qr_data        LONGBLOB  NULL AFTER signature_data;
> ```
>
> > **Schema update (2026-09-03):** OpenEdge renamed `id-type` to `tran-type` and added `sfx`, `photo`, `gender`, `date-issued`, `id-no`, `year-applied`, and `control-no`. If your OpenEdge `"id-print"` table was created with the older `.df`, reload the updated `.df` (Step 0-E) or add the new fields with `ADD FIELD` in the Data Dictionary, then run the ALTER above on MySQL.

### `sync_state` (bridge state table — MySQL only, auto-created)

The bridge keeps its own bookkeeping in a tiny table called `sync_state`. **You do not need to create it manually** — the bridge runs `CREATE TABLE IF NOT EXISTS` on demand, so it appears in MySQL automatically after the first sync cycle.

```sql
CREATE TABLE IF NOT EXISTS sync_state (
    state_key   VARCHAR(64)  NOT NULL PRIMARY KEY,
    state_value VARCHAR(64)  NOT NULL,
    updated_utc DATETIME     NOT NULL
) ENGINE=InnoDB;
```

| Key | Meaning |
|-----|---------|
| `last_full_sync_utc` | UTC timestamp of the last successful FULL sync. Used to decide whether the next cycle can be INCREMENTAL. Survives restarts, so a reboot never forces a redundant full sync. |

### Column Comparison — Plain English

Think of each row as **one person's ID-card application folder**. The columns are the details written on that folder.

| OpenEdge name | MySQL name | Plain English meaning |
|---|---|---|
| `batch-no` | `batch_no` | **Group number** — which batch/run of applications this person came in with (like a batch printing number) |
| `batch-date` | `batch_date` | **Date of that group** — when their batch was made |
| `app-no` | `app_no` | **Application number** — the unique file number of this person's ID application. This is the "fingerprint" that tells us which person is which |
| `dob` | `dob` | **Date of birth** |
| `cp-no` | `cp_no` | **The person's reference number** (their own control/card number) |
| `fullname` | `fullname` | **Full name** — complete name all in one |
| `hcp-fullname` | `hcp_fullname` | **Doctor's name** — the healthcare provider attached to this person |
| `address` | `address` | **Home address** |
| `hcp-cpno` | `hcp_cpno` | **Doctor's number** — the healthcare provider's own reference number |
| `lname` | `lname` | **Last name** (surname) |
| `fname` | `fname` | **First name** |
| `mname` | `mname` | **Middle name** |
| `spa-docno` | `spa_docno` | **Permission letter number** — the number of the Special Power of Attorney (SPA) document |
| `spa-atty` | `spa_atty` | **Lawyer's name** — the attorney who signed that permission letter |
| `spa-dt-issue` | `spa_dt_issue` | **Date the permission letter was issued** |
| `page-no` | `page_no` | **Page number** — which page in the record book this person is on |
| `book-no` | `book_no` | **Book number** — which record book |
| `series-no` | `series_no` | **Series number** — the serial number of the ID |
| `tran-type` | `tran_type` | **Kind of ID / transaction type** — `1` = regular (RTC), `2` = senior citizen |
| `image-data` | `image_data` | **The person's photo** (picture file) |
| `id-code` | `id_code` | **ID code** — the code number printed on the card |
| `sfx` | `sfx` | **Name suffix** — e.g. Jr., Sr., III |
| `photo` | `photo` | **Photo** — the cardholder's photograph (BLOB image) |
| `gender` | `gender` | **Gender** — `1` = Female, `2` = Male |
| `date-issued` | `date_issued` | **Date the ID card was issued** |
| `id-no` | `id_no` | **ID number** — the person's ID number |
| `year-applied` | `year_applied` | **Year the application was filed** (e.g. `2026`) |
| `control-no` | `control_no` | **Control number** — application/control tracking number |
| `printed` | `printed` | **Has it been printed yet?** — `yes` or `no` |
| `printed-dt` | `printed_dt` | **Date it was printed** |
| `printed-tm` | `printed_tm` | **Time it was printed** |
| — | `signature_data` | **Person's signature picture** — *MySQL only*. Not in OpenEdge, not synced back. Entrust fills this in |
| — | `qr_data` | **QR code picture** — *MySQL only*. Not in OpenEdge, not synced back. Entrust fills this in |
| — | `print_count` | **How many times it got printed** — *MySQL only*. Instant ID counts this; it's never overwritten by the bridge |
| — | `id` | **Row number** — MySQL's own internal counter. Not business data, ignore it |

**30-second summary:** The bridge copies the person's details (name, birthdate, address, photo, etc.) **from OpenEdge → MySQL**, like photocopying the folder. `app_no` is the **matchmaker** — it makes sure updates land on the right person. The three columns that exist **only in MySQL** (`signature_data`, `qr_data`, `print_count`) are filled in by the **Entrust machine**, and the bridge never touches them.

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
   - **Port:** (provided by your OpenEdge team, or `11918` if local)
   - **Database:** (provided by your OpenEdge team, or `iddb` if local)
   - **User ID / Password:** (provided by your OpenEdge team)
6. Click **Test Connect** to verify, then **OK**

### 2. Create the OpenEdge Tables

Connect to your OpenEdge database using any ODBC-compatible SQL client (DBeaver, HeidiSQL, etc.) and paste the SQL from the [Database Tables](#database-tables) section above:

- Run the **`"id-print"`** CREATE TABLE

#### 2-B. Grant Table Permissions to the Login User

The bridge connects to OpenEdge using the **User ID** configured in the ODBC DSN (e.g., `admin`). If your login user is **not** `admin`, it needs explicit permissions on the table — otherwise the bridge fails with an access-denied error when it starts.

Run the following from a DBA account (e.g., `admin`), replacing `your_login_user` with the actual user you put in the DSN:

```sql
GRANT SELECT, INSERT, UPDATE, DELETE ON pub."id-print" TO your_login_user;
```

> **Note:** `SELECT` is required to read records for the OpenEdge → MySQL sync, and `UPDATE` is required for the printed back-sync (`printed = yes`). `INSERT`/`DELETE` are included for completeness. `pub` is the default schema area, and the double quotes around `"id-print"` are required because of the hyphen in the table name.

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
    "IntervalSeconds": 10,
    "FullSyncIntervalMinutes": 0
  }
}
```

| Setting | Description |
|---------|-------------|
| `OpenEdge` | ODBC DSN name, user, and password |
| `MySql` | MySQL server, database, user, and password |
| `Sync:IntervalSeconds` | How often the sync loop runs (default: 10s) |
| `Sync:FullSyncIntervalMinutes` | Minutes between periodic FULL syncs. `0` = disabled (recommended; OpenEdge rows are write-once, so only the first run seeds). Positive value (e.g., `60`) = re-read the whole table that often, catching in-place edits if they ever happen |

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
| `Failure getting schema read lock (12138)` | SQL engine not initialized for database | 1. Verify `-ServerType Both` or `-ServerType SQL` on proserve<br>2. In sqlexp run: `UPDATE TABLE STATISTICS AND INDEX STATISTICS AND ALL COLUMN STATISTICS;`<br>3. If still failing, restart DB: `proshut` then `proserve` with `-SQLTrc 3` |
| MySQL not accessible from Entrust | MySQL not running or wrong port | Check XAMPP Control Panel; MySQL must be **Started** |
| OpenEdge SQL CREATE TABLE fails with "Invalid table name" | Hyphens in table/field names not quoted | Wrap names containing hyphens in double quotes: `"id-print"`, `"batch-no"` |
| `image-data` field too small | VARBINARY length insufficient | The OpenEdge schema specifies MAX-WIDTH 110; adjust as needed |
| Full sync on every start (log says `[FULL]`) | `sync_state` missing or MySQL unreachable at startup | The bridge auto-creates `sync_state`; after the first successful full sync it records a timestamp and later starts use `[INCREMENTAL]`. If MySQL is down at startup it falls back to FULL — that is safe and self-corrects once MySQL is reachable |

---

## Sync Interval Tuning

### `IntervalSeconds` — how often the bridge checks for new batches

The default sync interval is **10 seconds**. Adjust in `appsettings.json`:

```json
"Sync": {
  "IntervalSeconds": 30
}
```

- **Lower** (e.g., 5s) = faster sync, more database load
- **Higher** (e.g., 60s) = slower sync, less load
- Recommended: 10–30 seconds for production

The interval is an **idle pause between cycles** (work → wait → work), not a fixed wall-clock schedule. A slow sync simply delays the next check; cycles never overlap.

### `FullSyncIntervalMinutes` — how often the whole table is re-read

```json
"Sync": {
  "FullSyncIntervalMinutes": 0
}
```

| Value | Behavior |
|-------|----------|
| `0` (default, recommended) | FULL sync only on the **first run** (seeds MySQL). All later cycles are INCREMENTAL (`batch-no` > highest in MySQL). Restarts never re-trigger a full sync. |
| Positive (e.g., `60`) | Additionally re-reads the entire table when that many minutes have passed since the last full sync. Only needed if OpenEdge rows can be edited in place. |

**How the decision works:** after every successful FULL sync the bridge writes the completion time to `sync_state.last_full_sync_utc` in MySQL. On each cycle it compares that timestamp against `FullSyncIntervalMinutes`. If MySQL is unreachable at startup, the bridge falls back to FULL — the safe default that guarantees a complete copy.

**Why `0` is right for this deployment:** OpenEdge `id-print` data is batch-based and write-once — new records always arrive in new batches (higher `batch-no`), and existing rows are not edited. The incremental query therefore sees every new record, and a periodic full re-read only wastes database load on a table that never changes. If the provider ever starts correcting existing rows, set this to `60` (hourly refresh) or similar.

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

> **After upgrading from v1.1 or earlier:** the new `sync_state` table does not exist yet, so the first run performs one FULL sync to seed MySQL and record the timestamp. From then on, only new batches are synced — including across restarts.

---

## Checklist for Target PC

- [ ] Windows 10/11 or Server 2016+ (64-bit)
- [ ] OpenEdge ODBC driver installed and DSN configured
- [ ] XAMPP installed and MySQL running
- [ ] `instantid_bridge` database created in MySQL
- [ ] `id-print` / `id_print` table created in **both** OpenEdge and MySQL
- [ ] `sync_state` table present in MySQL (auto-created by the bridge after the first run)
- [ ] `mysql-connector-j-8.0.33.jar` placed in Entrust's lib folder
- [ ] Bridge `publish/` folder copied to `C:\OpenEdgeBridge\`
- [ ] `appsettings.json` updated with correct connection strings
- [ ] Windows Service installed and started
- [ ] Logs checked for errors
