using System.Data.Common;

namespace EnterpriseSmartHrm.Application.Common.Abstractions;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();

    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
