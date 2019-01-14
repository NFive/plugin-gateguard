namespace NFive.GateGuard.Shared
{
	public static class GateGuardEvents
	{
		public const string ClientAllowed = "nfive:gateguard:clientAllowed";
		public const string ClientDropped = "nfive:gateguard:clientDropped";

		public const string RuleCreate = "nfive:gateguard:ruleCreate";
		public const string RuleDelete = "nfive:gateguard:ruleDelete";
	}
}
