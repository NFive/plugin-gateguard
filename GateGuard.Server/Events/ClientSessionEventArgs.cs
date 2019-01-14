using NFive.SDK.Core.Models.Player;
using NFive.SDK.Core.Plugins;
using System;

namespace NFive.GateGuard.Server.Events
{
	public class ClientSessionEventArgs : EventArgs
	{
		public Client Client { get; set; }

		public Session Session { get; set; }

		public ClientSessionEventArgs(Client client, Session session)
		{
			this.Client = client;
			this.Session = session;
		}
	}
}
