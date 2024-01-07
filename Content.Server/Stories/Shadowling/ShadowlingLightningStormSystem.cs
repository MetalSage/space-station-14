using Content.Server.Emp;
using Content.Server.Lightning;
using Content.Server.Power.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.SpaceStories.Shadowling;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.SpaceStories.Shadowling;
public sealed class ShadowlingLightningStormSystem : EntitySystem
{
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingLightningStormEvent>(OnLightningStormEvent);
    }

    private void OnLightningStormEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingLightningStormEvent ev)
    {
        ev.Handled = true;
        var poweredQuery = GetEntityQuery<ApcPowerReceiverComponent>();
        var mobQuery = GetEntityQuery<MobThresholdsComponent>();
        var validEnts = new HashSet<EntityUid>();
        foreach (var ent in _lookup.GetEntitiesInRange(uid, 9))
        {
            if (TryComp<ShadowlingComponent>(ent, out var _))
                continue;

            if (mobQuery.HasComponent(ent))
                validEnts.Add(ent);

            if (_random.Prob(0.01f) && poweredQuery.HasComponent(ent))
                validEnts.Add(ent);
        }

        foreach (var ent in validEnts)
        {
            _lightning.ShootLightning(uid, ent);
        }

        var empRange = 12;

        _emp.EmpPulse(_transform.GetMapCoordinates(Transform(uid)), empRange, 10000, 30);
    }
}
