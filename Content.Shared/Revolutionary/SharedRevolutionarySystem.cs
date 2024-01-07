// edited by Space Stories
using Content.Shared.IdentityManagement;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.Revolutionary.Components;
using Content.Shared.SpaceStories.Mindshield;
using Content.Shared.Stunnable;

namespace Content.Shared.Revolutionary;

public sealed class SharedRevolutionarySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _sharedStun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeadRevolutionaryComponent, MindShieldImplantedEvent>(HeadRevMindShieldImplanted);
        SubscribeLocalEvent<RevolutionaryComponent, MindShieldImplantedEvent>(RevMindShieldImplanted);
    }

    /// <summary>
    /// When the mindshield is implanted in the head rev it will remove the mindshield component
    /// </summary>
    private void HeadRevMindShieldImplanted(EntityUid uid, HeadRevolutionaryComponent comp, MindShieldImplantedEvent ev)
    {
        RemCompDeferred<MindShieldComponent>(uid);
    }

    /// <summary>
    /// When the mindshield is implanted in the rev it will popup saying they were deconverted
    /// </summary>
    private void RevMindShieldImplanted(EntityUid uid, RevolutionaryComponent comp, MindShieldImplantedEvent ev)
    {
        var stunTime = TimeSpan.FromSeconds(4);
        var name = Identity.Entity(uid, EntityManager);
        RemComp<RevolutionaryComponent>(uid);
        _sharedStun.TryParalyze(uid, stunTime, true);
        _popupSystem.PopupEntity(Loc.GetString("rev-break-control", ("name", name)), uid);
    }
}
