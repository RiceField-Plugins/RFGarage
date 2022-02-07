#if DEBUG
using System;
using System.Threading.Tasks;
using RFGarage.DatabaseManagers;
using RFGarage.Enums;
using RFRocketLibrary.Plugins;

namespace RFGarage.Commands
{
    [Aliases("gom")]
    [AllowedCaller(Rocket.API.AllowedCaller.Both)]
    [Permissions("garageoldmigrate")]
    [CommandInfo(Syntax: "/garageoldmigrate <to: mysql|litedb|json>",
        Help: "Migrate garage database from Old RFGarage to RFGarage.")]
    public class GarageOldMigrateCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 1)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!Enum.TryParse<EDatabase>(context.CommandRawArguments[0], true, out var to))
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!GarageManager.Ready)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.DATABASE_NOT_READY.ToString()));
                return;
            }

            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.MIGRATION_START.ToString(), "RFGarage Old", to));
            await Plugin.Inst.Database.GarageManager.MigrateGarageAsync(to);
            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.MIGRATION_FINISH.ToString()));
        }
    }
}
#endif