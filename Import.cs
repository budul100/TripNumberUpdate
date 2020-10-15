using CsvHelper.Configuration.Attributes;

namespace TripNumberUpdate
{
    public class Import
    {
        #region Public Properties

        [Index(1)]
        public string BEGINDATE { get; set; }

        [Index(2)]
        public string ENDDATE { get; set; }

        [Index(6)]
        public string KEY { get; set; }

        [Index(4)]
        public string NEW_ID { get; set; }

        [Index(3)]
        public string OLD_ID { get; set; }

        [Index(0)]
        public string PROJECT { get; set; }

        [Index(5)]
        public string RUNNING_DAY { get; set; }

        #endregion Public Properties
    }
}