using System;
using System.Threading.Tasks;
using RFGarage.DatabaseManagers;
using RFGarage.Enums;
using RFGarage.Utils;
using RFRocketLibrary.Plugins;

namespace RFGarage.Commands
{
    [Aliases("gm")]
    [AllowedCaller(Rocket.API.AllowedCaller.Both)]
    [Permissions("garagemigrate")]
    [CommandInfo(Syntax: "/garagemigrate <from: mysql|litedb|json> <to: mysql|litedb|json>",
        Help: "Migrate garage database from one to another.")]
    public class GarageMigrateCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            if (!Enum.TryParse<EDatabase>(context.CommandRawArguments[0], true, out var from) ||
                !Enum.TryParse<EDatabase>(context.CommandRawArguments[1], true, out var to))
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            if (from == to)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.SAME_DATABASE.ToString()),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            if (!GarageManager.Ready)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.DATABASE_NOT_READY.ToString()),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.MIGRATION_START.ToString(), from, to),
                Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
            await Plugin.Inst.Database.GarageManager.MigrateAsync(from, to);
            await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.MIGRATION_FINISH.ToString()),
                Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
        }
    }
}