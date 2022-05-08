using System;
using System.Threading.Tasks;
using RFGarage.DatabaseManagers;
using RFGarage.Enums;
using RFGarage.Utils;
using Rocket.API;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace RFGarage.Commands
{
    [CommandAliases("gm")]
    [CommandActor(AllowedCaller.Both)]
    [CommandPermissions("garagemigrate")]
    [CommandInfo("Migrate garage database from one to another.", "/garagemigrate <from: mysql|litedb|json> <to: mysql|litedb|json>", AllowSimultaneousCalls = false)]
    public class GarageMigrateCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (!Enum.TryParse<EDatabase>(context.CommandRawArguments[0], true, out var from) ||
                !Enum.TryParse<EDatabase>(context.CommandRawArguments[1], true, out var to))
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (from == to)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.SAME_DATABASE.ToString()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (!GarageManager.Ready)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.DATABASE_NOT_READY.ToString()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.MIGRATION_START.ToString(), from, to),
                RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
            await DatabaseManager.Queue.Enqueue(async () => await GarageManager.MigrateAsync(from, to))!;
            await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.MIGRATION_FINISH.ToString()),
                RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
        }
    }
}