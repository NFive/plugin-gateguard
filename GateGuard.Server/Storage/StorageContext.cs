using NFive.GateGuard.Server.Models;
using NFive.SDK.Server.Storage;
using System.Data.Entity;
using NFive.SDK.Core.Models.Player;

namespace NFive.GateGuard.Server.Storage
{
	public class StorageContext : EFContext<StorageContext>
	{
		public DbSet<Session> Sessions { get; set; }

		public DbSet<GuardRule> GuardRules { get; set; }
	}
}
