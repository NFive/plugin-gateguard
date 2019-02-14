using JetBrains.Annotations;
using Newtonsoft.Json;
using NFive.SDK.Core.Models;
using NFive.SDK.Core.Models.Player;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFive.GateGuard.Server.Models
{
	public class GuardRule : IdentityModel
	{

		/// <summary>
		/// Gets or sets the player user identifier.
		/// </summary>
		/// <value>
		/// The player user identifier.
		/// </value>
		[Required]
		[ForeignKey("PlayerUser")]
		public Guid PlayerUserId { get; set; }


		/// <summary>
		/// Gets or sets the player user.
		/// </summary>
		/// <value>
		/// The player user.
		/// </value>
		[JsonIgnore]
		public virtual User PlayerUser { get; set; }


		/// <summary>
		/// Optionally Gets or sets the whitelisted or banned license.
		/// </summary>
		/// <value>
		/// The license.
		/// </value>
		[CanBeNull]
		[StringLength(40, MinimumLength = 40)]
		public string License { get; set; }


		/// <summary>
		/// Optionally Gets or sets the whitelisted or banned steam identifier.
		/// </summary>
		/// <value>
		/// The steam identifier.
		/// </value>
		public long? SteamId { get; set; }


		/// <summary>
		/// Optionally Gets or sets the whitelisted or banned ip address.
		/// </summary>
		/// <value>
		/// The ip address.
		/// </value>
		[CanBeNull]
		[StringLength(15, MinimumLength = 7)]
		public string IpAddress { get; set; }


		/// <summary>
		/// Gets or sets the staff user identifier.
		/// </summary>
		/// <value>
		/// The staff user identifier.
		/// </value>
		[Required]
		[ForeignKey("StaffUser")]
		public Guid StaffUserId { get; set; }


		/// <summary>
		/// Gets or sets the staff user.
		/// </summary>
		/// <value>
		/// The staff user.
		/// </value>
		[JsonIgnore]
		public virtual User StaffUser { get; set; }


		/// <summary>
		/// Gets or sets the reason.
		/// </summary>
		/// <value>
		/// The reason.
		/// </value>
		[CanBeNull]
		public string Reason { get; set; }


		/// <summary>
		/// Optionally Gets or sets the expiry date and time.
		/// </summary>
		/// <value>
		/// The expiry.
		/// </value>
		public DateTime? Expiry { get; set; }
	}
}
