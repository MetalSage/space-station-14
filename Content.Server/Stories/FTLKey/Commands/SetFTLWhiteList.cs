using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server.Administration;
using Content.Server.Shuttles.Components;
using Content.Server.Stories.Systems;

namespace Content.Server.Stories.FTLKey.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class SetFTLWhiteList : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        public string Command => "setftlwhitelist";
        public string Description => "Create White List for current FTL point";
        public string Help => "setftlwhitelist <Grid or Map Uid> <FTL Tag 1> <...>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player is not { } player)
            {
                shell.WriteLine("shell-server-cannot");
                return;
            }

            if (args.Length != 2)
            {
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!int.TryParse(args[0], out var gridId))
            {
                shell.WriteLine(Loc.GetString("shell-argument-must-be-number"));
                return;
            }

            var grid = new EntityUid(gridId);

            if (!grid.IsValid() || !_entities.EntityExists(grid))
            {
                shell.WriteLine(Loc.GetString("shell-invalid-entity-id"));
                return;
            }

            var ftl = _entities.EnsureComponent<FTLDestinationComponent>(grid);
            ftl.Whitelist = DestinationWL.CreateList(args[1]);
        }
    }
}
