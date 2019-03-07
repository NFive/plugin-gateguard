using JetBrains.Annotations;
using NFive.GateGuard.Server.Extensions;
using NFive.GateGuard.Server.Models;
using NFive.GateGuard.Server.Storage;
using NFive.GateGuard.Shared;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Server.Controllers;
using NFive.SDK.Server.Events;
using NFive.SDK.Server.Rcon;
using NFive.SDK.Server.Rpc;
using System;
using System.Linq;
using System.Threading.Tasks;
using NFive.SDK.Server.Wrappers;

namespace NFive.GateGuard.Server
{
	[PublicAPI]
	public class GateGuardController : ConfigurableController<Configuration>
	{
		private readonly object lockObject = new object();
		private RulesConfig rules;

		public GateGuardController(ILogger logger, IEventManager events, IRpcHandler rpc, IRconManager rcon, Configuration configuration) : base(logger, events, rpc, rcon, configuration)
		{
			this.Logger.Info($"Running in {this.Configuration.Mode.ToString().ToLower()} mode");
			this.Logger.Info($"Database update interval: Every {this.Configuration.Database.ReloadInterval.ToFriendly()}");

			this.Rpc.Event(GateGuardEvents.RuleCreate).On<Guid, GateGuard.AccessRule, string, DateTime?>(OnRuleCreate);
			this.Rpc.Event(GateGuardEvents.RuleDelete).On<Guid>(OnRuleDelete);

			// Listen for NFive session events
			var sessions = new SessionManager(this.Events, this.Rpc);
			sessions.SessionCreated += OnSessionCreated;

			// ReSharper disable once FunctionNeverReturns
			Task.Factory.StartNew(async () =>
			{
				while (true)
				{
					Load();

					await Task.Delay(this.Configuration.Database.ReloadInterval);
				}
			});
		}


		/// <summary>
		/// Reloading base configuration
		/// </summary>
		/// <param name="configuration">The configuration to reload</param>
		public override void Reload(Configuration configuration)
		{
			base.Reload(configuration);

			// Load config now
			Load();
		}


		/// <summary>
		/// Checks if connecting client passes access validation
		/// </summary>
		/// <param name="sender">The plugin sending the handler</param>
		/// <param name="e">The connecting client</param>
		private async void OnSessionCreated(object sender, ClientSessionDeferralsEventArgs e)
		{
			// Steam is required and client isn't running it
			if (this.Configuration.Steam.Required && !e.Client.SteamId.HasValue)
			{
				// Drop the client
				e.Deferrals.Done(this.Configuration.Steam.Message);

				// Notify other plugins client was dropped
				await this.Events.RaiseAsync(GateGuardEvents.ClientDropped, e.Client, e.Session);

				this.Logger.Info($"Client {e.Client.Name} [{e.Session.UserId}] session [{e.Session.Id}] dropped: No Steam ID");

				return;
			}

			// Check if the client exists in the rule set
			var hasRule = this.rules.Licenses.Contains(e.Client.License) || e.Client.SteamId.HasValue && this.rules.Steam.Contains(e.Client.SteamId.Value) || this.rules.Ips.Contains(e.Client.EndPoint);

			// Whitelist mode and client is in rules
			// Blacklist mode and client is not in rules
			if (this.Configuration.Mode == BlockMode.Whitelist && hasRule ||
				this.Configuration.Mode == BlockMode.Blacklist && !hasRule)
			{
				// Notify other plugins client was allowed
				await this.Events.RaiseAsync(GateGuardEvents.ClientAllowed, e.Client, e.Session);

				// Access allowed
				return;
			}

			// Gracefully dismiss session with reason for dismissal
			using (var context = new StorageContext())
			{
				var dbSession = context.Sessions.Single(s => s.Id == e.Session.Id);
				dbSession.DisconnectReason = this.Configuration.Message;
				dbSession.Disconnected = DateTime.UtcNow;

				await context.SaveChangesAsync();
			}

			// Drop the client
			e.Deferrals.Done(this.Configuration.Message);

			// Notify other plugins client was dropped
			await this.Events.RaiseAsync(GateGuardEvents.ClientDropped, e.Client, e.Session);

			this.Logger.Info($"Client {e.Client.Name} [{e.Session.UserId}] session [{e.Session.Id}] dropped");
		}


		/// <summary>
		/// Loads all access rules
		/// </summary>
		private void Load()
		{
			// Lock to avoid races
			lock (this.lockObject)
			{
				// Load rules from config
				this.rules = new RulesConfig();

				using (var context = new StorageContext())
				{
					try
					{
						var now = DateTime.UtcNow;

						// Find all rules which are not deleted and haven't expired
						var dbRules = context.GuardRules.Where(r => !r.Deleted.HasValue && (!r.Expiry.HasValue || r.Expiry.Value > now));

						// Add database rules
						this.rules.Ips = this.Configuration.Rules.Ips.Union(dbRules.Where(r => r.IpAddress != null).Select(r => r.IpAddress)).ToList();
						this.rules.Licenses = this.Configuration.Rules.Licenses.Union(dbRules.Where(r => r.License != null).Select(r => r.License)).ToList();
						this.rules.Steam = this.Configuration.Rules.Steam.Union(dbRules.Where(r => r.SteamId.HasValue).Select(r => r.SteamId.Value)).ToList();
					}
					catch (Exception ex)
					{
						this.Logger.Error(ex, $"Load Rules Exception: {ex.InnerException.Message}");
					}
				}
			}
		}


		/// <summary>
		/// Creates a new rule and adds it to the database storage.
		/// </summary>
		/// <param name="e">The Rpc Event Handler</param>
		/// <param name="userId">The specified user</param>
		/// <param name="accessRule">The new rule to create</param>
		/// <param name="reason">Reason for rule creation</param>
		/// <param name="expiry">Optional expiration date for rule</param>
		private async void OnRuleCreate(IRpcEvent e, Guid userId, GateGuard.AccessRule accessRule, string reason, DateTime? expiry)
		{
			var rule = new GuardRule
			{
				PlayerUserId = userId,
				StaffUserId = e.User.Id,
				Reason = reason
			};

			if (!string.IsNullOrEmpty(accessRule.License)) rule.License = accessRule.License;
			if (accessRule.SteamId.HasValue) rule.SteamId = accessRule.SteamId;
			if (!string.IsNullOrEmpty(accessRule.IpAddress)) rule.IpAddress = accessRule.IpAddress;

			using (var context = new StorageContext())
			using (var transaction = context.Database.BeginTransaction())
			{
				try
				{
					context.GuardRules.Add(rule);
					await context.SaveChangesAsync();
					transaction.Commit();

					if (!string.IsNullOrEmpty(rule.License)) this.rules.Licenses.Add(rule.License);
					if (rule.SteamId.HasValue) this.rules.Steam.Add(rule.SteamId.Value);
					if (!string.IsNullOrEmpty(rule.IpAddress)) this.rules.Ips.Add(rule.IpAddress);

					this.Logger.Info($"Added new rule [{rule.Id}] for user {rule.PlayerUser.Name} [{rule.PlayerUser.Id}] by {rule.StaffUser.Name} for reason: {rule.Reason}");
				}
				catch (Exception ex)
				{
					transaction.Rollback();

					this.Logger.Error(ex, "Error creating rule");
				}
			}
		}


		/// <summary>
		/// Deletes a rule for a specified user
		/// </summary>
		/// <param name="e">The RPC Event handler.</param>
		/// <param name="userId">The identifier of the user to delete the rule for.</param>
		private async void OnRuleDelete(IRpcEvent e, Guid userId)
		{
			using (var context = new StorageContext())
			using (var transaction = context.Database.BeginTransaction())
			{
				try
				{
					var rule = context.GuardRules.Single(c => c.PlayerUserId == userId);
					rule.Deleted = DateTime.UtcNow;

					await context.SaveChangesAsync();
					transaction.Commit();

					Load();
				}
				catch (Exception ex)
				{
					this.Logger.Error(ex, "Error deleting rule");
					transaction.Rollback();
				}
			}
		}
	}
}
