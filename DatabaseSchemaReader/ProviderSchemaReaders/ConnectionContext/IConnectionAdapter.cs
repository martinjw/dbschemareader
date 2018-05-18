using System;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext
{
    public interface IConnectionAdapter : IDisposable
    {
        DbConnection DbConnection { get; }
        DbTransaction DbTransaction { get; }
    }
}