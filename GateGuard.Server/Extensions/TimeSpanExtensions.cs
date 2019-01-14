using System;
using System.Linq;

namespace NFive.GateGuard.Server.Extensions
{
	public static class TimeSpanExtensions
	{
		public static string ToFriendly(this TimeSpan timeSpan) => string.Join(", ", new[]
			{
				Tuple.Create("day", timeSpan.Days),
				Tuple.Create("hour", timeSpan.Hours),
				Tuple.Create("minute", timeSpan.Minutes),
				Tuple.Create("second", timeSpan.Seconds)
			}
			.Where(i => i.Item2 > 0)
			.Select(p => $"{p.Item2} {p.Item1}{(p.Item2 > 1 ? "s" : string.Empty)}"));
	}
}
