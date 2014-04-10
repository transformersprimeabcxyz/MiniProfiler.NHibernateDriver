using System;
using System.Data;
using System.Data.Common;
using NHibernate.AdoNet;
using NHibernate.Driver;

namespace MiniProfiler.NHibernateSqlDriver
{
	public class ProfiledSql2008ClientDriver : Sql2008ClientDriver, IEmbeddedBatcherFactoryProvider
	{
		public override IDbCommand CreateCommand()
		{
			if (StackExchange.Profiling.MiniProfiler.Current != null)
			{
				return (DbCommand)
					new DbCommandProxy((DbCommand)base.CreateCommand(), StackExchange.Profiling.MiniProfiler.Current)
					.GetTransparentProxy();
			}

			return base.CreateCommand();
		}

		Type IEmbeddedBatcherFactoryProvider.BatcherFactoryClass
		{
			get { return typeof(ProfiledSqlClientBatcherFactory); }
		}
	}
}