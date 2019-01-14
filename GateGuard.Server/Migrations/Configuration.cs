using JetBrains.Annotations;
using NFive.GateGuard.Server.Storage;
using NFive.SDK.Server.Migrations;

namespace NFive.GateGuard.Server.Migrations
{
	[UsedImplicitly]
	public sealed class Configuration : MigrationConfiguration<StorageContext> { }
}
