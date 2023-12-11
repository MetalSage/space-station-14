using System.Linq;
using Content.Shared.Physics;
using Content.Shared.SpaceStories.Shadowling;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server.SpaceStories.Shadowling;
public sealed class ShadowlingShadowWalkSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingForceComponent, ShadowlingShadowWalkEvent>(OnShadowlingShadowWalkEvent);
    }

    private void OnShadowlingShadowWalkEvent(EntityUid uid, ShadowlingForceComponent component, ref ShadowlingShadowWalkEvent ev)
    {
        if (!TryComp<FixturesComponent>(uid, out var fixtures))
            return;

        BeginShadowWalk(uid, component, fixtures);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ShadowlingForceComponent, FixturesComponent>();

        while (query.MoveNext(out var uid, out var comp, out var fixtures))
        {
            if (comp.InShadowWalk && curTime > comp.ShadowWalkEndsAt)
            {
                EndShadowWalk(uid, comp, fixtures);
            }
        }
    }

    private void BeginShadowWalk(EntityUid uid, ShadowlingForceComponent shadowling, FixturesComponent fixtures)
    {
        var fixture = fixtures.Fixtures.First();

        _physics.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) CollisionGroup.Opaque, fixtures);
        _physics.SetCollisionLayer(uid, fixture.Key, fixture.Value, (int) CollisionGroup.GhostImpassable, fixtures);

        var curTime = _timing.CurTime;

        shadowling.ShadowWalkEndsAt = curTime.Add(shadowling.ShadowWalkEndsIn);
        shadowling.InShadowWalk = true;

        Dirty(uid, shadowling);
    }

    private void EndShadowWalk(EntityUid uid, ShadowlingForceComponent shadowling, FixturesComponent fixtures)
    {
        var fixture = fixtures.Fixtures.First();
        _physics.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) CollisionGroup.MobMask, fixtures);
        _physics.SetCollisionLayer(uid, fixture.Key, fixture.Value, (int) CollisionGroup.MobLayer, fixtures);
        shadowling.InShadowWalk = false;
        Dirty(uid, shadowling);
    }
}
