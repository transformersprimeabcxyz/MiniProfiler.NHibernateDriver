using System.Data;
using System.Data.SqlClient;
using NHibernate;
using NHibernate.AdoNet;
using StackExchange.Profiling.Data;

namespace MiniProfiler.NHibernateSqlDriver
{
	internal class ProfiledSqlClientBatchingBatcher : SqlClientBatchingBatcher
	{
		private readonly IDbProfiler _profiler;

		public ProfiledSqlClientBatchingBatcher(ConnectionManager connectionManager, IInterceptor interceptor, IDbProfiler profiler)
			: base(connectionManager, interceptor)
		{
			_profiler = profiler;
		}

		public override void AddToBatch(IExpectation expectation)
		{
			base.AddToBatch(expectation);

			if (_profiler != null)
			{
				_profiler.ExecuteStart((SqlCommand)CurrentCommand, ExecuteType.NonQuery);
			}
		}

		protected override void DoExecuteBatch(IDbCommand cmd)
		{
			// This command doesn't contain any useful command text, it's a placeholder 
			// to time the entire batch.
			var batchCommand = new SqlCommand("Executing Batch.");

			if (_profiler != null)
			{
				_profiler.ExecuteStart(batchCommand, ExecuteType.NonQuery);
			}

			base.DoExecuteBatch(cmd);

			if (_profiler != null)
			{
				_profiler.ExecuteFinish(batchCommand, ExecuteType.NonQuery, null);
			}
		}
	}
}