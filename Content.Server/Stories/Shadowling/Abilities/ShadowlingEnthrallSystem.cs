using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.Body.Components;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Stories.Shadowling;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Server.Stories.Shadowling;
public sealed class ShadowlingEnthrallSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingEnthrallEvent>(OnEnthrallEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHypnosisEvent>(OnHypnosisEvent);
        SubscribeLocalEvent<ShadowlingComponent, EnthrallDoAfterEvent>(OnEnthrallDoAfterEvent);
    }

    private void OnEnthrallEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingEnthrallEvent ev)
    {
        if (ev.Handled)
            return;

        if (TryComp<ShadowlingComponent>(ev.Target, out _))
            return;

        // You cannot enthrall someone with wrong body
        if (!TryComp<BodyComponent>(ev.Target, out var body) || body.Prototype == null || !component.EnthrallablePrototypes.Contains(body.Prototype.Value.Id))
            return;
        // You cannot enthrall someone without mind
        var shadowlingEnthrallRequireMindAvailability = _config.GetCVar(CCVars.ShadowlingEnthrallRequireMindAvailability);
        if (shadowlingEnthrallRequireMindAvailability && (!TryComp<MindContainerComponent>(ev.Target, out var mind) || !mind.HasMind))
        {
            _popup.PopupEntity("Вы можете порабощать существ только в сознании", uid, uid);
            return;
        }
        // You cannot enthrall someone or something not biological (borgs for example)
        if (!TryComp<DamageableComponent>(ev.Target, out var damage) || damage.DamageContainerID != "Biological")
            return;

        var coords = _transform.GetWorldPosition(ev.Target);
        var distance = (_transform.GetWorldPosition(uid) - coords).Length();

        if (distance > 2)
            return;

        ev.Handled = true;

        if (TryComp<MindShieldComponent>(ev.Target, out _))
        {
            _popup.PopupEntity("Вы поглощаете чей-то разум... Некий барьер полностью отражает вашу атаку", ev.Performer, ev.Performer);
            _popup.PopupEntity("Ваш разум поглощается тенями... Но некий барьер полностью изгоняет их из вашего разума", ev.Target, ev.Target);
            _stamina.TryTakeStamina(ev.Performer, 50);
            return;
        }

        _popup.PopupEntity("Вы поглощаете чей-то разум...", ev.Performer, ev.Performer);
        _popup.PopupEntity("Ваш разум поглощается тенями...", ev.Target, ev.Target);

        var doAfter = new DoAfterArgs(EntityManager, ev.Performer, 30, new EnthrallDoAfterEvent(), ev.Performer, ev.Target)
        {
            BreakOnUserMove = true,
            BlockDuplicate = true,
            BreakOnDamage = true,
            BreakOnTargetMove = true
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnHypnosisEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingHypnosisEvent ev)
    {
        ev.Handled = false;
        if (TryComp<ShadowlingComponent>(ev.Target, out _))
            return;

        // You cannot enthrall someone without body
        if (!TryComp<BodyComponent>(ev.Target, out _))
            return;

        // You cannot enthrall someone without mind
        var shadowlingEnthrallRequireMindAvailability = _config.GetCVar(CCVars.ShadowlingEnthrallRequireMindAvailability);
        if (shadowlingEnthrallRequireMindAvailability && (!TryComp<MindContainerComponent>(ev.Target, out var mind) || !mind.HasMind))
        {
            _popup.PopupEntity("Вы можете порабощать существ только в сознании", uid, uid);
            return;
        }

        // You cannot enthrall someone or something not biological (borgs for example)
        if (!TryComp<DamageableComponent>(ev.Target, out var damage) || damage.DamageContainerID != "Biological")
            return;

        ev.Handled = true;

        if (TryComp<MindShieldComponent>(ev.Target, out _))
        {
            _popup.PopupEntity("Некий барьер полностью отражает вашу атаку", ev.Performer, ev.Performer);
            _popup.PopupEntity("Некий барьер отразил сильнейшую ментальную атаку", ev.Target, ev.Target);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, ev.Performer, 1, new EnthrallDoAfterEvent(), ev.Target)
        {
            BlockDuplicate = true
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnEnthrallDoAfterEvent(EntityUid uid, ShadowlingComponent shadowling, ref EnthrallDoAfterEvent ev)
    {
        if (ev.Target is not { } target)
            return;

        if (ev.Cancelled)
            return;

        _popup.PopupEntity("Ваш разум поглощён тенями", target, target);
        _popup.PopupEntity("Вы стали чуть сильнее", ev.User, ev.User);
        _stamina.TakeStaminaDamage(target, 100);

        _shadowling.Enthrall(target, uid);

        var announcementString = "Станция, говорит Центральное Командование. Сканерами дальнего действия обнаружена большая концентрация психической блюспейс-энергии. Событие вознесения тенеморфов неизбежно. Предотвратите это любой ценой!";
        _chat.DispatchGlobalAnnouncement(announcementString, colorOverride: Color.FromName("red"));
    }
}
