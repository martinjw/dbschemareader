namespace DatabaseSchemaReader
{
#if NET35
    /// <summary>
    /// .net 3.5 version of the .net 4.0 System.Threading.CancellationToken
    /// </summary>
    public struct CancellationToken
    {
        private static readonly CancellationToken _none = new CancellationToken();

        /// <summary>
        ///   Gets an empty token.
        /// </summary>
        /// 
        public static CancellationToken None
        {
            get { return _none; }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether cancellation has been requested.
        /// </summary>
        /// 
        public bool IsCancellationRequested { get; set; }
    }
#endif
}
