﻿using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using SharePointBot.Services;
using SharePointBot.Model;
using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;
using SharePointBot.AutofacModules;
using SharePointBot.Services.Interfaces;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace SharePointBot.Dialogs
{
    [Serializable]
    public class GetSiteDialog : IDialog<BotSite>
    {
        private ISharePointBotStateService _sharePointBotStateService;

        public GetSiteDialog(ISharePointBotStateService sharePointBotStateService)
        {
            _sharePointBotStateService = sharePointBotStateService;
        }


        public async Task StartAsync(IDialogContext context)
        {
            BotSite currentSite = null;

            _sharePointBotStateService.BotContext = context;
            currentSite = await _sharePointBotStateService.GetCurrentSite();
           

            if (currentSite != null)
            {
                var siteNameToDisplay = !string.IsNullOrEmpty(currentSite.Alias) ? currentSite.Alias : currentSite.Title;

                await context.PostAsync($"You are on site '{siteNameToDisplay}' ({currentSite.Url}).");
            }
            else
            {
                await context.PostAsync("You haven't selected a site.");
            }

            context.Done(currentSite);
        }
    }
}