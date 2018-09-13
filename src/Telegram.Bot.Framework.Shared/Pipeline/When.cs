﻿using Telegram.Bot.Framework.Abstractions;

namespace Telegram.Bot.Framework
{
    public static class When
    {
        public static bool IsWebhook(IUpdateContext context)
            => context.IsWebhook;

        public static bool NewTextMessage(IUpdateContext context) =>
            context.Update.Message?.Text != null;

        public static bool MembersChanged(IUpdateContext context) =>
            context.Update.Message?.NewChatMembers != null ||
            context.Update.Message?.LeftChatMember != null ||
            context.Update.ChannelPost?.NewChatMembers != null ||
            context.Update.ChannelPost?.LeftChatMember != null
        ;

        public static bool LocationMessage(IUpdateContext context) =>
            context.Update.Message?.Location != null;
    }
}
