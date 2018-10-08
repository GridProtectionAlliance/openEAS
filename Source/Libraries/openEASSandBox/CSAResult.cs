using GSF.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace openEASSandBox
{
    [TableName("CSA_2_Result")]
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
        public OperationType OutOpTypeA { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OperationType OutOpTypeB { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public OperationType OutOpTypeC { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public IsResonance IsResonanceA { get; set; }
        public double ResFrequencyA { get; set; }
        public double PeakPUVA { get; set; }
        public double VrmsA_bf { get; set; }
        public double VrmsA_af { get; set; }
        public double DVrmsA { get; set; }
        public double PeakIA { get; set; }
        public double IrmsA_bf { get; set; }
        public double IrmsA_af { get; set; }
        public double DIrmsA { get; set; }
        public double SwitchXientFreqIAHz { get; set; }
        public double DQA { get; set; }
        public double PFAInit { get; set; }
        public double PFAEnd { get; set; }
        public double THDIA_bf { get; set; }
        public double THDIA_af { get; set; }
        public double DTHDIA { get; set; }
        public double THDVA_bf { get; set; }
        public double THDVA_af { get; set; }
        public double DTHDVA { get; set; }
        public int StepA_bf { get; set; }
        public int StepA_af { get; set; }
        public double XCapA_bf { get; set; }
        public double XCapA_af { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public IsResonance IsResonanceB { get; set; }
        public double ResFrequencyB { get; set; }
        public double PeakPUVB { get; set; }
        public double VrmsB_bf { get; set; }
        public double VrmsB_af { get; set; }
        public double DVrmsB { get; set; }
        public double PeakIB { get; set; }
        public double IrmsB_bf { get; set; }
        public double IrmsB_af { get; set; }
        public double DIrmsB { get; set; }
        public double SwitchXientFreqIBHz { get; set; }
        public double DQB { get; set; }
        public double PFBInit { get; set; }
        public double PFBEnd { get; set; }
        public double THDIB_bf { get; set; }
        public double THDIB_af { get; set; }
        public double DTHDIB { get; set; }
        public double THDVB_bf { get; set; }
        public double THDVB_af { get; set; }
        public double DTHDVB { get; set; }
        public double StepB_bf { get; set; }
        public double StepB_af { get; set; }
        public double XCapB_bf { get; set; }
        public double XCapB_af { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FieldDataType(System.Data.DbType.String)]
        public IsResonance IsResonanceC { get; set; }
        public double ResFrequencyC { get; set; }
        public double PeakPUVC { get; set; }
        public double VrmsC_bf { get; set; }
        public double VrmsC_af { get; set; }
        public double DVrmsC { get; set; }
        public double PeakIC { get; set; }
        public double IrmsC_bf { get; set; }
        public double IrmsC_af { get; set; }
        public double DIrmsC { get; set; }
        public double SwitchXientFreqICHz { get; set; }
        public double DQC { get; set; }
        public double PFCInit { get; set; }
        public double PFCEnd { get; set; }
        public double THDIC_bf { get; set; }
        public double THDIC_af { get; set; }
        public double DTHDIC { get; set; }
        public double THDVC_bf { get; set; }
        public double THDVC_af { get; set; }
        public double DTHDVC { get; set; }
        public double StepC_bf { get; set; }
        public double StepC_af { get; set; }
        public double XCapC_bf { get; set; }
        public double XCapC_af { get; set; }

        public double SwitchedOutDurA { get; set; }
        public double SwitchedOutDurB { get; set; }
        public double SwitchedOutDurC { get; set; }

        public double StepChangeRpA { get; set; }
        public double StepChangeRpB { get; set; }
        public double StepChangeRpC { get; set; }

        public double RpBeforeSwitchedOutA { get; set; }
        public double RpBeforeSwitchedOutB { get; set; }
        public double RpBeforeSwitchedOutC { get; set; }

        public double RpAfterSwitchedOutA { get; set; }
        public double RpAfterSwitchedOutB { get; set; }
        public double RpAfterSwitchedOutC { get; set; }

        public double StepChangeXpA { get; set; }
        public double StepChangeXpB { get; set; }
        public double StepChangeXpC { get; set; }

        public double XpBeforeSwitchedOutA { get; set; }
        public double XpBeforeSwitchedOutB { get; set; }
        public double XpBeforeSwitchedOutC { get; set; }

        public double XpAfterSwitchedOutA { get; set; }
        public double XpAfterSwitchedOutB { get; set; }
        public double XpAfterSwitchedOutC { get; set; }

        public double TimeOfEvent { get; set; }
        public double Time1stClosingOp { get; set; }
        public double Time2ndClosingOp { get; set; }
        public double PhaseDiff1stClosing { get; set; }
        public double FirstCloseEnergy { get; set; }

        public double FirstCloseA { get; set; }
        public double SecondCloseA { get; set; }
        public double FirstCloseB { get; set; }
        public double SecondCloseB { get; set; }
        public double FirstCloseC { get; set; }
        public double SecondCloseC { get; set; }

        public double AbsOpenTimeA { get; set; }
        public double AbsOpenTimeB { get; set; }
        public double AbsOpenTimeC { get; set; }


    }

    public enum IsDataErr : int
    {
        Complete = 0,
        Bad = 1
    }

    public enum OperationType: int
    {
        UnidentifiedOrFailed = 0,
        MissingPoleCondition = 11,
        SympatheticRinging = 12,
        CapBankDeEnergize = 2,
        ClosingWithoutControl = 30,
        ClosingWithControl = 31
    }

    public enum IsResonance : int {
        No = 0,
        Yes = 1
    }

}
