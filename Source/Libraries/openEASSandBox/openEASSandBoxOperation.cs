using FaultData.DataAnalysis;
using FaultData.DataOperations;
using FaultData.DataResources;
using FaultData.DataSets;
using GSF.Data;
using log4net;
using MathWorks.MATLAB.NET.Arrays;
using openXDA.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using frootCapSwitch;
using GSF.Data.Model;

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
                int isMonLoc_at_CapCSW = lineSetting?.IsMonLocAtCapCSW ?? defaultSetting.IsMonLocAtCapCSW;
                double fundf = SystemFrequency;
                double vTHDLimit = lineSetting?.VTHDLimit ?? defaultSetting.VTHDLimit;
                double unbalLimit = lineSetting?.UnbalLimit ?? defaultSetting.UnbalLimit;
                int capGrounding = lineSetting?.CapGrounding ?? defaultSetting.CapGrounding;
                double refQ3 = lineSetting?.RefQ3 ?? defaultSetting.RefQ3;
                double normal_upper = lineSetting?.NormalUpper ?? defaultSetting.NormalUpper;
                double normal_lower = lineSetting?.NormalLower ?? defaultSetting.NormalLower;
                double premature_upper = lineSetting?.PrematureUpper ?? defaultSetting.PrematureUpper;
                double premature_lower = lineSetting?.PrematureLower ?? defaultSetting.PrematureLower;
                double delayed_lower = lineSetting?.DelayedLower ?? defaultSetting.DelayedLower;
                double delayed_upper = lineSetting?.DelayedUpper ?? defaultSetting.DelayedUpper;
                double[] syncParsInit = { normal_upper, normal_lower, premature_upper, premature_lower, delayed_lower, delayed_upper };
                MWArray syncPars = new MWNumericArray(1, 6, syncParsInit); //#rows,#cols,double[]

                VIDataGroup viDataGroup = cycleDataResource.VIDataGroups[i];

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
                using (Class1 class1 = new Class1())
                {
                    MWArray[] arrays = class1.frootCapSwitch(numArgsOut, isMonLoc_at_CapCSW, fundf, tv, va, vb, vc, ti, ia, ib, ic, ovNSPC, oiNSPC, vTHDLimit, unbalLimit, capGrounding, refQ3, syncPars);

                    try
                    {
                        CSAResult result = new CSAResult();

                        result.EventID = evt.ID;
                        result.IsDataError = (IsDataErr)Convert.ToInt32(arrays[0].ScalarAsObject());
                        result.IsCapSwitch = (IsCapSwitch)Convert.ToInt32(arrays[1].ScalarAsObject());
                        result.IsCapSwitchCondL = (IsCapSwitchCondL)Convert.ToInt32(arrays[2].ScalarAsObject());
                        result.OutFrequency = Convert.ToDouble(arrays[3].ScalarAsObject());
                        result.OutVoltagesMax = Convert.ToDouble(arrays[4][1].ScalarAsObject());
                        result.OutVoltagesMean = Convert.ToDouble(arrays[4][2].ScalarAsObject());

                        // If using GP module
                        if (isMonLoc_at_CapCSW == 0)
                        {
                            result.OutVTHDFlag = (OutVTHD)Convert.ToInt32(arrays[7][1].ScalarAsObject());
                            result.OutVTHDBefore = Convert.ToDouble(arrays[7][2].ScalarAsObject());
                            result.OutVTHDAfter = Convert.ToDouble(arrays[7][3].ScalarAsObject());
                            result.OutVthDIncrease = Convert.ToDouble(arrays[7][4].ScalarAsObject());
                            (new TableOperations<CSAResult>(connection)).AddNewRecord(result);
                            int CSAResultID = connection.ExecuteScalar<int>("SELECT @@IDENTITY");

                            CSAGPResult gpResult = new CSAGPResult();
                            gpResult.CSAResultID = CSAResultID;
                            gpResult.OutQConditionRPBFlag = (OutQConditionRPBFlag)Convert.ToInt32(arrays[5][1].ScalarAsObject());
                            gpResult.OutQConditionMRPC = Convert.ToDouble(arrays[5][2].ScalarAsObject());
                            gpResult.OutQConditionRPCA = Convert.ToDouble(arrays[5][3].ScalarAsObject());
                            gpResult.OutQConditionRPCB = Convert.ToDouble(arrays[5][4].ScalarAsObject());
                            gpResult.OutQConditionRPCC = Convert.ToDouble(arrays[5][5].ScalarAsObject());
                            gpResult.OutQConditionMPFI = Convert.ToDouble(arrays[5][6].ScalarAsObject());
                            gpResult.OutQConditionPFA = Convert.ToDouble(arrays[5][7].ScalarAsObject());
                            gpResult.OutQConditionPFB = Convert.ToDouble(arrays[5][8].ScalarAsObject());
                            gpResult.OutQConditionPFC = Convert.ToDouble(arrays[5][9].ScalarAsObject());
                            gpResult.OutRestrikeFlag = (OutRestrike)Convert.ToInt32(arrays[6][1].ScalarAsObject());
                            gpResult.OutRestrikeNum = Convert.ToInt32(arrays[6][2].ScalarAsObject());
                            gpResult.OutRestrikePHA = (OutRestrike)Convert.ToInt32(arrays[6][3].ScalarAsObject());
                            gpResult.OutRestrikePHB = (OutRestrike)Convert.ToInt32(arrays[6][4].ScalarAsObject());
                            gpResult.OutRestrikePHC = (OutRestrike)Convert.ToInt32(arrays[6][5].ScalarAsObject());
                            (new TableOperations<CSAGPResult>(connection)).AddNewRecord(gpResult);
                        }
                        else {
                            result.OutVTHDFlag = (OutVTHD)Convert.ToInt32(arrays[5][1].ScalarAsObject());
                            result.OutVTHDBefore = Convert.ToDouble(arrays[5][2].ScalarAsObject());
                            result.OutVTHDAfter = Convert.ToDouble(arrays[5][3].ScalarAsObject());
                            result.OutVthDIncrease = Convert.ToDouble(arrays[5][4].ScalarAsObject());
                            (new TableOperations<CSAResult>(connection)).AddNewRecord(result);
                            int CSAResultID = connection.ExecuteScalar<int>("SELECT @@IDENTITY");

                            CSASPResult spResult = new CSASPResult();
                            spResult.CSAResultID = CSAResultID;
                            spResult.OutCapXingANotFailedEnergizing = (OutCapXing)Convert.ToInt32(arrays[8][1].ScalarAsObject());
                            spResult.OutCapXingAReactivePowerContribution = Convert.ToDouble(arrays[8][2].ScalarAsObject());
                            spResult.OutCapXingAReactivePowerDeviation = Convert.ToDouble(arrays[8][3].ScalarAsObject());
                            spResult.OutCapXingBNotFailedEnergizing = (OutCapXing)Convert.ToInt32(arrays[9][1].ScalarAsObject());
                            spResult.OutCapXingBReactivePowerContribution = Convert.ToDouble(arrays[9][2].ScalarAsObject());
                            spResult.OutCapXingBReactivePowerDeviation = Convert.ToDouble(arrays[9][3].ScalarAsObject());
                            spResult.OutCapXingCNotFailedEnergizing = (OutCapXing)Convert.ToInt32(arrays[10][1].ScalarAsObject());
                            spResult.OutCapXingCReactivePowerContribution = Convert.ToDouble(arrays[10][2].ScalarAsObject());
                            spResult.OutCapXingCReactivePowerDeviation = Convert.ToDouble(arrays[10][3].ScalarAsObject());
                            spResult.OutRestrikeEnhancedADeenergizationOperation = (OutRestrikeEnhancedDeenergizationOperation)Convert.ToInt32(arrays[11][1].ScalarAsObject());
                            spResult.OutRestrikeEnhancedADeenergizationLevel = (OutRestrikeEnhancedDeenergizationLevel)Convert.ToInt32(arrays[11][2].ScalarAsObject());
                            spResult.OutRestrikeEnhancedBDeenergizationOperation = (OutRestrikeEnhancedDeenergizationOperation)Convert.ToInt32(arrays[12][1].ScalarAsObject());
                            spResult.OutRestrikeEnhancedBDeenergizationLevel = (OutRestrikeEnhancedDeenergizationLevel)Convert.ToInt32(arrays[12][2].ScalarAsObject());
                            spResult.OutRestrikeEnhancedCDeenergizationOperation = (OutRestrikeEnhancedDeenergizationOperation)Convert.ToInt32(arrays[13][1].ScalarAsObject());
                            spResult.OutRestrikeEnhancedCDeenergizationLevel = (OutRestrikeEnhancedDeenergizationLevel)Convert.ToInt32(arrays[13][2].ScalarAsObject());
                            spResult.OutSyncAPhaseAngleDeviation = Convert.ToDouble(arrays[14][1].ScalarAsObject());
                            spResult.OutSyncAStatus = (OutSyncStatus)Convert.ToInt32(arrays[14][2].ScalarAsObject());
                            spResult.OutSyncBPhaseAngleDeviation = Convert.ToDouble(arrays[14][3].ScalarAsObject());
                            spResult.OutSyncBStatus = (OutSyncStatus)Convert.ToInt32(arrays[14][4].ScalarAsObject());
                            spResult.OutSyncCPhaseAngleDeviation = Convert.ToDouble(arrays[14][5].ScalarAsObject());
                            spResult.OutSyncCStatus = (OutSyncStatus)Convert.ToInt32(arrays[14][6].ScalarAsObject());
                            (new TableOperations<CSASPResult>(connection)).AddNewRecord(spResult);
                        }

                        Log.InfoFormat("CSA for {0} written to the database.", evt.ID);
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
            return (new TableOperations<Event>(connection)).QueryRecordWhere("LineID = {0} AND StartTime = {1} AND EndTime = {2} AND Samples = {3}", dataGroup.Line.ID, dataGroup.StartTime, dataGroup.EndTime, dataGroup.Samples);
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


        #endregion

        #region [ Static ]
        // Used for logging messages
        private static readonly ILog Log = LogManager.GetLogger(typeof(openEASSandBoxOperation));
        #endregion


    }
}
