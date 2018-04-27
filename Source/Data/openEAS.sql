-- Add custom data tables
-- for analysis results here

-- Example:
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CSAResult')
BEGIN
	CREATE TABLE CSAResult
	(
		ID INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        EventID INT NOT NULL REFERENCES Event(ID),
        IsDataError VARCHAR(20) NOT NULL,
		IsCapSwitch VARCHAR(20) NOT NULL,
		IsCapSwitchCondL VARCHAR(20) NOT NULL,
		OutFrequency FLOAT NOT NULL,
		OutVoltagesMax FLOAT NOT NULL,
		OutVoltagesMean FLOAT NOT NULL,
		OutQConditionRPBFlag VARCHAR(20) NOT NULL,
		OutQConditionMRPC FLOAT NOT NULL,
		OutQConditionRPCA FLOAT NOT NULL,
		OutQConditionRPCB FLOAT NOT NULL,
		OutQConditionRPCC FLOAT NOT NULL,
		OutQConditionMPFI FLOAT NOT NULL,
		OutQConditionPFA FLOAT NOT NULL,
		OutQConditionPFB FLOAT NOT NULL,
		OutQConditionPFC FLOAT NOT NULL,
		OutRestrikeFlag VARCHAR(20) NOT NULL,
		OutRestrikeNum INT NOT NULL,
		OutRestrikePHA VARCHAR(20) NOT NULL,
		OutRestrikePHB VARCHAR(20) NOT NULL,
		OutRestrikePHC VARCHAR(20) NOT NULL,
		OutVTHDFlag VARCHAR(20) NOT NULL,
		OutVTHDBefore FLOAT NOT NULL,
		OutVTHDAfter FLOAT NOT NULL,
		OutVTHDIncrease FLOAT NOT NULL
	)

    CREATE NONCLUSTERED INDEX IX_CSAResult_EventID
    ON CSAResult(EventID ASC)
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'HasCSAResult')
BEGIN
	DROP FUNCTION HasCSAResult
END
GO

CREATE FUNCTION HasCSAResult
(
	@eventID INT
)
RETURNS INT
AS BEGIN
	DECLARE @hasCSAResult INT

	SELECT @hasCSAResult = COUNT(*)
	FROM CSAResult
	WHERE
		EventID = @eventID AND
		IsCapSwitch = 'Yes'

	RETURN @hasCSAResult
END
GO

MERGE EASExtension AS Target
USING (VALUES('CSAService', 'HasCSAResult')) AS Source(ServiceName, HasResultFunction)
ON Source.ServiceName = Target.ServiceName
WHEN MATCHED THEN
    UPDATE SET HasResultFunction = Source.HasResultFunction
WHEN NOT MATCHED THEN
    INSERT VALUES(Source.ServiceName, Source.HasResultFunction);
GO