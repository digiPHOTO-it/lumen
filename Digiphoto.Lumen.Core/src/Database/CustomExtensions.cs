using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.Common;
using System.Collections;

namespace Digiphoto.Lumen.src.Database {

	public static class CustomExtensions {

		private static readonly string entityAssemblyName =
			"system.data.entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

		public static string ToTraceString( this IQueryable query ) {
			System.Reflection.MethodInfo toTraceStringMethod = query.GetType().GetMethod( "ToTraceString" );

			if( toTraceStringMethod != null )
				return toTraceStringMethod.Invoke( query, null ).ToString();
			else
				return "";
		}

		public static string ToTraceString( this ObjectContext ctx ) {
			Assembly entityAssemly = Assembly.Load( entityAssemblyName );

			Type updateTranslatorType = entityAssemly.GetType(
				"System.Data.Mapping.Update.Internal.UpdateTranslator" );

			Type functionUpdateCommandType = entityAssemly.GetType(
				"System.Data.Mapping.Update.Internal.FunctionUpdateCommand" );

			Type dynamicUpdateCommandType = entityAssemly.GetType(
				"System.Data.Mapping.Update.Internal.DynamicUpdateCommand" );

			object [] ctorParams = new object []
                        {
                            ctx.ObjectStateManager,
                            ((EntityConnection)ctx.Connection).GetMetadataWorkspace(),
                            (EntityConnection)ctx.Connection,
                            ctx.CommandTimeout
                        };
			object updateTranslator = Activator.CreateInstance( updateTranslatorType,
							BindingFlags.NonPublic | BindingFlags.Instance, null, ctorParams, null );

			MethodInfo produceCommandsMethod = updateTranslatorType
				.GetMethod( "ProduceCommands", BindingFlags.Instance | BindingFlags.NonPublic );
			object updateCommands = produceCommandsMethod.Invoke( updateTranslator, null );

			List<DbCommand> dbCommands = new List<DbCommand>();

			foreach( object o in (IEnumerable)updateCommands ) {
				if( functionUpdateCommandType.IsInstanceOfType( o ) ) {
					FieldInfo m_dbCommandField = functionUpdateCommandType.GetField(
						"m_dbCommand", BindingFlags.Instance | BindingFlags.NonPublic );

					dbCommands.Add( (DbCommand)m_dbCommandField.GetValue( o ) );
				} else if( dynamicUpdateCommandType.IsInstanceOfType( o ) ) {
					MethodInfo createCommandMethod = dynamicUpdateCommandType.GetMethod(
						"CreateCommand", BindingFlags.Instance | BindingFlags.NonPublic );

					object [] methodParams = new object []
                    {
                        updateTranslator,
                        new Dictionary<int, object>()
                    };

					dbCommands.Add( (DbCommand)createCommandMethod.Invoke( o, methodParams ) );
				} else {
					throw new NotSupportedException( "Unknown UpdateCommand Kind" );
				}
			}

			StringBuilder traceString = new StringBuilder();
			foreach( DbCommand command in dbCommands ) {
				traceString.AppendLine( "=============== BEGIN COMMAND ===============" );
				traceString.AppendLine();

				traceString.AppendLine( command.CommandText );
				foreach( DbParameter param in command.Parameters ) {
					traceString.AppendFormat( "{0} = {1}", param.ParameterName, param.Value );
					traceString.AppendLine();
				}

				traceString.AppendLine();
				traceString.AppendLine( "=============== END COMMAND ===============" );
			}

			return traceString.ToString();
		}
	}
}