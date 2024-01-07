using System.Linq;
using Content.Shared.Movement.Systems;
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
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingShadowWalkEvent>(OnShadowWalkEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingPlaneShiftEvent>(OnPlaneShiftEvent);
    }

    private void OnShadowWalkEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingShadowWalkEvent ev)
    {
        if (!TryComp<FixturesComponent>(uid, out var fixtures) || ev.Handled)
            return;
        ev.Handled = true;

        if (!component.InShadowWalk)
            BeginShadowWalk(uid, component, fixtures);
        else
            EndShadowWalk(uid, component, fixtures);
    }

    private void OnPlaneShiftEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingPlaneShiftEvent ev)
    {
        if (!TryComp<FixturesComponent>(uid, out var fixtures) || ev.Handled)
            return;
        ev.Handled = true;

        if (!component.InShadowWalk)
            BeginPlaneShift(uid, component, fixtures);
        else
            EndPlaneShift(uid, component, fixtures);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ShadowlingComponent, FixturesComponent>();

        while (query.MoveNext(out var uid, out var comp, out var fixtures))
        {
            if (comp.InShadowWalk && comp.Stage != ShadowlingStage.Ascended && curTime > comp.ShadowWalkEndsAt)
            {
                EndShadowWalk(uid, comp, fixtures);
            }
        }
    }

    private void BeginShadowWalk(EntityUid uid, ShadowlingComponent shadowling, FixturesComponent fixtures)
    {
        _speed.ChangeBaseSpeed(uid, 5, 8.5f, 20);

        var fixture = fixtures.Fixtures.First();

        _physics.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) CollisionGroup.None, fixtures);
        _physics.SetCollisionLayer(uid, fixture.Key, fixture.Value, (int) CollisionGroup.Opaque, fixtures);

        var curTime = _timing.CurTime;

        shadowling.ShadowWalkEndsAt = curTime.Add(shadowling.ShadowWalkEndsIn);
        shadowling.InShadowWalk = true;

        Dirty(uid, shadowling);
    }

    private void EndShadowWalk(EntityUid uid, ShadowlingComponent shadowling, FixturesComponent fixtures)
    {
        _speed.ChangeBaseSpeed(uid, 2.5f, 4.5f, 20);
        var fixture = fixtures.Fixtures.First();
        _physics.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) CollisionGroup.MobMask, fixtures);
        _physics.SetCollisionLayer(uid, fixture.Key, fixture.Value, (int) CollisionGroup.MobLayer, fixtures);
        shadowling.InShadowWalk = false;
        Dirty(uid, shadowling);
    }

    private void BeginPlaneShift(EntityUid uid, ShadowlingComponent shadowling, FixturesComponent fixtures)
    {
        _speed.ChangeBaseSpeed(uid, 10, 10, 20);

        var fixture = fixtures.Fixtures.First();

        _physics.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) CollisionGroup.None, fixtures);
        _physics.SetCollisionLayer(uid, fixture.Key, fixture.Value, (int) CollisionGroup.Opaque, fixtures);

        var curTime = _timing.CurTime;

        shadowling.ShadowWalkEndsAt = curTime.Add(shadowling.ShadowWalkEndsIn);
        shadowling.InShadowWalk = true;

        Dirty(uid, shadowling);
    }

    private void EndPlaneShift(EntityUid uid, ShadowlingComponent shadowling, FixturesComponent fixtures)
    {
        _speed.ChangeBaseSpeed(uid, 8, 8, 20);
        var fixture = fixtures.Fixtures.First();
        _physics.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) CollisionGroup.FlyingMobMask, fixtures);
        _physics.SetCollisionLayer(uid, fixture.Key, fixture.Value, (int) CollisionGroup.FlyingMobLayer, fixtures);
        shadowling.InShadowWalk = false;
        Dirty(uid, shadowling);
    }
}
