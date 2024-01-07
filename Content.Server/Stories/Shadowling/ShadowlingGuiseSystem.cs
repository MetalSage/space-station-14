using Content.Server.Stealth;
using Content.Shared.SpaceStories.Shadowling;
using Content.Shared.Stealth.Components;
using Robust.Shared.Timing;

namespace Content.Server.SpaceStories.Shadowling;

public sealed class ShadowlingGuiseSystem : EntitySystem
{
    [Dependency] private readonly StealthSystem _stealth = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingGuiseEvent>(OnGuise);
    }

    private void OnGuise(EntityUid uid, ShadowlingComponent component, ref ShadowlingGuiseEvent ev)
    {
        var stealth = EnsureComp<StealthComponent>(uid);

        _stealth.SetVisibility(uid, stealth.MinVisibility, stealth);
        _stealth.SetEnabled(uid, true, stealth);

        var curTime = _timing.CurTime;
        component.GuiseEndsAt = curTime.Add(component.GuiseEndsIn);
        Dirty(uid, component);
        ev.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<ShadowlingComponent, StealthComponent>();
        while (query.MoveNext(out var uid, out var comp, out var stealth))
        {
            if (comp.GuiseEndsAt < curTime && stealth.Enabled)
            {
                _stealth.SetVisibility(uid, stealth.MaxVisibility, stealth);
                _stealth.SetEnabled(uid, false, stealth);
            }
        }
    }
}
