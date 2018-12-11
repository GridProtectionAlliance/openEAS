using GSF.ComponentModel.DataAnnotations;
using GSF.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;

namespace openEASSandBox
{
    [TableName("CSA_2_Result")]
    public class CSAResult
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public int EventID { get; set; }
        public bool Valid { get; set; }

        [Label("A flag for data error")]
        public string IsDataError { get; set; }

        [Label("Type of capacitor events or conditions for phase A")]
        public string OutOpTypeA { get; set; }
        [Label("Type of capacitor events or conditions for phase B")]
        public string OutOpTypeB { get; set; }
        [Label("Type of capacitor events or conditions for phase C")]
        public string OutOpTypeC { get; set; }

        [Label("Harmonic resonance for phase A")]
        public string IsResonanceA { get; set; }
        [Label("Frequency during the resonance condition for phase A")]
        public double ResFrequencyA { get; set; }
        [Label("Instantaneous peak voltage in pu for phase A")]
        public double PeakPUVA { get; set; }
        [Label("Line-to-neutral rms voltage before energizing in pu (beginning) for phase A")]
        public double VrmsA_bf { get; set; }
        [Label("Line-to-neutral rms voltage after energizing operation completes in pu for phase A")]
        public double VrmsA_af { get; set; }
        [Label("Change in rms voltes (end minus beginning) in percent for phase A")]
        public double DVrmsA { get; set; }
        [Label("Instantaneous peak current of phase A in Apk")]
        public double PeakIA { get; set; }
        [Label("Current in Arms before energizing (beginning) for phase A")]
        public double IrmsA_bf { get; set; }
        [Label("Current in Arms after energizing operation completes for phase A")]
        public double IrmsA_af { get; set; }
        [Label("Change in current (end minus beginning) for phase A")]
        public double DIrmsA { get; set; }
        [Label("Frequency of the switching transient during the energizing in Hz for phase A")]
        public double SwitchXientFreqIAHz { get; set; }
        [Label("Change in reactive power (end minus beginning) in kvar for phase A")]
        public double DQA { get; set; }
        [Label("Power factor before the switching operation for phase A (dimensionless, 1 = unity pf)")]
        public double PFAInit { get; set; }
        [Label("Power factor after the switching operation for phase A (dimensionless, 1 = unity pf)")]
        public double PFAEnd { get; set; }
        [Label("THD of current on the basis of the fundamental before the switching op for phase A")]
        public double THDIA_bf { get; set; }
        [Label("THD of current on the basis of the fundamental after the switching op for phase A")]
        public double THDIA_af { get; set; }
        [Label("Change in THD of current (after - before) in percent for phase A")]
        public double DTHDIA { get; set; }
        [Label("THD of voltage on the basis of the fundamental before the switching op for phase A")]
        public double THDVA_bf { get; set; }
        [Label("THD of voltage on the basis of the fundamental after the switching op for phase A")]
        public double THDVA_af { get; set; }
        [Label("Change in THD of voltage (after - before) in percent for phase A")]
        public double DTHDVA { get; set; }
        [Label("The number of capacitor step before energizing for phase A")]
        public int StepA_bf { get; set; }
        [Label("The number of capacitor step after energizing for phase A")]
        public int StepA_af { get; set; }
        [Label("Capacitive reactance before energizing in ohm for phase A")]
        public double XCapA_bf { get; set; }
        [Label("Capacitive reactance after energizing in ohm for phase A")]
        public double XCapA_af { get; set; }

        [Label("Harmonic resonance for phase B")]
        public string IsResonanceB { get; set; }
        [Label("Frequency during the resonance condition for phase B")]
        public double ResFrequencyB { get; set; }
        [Label("Instantaneous peak voltage in pu for phase B")]
        public double PeakPUVB { get; set; }
        [Label("Line-to-neutral rms voltage before energizing in pu (beginning) for phase B")]
        public double VrmsB_bf { get; set; }
        [Label("Line-to-neutral rms voltage after energizing operation completes in pu for phase B")]
        public double VrmsB_af { get; set; }
        [Label("Change in rms voltes (end minus beginning) in percent for phase B")]
        public double DVrmsB { get; set; }
        [Label("Instantaneous peak current of phase B in Apk")]
        public double PeakIB { get; set; }
        [Label("Current in Arms before energizing (beginning) for phase B")]
        public double IrmsB_bf { get; set; }
        [Label("Current in Arms after energizing operation completes for phase B")]
        public double IrmsB_af { get; set; }
        [Label("Change in current (end minus beginning) for phase B")]
        public double DIrmsB { get; set; }
        [Label("Frequency of the switching transient during the energizing in Hz for phase B")]
        public double SwitchXientFreqIBHz { get; set; }
        [Label("Change in reactive power (end minus beginning) in kvar for phase B")]
        public double DQB { get; set; }
        [Label("Power factor before the switching operation for phase B (dimensionless, 1 = unity pf)")]
        public double PFBInit { get; set; }
        [Label("Power factor after the switching operation for phase B (dimensionless, 1 = unity pf)")]
        public double PFBEnd { get; set; }
        [Label("THD of current on the basis of the fundamental before the switching op for phase B")]
        public double THDIB_bf { get; set; }
        [Label("THD of current on the basis of the fundamental after the switching op for phase B")]
        public double THDIB_af { get; set; }
        [Label("Change in THD of current (after - before) in percent for phase B")]
        public double DTHDIB { get; set; }
        [Label("THD of current on the basis of the fundamental before the switching op for phase B")]
        public double THDVB_bf { get; set; }
        [Label("THD of voltage on the basis of the fundamental after the switching op for phase B")]
        public double THDVB_af { get; set; }
        [Label("Change in THD of voltage (after - before) in percent for phase B")]
        public double DTHDVB { get; set; }
        [Label("The number of capacitor step before energizing for phase B")]
        public double StepB_bf { get; set; }
        [Label("The number of capacitor step after energizing for phase B")]
        public double StepB_af { get; set; }
        [Label("Capacitive reactance before energizing in ohm for phase B")]
        public double XCapB_bf { get; set; }
        [Label("Capacitive reactance after energizing in ohm for phase B")]
        public double XCapB_af { get; set; }

        [Label("Harmonic resonance for phase C")]
        public string IsResonanceC { get; set; }
        [Label("Frequency during the resonance condition for phase C")]
        public double ResFrequencyC { get; set; }
        [Label("Instantaneous peak voltage in pu for phase C")]
        public double PeakPUVC { get; set; }
        [Label("Line-to-neutral rms voltage before energizing in pu (beginning) for phase C")]
        public double VrmsC_bf { get; set; }
        [Label("Line-to-neutral rms voltage after energizing operation completes in pu for phase B")]
        public double VrmsC_af { get; set; }
        [Label("Change in rms voltes (end minus beginning) in percent for phase C")]
        public double DVrmsC { get; set; }
        [Label("Instantaneous peak current of phase C in Apk")]
        public double PeakIC { get; set; }
        [Label("Current in Arms before energizing (beginning) for phase C")]
        public double IrmsC_bf { get; set; }
        [Label("Current in Arms after energizing operation completes for phase C")]
        public double IrmsC_af { get; set; }
        [Label("Change in current (end minus beginning) for phase C")]
        public double DIrmsC { get; set; }
        [Label("Frequency of the switching transient during the energizing in Hz for phase C")]
        public double SwitchXientFreqICHz { get; set; }
        [Label("Change in reactive power (end minus beginning) in kvar for phase C")]
        public double DQC { get; set; }
        [Label("Power factor before the switching operation for phase C (dimensionless, 1 = unity pf)")]
        public double PFCInit { get; set; }
        [Label("Power factor after the switching operation for phase C (dimensionless, 1 = unity pf)")]
        public double PFCEnd { get; set; }
        [Label("THD of current on the basis of the fundamental before the switching op for phase C")]
        public double THDIC_bf { get; set; }
        [Label("THD of current on the basis of the fundamental after the switching op for phase C")]
        public double THDIC_af { get; set; }
        [Label("Change in THD of current (after - before) in percent for phase C")]
        public double DTHDIC { get; set; }
        [Label("THD of current on the basis of the fundamental before the switching op for phase C")]
        public double THDVC_bf { get; set; }
        [Label("THD of voltage on the basis of the fundamental after the switching op for phase C")]
        public double THDVC_af { get; set; }
        [Label("Change in THD of voltage (after - before) in percent for phase C")]
        public double DTHDVC { get; set; }
        [Label("The number of capacitor step before energizing for phase C")]
        public double StepC_bf { get; set; }
        [Label("The number of capacitor step after energizing for phase C")]
        public double StepC_af { get; set; }
        [Label("Capacitive reactance before energizing in ohm for phase C")]
        public double XCapC_bf { get; set; }
        [Label("Capacitive reactance after energizing in ohm for phase C")]
        public double XCapC_af { get; set; }

        [Label("Time between the first closing and the switching out of the R and X branch during phase A pre-insertion closing")]
        public double SwitchedOutDurA { get; set; }
        [Label("Time between the first closing and the switching out of the R and X branch during phase B pre-insertion closing")]
        public double SwitchedOutDurB { get; set; }
        [Label("Time between the first closing and the switching out of the R and X branch during phase C pre-insertion closing")]
        public double SwitchedOutDurC { get; set; }

        [Label("Step change in the resistance during pre-insertion closing in ohms for phase A")]
        public double StepChangeRpA { get; set; }
        [Label("Step change in the resistance during pre-insertion closing in ohms for phase B")]
        public double StepChangeRpB { get; set; }
        [Label("Step change in the resistance during pre-insertion closing in ohms for phase C")]
        public double StepChangeRpC { get; set; }

        [Label("Estimated resistance value before swithcing out the pre-insertion branch for phase A")]
        public double RpBeforeSwitchedOutA { get; set; }
        [Label("Estimated resistance value before swithcing out the pre-insertion branch for phase B")]
        public double RpBeforeSwitchedOutB { get; set; }
        [Label("Estimated resistance value before swithcing out the pre-insertion branch for phase C")]
        public double RpBeforeSwitchedOutC { get; set; }

        [Label("Estimated resistance value after swithcing out the pre-insertion branch for phase A")]
        public double RpAfterSwitchedOutA { get; set; }
        [Label("Estimated resistance value after swithcing out the pre-insertion branch for phase B")]
        public double RpAfterSwitchedOutB { get; set; }
        [Label("Estimated resistance value after swithcing out the pre-insertion branch for phase C")]
        public double RpAfterSwitchedOutC { get; set; }

        [Label("Step change in the reactance during pre-insertion closing in ohms for phase A")]
        public double StepChangeXpA { get; set; }
        [Label("Step change in the reactance during pre-insertion closing in ohms for phase B")]
        public double StepChangeXpB { get; set; }
        [Label("Step change in the reactance during pre-insertion closing in ohms for phase C")]
        public double StepChangeXpC { get; set; }

        [Label("Estimated reactance value before switching out the pre-insertion for branch for phase A")]
        public double XpBeforeSwitchedOutA { get; set; }
        [Label("Estimated reactance value before switching out the pre-insertion for branch for phase B")]
        public double XpBeforeSwitchedOutB { get; set; }
        [Label("Estimated reactance value before switching out the pre-insertion for branch for phase C")]
        public double XpBeforeSwitchedOutC { get; set; }

        [Label("Estimated reactance value after switching out the pre-insertion for branch for phase A")]
        public double XpAfterSwitchedOutA { get; set; }
        [Label("Estimated reactance value after switching out the pre-insertion for branch for phase B")]
        public double XpAfterSwitchedOutB { get; set; }
        [Label("Estimated reactance value after switching out the pre-insertion for branch for phase C")]
        public double XpAfterSwitchedOutC { get; set; }

        [Label("Time instant of the first sample point")]
        public double TimeOfEvent { get; set; }

        [Label("Time instanst of the first closing with refrence to first sample point for phase A")]
        public double Time1stClosingOpA { get; set; }
        [Label("Time instanst of the first closing with refrence to first sample point for phase B")]
        public double Time1stClosingOpB { get; set; }
        [Label("Time instanst of the first closing with refrence to first sample point for phase C")]
        public double Time1stClosingOpC { get; set; }

        [Label("Time instanst of the second closing with refrence to first sample point for phase A")]
        public double Time2ndClosingOpA { get; set; }
        [Label("Time instanst of the second closing with refrence to first sample point for phase B")]
        public double Time2ndClosingOpB { get; set; }
        [Label("Time instanst of the second closing with refrence to first sample point for phase C")]
        public double Time2ndClosingOpC { get; set; }

        [Label("Phase angle difference between voltage(reference) and current at the time for the first closing for phase A")]
        public double PhaseDiff1stClosingA { get; set; }
        [Label("Phase angle difference between voltage(reference) and current at the time for the first closing for phase B")]
        public double PhaseDiff1stClosingB { get; set; }
        [Label("Phase angle difference between voltage(reference) and current at the time for the first closing for phase C")]
        public double PhaseDiff1stClosingC { get; set; }

        [Label("i^2t during the first closing for phase A")]
        public double FirstCloseEnergyA { get; set; }
        [Label("i^2t during the first closing for phase B")]
        public double FirstCloseEnergyB { get; set; }
        [Label("i^2t during the first closing for phase C")]
        public double FirstCloseEnergyC { get; set; }

        [Label("Time instant of the first closing for phase A")]
        public DateTime? FirstCloseA { get; set; }
        [Label("Time instant of the second closing for phase A")]
        public DateTime? SecondCloseA { get; set; }
        [Label("Time instant of the first closing for phase B")]
        public DateTime? FirstCloseB { get; set; }
        [Label("Time instant of the second closing for phase B")]
        public DateTime? SecondCloseB { get; set; }
        [Label("Time instant of the first closing for phase C")]
        public DateTime? FirstCloseC { get; set; }
        [Label("Time instant of the second closing for phase C")]
        public DateTime? SecondCloseC { get; set; }

        [Label("Time instant of the opening for phase A")]
        public DateTime? AbsOpenTimeA { get; set; }
        [Label("Time instant of the opening for phase B")]
        public DateTime? AbsOpenTimeB { get; set; }
        [Label("Time instant of the opening for phase C")]
        public DateTime? AbsOpenTimeC { get; set; }


    }

    public enum IsDataErr : int
    {
        [Description("Complete data")]
        Complete = 0,
        [Description("Bad data exists in at least one data input (tV, tI, Va, Vb, Vc, Ia, Ib, Ic)")]
        Bad = 1
    }

    public enum OperationType: int
    {
        [Description("Unidentified or failed analysis")]
        UnidentifiedOrFailed = 0,
        [Description("Missing pole condition")]
        MissingPoleCondition = 11,
        [Description("Sympathetic ringing event due to the energizing of a nearby cap. bank")]
        SympatheticRinging = 12,
        [Description("Capacitor bank de-energizing or opening")]
        CapBankDeEnergize = 2,
        [Description("Capacitor closing without any control")]
        ClosingWithoutControl = 30,
        [Description("Capacitor closing with pre-insertion control")]
        ClosingWithControl = 31
    }

    public enum IsResonance : int {
        [Description("No harmonic resonance")]
        No = 0,
        [Description("Harmonic resonance exists")]
        Yes = 1
    }

}
