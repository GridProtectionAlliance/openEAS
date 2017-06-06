-- Add custom data tables
-- for analysis results here

-- Example:
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'openEASResult')
BEGIN
    CREATE TABLE openEASResult
    (
        ID INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        EventID INT NOT NULL REFERENCES Event(ID),
        MyResult FLOAT NOT NULL
    )

    CREATE NONCLUSTERED INDEX IX_openEASResult_EventID
    ON openEASResult(EventID ASC)
END
GO

-- For PQ Dashboard integration,
-- uncomment and modify the following lines
-- this needs to return the confidence level of the analytic

--IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'HasopenEASResult')
--BEGIN
--  DROP FUNCTION HasopenEASResult
--END
--GO
--
--CREATE FUNCTION HasopenEASResult
--(
--    @eventID INT
--)
--RETURNS varchar(max)
--AS BEGIN
--    RETURN COALESCE(SELECT ConfidenceLevel From openEASResult Where EventID = @eventID, "High")
--END
--GO
--
--INSERT INTO EASExtension VALUES('openEAS', 'HasopenEASResult')
--GO
