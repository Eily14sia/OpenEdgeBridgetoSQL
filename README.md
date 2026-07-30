# OpenEdge → MySQL Bridge (C# .NET 8)

## Project Goal

Create a Windows Service (develop first as a Console Application) that synchronizes data from a Progress OpenEdge database into a MySQL database.

Architecture:

```
Progress OpenEdge
        │
        │ ODBC Driver
        ▼
C# (.NET 8)
        │
        ▼
MySQL
        │
        ▼
Entrust Instant ID
```

---

# Requirements

## Software

- Visual Studio 2022
- .NET 8
- MySQL Server
- MySQL Workbench
- Progress OpenEdge 12.8
- OpenEdge ODBC Driver (Wire Protocol)

NuGet Packages

```
MySql.Data
Microsoft.Extensions.Hosting.WindowsServices
```

---

# Current Environment

## OpenEdge

Database

```
instantID
```

Host

```
localhost
```

Port

```
20940
```

Table

```
cardholder
```

Columns

```
ID
EMPLOYEE_ID
FIRST_NAME
LAST_NAME
DEPARTMENT
POSITION
PRINT_COUNT
```

---

## MySQL

Database

```
instantid_bridge
```

Table

```
cardholders
```

Schema

```sql
CREATE TABLE cardholders
(
    id INT AUTO_INCREMENT PRIMARY KEY,

    openedge_id INT NOT NULL,

    employee_id VARCHAR(50) UNIQUE,

    first_name VARCHAR(100),
    last_name VARCHAR(100),

    department VARCHAR(100),

    position VARCHAR(100),

    print_count INT DEFAULT 0,

    synced_at DATETIME,

    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    ON UPDATE CURRENT_TIMESTAMP
);
```

---

# ODBC

Use a System DSN.

DSN Name

```
OpenEdgeInstantID
```

Driver

```
DataDirect OpenEdge Wire Protocol
```

Host

```
localhost
```

Port

```
20940
```

Database

```
instantID
```

---

# Project Structure

```
OpenEdgeBridge
│
├── Models
│     Cardholder.cs
│
├── Repositories
│     OpenEdgeRepository.cs
│     MySqlRepository.cs
│
├── Services
│     SyncService.cs
│
├── appsettings.json
│
└── Program.cs
```

---

# Model

```csharp
public class Cardholder
{
    public int Id { get; set; }

    public string EmployeeId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Department { get; set; }

    public string Position { get; set; }

    public int PrintCount { get; set; }
}
```

---

# appsettings.json

```json
{
  "ConnectionStrings": {
    "OpenEdge": "DSN=OpenEdgeInstantID;",
    "MySql": "Server=localhost;Database=instantid_bridge;Uid=root;Pwd=;"
  },
  "Sync": {
    "IntervalSeconds": 10
  }
}
```

---

# Repository Pattern

Create

```
Repositories/OpenEdgeRepository.cs
```

Responsibilities

- Open ODBC connection
- Read cardholder table
- Return List<Cardholder>

Methods

```
Task<List<Cardholder>> GetCardholdersAsync();
Task UpdatePrintCountAsync(int id);
```

---

Create

```
Repositories/MySqlRepository.cs
```

Responsibilities

- Open MySQL connection
- Insert records
- Update existing records
- Use UPSERT

Methods

```
Task UpsertCardholderAsync(Cardholder cardholder);
Task<List<Cardholder>> GetPendingPrintsAsync();
```

---

# Service Layer

Create

```
Services/SyncService.cs
```

Responsibilities

- Read OpenEdge
- Compare with MySQL
- Insert if new
- Update if changed
- Write logs
- Repeat every configured interval

Methods

```
SyncOpenEdgeToMySql()

SyncPrintCountBackToOpenEdge()
```

---

# Program.cs

Console Version

```
while(true)
{
    await syncService.SyncOpenEdgeToMySql();

    await syncService.SyncPrintCountBackToOpenEdge();

    await Task.Delay(10000);
}
```

Later convert to Worker Service.

---

# OpenEdge SQL

Read

```sql
SELECT
    ID,
    EMPLOYEE_ID,
    FIRST_NAME,
    LAST_NAME,
    DEPARTMENT,
    POSITION,
    PRINT_COUNT
FROM cardholder;
```

Update

```sql
UPDATE cardholder
SET PRINT_COUNT = PRINT_COUNT + 1
WHERE ID = ?;
```

---

# MySQL UPSERT

```sql
INSERT INTO cardholders
(
    openedge_id,
    employee_id,
    first_name,
    last_name,
    department,
    position,
    print_count,
    synced_at
)
VALUES
(
    @id,
    @employee,
    @first,
    @last,
    @department,
    @position,
    @count,
    NOW()
)
ON DUPLICATE KEY UPDATE
first_name=VALUES(first_name),
last_name=VALUES(last_name),
department=VALUES(department),
position=VALUES(position),
print_count=VALUES(print_count),
synced_at=NOW();
```

---

# Synchronization Flow

```
Loop Forever

↓

Connect OpenEdge

↓

Read cardholder table

↓

For each record

↓

Does employee_id exist in MySQL?

↓

NO

↓

INSERT

↓

YES

↓

UPDATE

↓

Sleep 10 seconds

↓

Repeat
```

---

# Future Print Flow

```
Entrust Instant ID

↓

Print ID

↓

Increase print_count in MySQL

↓

Bridge detects change

↓

Update OpenEdge

↓

Done
```

---

# Logging

Create Logs folder.

Log

- Startup
- Shutdown
- Synchronization
- SQL Errors
- ODBC Errors
- MySQL Errors
- Print Count Updates

Example

```
2026-07-22 15:20:11

Connected to OpenEdge

15 records synchronized

0 failed
```

---

# Error Handling

Retry

```
3 attempts
```

Reconnect automatically

Handle

- ODBC exceptions
- MySQL exceptions
- Timeout
- Lost Connection

---

# Coding Guidelines

- Use async/await
- Repository Pattern
- Dependency Injection
- IConfiguration
- ILogger
- Strongly typed models
- No hardcoded SQL except repositories
- Parameterized queries only
- SOLID principles
- Clean Architecture where practical
- Dispose all database connections with using statements

---

# Development Phases

## Phase 1

- Connect to OpenEdge
- Read cardholder
- Display records

## Phase 2

- Connect to MySQL
- Insert records

## Phase 3

- Synchronize updates

## Phase 4

- Synchronize print_count back to OpenEdge

## Phase 5

- Convert Console App to Windows Service

## Phase 6

- Production deployment