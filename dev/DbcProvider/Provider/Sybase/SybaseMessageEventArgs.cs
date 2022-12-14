using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Sqt.DbcProvider.Provider.Sybase
{
	/// <summary>
	/// Specialization of a message returned by a server for sybase ase server.
	/// </summary>
	public class SybaseMessageEventArgs : DbMessageEventArgs
	{
		public SybaseMessageEventArgs( string message ) : base( message )
		{
		}

		public SybaseMessageEventArgs( SybaseMessage message )
			: base( GetSeverity( message ), message.Server, message.Catalog, message.Procedure, message.LineNumber, message.Message )
		{
		}

		private static TraceLevel GetSeverity( SybaseMessage message )
		{
			return TraceLevel.Info;
		}
	}
}
