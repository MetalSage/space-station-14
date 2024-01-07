using Content.Client.Antag;
using Content.Shared.SpaceStories.Shadowling;
using Content.Shared.StatusIcon.Components;

namespace Content.Client.SpaceStories.Shadowling;
public sealed class ShadowlingIconSystem : AntagStatusIconSystem<ShadowlingComponent>
{
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, ShadowlingComponent shadowling, ref GetStatusIconsEvent args)
    {
        if (_shadowling.IsShadowlingSlave(shadowling))
            GetStatusIcon("ShadowlingThrallFaction", ref args);
        else
            GetStatusIcon("ShadowlingFaction", ref args);
    }
}
