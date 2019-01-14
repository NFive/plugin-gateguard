using NFive.GateGuard.Server.Models;
using NFive.SDK.Server.Storage;
using System.Data.Entity;

namespace NFive.GateGuard.Server.Storage
{
	public class StorageContext : EFContext<StorageContext>
	{
		public DbSet<GuardRule> GuardRules { get; set; }
	}
}
