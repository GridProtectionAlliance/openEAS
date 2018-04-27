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
        private enum IsDataErr : int
        {
            Complete = 0,
            Bad = 1
        }

        private enum IsCapSwitch : int
        {
            No = -1,
            Undetermined = 0,
            Yes = 1
        }

        private enum IsCapSwitchCondL : int
        {
            Low = 1,
            Moderate = 2,
            High = 3
        }

        private enum OutQConditionRPBFlag : int
        {
            Balanced = -1,
            Unknown = 0,
            Unbalanced = 1
        }

        private enum OutRestrike : int
        {
            No = 0,
            Yes = 1
        }

        private enum OutVTHD : int
        {
            None = 0,
            Yes = 1
        }

        private class CSAResult
        {
            public int EventID;
            public IsDataErr IsDataError;
            public IsCapSwitch IsCapSwitch;
            public IsCapSwitchCondL IsCapSwitchCondL;
            public double OutFrequency;
            public double OutVoltagesMax;
            public double OutVoltagesMean;
            public OutQConditionRPBFlag OutQConditionRPBFlag;
            public double OutQConditionMRPC;
            public double OutQConditionRPCA;
            public double OutQConditionRPCB;
            public double OutQConditionRPCC;
            public double OutQConditionMPFI;
            public double OutQConditionPFA;
            public double OutQConditionPFB;
            public double OutQConditionPFC;
            public OutRestrike OutRestrikeFlag;
            public int OutRestrikeNum;
            public OutRestrike OutRestrikePHA;
            public OutRestrike OutRestrikePHB;
            public OutRestrike OutRestrikePHC;
            public OutVTHD OutVTHDFlag;
            public double OutVTHDBefore;
            public double OutVTHDAfter;
            public double OutVthDIncrease;
        }

        private double m_systemFrequency;

        private List<CSAResult> m_results;

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
        public openEASSandBoxOperation()
        {
            m_results = new List<CSAResult>();
        }


        #endregion

        #region [ Methods ]
        public override void Execute(MeterDataSet meterDataSet)
        {
            Log.InfoFormat("Processing {0} waveforms from {1}...", meterDataSet.DataSeries.Count, meterDataSet.Meter.Name);

            using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
            {
                Process(meterDataSet, connection);
                Load(connection);
            }
            // Execute data analysis
        }

        private void Process(MeterDataSet meterDataSet, AdoDataConnection connection)
        {
            CycleDataResource cycleDataResource;
            Log.InfoFormat("Processing {0} waveforms from {1}...", meterDataSet.DataSeries.Count, meterDataSet.Meter.Name);
            cycleDataResource = meterDataSet.GetResource<CycleDataResource>();
            // Execute data analysis

            // default data for dll call
            int numArgsOut = 8;          // Or 15 for specific purposes
            int isMonLoc_at_CapCSW = 0;  // Or 1 for specific purposes
            double fundf = 60.0;         // or 50 depending on location - unit is Herz
            double vTHDLimit = 5.0; // default is 5 percent
            double unbalLimit = 10.0; // default is 10 percent
            int capGrounding = 1; // default is 1
            double refQ3 = 0.0;
            double normal_upper = 15.0; // default value
            double normal_lower = -15.0; // default value
            double premature_upper = normal_lower;
            double premature_lower = -90.0; // default value
            double delayed_lower = normal_upper;
            double delayed_upper = 90.0; // default value
            double[] syncParsInit = { normal_upper, normal_lower, premature_upper, premature_lower, delayed_lower, delayed_upper };
            MWArray syncPars = new MWNumericArray(1, 6, syncParsInit); //#rows,#cols,double[]

            switch (meterDataSet.Meter.AssetKey)
            {
                case "Trinity AL 161-B1018-Caps":
                    isMonLoc_at_CapCSW = 1;
                    fundf = 60.0D;
                    vTHDLimit = 5.0D;
                    unbalLimit = 5.0D;
                    capGrounding = 1;
                    refQ3 = 84000.0D;
                    normal_upper = 15.0;
                    normal_lower = -15.0;
                    premature_upper = normal_lower;
                    premature_lower = -90.0;
                    delayed_lower = normal_upper;
                    delayed_upper = 90.0;
                    syncParsInit = new double[] { normal_upper, normal_lower, premature_upper, premature_lower, delayed_lower, delayed_upper };
                    syncPars = new MWNumericArray(1, 6, syncParsInit);
                    break;

                case "Belfast TN 161-B1004-Caps":
                    isMonLoc_at_CapCSW = 1;
                    fundf = 60.0D;
                    vTHDLimit = 5.0D;
                    unbalLimit = 5.0D;
                    capGrounding = 1;
                    refQ3 = 18000.0D;
                    normal_upper = 15.0;
                    normal_lower = -15.0;
                    premature_upper = normal_lower;
                    premature_lower = -90.0;
                    delayed_lower = normal_upper;
                    delayed_upper = 90.0;
                    syncParsInit = new double[] { normal_upper, normal_lower, premature_upper, premature_lower, delayed_lower, delayed_upper };
                    syncPars = new MWNumericArray(1, 6, syncParsInit);
                    break;

                case "Tazewell TN 161=B1004-Caps":
                    isMonLoc_at_CapCSW = 1;
                    fundf = 60.0D;
                    vTHDLimit = 5.0D;
                    unbalLimit = 5.0D;
                    capGrounding = 1;
                    refQ3 = 18000.0D;
                    normal_upper = 15.0;
                    normal_lower = -15.0;
                    premature_upper = normal_lower;
                    premature_lower = -90.0;
                    delayed_lower = normal_upper;
                    delayed_upper = 90.0;
                    syncParsInit = new double[] { normal_upper, normal_lower, premature_upper, premature_lower, delayed_lower, delayed_upper };
                    syncPars = new MWNumericArray(1, 6, syncParsInit);
                    break;

                case "Cumberland FP 161-B1014-Caps":
                    isMonLoc_at_CapCSW = 1;
                    fundf = 60.0D;
                    vTHDLimit = 5.0D;
                    unbalLimit = 5.0D;
                    capGrounding = 1;
                    refQ3 = 18000.0D;
                    normal_upper = 15.0;
                    normal_lower = -15.0;
                    premature_upper = normal_lower;
                    premature_lower = -90.0;
                    delayed_lower = normal_upper;
                    delayed_upper = 90.0;
                    syncParsInit = new double[] { normal_upper, normal_lower, premature_upper, premature_lower, delayed_lower, delayed_upper };
                    syncPars = new MWNumericArray(1, 6, syncParsInit);
                    break;

                case "North Nashville TN 161-B1004-Caps":
                    isMonLoc_at_CapCSW = 1;
                    fundf = 60.0D;
                    vTHDLimit = 5.0D;
                    unbalLimit = 5.0D;
                    capGrounding = 1;
                    refQ3 = 84000.0D;
                    normal_upper = 15.0;
                    normal_lower = -15.0;
                    premature_upper = normal_lower;
                    premature_lower = -90.0;
                    delayed_lower = normal_upper;
                    delayed_upper = 90.0;
                    syncParsInit = new double[] { normal_upper, normal_lower, premature_upper, premature_lower, delayed_lower, delayed_upper };
                    syncPars = new MWNumericArray(1, 6, syncParsInit);
                    break;

                case "Memphis Junction KY 161-Caps":
                    isMonLoc_at_CapCSW = 1;
                    fundf = 60.0D;
                    vTHDLimit = 5.0D;
                    unbalLimit = 5.0D;
                    capGrounding = 1;
                    refQ3 = 18000.0D;
                    normal_upper = 15.0;
                    normal_lower = -15.0;
                    premature_upper = normal_lower;
                    premature_lower = -90.0;
                    delayed_lower = normal_upper;
                    delayed_upper = 90.0;
                    syncParsInit = new double[] { normal_upper, normal_lower, premature_upper, premature_lower, delayed_lower, delayed_upper };
                    syncPars = new MWNumericArray(1, 6, syncParsInit);
                    break;
            }

            for (int i = 0; i < cycleDataResource.DataGroups.Count; ++i)
            {
                int eventID = GetEventID(cycleDataResource.DataGroups[i], connection);

                if (eventID < 0) continue;

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

                        result.EventID = eventID;
                        result.IsDataError = (IsDataErr)Convert.ToInt32(arrays[0].ScalarAsObject());
                        result.IsCapSwitch = (IsCapSwitch)Convert.ToInt32(arrays[1].ScalarAsObject());
                        result.IsCapSwitchCondL = (IsCapSwitchCondL)Convert.ToInt32(arrays[2].ScalarAsObject());
                        result.OutFrequency = Convert.ToDouble(arrays[3].ScalarAsObject());
                        result.OutVoltagesMax = Convert.ToDouble(arrays[4][1].ScalarAsObject());
                        result.OutVoltagesMean = Convert.ToDouble(arrays[4][2].ScalarAsObject());
                        result.OutQConditionRPBFlag = (OutQConditionRPBFlag)Convert.ToInt32(arrays[5][1].ScalarAsObject());
                        result.OutQConditionMRPC = Convert.ToDouble(arrays[5][2].ScalarAsObject());
                        result.OutQConditionRPCA = Convert.ToDouble(arrays[5][3].ScalarAsObject());
                        result.OutQConditionRPCB = Convert.ToDouble(arrays[5][4].ScalarAsObject());
                        result.OutQConditionRPCC = Convert.ToDouble(arrays[5][5].ScalarAsObject());
                        result.OutQConditionMPFI = Convert.ToDouble(arrays[5][6].ScalarAsObject());
                        result.OutQConditionPFA = Convert.ToDouble(arrays[5][7].ScalarAsObject());
                        result.OutQConditionPFB = Convert.ToDouble(arrays[5][8].ScalarAsObject());
                        result.OutQConditionPFC = Convert.ToDouble(arrays[5][9].ScalarAsObject());
                        result.OutRestrikeFlag = (OutRestrike)Convert.ToInt32(arrays[6][1].ScalarAsObject());
                        result.OutRestrikeNum = Convert.ToInt32(arrays[6][2].ScalarAsObject());
                        result.OutRestrikePHA = (OutRestrike)Convert.ToInt32(arrays[6][3].ScalarAsObject());
                        result.OutRestrikePHB = (OutRestrike)Convert.ToInt32(arrays[6][4].ScalarAsObject());
                        result.OutRestrikePHC = (OutRestrike)Convert.ToInt32(arrays[6][5].ScalarAsObject());
                        result.OutVTHDFlag = (OutVTHD)Convert.ToInt32(arrays[7][1].ScalarAsObject());
                        result.OutVTHDBefore = Convert.ToDouble(arrays[7][2].ScalarAsObject());
                        result.OutVTHDAfter = Convert.ToDouble(arrays[7][3].ScalarAsObject());
                        result.OutVthDIncrease = Convert.ToDouble(arrays[7][4].ScalarAsObject());

                        m_results.Add(result);
                    }
                    finally
                    {
                        foreach (MWArray array in arrays)
                            array.Dispose();
                    }
                }
            }
        }

        private void Load(AdoDataConnection connection)
        {
            // Write analysis results to the database
            int resultsCount = m_results.Count;
            int resultID;

            foreach (CSAResult result in m_results)
            {
                connection.ExecuteNonQuery("INSERT INTO CSAResult(EventID, IsDataError, IsCapSwitch, IsCapSwitchCondL, OutFrequency, OutVoltagesMax, OutVoltagesMean," +
                    "OutQConditionRPBFlag, OutQConditionMRPC, OutQConditionRPCA, OutQConditionRPCB, OutQConditionRPCC, OutQConditionMPFI, OutQConditionPFA, OutQConditionPFB, OutQConditionPFC," +
                    " OutRestrikeFlag, OutRestrikeNum, OutRestrikePHA, OutRestrikePHB, OutRestrikePHC, OutVTHDFlag, OutVTHDBefore, OutVTHDAfter, OutVTHDIncrease) VALUES({0}, {1}, {2}, {3}, {4}, {5}," +
                    "{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24})", result.EventID, result.IsDataError.ToString(), result.IsCapSwitch.ToString(), result.IsCapSwitchCondL.ToString(), result.OutFrequency, result.OutVoltagesMax,
                    result.OutVoltagesMean, result.OutQConditionRPBFlag.ToString(), result.OutQConditionMRPC, result.OutQConditionRPCA, result.OutQConditionRPCB, result.OutQConditionRPCB,
                    result.OutQConditionMPFI, result.OutQConditionPFA, result.OutQConditionPFB, result.OutQConditionPFC, result.OutRestrikeFlag.ToString(), result.OutRestrikeNum, result.OutRestrikePHA.ToString(),
                    result.OutRestrikePHB.ToString(), result.OutRestrikePHC.ToString(), result.OutVTHDFlag.ToString(), result.OutVTHDBefore, result.OutVTHDAfter, result.OutVthDIncrease);
                resultID = connection.ExecuteScalar<int>("SELECT @@IDENTITY");

            }
            

            Log.InfoFormat("{0} results written to the database.", resultsCount);


        }

        private int GetEventID(DataGroup dataGroup, AdoDataConnection connection)
        {
            return connection.ExecuteScalar<int?>("LineID = {0} AND StartTime = {1} AND EndTime = {2} AND Samples = {3}", dataGroup.Line.ID, dataGroup.StartTime, dataGroup.EndTime, dataGroup.Samples) ?? -1;
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
