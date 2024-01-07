using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.SpaceStories.Shadowling;

namespace Content.Server.SpaceStories.Shadowling;
public sealed class ShadowlingBlackRecuperationSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedShadowlingSystem _shadowling = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingBlackRecuperationEvent>(OnBlackRecuperationEvent);
    }

    private void OnBlackRecuperationEvent(EntityUid uid, ShadowlingComponent component, ShadowlingBlackRecuperationEvent ev)
    {
        if (ev.Handled)
            return;

        if (!TryComp<ShadowlingComponent>(ev.Performer, out _))
            return;

        if (!TryComp<DamageableComponent>(ev.Target, out var slaveDamageable))
            return;

        if (!TryComp<MobStateComponent>(ev.Target, out var slaveState))
            return;

        // you can't heal yourself!
        if (uid == ev.Target)
            return;

        ev.Handled = true;

        if (HasComp<ShadowlingThrallComponent>(ev.Target))
        {
            if (slaveState.CurrentState == MobState.Alive)
            {
                _damageable.SetAllDamage(ev.Target, slaveDamageable, 0);
                RemCompDeferred<ShadowlingThrallComponent>(ev.Target);
                var lowerShadowling = EnsureComp<ShadowlingComponent>(ev.Target);
                _shadowling.SetStage(ev.Target, lowerShadowling, ShadowlingStage.Lower);
                _popup.PopupEntity("Ваше тело сливается с тенью...", ev.Target, ev.Target);
            }
            else
            {
                _damageable.SetAllDamage(ev.Target, slaveDamageable, 0);
                _mobState.ChangeMobState(ev.Target, MobState.Alive);
                _popup.PopupEntity("Ваши раны покрываются тенью и затягиваются...", ev.Target, ev.Target);
            }
        }

    }
}
