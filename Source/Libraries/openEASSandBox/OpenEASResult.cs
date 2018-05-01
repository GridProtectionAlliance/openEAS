using GSF.Data.Model;

namespace openEASSandBox
{
    public class OpenEASResult
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public int EventID { get; set; }
        public double MyResult { get; set; }

    }
}
