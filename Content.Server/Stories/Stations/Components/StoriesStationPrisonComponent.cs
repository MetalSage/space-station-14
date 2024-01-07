using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server.Stories.Stations.Prison;

/// <summary>
/// Spawns Space Prison
/// </summary>
[RegisterComponent]
public sealed partial class StoriesStationPrisonComponent : Component
{

    [DataField("map")]
    public ResPath Map = new("/Maps/Stories/Misc/prison.yml");

    /// <summary>
    /// Space Prison entity that was loaded.
    /// </summary>
    [DataField("entity")]
    public EntityUid Entity = EntityUid.Invalid;

    public MapId MapId = MapId.Nullspace;

}

