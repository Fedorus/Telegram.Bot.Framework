﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Framework.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensoin methods for adding a Telegram Bot to an Microsoft.Extensions.DependencyInjection.IServiceCollection
    /// </summary>
    public static class TelegramBotFrameworkIServiceCollectionExtensions
    {
        private static IServiceCollection _services;

        /// <summary>
        /// Adds a Telegram bot to the service collection using the bot's options
        /// </summary>
        /// <typeparam name="TBot">Type of Telegarm bot</typeparam>
        /// <param name="services">Instance of IServiceCollection</param>
        /// <param name="botOptions">Optins for configuring the bot</param>
        /// <returns>Instance of bot framework builder</returns>
        public static ITelegramBotFrameworkBuilder<TBot> AddTelegramBot<TBot>
            (this IServiceCollection services, BotOptions<TBot> botOptions)
            where TBot : BotBase<TBot>
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (botOptions == null)
            {
                throw new ArgumentNullException(nameof(botOptions));
            }

            _services = services;
            return new TelegramBotFrameworkBuilder<TBot>(botOptions);
        }

        /// <summary>
        /// Adds a Telegram bot to the service collection using configurations
        /// </summary>
        /// <typeparam name="TBot">Type of Telegarm bot</typeparam>
        /// <param name="services">Instance of IServiceCollection</param>
        /// <param name="config">Configuring for the bot</param>
        /// <returns>Instance of bot framework builder</returns>
        public static ITelegramBotFrameworkBuilder<TBot> AddTelegramBot<TBot>
            (this IServiceCollection services, IConfiguration config)
            where TBot : BotBase<TBot>
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _services = services;
            return new TelegramBotFrameworkBuilder<TBot>(config);
        }

        /// <summary>
        /// Responsible for configuring services for the bot and adding them to the container
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        public interface ITelegramBotFrameworkBuilder<TBot>
            where TBot : BotBase<TBot>
        {
            /// <summary>
            /// Configures an update handler for the bot
            /// </summary>
            /// <typeparam name="T">Type of update handler</typeparam>
            /// <returns>Itself</returns>
            ITelegramBotFrameworkBuilder<TBot> AddUpdateHandler<T>()
                where T : class, IUpdateHandler;

            /// <summary>
            /// Completes the configuration for the bot and adds all the services
            /// </summary>
            /// <returns>Instance of IServiceCollection</returns>
            IServiceCollection Configure();
        }

        /// <summary>
        /// Responsible for configuring services for the bot and adding them to the container
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        public class TelegramBotFrameworkBuilder<TBot> : ITelegramBotFrameworkBuilder<TBot>
            where TBot : BotBase<TBot>
        {
            private readonly List<Type> _handlerTypes = new List<Type>();

            private readonly BotOptions<TBot> _botOptions;

            private readonly IConfiguration _configuration;

            /// <summary>
            /// Initializes and instance of this class with the options provided
            /// </summary>
            /// <param name="botOptions">Optoins for the bot</param>
            public TelegramBotFrameworkBuilder(BotOptions<TBot> botOptions)
            {
                _botOptions = botOptions;
            }

            /// <summary>
            /// Initializes and instance of this class with the configuration provided
            /// </summary>
            /// <param name="configuration">Configuration for the bot</param>
            public TelegramBotFrameworkBuilder(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            /// <summary>
            /// Configures an update handler for the bot
            /// </summary>
            /// <typeparam name="T">Type of update handler</typeparam>
            /// <returns>Itself</returns>
            public ITelegramBotFrameworkBuilder<TBot> AddUpdateHandler<T>()
                where T : class, IUpdateHandler
            {
                _handlerTypes.Add(typeof(T));
                return this;
            }

            /// <summary>
            /// Completes the configuration for the bot and adds all the services
            /// </summary>
            /// <returns>Instance of IServiceCollection</returns>
            public IServiceCollection Configure()
            {
                EnsureValidConfiguration();

                if (_botOptions != null)
                {
                    _services.Configure<BotOptions<TBot>>(x =>
                    {
                        x.ApiToken = _botOptions.ApiToken;
                        x.BotUserName = _botOptions.BotUserName;
                        x.WebhookUrl = _botOptions.WebhookUrl;
                        x.PathToCertificate = _botOptions.PathToCertificate;
                        x.GameOptions = _botOptions.GameOptions;
                    });
                }
                else
                {
                    _services.Configure<BotOptions<TBot>>(_configuration);
                }

                _services.AddScoped<TBot>();

                _handlerTypes.ForEach(x => _services.AddTransient(x));

                _services.AddScoped<IUpdateHandlersAccessor<TBot>>(factory =>
                {
                    var handlers = _handlerTypes.Select(x => (IUpdateHandler)factory.GetRequiredService(x)).ToArray();
                    return new UpdateHanldersAccessor<TBot>(handlers);
                });

                _services.AddScoped<IUpdateParser<TBot>, UpdateParser<TBot>>();

                _services.AddScoped<IBotManager<TBot>, BotManager<TBot>>();

                return _services;
            }

            private void EnsureValidConfiguration()
            {
                if (!_handlerTypes.Any())
                {
                    throw new ConfigurationException("No update handler is provided", $"Use {nameof(AddUpdateHandler)} method");
                }
                // ToDo: Validate others
            }
        }
    }
}