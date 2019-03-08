using JetBrains.Annotations;
using NFive.GateGuard.Shared;
using NFive.SDK.Core.Models.Player;
using NFive.SDK.Server.Events;
using NFive.SDK.Server.Rpc;
using System;

namespace NFive.GateGuard.Server
{
	/// <summary>
	/// Wrapper library for accessing and modifying GateGuard rules and events from external plugins.
	/// </summary>
	[PublicAPI]
	public class GateGuard
	{
		/// <summary>
		/// The controller event manager.
		/// </summary>
		protected readonly IEventManager Events;

		/// <summary>
		/// The controller RPC handler.
		/// </summary>
		protected readonly IRpcHandler Rpc;

		/// <summary>
		/// Occurs when a connecting client has been allowed in.
		/// </summary>
		public event EventHandler<ClientSessionEventArgs> ClientAllowed;

		/// <summary>
		/// Occurs when a connecting client has been dropped.
		/// </summary>
		public event EventHandler<ClientSessionEventArgs> ClientDropped;

		/// <summary>
		/// Initializes a new instance of the <see cref="GateGuard"/> wrapper.
		/// </summary>
		/// <param name="events">The controller event manager.</param>
		/// <param name="rpc">The controller RPC handler.</param>
		public GateGuard(IEventManager events, IRpcHandler rpc)
		{
			this.Events = events;
			this.Rpc = rpc;

			this.Events.On<IClient, Session>(GateGuardEvents.ClientAllowed, (c, d) => this.ClientAllowed?.Invoke(this, new ClientSessionEventArgs(c, d)));
			this.Events.On<IClient, Session>(GateGuardEvents.ClientDropped, (c, d) => this.ClientDropped?.Invoke(this, new ClientSessionEventArgs(c, d)));
		}

		/// <summary>
		/// Creates a new rule and adds it to the database storage.
		/// </summary>
		/// <param name="staffUserId">The identifier of the user who creates the rule.</param>
		/// <param name="userId">The identifier of the user to create the rule for.</param>
		/// <param name="rule">The rule to add.</param>
		/// <param name="reason">The reason for the rule.</param>
		/// <param name="expiry">Optional expiry date for the rule.</param>
		public void CreateRule(Guid staffUserId, Guid userId, AccessRule rule, string reason, DateTime? expiry = default(DateTime?))
		{
			this.Events.Raise(GateGuardEvents.RuleCreate, staffUserId, userId, rule, reason, expiry);
		}

		/// <summary>
		/// Deletes a rule for a specified user.
		/// </summary>
		/// <param name="staffUserId">The identifier of the user who deletes the rule.</param>
		/// <param name="userId">The identifier of the user to delete rule for.</param>
		/// <param name="reason">The reason for the rule.</param>
		public void DeleteRule(Guid staffUserId, Guid userId, string reason = null)
		{
			this.Events.Raise(GateGuardEvents.RuleDelete, staffUserId, userId, reason);
		}


		/// <summary>
		/// Access rule definition
		/// </summary>
		public class AccessRule
		{
			[CanBeNull]
			public string License { get; set; }

			public long? SteamId { get; set; }

			[CanBeNull]
			public string IpAddress { get; set; }
		}
	}
}
