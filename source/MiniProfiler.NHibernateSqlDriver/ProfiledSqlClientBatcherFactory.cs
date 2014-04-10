using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Engine;

namespace MiniProfiler.NHibernateSqlDriver
{
	internal class ProfiledSqlClientBatcherFactory : IBatcherFactory
	{
		public virtual IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
		{
			if (StackExchange.Profiling.MiniProfiler.Current != null)
			{
				return new ProfiledSqlClientBatchingBatcher(connectionManager, interceptor, StackExchange.Profiling.MiniProfiler.Current);
			}

			return new SqlClientBatchingBatcher(connectionManager, interceptor);
		}
	}
}