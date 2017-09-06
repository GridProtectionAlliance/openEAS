using FaultData.Database;
using FaultData.DataOperations;
using FaultData.DataSets;
using log4net;

namespace openEASSandBox
{
    public class openEASSandBoxOperation : DataOperationBase<MeterDataSet>
    {
        public override void Prepare(DbAdapterContainer dbAdapterContainer)
        {
            // Prepare for data analysis
        }

        public override void Execute(MeterDataSet meterDataSet)
        {
            Log.InfoFormat("Processing {0} waveforms from {1}...", meterDataSet.DataSeries.Count, meterDataSet.Meter.Name);

            // Execute data analysis
        }

        public override void Load(DbAdapterContainer dbAdapterContainer)
        {
            int resultsCount = 0;

            // Write analysis results to the database

            Log.InfoFormat("{0} results written to the database.", resultsCount);
        }

        // Used for logging messages
        private static readonly ILog Log = LogManager.GetLogger(typeof(openEASSandBoxOperation));
    }
}
