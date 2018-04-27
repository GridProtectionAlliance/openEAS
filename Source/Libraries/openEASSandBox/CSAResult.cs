using GSF.Data.Model;

namespace openEASSandBox
{
    public class CSAResult
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int EventID { get; set; }
        public IsDataErr IsDataError { get; set; }
        public IsCapSwitch IsCapSwitch { get; set; }
        public IsCapSwitchCondL IsCapSwitchCondL { get; set; }
        public double OutFrequency { get; set; }
        public double OutVoltagesMax { get; set; }
        public double OutVoltagesMean { get; set; }
    }

    public class CSAGPResult {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int CSAResultID { get; set; }

        public OutQConditionRPBFlag OutQConditionRPBFlag { get; set; }
        public double OutQConditionMRPC { get; set; }
        public double OutQConditionRPCA { get; set; }
        public double OutQConditionRPCB { get; set; }
        public double OutQConditionRPCC { get; set; }
        public double OutQConditionMPFI { get; set; }
        public double OutQConditionPFA { get; set; }
        public double OutQConditionPFB { get; set; }
        public double OutQConditionPFC { get; set; }
        public OutRestrike OutRestrikeFlag { get; set; }
        public int OutRestrikeNum { get; set; }
        public OutRestrike OutRestrikePHA { get; set; }
        public OutRestrike OutRestrikePHB { get; set; }
        public OutRestrike OutRestrikePHC { get; set; }
        public OutVTHD OutVTHDFlag { get; set; }
        public double OutVTHDBefore { get; set; }
        public double OutVTHDAfter { get; set; }
        public double OutVthDIncrease { get; set; }
    }

    public class CSASPResult {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int CSAResultID { get; set; }
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


}
