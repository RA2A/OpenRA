using System;

namespace OpenRA.Server
{
	public class OrahostListener : ServerTrait, IClientJoined, INotifyServerEmpty, INotifyServerStart {
		public void ClientJoined(Server server, Connection connection)
		{
			Program.WriteOrahost("joined \"" + server.GetClient(connection).Name + "\"");
		}

		public void ServerEmpty(Server server)
		{
			Program.WriteOrahost("empty");
		}

		public void ServerStarted(Server server)
		{
			Program.WriteOrahost("started");
		}
	}
}