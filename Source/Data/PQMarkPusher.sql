-- Add custom data tables
-- for analysis results here

-- Example:
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PQMarkPusherResult')
BEGIN
    CREATE TABLE PQMarkPusherResult
    (
        ID INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        EventID INT NOT NULL REFERENCES Event(ID),
        MyResult FLOAT NOT NULL
    )

    CREATE NONCLUSTERED INDEX IX_PQMarkPusherResult_EventID
    ON PQMarkPusherResult(EventID ASC)
END
GO

-- For PQ Dashboard integration,
-- uncomment and modify the following lines
-- this needs to return the confidence level of the analytic

--IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'HasPQMarkPusherResult')
--BEGIN
--  DROP FUNCTION HasPQMarkPusherResult
--END
--GO
--
--CREATE FUNCTION HasPQMarkPusherResult
--(
--    @eventID INT
--)
--RETURNS varchar(max)
--AS BEGIN
--    RETURN COALESCE((SELECT Top 1 ConfidenceLevel From PQMarkPusherResult Where EventID = @eventID), "High")
--END
--GO
--
--INSERT INTO EASExtension VALUES('PQMarkPusher', 'HasPQMarkPusherResult')
--GO
