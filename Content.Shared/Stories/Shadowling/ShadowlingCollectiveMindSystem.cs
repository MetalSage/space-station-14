using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;

namespace Content.Shared.SpaceStories.Shadowling;
public sealed class ShadowlingCollectiveMindSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingCollectiveMindEvent>(OnCollectiveEvent);
    }

    private void OnCollectiveEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingCollectiveMindEvent ev)
    {
        if (ev.Handled)
            return;
        ev.Handled = true;

        var slaves = GetSlavesCount(uid, component);
        _popup.PopupEntity(string.Format("У вас {0} порабощённых", slaves), uid, uid);

        ShadowlingStage? nextPhase = null;

        switch (component.Stage)
        {
            case ShadowlingStage.Start:
                if (slaves >= 3)
                    nextPhase = ShadowlingStage.Basic;
                break;
            case ShadowlingStage.Basic:
                if (slaves >= 5)
                    nextPhase = ShadowlingStage.Medium;
                break;
            case ShadowlingStage.Medium:
                if (slaves >= 9)
                    nextPhase = ShadowlingStage.High;
                break;
            case ShadowlingStage.High:
                if (slaves >= 15)
                    nextPhase = ShadowlingStage.Final;
                break;
        }

        if (nextPhase is not { } notNullNextPhase)
            return;

        _popup.PopupEntity("Новые способности разблокированы", uid, uid);
        component.Stage = notNullNextPhase;
        Dirty(uid, component);
    }

    private int GetSlavesCount(EntityUid uid, ShadowlingComponent component)
    {
        var counter = 0;

        foreach (var slave in component.Slaves)
        {
            var state = Comp<MobStateComponent>(slave);

            if (state.CurrentState == MobState.Alive)
            {
                counter++;
            }
        }

        return counter;
    }
}
