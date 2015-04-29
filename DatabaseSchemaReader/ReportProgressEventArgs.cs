using System;

namespace DatabaseSchemaReader
{
    /// <summary>
    /// ReportProgressEventArgs
    /// </summary>
    public class ReportProgressEventArgs: EventArgs
    {
        #region Properties
        /// <summary>
        /// ProgressType Reading, Processing
        /// </summary>
        public string ProgressType { get; private set; }

        /// <summary>
        /// ProgressType like Table, View, SP, etc.
        /// </summary>
        public string StructureType { get; private set; }

        /// <summary>
        /// StructureName like Table/View/SP name
        /// </summary>
        public string StructureName { get; private set; }
        
        /// <summary>
        /// Index of current ProgressName inside the collection if available
        /// </summary>
        public int? Index { get; private set; }
        
        /// <summary>
        /// Count of ProgressType entries if available
        /// </summary>
        public int? Count { get; private set; }
        #endregion

        #region Constructor
        private ReportProgressEventArgs(string progressType, string structureType, string progressName, int? index, int? count)
        {
            ProgressType = progressType;
            StructureType = structureType;
            StructureName = progressName;
            Index = index;
            Count = count;
        }
        #endregion

        #region Methods
        internal static void RaiseReportProgress(EventHandler<ReportProgressEventArgs> reportProgressEvent, object sender, string progressType, string structureType, string structureName, int? index, int? count)
        {
            if(reportProgressEvent != null)
                reportProgressEvent(sender, new ReportProgressEventArgs(progressType, structureType, structureName, index, count));
        }
        #endregion
    }
}
