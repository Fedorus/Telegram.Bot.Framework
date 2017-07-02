﻿namespace Telegram.Bot.Framework.Abstractions
{
    /// <summary>
    /// Configurations for the bot
    /// </summary>
    /// <typeparam name="TBot">Type of Bot</typeparam>
    public interface IBotOptions<TBot>
        where TBot : class, IBot
    {
        /// <summary>
        /// Telegram API token
        /// </summary>
        string ApiToken { get; set; }

        /// <summary>
        /// User name of bot registered on Telegram
        /// </summary>
        string BotUserName { get; set; }

        /// <summary>
        /// Base bot url to be used for webhook and game updates
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// Path to TLS certificate file. The .pem public key file used for encrypting and authenticating webhooks
        /// </summary>
        string PathToCertificate { get; set; }
    }
}
