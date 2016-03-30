-- Add custom data tables
-- for analysis results here

-- Example:
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SandBoxResult')
BEGIN
	CREATE TABLE SandBoxResult
	(
		ID INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        EventID INT NOT NULL REFERENCES Event(ID),
        MyResult FLOAT NOT NULL
	)
END
GO

-- For PQ Dashboard integration,
-- uncomment and modify the following lines

--CREATE FUNCTION HasSdbxResult
--(
--    @eventID INT
--)
--RETURNS INT
--AS BEGIN
--    DECLARE @hasResult INT
--
--    SELECT @hasResult = COUNT(*)
--    FROM SandBoxResult
--    WHERE EventID = @eventID
--
--    RETURN @hasResult
--END
--GO
--
--INSERT INTO EASExtension VALUES('XDASandBox', 'HasSdbxResult')
--GO
