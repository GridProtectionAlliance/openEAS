using FaultData.DataAnalysis;
using FaultData.DataOperations;
using FaultData.DataResources;
using FaultData.DataSets;
using GSF;
using GSF.Data;
using log4net;
using MathWorks.MATLAB.NET.Arrays;
using openXDA.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
 using GSF.Data.Model;
using System.Data;

namespace openEASSandBox
{
    internal static class MWArrayExtensions
    {
        public static object ScalarAsObject(this MWArray array)
        {
            return array
                .ToArray()
                .Cast<object>()
                .Single();
        }
    }

    public class openEASSandBoxOperation : DataOperationBase<MeterDataSet>
    {
        #region [ Members ]
        private double m_systemFrequency;

        #endregion

        #region [ Properties ]

        [Setting]
        public double SystemFrequency
        {
            get
            {
                return m_systemFrequency;
            }
            set
            {
                m_systemFrequency = value;
            }
        }

        #endregion

        #region [ Constructors ]
        #endregion

        #region [ Methods ]
        public override void Execute(MeterDataSet meterDataSet)
        {
            Log.InfoFormat("Processing {0} waveforms from {1}...", meterDataSet.DataSeries.Count, meterDataSet.Meter.Name);

            using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
            {
                Process(meterDataSet, connection);
            }
            // Execute data analysis
        }

        private void Process(MeterDataSet meterDataSet, AdoDataConnection connection)
        {
            CycleDataResource cycleDataResource;
            Log.InfoFormat("Processing {0} waveforms from {1}...", meterDataSet.DataSeries.Count, meterDataSet.Meter.Name);
            cycleDataResource = meterDataSet.GetResource<CycleDataResource>();
            // Execute data analysis


            for (int i = 0; i < cycleDataResource.DataGroups.Count; ++i)
            {
                Event evt = GetEvent(cycleDataResource.DataGroups[i], connection);
                if (evt == null) continue;

                CSALineSetting lineSetting = (new TableOperations<CSALineSetting>(connection)).QueryRecordWhere("LineID = {0}", evt.LineID);
                CSALineSetting defaultSetting = (new TableOperations<CSALineSetting>(connection)).NewRecord();

                int numArgsOut = lineSetting?.NumArgsOut ?? defaultSetting.NumArgsOut;
                double fundf = SystemFrequency;

                VIDataGroup viDataGroup = cycleDataResource.VIDataGroups[i];

                if (!viDataGroup.AllVIChannelsDefined) return;

                using (MWNumericArray iTHDLimit = new MWNumericArray(lineSetting?.ITHDLimit ?? defaultSetting.ITHDLimit))
                using (MWNumericArray nominalVoltage = new MWNumericArray(new double[,] { { lineSetting?.NominalVoltage ?? defaultSetting.NominalVoltage } }))
                using (MWNumericArray unloadedCurrent = new MWNumericArray(new double[] { lineSetting?.UnloadedCurrent ?? defaultSetting.UnloadedCurrent }))
                using (MWNumericArray nominalBuskVLL = new MWNumericArray(new double[] { lineSetting?.NominalBuskVLL ?? defaultSetting.NominalBuskVLL }))
                using (MWNumericArray t2ndClosing = new MWNumericArray(new double[,] { { lineSetting?.T2ndClosing ?? defaultSetting.T2ndClosing } }))
                using (MWNumericArray capSwitcherType = new MWNumericArray(new double[,] { { lineSetting?.CapSwitcherType ?? defaultSetting.CapSwitcherType } }))
                using (MWNumericArray stepSizeQ3 = new MWNumericArray(new double[,] { { lineSetting?.StepSizeQ3 ?? defaultSetting.StepSizeQ3 } }))
                using (MWNumericArray date = new MWNumericArray(new double[] { -99999}))
                using (MWNumericArray second = new MWNumericArray(new double[,] { { evt.StartTime.Second + evt.StartTime.Millisecond/1000.0D } }))
                using (MWNumericArray minute = new MWNumericArray(new double[,] { { evt.StartTime.Minute } }))
                using (MWNumericArray hour = new MWNumericArray(new double[,] { { evt.StartTime.Hour } }))
                using (MWArray tv = new MWNumericArray(viDataGroup.VA.DataPoints.Count, 1, GetTimeArray(viDataGroup.VA)))
                using (MWArray va = new MWNumericArray(viDataGroup.VA.DataPoints.Count, 1, GetValueArray(viDataGroup.VA)))
                using (MWArray vb = new MWNumericArray(viDataGroup.VB.DataPoints.Count, 1, GetValueArray(viDataGroup.VB)))
                using (MWArray vc = new MWNumericArray(viDataGroup.VC.DataPoints.Count, 1, GetValueArray(viDataGroup.VC)))
                using (MWArray ti = new MWNumericArray(viDataGroup.IA.DataPoints.Count, 1, GetTimeArray(viDataGroup.IA)))
                using (MWArray ia = new MWNumericArray(viDataGroup.IA.DataPoints.Count, 1, GetValueArray(viDataGroup.IA)))
                using (MWArray ib = new MWNumericArray(viDataGroup.IB.DataPoints.Count, 1, GetValueArray(viDataGroup.IB)))
                using (MWArray ic = new MWNumericArray(viDataGroup.IC.DataPoints.Count, 1, GetValueArray(viDataGroup.IC)))
                using (MWArray ovNSPC = new MWNumericArray(Math.Round(viDataGroup.VA.SampleRate / fundf)))
                using (MWArray oiNSPC = new MWNumericArray(Math.Round(viDataGroup.IA.SampleRate / fundf)))
                using (SPCapSwitchPI.capsw capsw = new SPCapSwitchPI.capsw())
                {
                    MWArray[] arrays = new MWArray[] { };
                    try 
                    {
                        arrays = capsw.frootSPCapSwitchPIV2(numArgsOut, fundf, tv, va, vb, vc, ti, ia, ib, ic, ovNSPC, oiNSPC, nominalBuskVLL, nominalVoltage, unloadedCurrent, iTHDLimit, stepSizeQ3, t2ndClosing, capSwitcherType, date, second, minute, hour);

                        CSAResult result = new CSAResult();

                        result.EventID = evt.ID;
                        result.IsDataError = ((IsDataErr)Convert.ToInt32(arrays[0].ScalarAsObject())).GetDescription();
                        result.OutOpTypeA = ((OperationType)Convert.ToInt32(arrays[1].ScalarAsObject())).GetDescription();
                        result.OutOpTypeB = ((OperationType)Convert.ToInt32(arrays[2].ScalarAsObject())).GetDescription();
                        result.OutOpTypeC = ((OperationType)Convert.ToInt32(arrays[3].ScalarAsObject())).GetDescription();

                        result.IsResonanceA = ((IsResonance)Convert.ToInt32(arrays[4][1].ScalarAsObject())).GetDescription();
                        result.ResFrequencyA = Convert.ToDouble(arrays[4][2].ScalarAsObject());
                        result.PeakPUVA = Convert.ToDouble(arrays[4][3].ScalarAsObject());
                        result.VrmsA_bf = Convert.ToDouble(arrays[4][4].ScalarAsObject());
                        result.VrmsA_af = Convert.ToDouble(arrays[4][5].ScalarAsObject());
                        result.DVrmsA = Convert.ToDouble(arrays[4][6].ScalarAsObject());
                        result.PeakIA = Convert.ToDouble(arrays[4][7].ScalarAsObject());
                        result.IrmsA_bf = Convert.ToDouble(arrays[4][8].ScalarAsObject());
                        result.IrmsA_af = Convert.ToDouble(arrays[4][9].ScalarAsObject());
                        result.DIrmsA = Convert.ToDouble(arrays[4][10].ScalarAsObject());
                        result.SwitchXientFreqIAHz = Convert.ToDouble(arrays[4][11].ScalarAsObject());
                        result.DQA = Convert.ToDouble(arrays[4][12].ScalarAsObject());
                        result.PFAInit = Convert.ToDouble(arrays[4][13].ScalarAsObject());
                        result.PFAEnd = Convert.ToDouble(arrays[4][14].ScalarAsObject());
                        result.THDIA_bf = Convert.ToDouble(arrays[4][15].ScalarAsObject());
                        result.THDIA_af = Convert.ToDouble(arrays[4][16].ScalarAsObject());
                        result.DTHDIA = Convert.ToDouble(arrays[4][17].ScalarAsObject());
                        result.THDVA_bf = Convert.ToDouble(arrays[4][18].ScalarAsObject());
                        result.THDVA_af = Convert.ToDouble(arrays[4][19].ScalarAsObject());
                        result.DTHDVA = Convert.ToDouble(arrays[4][20].ScalarAsObject());
                        result.StepA_bf = Convert.ToInt32(arrays[4][21].ScalarAsObject());
                        result.StepA_af = Convert.ToInt32(arrays[4][22].ScalarAsObject());
                        result.XCapA_bf = Convert.ToDouble(arrays[4][23].ScalarAsObject());
                        result.XCapA_af = Convert.ToDouble(arrays[4][24].ScalarAsObject());

                        result.IsResonanceB = ((IsResonance)Convert.ToInt32(arrays[5][1].ScalarAsObject())).GetDescription();
                        result.ResFrequencyB = Convert.ToDouble(arrays[5][2].ScalarAsObject());
                        result.PeakPUVB = Convert.ToDouble(arrays[5][3].ScalarAsObject());
                        result.VrmsB_bf = Convert.ToDouble(arrays[5][5].ScalarAsObject());
                        result.VrmsB_af = Convert.ToDouble(arrays[5][5].ScalarAsObject());
                        result.DVrmsB = Convert.ToDouble(arrays[5][6].ScalarAsObject());
                        result.PeakIB = Convert.ToDouble(arrays[5][7].ScalarAsObject());
                        result.IrmsB_bf = Convert.ToDouble(arrays[5][8].ScalarAsObject());
                        result.IrmsB_af = Convert.ToDouble(arrays[5][9].ScalarAsObject());
                        result.DIrmsB = Convert.ToDouble(arrays[5][10].ScalarAsObject());
                        result.SwitchXientFreqIBHz = Convert.ToDouble(arrays[5][11].ScalarAsObject());
                        result.DQB = Convert.ToDouble(arrays[5][12].ScalarAsObject());
                        result.PFBInit = Convert.ToDouble(arrays[5][13].ScalarAsObject());
                        result.PFBEnd = Convert.ToDouble(arrays[5][15].ScalarAsObject());
                        result.THDIB_bf = Convert.ToDouble(arrays[5][15].ScalarAsObject());
                        result.THDIB_af = Convert.ToDouble(arrays[5][16].ScalarAsObject());
                        result.DTHDIB = Convert.ToDouble(arrays[5][17].ScalarAsObject());
                        result.THDVB_bf = Convert.ToDouble(arrays[5][18].ScalarAsObject());
                        result.THDVB_af = Convert.ToDouble(arrays[5][19].ScalarAsObject());
                        result.DTHDVB = Convert.ToDouble(arrays[5][20].ScalarAsObject());
                        result.StepB_bf = Convert.ToInt32(arrays[5][21].ScalarAsObject());
                        result.StepB_af = Convert.ToInt32(arrays[5][22].ScalarAsObject());
                        result.XCapB_bf = Convert.ToDouble(arrays[5][23].ScalarAsObject());
                        result.XCapB_af = Convert.ToDouble(arrays[5][24].ScalarAsObject());

                        result.IsResonanceC = ((IsResonance)Convert.ToInt32(arrays[6][1].ScalarAsObject())).GetDescription();
                        result.ResFrequencyC = Convert.ToDouble(arrays[6][2].ScalarAsObject());
                        result.PeakPUVC = Convert.ToDouble(arrays[6][3].ScalarAsObject());
                        result.VrmsC_bf = Convert.ToDouble(arrays[6][6].ScalarAsObject());
                        result.VrmsC_af = Convert.ToDouble(arrays[6][6].ScalarAsObject());
                        result.DVrmsC = Convert.ToDouble(arrays[6][6].ScalarAsObject());
                        result.PeakIC = Convert.ToDouble(arrays[6][7].ScalarAsObject());
                        result.IrmsC_bf = Convert.ToDouble(arrays[6][8].ScalarAsObject());
                        result.IrmsC_af = Convert.ToDouble(arrays[6][9].ScalarAsObject());
                        result.DIrmsC = Convert.ToDouble(arrays[6][10].ScalarAsObject());
                        result.SwitchXientFreqICHz = Convert.ToDouble(arrays[6][11].ScalarAsObject());
                        result.DQC = Convert.ToDouble(arrays[6][12].ScalarAsObject());
                        result.PFCInit = Convert.ToDouble(arrays[6][13].ScalarAsObject());
                        result.PFCEnd = Convert.ToDouble(arrays[6][16].ScalarAsObject());
                        result.THDIC_bf = Convert.ToDouble(arrays[6][16].ScalarAsObject());
                        result.THDIC_af = Convert.ToDouble(arrays[6][16].ScalarAsObject());
                        result.DTHDIC = Convert.ToDouble(arrays[6][17].ScalarAsObject());
                        result.THDVC_bf = Convert.ToDouble(arrays[6][18].ScalarAsObject());
                        result.THDVC_af = Convert.ToDouble(arrays[6][19].ScalarAsObject());
                        result.DTHDVC = Convert.ToDouble(arrays[6][20].ScalarAsObject());
                        result.StepC_bf = Convert.ToInt32(arrays[6][21].ScalarAsObject());
                        result.StepC_af = Convert.ToInt32(arrays[6][22].ScalarAsObject());
                        result.XCapC_bf = Convert.ToDouble(arrays[6][23].ScalarAsObject());
                        result.XCapC_af = Convert.ToDouble(arrays[6][24].ScalarAsObject());

                        result.SwitchedOutDurA = Convert.ToDouble(arrays[7][1].ScalarAsObject());
                        result.SwitchedOutDurB = Convert.ToDouble(arrays[7][2].ScalarAsObject());
                        result.SwitchedOutDurC = Convert.ToDouble(arrays[7][3].ScalarAsObject());

                        result.StepChangeRpA = Convert.ToDouble(arrays[8][1].ScalarAsObject());
                        result.StepChangeRpB = Convert.ToDouble(arrays[8][2].ScalarAsObject());
                        result.StepChangeRpC = Convert.ToDouble(arrays[8][3].ScalarAsObject());

                        result.RpBeforeSwitchedOutA = Convert.ToDouble(arrays[9][1].ScalarAsObject());
                        result.RpBeforeSwitchedOutB = Convert.ToDouble(arrays[9][2].ScalarAsObject());
                        result.RpBeforeSwitchedOutC = Convert.ToDouble(arrays[9][3].ScalarAsObject());

                        result.RpAfterSwitchedOutA = Convert.ToDouble(arrays[10][1].ScalarAsObject());
                        result.RpAfterSwitchedOutB = Convert.ToDouble(arrays[10][2].ScalarAsObject());
                        result.RpAfterSwitchedOutC = Convert.ToDouble(arrays[10][3].ScalarAsObject());

                        result.StepChangeXpA = Convert.ToDouble(arrays[11][1].ScalarAsObject());
                        result.StepChangeXpB = Convert.ToDouble(arrays[11][2].ScalarAsObject());
                        result.StepChangeXpC = Convert.ToDouble(arrays[11][3].ScalarAsObject());

                        result.XpBeforeSwitchedOutA = Convert.ToDouble(arrays[12][1].ScalarAsObject());
                        result.XpBeforeSwitchedOutB = Convert.ToDouble(arrays[12][2].ScalarAsObject());
                        result.XpBeforeSwitchedOutC = Convert.ToDouble(arrays[12][3].ScalarAsObject());

                        result.XpAfterSwitchedOutA = Convert.ToDouble(arrays[13][1].ScalarAsObject());
                        result.XpAfterSwitchedOutB = Convert.ToDouble(arrays[13][2].ScalarAsObject());
                        result.XpAfterSwitchedOutC = Convert.ToDouble(arrays[13][3].ScalarAsObject());

                        result.TimeOfEvent = Convert.ToDouble(arrays[14].ScalarAsObject());

                        result.Time1stClosingOpA = Convert.ToDouble(arrays[15][1].ScalarAsObject());
                        result.Time1stClosingOpB = Convert.ToDouble(arrays[15][2].ScalarAsObject());
                        result.Time1stClosingOpC = Convert.ToDouble(arrays[15][3].ScalarAsObject());

                        result.Time2ndClosingOpA = Convert.ToDouble(arrays[16][1].ScalarAsObject());
                        result.Time2ndClosingOpB = Convert.ToDouble(arrays[16][2].ScalarAsObject());
                        result.Time2ndClosingOpC = Convert.ToDouble(arrays[16][3].ScalarAsObject());

                        result.PhaseDiff1stClosingA = Convert.ToDouble(arrays[17][1].ScalarAsObject());
                        result.PhaseDiff1stClosingB = Convert.ToDouble(arrays[17][2].ScalarAsObject());
                        result.PhaseDiff1stClosingC = Convert.ToDouble(arrays[17][3].ScalarAsObject());

                        result.FirstCloseEnergyA = Convert.ToDouble(arrays[18][1].ScalarAsObject());
                        result.FirstCloseEnergyB = Convert.ToDouble(arrays[18][2].ScalarAsObject());
                        result.FirstCloseEnergyC = Convert.ToDouble(arrays[18][3].ScalarAsObject());

                        result.FirstCloseA = Convert.ToDouble(arrays[19].ScalarAsObject());
                        result.SecondCloseA = Convert.ToDouble(arrays[20].ScalarAsObject());
                        result.FirstCloseB = Convert.ToDouble(arrays[21].ScalarAsObject());
                        result.SecondCloseB = Convert.ToDouble(arrays[22].ScalarAsObject());
                        result.FirstCloseC = Convert.ToDouble(arrays[23].ScalarAsObject());
                        result.SecondCloseC = Convert.ToDouble(arrays[24].ScalarAsObject());

                        result.AbsOpenTimeA = Convert.ToDouble(arrays[25].ScalarAsObject());
                        result.AbsOpenTimeB = Convert.ToDouble(arrays[26].ScalarAsObject());
                        result.AbsOpenTimeC = Convert.ToDouble(arrays[27].ScalarAsObject());

                        (new TableOperations<CSAResult>(connection)).AddNewRecord(result);

                        Log.InfoFormat("CSA_2 for {0} written to the database.", evt.ID);
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                    }
                    finally
                    {
                        foreach (MWArray array in arrays)
                            array.Dispose();
                    }
                }
            }
        }

        private Event GetEvent(DataGroup dataGroup, AdoDataConnection connection)
        {
            return (new TableOperations<Event>(connection)).QueryRecordWhere("LineID = {0} AND StartTime = {1} AND EndTime = {2} AND Samples = {3}", dataGroup.Line.ID, ToDateTime2(connection, dataGroup.StartTime), ToDateTime2(connection, dataGroup.EndTime), dataGroup.Samples);
        }

        private double[] GetValueArray(DataSeries dataSeries)
        {
            return dataSeries.DataPoints
                .Select(dataPoint => dataPoint.Value)
                .ToArray();
        }

        private double[] GetTimeArray(DataSeries dataSeries)
        {
            DateTime startTime;

            if (dataSeries.DataPoints.Count == 0)
                return new double[0];

            startTime = dataSeries[0].Time;

            return dataSeries.DataPoints
                .Select(dataPoint => dataPoint.Time)
                .Select(time => time - startTime)
                .Select(timeSpan => timeSpan.TotalSeconds)
                .ToArray();
        }

        private IDbDataParameter ToDateTime2(AdoDataConnection connection, DateTime dateTime)
        {
            using (IDbCommand command = connection.Connection.CreateCommand())
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.DbType = DbType.DateTime2;
                parameter.Value = dateTime;
                return parameter;
            }
        }


        #endregion

        #region [ Static ]
        // Used for logging messages
        private static readonly ILog Log = LogManager.GetLogger(typeof(openEASSandBoxOperation));
        #endregion


    }
}
