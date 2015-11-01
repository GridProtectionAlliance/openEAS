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
