using Content.Server.Flash;
using Content.Server.Popups;
using Content.Shared.Eye.Blinding.Systems;

namespace Content.Shared.SpaceStories.Eye;
public sealed class WeakEyesSystem : EntitySystem
{
    [Dependency] private readonly BlindableSystem _blind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakEyesComponent, FlashAttemptEvent>(OnAfterFlashed);
    }

    private void OnAfterFlashed(EntityUid uid, WeakEyesComponent weakEyes, FlashAttemptEvent args)
    {
        _popup.PopupClient("Яркая вспышка света жгёт ваши глаза!", args.Target, args.Target);
        _blind.AdjustEyeDamage(args.Target, 2);
        _blind.UpdateIsBlind(args.Target);
    }
}
