using GSF.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace openEASSandBox
{
    public class CSAResult
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int EventID { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public IsDataErr IsDataError { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public IsCapSwitch IsCapSwitch { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public IsCapSwitchCondL IsCapSwitchCondL { get; set; }

        public double OutFrequency { get; set; }
        public double OutVoltagesMax { get; set; }
        public double OutVoltagesMean { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutVTHD OutVTHDFlag { get; set; }

        public double OutVTHDBefore { get; set; }
        public double OutVTHDAfter { get; set; }
        public double OutVthDIncrease { get; set; }
    }

    public class CSAGPResult {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int CSAResultID { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutQConditionRPBFlag OutQConditionRPBFlag { get; set; }

        public double OutQConditionMRPC { get; set; }
        public double OutQConditionRPCA { get; set; }
        public double OutQConditionRPCB { get; set; }
        public double OutQConditionRPCC { get; set; }
        public double OutQConditionMPFI { get; set; }
        public double OutQConditionPFA { get; set; }
        public double OutQConditionPFB { get; set; }
        public double OutQConditionPFC { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrike OutRestrikeFlag { get; set; }

        public int OutRestrikeNum { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrike OutRestrikePHA { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrike OutRestrikePHB { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrike OutRestrikePHC { get; set; }
    }

    public class CSASPResult {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int CSAResultID { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutCapXing OutCapXingANotFailedEnergizing { get; set; }

        public double OutCapXingAReactivePowerContribution { get; set; }
        public double OutCapXingAReactivePowerDeviation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutCapXing OutCapXingBNotFailedEnergizing { get; set; }

        public double OutCapXingBReactivePowerContribution { get; set; }
        public double OutCapXingBReactivePowerDeviation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutCapXing OutCapXingCNotFailedEnergizing { get; set; }

        public double OutCapXingCReactivePowerContribution { get; set; }
        public double OutCapXingCReactivePowerDeviation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrikeEnhancedDeenergizationOperation OutRestrikeEnhancedADeenergizationOperation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrikeEnhancedDeenergizationLevel OutRestrikeEnhancedADeenergizationLevel { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrikeEnhancedDeenergizationOperation OutRestrikeEnhancedBDeenergizationOperation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrikeEnhancedDeenergizationLevel OutRestrikeEnhancedBDeenergizationLevel { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrikeEnhancedDeenergizationOperation OutRestrikeEnhancedCDeenergizationOperation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutRestrikeEnhancedDeenergizationLevel OutRestrikeEnhancedCDeenergizationLevel { get; set; }


        public double OutSyncAPhaseAngleDeviation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutSyncStatus OutSyncAStatus { get; set; }

        public double OutSyncBPhaseAngleDeviation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutSyncStatus OutSyncBStatus { get; set; }

        public double OutSyncCPhaseAngleDeviation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OutSyncStatus OutSyncCStatus { get; set; }
    }

    public enum IsDataErr : int
    {
        Complete = 0,
        Bad = 1
    }

    public enum IsCapSwitch : int
    {
        No = -1,
        Undetermined = 0,
        Yes = 1
    }

    public enum IsCapSwitchCondL : int
    {
        Low = 1,
        Moderate = 2,
        High = 3
    }

    public enum OutQConditionRPBFlag : int
    {
        Balanced = -1,
        Unknown = 0,
        Unbalanced = 1
    }

    public enum OutRestrike : int
    {
        No = 0,
        Yes = 1
    }

    public enum OutVTHD : int
    {
        None = 0,
        Yes = 1
    }

    public enum OutCapXing : int {
        Successful = 0,
        Failed = 1
    }

    public enum OutRestrikeEnhancedDeenergizationOperation: int
    {
        Complete = 1,
        NotComplete = -1
    }

    public enum OutRestrikeEnhancedDeenergizationLevel : int
    {
        None = 1,
        Some = 2
    }

    public enum OutSyncStatus : int {
        Normal = 0,
        Premature = -1,
        Delayed = 1,
        Failed = 2
    }

}
