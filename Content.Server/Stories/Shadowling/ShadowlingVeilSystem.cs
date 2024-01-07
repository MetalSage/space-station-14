using Content.Server.Emp;
using Content.Shared.SpaceStories.Shadowling;
using Robust.Server.GameObjects;

namespace Content.Server.SpaceStories.Shadowling;
public sealed class ShadowlingVeilSystem : EntitySystem
{
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly EmpSystem _emp = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingVeilEvent>(OnVeilEvent);
    }

    private void OnVeilEvent(EntityUid performer, ShadowlingComponent component, ref ShadowlingVeilEvent ev)
    {
        ev.Handled = true;

        var lights = _shadowling.GetEntitiesAroundShadowling<PointLightComponent>(performer, 15);

        foreach (var entity in lights)
        {
            var meta = Comp<MetaDataComponent>(entity);

            if (meta.EntityPrototype == null || component.VeilBlacklist.Contains(meta.EntityPrototype.ID))
                continue;

            _emp.DoEmpEffects(entity, 50000, 60);
        }
    }
}
