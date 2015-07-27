using System;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// A simple way to manage nesting
    /// </summary>
    class Nester : IDisposable
    {
        private readonly ClassBuilder _classBuilder;

        public Nester(ClassBuilder classBuilder)
        {
            _classBuilder = classBuilder;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //this isn't actually a managed disposal resource
                _classBuilder.EndNest();
            }
        }
        #endregion
    }
}
