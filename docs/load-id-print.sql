-- Load sample data into pub."id-print" (iddb) — shows ONE RTC row then ONE OSCA row.
-- Run with sqlexp (server must be running with -ServerType Both):
--   sqlexp -db iddb -S 11918 -H localhost -user admin -password <pw> -infile load-id-print.sql
-- Or paste the INSERT below interactively at the SQL> prompt.
--
-- The "id-print" table holds TWO ID kinds side by side, distinguished by "tran-type":
--   "tran-type" = 1 (RTC)           -> keyed by batch-no + app-no; batch/app are filled in
--   "tran-type" = 2 (OSCA / senior) -> keyed by control-no + id-no; batch-no/app-no are NULL
-- "trans-no" is the table-wide PRIMARY key (id-idx) and is present on BOTH kinds.
-- "id-type" is 1=New, 2=ReIssue/Reprint (both kinds).
-- Logical "printed" is 0/1 in SQL (OpenEdge LOGICAL).

INSERT INTO pub."id-print" (
    "trans-no", "trans-date", "trans-time",
    "batch-no", "batch-date", "app-no",
    "dob", "cp-no", "fullname", "hcp-fullname", "address", "hcp-cpno",
    "lname", "fname", "mname",
    "spa-docno", "page-no", "book-no", "series-no", "spa-atty", "spa-dt-issue",
    "printed", "printed-dt", "printed-tm",
    "tran-type", "image-data", "id-code", "sfx", "photo",
    "gender", "date-issued", "id-no", "year-applied", "control-no",
    "id-type", "PVC-No", "age"
)
VALUES
-- RTC record (tran-type 1): batch-no + app-no are its key; control-no/id-no left empty
(1001, '2026-01-15', '10:30:00',
 1, '2026-01-15', 1001,
 '1990-05-20', 'CP-001', 'Juan Dela Cruz', 'Dr. Maria Santos', '123 Rizal St, Manila', 'HCP-001',
 'Dela Cruz', 'Juan', 'Santos',
 'SPA-001', 'P001', 'BK001', 'SR001', 'Atty. Reyes', '2025-12-01',
 0, NULL, '',
 1, NULL, 5001, '', NULL,
 1, NULL, '', 0, 0,
 1, 'PVC-000001', 0),

-- OSCA / senior record (tran-type 2): control-no + id-no are its key; batch-no/app-no are NULL
(2001, '2026-01-16', '09:15:00',
 NULL, NULL, NULL,
 '1941-11-18', '', 'Maria Clara Santos', '', '456 Mabini St, Quezon City', '',
 'Santos', 'Maria', 'Clara',
 '', '', '', '', '', NULL,
 0, NULL, '',
 2, NULL, 0, '', NULL,
 2, '2026-01-16', 'OSCA-2026-0001', 2026, 9001,
 1, 'PVC-000901', 84);

COMMIT WORK;
