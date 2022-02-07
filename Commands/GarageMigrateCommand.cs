using System;
using System.Threading.Tasks;
using RFGarage.DatabaseManagers;
using RFGarage.Enums;
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
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!Enum.TryParse<EDatabase>(context.CommandRawArguments[0], true, out var from) ||
                !Enum.TryParse<EDatabase>(context.CommandRawArguments[1], true, out var to))
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (from == to)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.SAME_DATABASE.ToString()));
                return;
            }

            if (!GarageManager.Ready)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.DATABASE_NOT_READY.ToString()));
                return;
            }

            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.MIGRATION_START.ToString(), from, to));
            await Plugin.Inst.Database.GarageManager.MigrateAsync(from, to);
            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.MIGRATION_FINISH.ToString()));
        }
    }
}