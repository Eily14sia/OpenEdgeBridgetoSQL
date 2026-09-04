-- Load sample data into pub."id-print" (iddb)
-- Run with sqlexp (server must be running with -ServerType Both):
--   sqlexp -db iddb -S 11918 -H localhost -user admin -password <pw> -infile load-id-print.sql
-- Or paste the INSERT below interactively at the SQL> prompt.
--
-- Notes on conversion from the .d dump (dateformat=mdy-1950):
--   2-digit years: < 50 -> 20xx, >= 50 -> 19xx
--   logical yes/no -> 1/0
--   unknown (?)      -> NULL
--   printed-tm stays a character string (e.g. '08/24/2026  4:21:44 PM')

INSERT INTO pub."id-print" (
    "batch-no", "batch-date", "app-no", "dob",
    "cp-no", "fullname", "hcp-fullname", "address", "hcp-cpno",
    "lname", "fname", "mname",
    "spa-docno", "page-no", "book-no", "series-no", "spa-atty", "spa-dt-issue",
    "printed", "printed-dt", "printed-tm",
    "tran-type", "image-data", "id-code", "sfx", "photo",
    "gender", "date-issued", "id-no", "year-applied", "control-no"
)
VALUES
(1, '2026-08-07', 22, '2000-01-01', 'APP CP2XX', 'DELA CRUZ X, JUANITOX MMX SSX', 'DELA CRUZ--, JUANITA-- M-- S--', '123-- BONI AVENUE cor A. MARTINEZ 222 XXX NEW ZANIGA, Mandaluyong City 1550', 'HCP CP1', 'DELA CRUZ x', 'JUANITOx', 'MMx', '', '', '', '', 'SPA ATTY', '2026-07-11', 1, NULL, '08/24/2026  4:21:44 PM', 1, NULL, 0, 'SSx', NULL, 0, NULL, 0, 0, 0),
(1, '2026-08-07', 23, '2001-01-01', 'HCP CP1', 'DELA CRUZ--, JUANITA-- M-- S--', 'DELA CRUZ X, JUANITOX MMX SSX', '123-- BONI AVENUE cor A. MARTINEZ 222 XXX NEW ZANIGA, Mandaluyong City 1550', 'APP CP2XX', 'DELA CRUZ--', 'JUANITA--', 'M--', '', '', '', '', 'SPA ATTY', '2026-07-11', 0, NULL, '', 1, NULL, 0, 'S--', NULL, 0, NULL, 0, 0, 0),
(1, '2026-08-07', 30, '1955-04-16', '1321', 'DCAMPO, MYRNA D', 'AVELINO, SHARON LEDDA', '  A.T REYES ST. COR. AGLIPAY ST. POBLACION, Mandaluyong City 1550', '4535', 'DCAMPO', 'MYRNA', 'D', '', '', '', '', 'werwr', '2002-02-02', 0, NULL, '', 1, NULL, 0, '', NULL, 0, NULL, 0, 0, 0),
(1, '2026-08-07', 31, '1981-08-19', '4535', 'AVELINO, SHARON LEDDA', 'DCAMPO, MYRNA D', ' A.T REYES ST. COR. AGLIPAY ST. POBLACION, Mandaluyong City 1550', '1321', 'AVELINO', 'SHARON', 'LEDDA', '', '', '', '', '', NULL, 1, NULL, '08/07/2026 10:46:30 PM', 1, NULL, 0, '', NULL, 0, NULL, 0, 0, 0),
(1, '2026-08-07', 28, '1956-01-15', '13', 'ELE JR, GAUDENCIO DARUSIN', 'QUINTAL, ARNULFO F', '  HAIG ST, COR BERNARDO DAANG BAKAL, Mandaluyong City 1550', '24234', 'ELE JR', 'GAUDENCIO', 'DARUSIN', '', '', '', '', 'sfsf', '2026-01-01', 0, NULL, '', 1, NULL, 0, '', NULL, 0, NULL, 0, 0, 0),
(1, '2026-08-07', 29, '1941-11-18', '24234', 'QUINTAL, ARNULFO F', 'ELE JR, GAUDENCIO DARUSIN', ' HAIG ST, COR BERNARDO DAANG BAKAL, Mandaluyong City 1550', '13', 'QUINTAL', 'ARNULFO', 'F', '', '', '', '', '', NULL, 1, NULL, '09/01/2026  2:16:21 PM', 1, NULL, 0, '', NULL, 0, NULL, 0, 0, 0),
(2, '2026-08-07', 26, '2000-01-01', '1321', 'DELA CRUZ, FERDINAND', 'SANTOS----, DOWERS-- S-- ----', '123 A. Bonifacio St. COR. PINAGTIPUNAN HAGDAN BATO LIBIS, Mandaluyong City 1550', '1321', 'DELA CRUZ', 'FERDINAND', '', '', '', '', '', 'spa atty', '2026-07-14', 1, NULL, '08/07/2026 10:45:53 PM', 1, NULL, 0, '', NULL, 0, NULL, 0, 0, 0),
(2, '2026-08-07', 27, '1946-07-17', '1321', 'SANTOS----, DOWERS-- S-- ----', 'DELA CRUZ, FERDINAND', '123 A. Bonifacio St. COR. PINAGTIPUNAN HAGDAN BATO LIBIS, Mandaluyong City 1550', '1321', 'SANTOS----', 'DOWERS--', 'S--', '', '', '', '', '', NULL, 1, NULL, '08/07/2026 10:45:35 PM', 1, NULL, 0, '----', NULL, 0, NULL, 0, 0, 0),
(3, '2026-08-07', 14, '2002-01-07', '09109940731', 'CUENCO, CATHERINE KATE FEREDO', 'EGERA, KARL CHRISTIAN GALGANA', '894 E. PANTALEON BARANGKA ITAAS, Mandaluyong City 1330', '090012032554', 'CUENCO', 'CATHERINE KATE', 'FEREDO', '', '', '', '', '', NULL, 0, NULL, '', 1, NULL, 0, '', NULL, 0, NULL, 0, 0, 0);

COMMIT WORK;
