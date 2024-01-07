using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.SpaceStories.Shadowling;

/// <summary>
/// Is relayed at the end of the sericulturing doafter.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class EnthrallDoAfterEvent : SimpleDoAfterEvent
{
}
