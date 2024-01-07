using Content.Shared.Rejuvenate;
using Content.Shared.SpaceStories.Shadowling;

namespace Content.Server.SpaceStories.Shadowling;

public sealed class ShadowlingRapidReHatchSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingRapidReHatchEvent>(OnRapidReHatchEvent);
    }

    private void OnRapidReHatchEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingRapidReHatchEvent ev)
    {
        ev.Handled = true;
        var rejuvenate = new RejuvenateEvent();
        RaiseLocalEvent(uid, rejuvenate);
    }
}
