using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using StackExchange.Profiling.Data;

namespace MiniProfiler.NHibernateSqlDriver
{
	internal class DbCommandProxy : RealProxy, IRemotingTypeInfo
	{
		private const string FieldGetter = "FieldGetter";
		private const string ExecuteReader = "ExecuteReader";
		private const string ExecuteNonQuery = "ExecuteNonQuery";
		private const string ExecuteScalar = "ExecuteScalar";

		private readonly DbCommand _instance;
		private readonly IDbProfiler _profiler;
		private static readonly string[] MethodNamesToProxy = new[] { ExecuteReader, ExecuteNonQuery, ExecuteScalar };

		public DbCommandProxy(DbCommand instance, IDbProfiler profiler)
			: base(instance.GetType())
		{
			_instance = instance;
			_profiler = profiler;
		}

		public override IMessage Invoke(IMessage msg)
		{
			var methodMessage = (IMethodCallMessage)msg;

			object returnValue = null;

			if (MethodNamesToProxy.Contains(methodMessage.MethodName))
			{
				try
				{
					if (_profiler != null)
					{
						switch (methodMessage.MethodName)
						{
							case ExecuteReader:
								_profiler.ExecuteStart(_instance, ExecuteType.Reader);
								break;
							case ExecuteScalar:
								_profiler.ExecuteStart(_instance, ExecuteType.Scalar);
								break;
							case ExecuteNonQuery:
								_profiler.ExecuteStart(_instance, ExecuteType.NonQuery);
								break;
						}

					}
					returnValue = methodMessage.MethodBase.Invoke(_instance, methodMessage.Args);
				}
				finally
				{
					if (_profiler != null)
					{
						switch (methodMessage.MethodName)
						{
							case ExecuteReader:
								_profiler.ExecuteFinish(_instance, ExecuteType.Reader, (DbDataReader)returnValue);
								break;
							case ExecuteScalar:
								_profiler.ExecuteFinish(_instance, ExecuteType.Scalar, null);
								break;
							case ExecuteNonQuery:
								_profiler.ExecuteFinish(_instance, ExecuteType.NonQuery, null);
								break;
						}
					}
				}
			}
			else if (methodMessage.MethodName == FieldGetter)
			{
				var field = (string)methodMessage.Args[1];
				var fi = _instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				returnValue = fi.GetValue(_instance);
			}

			if (returnValue == null)
			{
				returnValue = methodMessage.MethodBase.Invoke(_instance, methodMessage.InArgs);
			}

			return new ReturnMessage(returnValue, methodMessage.Args, methodMessage.ArgCount, methodMessage.LogicalCallContext, methodMessage);
		}

		public bool CanCastTo(Type fromType, object o)
		{
			if (fromType == typeof(SqlCommand))
			{
				return true;
			}

			if (fromType == typeof(DbCommand))
			{
				return true;
			}

			if (fromType == typeof(IDbCommand))
			{
				return true;
			}

			return false;
		}

		public string TypeName
		{
			get { return _instance.GetType().AssemblyQualifiedName; }
			set { }
		}
	}
}
