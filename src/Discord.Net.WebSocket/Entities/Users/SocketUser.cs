using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Model = Discord.API.User;

namespace Discord.WebSocket
{
    /// <summary>
    ///     Represents a WebSocket-based user.
    /// </summary>
    [DebuggerDisplay(@"{DebuggerDisplay,nq}")]
    public abstract class SocketUser : SocketEntity<ulong>, IUser
    {
        /// <inheritdoc />
        public abstract bool IsBot { get; internal set; }
        /// <inheritdoc />
        public abstract string Username { get; internal set; }
        /// <inheritdoc />
        public abstract ushort DiscriminatorValue { get; internal set; }
        /// <inheritdoc />
        public abstract string AvatarId { get; internal set; }
        /// <inheritdoc />
        public abstract string BannerId { get; internal set; }
        /// <inheritdoc />
        public abstract Color? AccentColor { get; internal set; }
        /// <inheritdoc />
        public abstract bool IsWebhook { get; }
        /// <inheritdoc />
        public UserProperties? PublicFlags { get; private set; }
        internal abstract SocketGlobalUser GlobalUser { get; }
        internal abstract SocketPresence Presence { get; set; }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);
        /// <inheritdoc />
        public string Discriminator => DiscriminatorValue.ToString("D4");
        /// <inheritdoc />
        public string Mention => MentionUtils.MentionUser(Id);
        /// <inheritdoc />
        public UserStatus Status => Presence.Status;
        /// <inheritdoc />
        public IImmutableSet<ClientType> ActiveClients => Presence.ActiveClients ?? ImmutableHashSet<ClientType>.Empty;
        /// <inheritdoc />
        public IImmutableList<IActivity> Activities => Presence.Activities ?? ImmutableList<IActivity>.Empty;
        /// <summary>
        ///     Gets mutual guilds shared with this user.
        /// </summary>
        /// <remarks>
        ///     This property will only include guilds in the same <see cref="DiscordSocketClient"/>.
        /// </remarks>
        public IReadOnlyCollection<SocketGuild> MutualGuilds
            => Discord.Guilds.Where(g => g.GetUser(Id) != null).ToImmutableArray();

        internal SocketUser(DiscordSocketClient discord, ulong id)
            : base(discord, id)
        {
        }
        internal virtual bool Update(ClientState state, Model model)
        {
            bool hasChanges = false;
            if (model.Avatar.IsSpecified && model.Avatar.Value != AvatarId)
            {
                AvatarId = model.Avatar.Value;
                hasChanges = true;
            }
            if (model.Banner.IsSpecified && model.Banner.Value != BannerId)
            {
                BannerId = model.Banner.Value;
                hasChanges = true;
            }
            if (model.AccentColor.IsSpecified && model.AccentColor.Value != AccentColor?.RawValue)
            {
                AccentColor = model.AccentColor.Value;
                hasChanges = true;
            }
            if (model.Discriminator.IsSpecified)
            {
                var newVal = ushort.Parse(model.Discriminator.Value, NumberStyles.None, CultureInfo.InvariantCulture);
                if (newVal != DiscriminatorValue)
                {
                    DiscriminatorValue = ushort.Parse(model.Discriminator.Value, NumberStyles.None, CultureInfo.InvariantCulture);
                    hasChanges = true;
                }
            }
            if (model.Bot.IsSpecified && model.Bot.Value != IsBot)
            {
                IsBot = model.Bot.Value;
                hasChanges = true;
            }
            if (model.Username.IsSpecified && model.Username.Value != Username)
            {
                Username = model.Username.Value;
                hasChanges = true;
            }
            if (model.PublicFlags.IsSpecified && model.PublicFlags.Value != PublicFlags)
            {
                PublicFlags = model.PublicFlags.Value;
                hasChanges = true;
            }
            return hasChanges;
        }

        /// <inheritdoc />
        public async Task<IDMChannel> CreateDMChannelAsync(RequestOptions options = null)
            => await UserHelper.CreateDMChannelAsync(this, Discord, options).ConfigureAwait(false);

        /// <inheritdoc />
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => CDN.GetUserAvatarUrl(Id, AvatarId, size, format);

        /// <inheritdoc />
        public string GetBannerUrl(ImageFormat format = ImageFormat.Auto, ushort size = 256)
            => CDN.GetUserBannerUrl(Id, BannerId, size, format);

        /// <inheritdoc />
        public string GetDefaultAvatarUrl()
            => CDN.GetDefaultUserAvatarUrl(DiscriminatorValue);

        /// <summary>
        ///     Gets the full name of the user (e.g. Example#0001).
        /// </summary>
        /// <returns>
        ///     The full name of the user.
        /// </returns>
        public override string ToString() => $"{Username}#{Discriminator}";
        private string DebuggerDisplay => $"{Username}#{Discriminator} ({Id}{(IsBot ? ", Bot" : "")})";
        internal SocketUser Clone() => MemberwiseClone() as SocketUser;
    }
}
