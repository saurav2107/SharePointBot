﻿using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SharePointBot.AutofacModules;
using SharePointBot.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SharePointBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var builder = new ContainerBuilder();
            RegisterDependencies(builder);
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            var config = GlobalConfiguration.Configuration;
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // We have to individually update the bot-level dependencies because the BotAuth functionality depends on it.
            Conversation.UpdateContainer(RegisterDependencies);
        }


        protected void RegisterDependencies(ContainerBuilder builder)
        {
            builder.RegisterModule(new DialogModule());

            builder.RegisterModule(new SharePointBotModule(
                ConfigurationManager.AppSettings["LuisModelId"],
                ConfigurationManager.AppSettings["LuisSubscriptionKey"]));


            builder.RegisterModule(new QnAMakerModule(
                   ConfigurationManager.AppSettings["QnASubscriptionKey"],
                   ConfigurationManager.AppSettings["QnAKbId"],
                   Constants.QnA.DefaultMessage,
                   Constants.QnA.Threshold,
                   Constants.QnA.top));

            // If specified in config, register trace logger for activitities. This will log all activities to whichever trace listeners are set up.
            bool traceAllActivities = false;
            bool.TryParse(ConfigurationManager.AppSettings["TraceAllActivities"], out traceAllActivities);
            if (traceAllActivities)
            {
                builder.RegisterType<TraceActivityLogger>().AsImplementedInterfaces().InstancePerDependency();
            }

#if DEBUG
#else
                // TODO : See issue #1 - this causes issues when commented in. Need to work out how to use Azure Storage.
                //builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                //var store = new TableBotDataStore(ConfigurationManager.AppSettings["StorageConnectionString"]);

                //builder.Register(c => store)
                //    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                //    .AsSelf()
                //    .SingleInstance();
#endif

        }
    }
}
