using FaultData.DataOperations;
using FaultData.DataSets;
using log4net;

namespace openEASSandBox
{
    public class openEASSandBoxOperation : DataOperationBase<MeterDataSet>
    {
        public override void Execute(MeterDataSet meterDataSet)
        {
            Log.InfoFormat("Processing {0} waveforms from {1}...", meterDataSet.DataSeries.Count, meterDataSet.Meter.Name);

            // Execute data analysis
        }

        // Used for logging messages
        private static readonly ILog Log = LogManager.GetLogger(typeof(openEASSandBoxOperation));
    }
}
