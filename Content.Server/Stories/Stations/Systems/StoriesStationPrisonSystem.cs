using System.Numerics;
using System.Threading;
using Content.Server.Access.Systems;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Server.GameTicking.Events;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Events;
using Content.Shared.Tag;
using Content.Shared.Tiles;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server.Stories.Systems;


namespace Content.Server.Stories.Stations.Prison;

public sealed partial class StoriesStationPrisonSystem : EntitySystem
{
    /*
     * Space prison
     */

    [Dependency] private readonly IAdminLogManager _logger = default!;
    [Dependency] private readonly IAdminManager _admin = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AccessReaderSystem _reader = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly CommunicationsConsoleSystem _commsConsole = default!;
    [Dependency] private readonly DockingSystem _dock = default!;
    [Dependency] private readonly IdCardSystem _idSystem = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        // Don't immediately invoke as roundstart will just handle it.
        SubscribeLocalEvent<StoriesStationPrisonComponent, ComponentShutdown>(OnPrisonShutdown);
        SubscribeLocalEvent<StoriesStationPrisonComponent, ComponentInit>(OnPrisonInit);
    }

    private void OnPrisonShutdown(EntityUid uid, StoriesStationPrisonComponent component, ComponentShutdown args)
    {
        QueueDel(component.Entity);
        component.Entity = EntityUid.Invalid;

        if (_mapManager.MapExists(component.MapId))
            _mapManager.DeleteMap(component.MapId);

        component.MapId = MapId.Nullspace;
    }

    private void OnPrisonInit(EntityUid uid, StoriesStationPrisonComponent component, ComponentInit args)
    {
        // Post mapinit? fancy
        if (TryComp<TransformComponent>(component.Entity, out var xform))
        {
            component.MapId = xform.MapID;
            return;
        }

        AddPrison(component);
    }

    private void AddPrison(StoriesStationPrisonComponent component)
    {
        // Check for existing centcomms and just point to that
        var query = AllEntityQuery<StoriesStationPrisonComponent>();

        while (query.MoveNext(out var otherComp))
        {
            if (otherComp == component)
                continue;

            component.MapId = otherComp.MapId;
            return;
        }

        var mapId = _mapManager.CreateMap();
        component.MapId = mapId;

        if (!string.IsNullOrEmpty(component.Map.ToString()))
        {
            var ent = _map.LoadGrid(mapId, component.Map.ToString());

            if (ent == null) return;

            component.Entity = ent.Value;
            _shuttle.AddFTLDestination(ent.Value, true);

            var ftl = EnsureComp<FTLDestinationComponent>(component.Entity);

            ftl.Whitelist = DestinationWL.CreateList("FTLDestinationAccessPrison");
        }
        else
        {
            _sawmill.Warning("No Space Prison map found, skipping setup.");
        }
    }

    public HashSet<MapId> GetPrisonMaps()
    {
        var query = AllEntityQuery<StoriesStationPrisonComponent>();
        var maps = new HashSet<MapId>(Count<StoriesStationPrisonComponent>());

        while (query.MoveNext(out var comp))
        {
            maps.Add(comp.MapId);
        }

        return maps;
    }
}
