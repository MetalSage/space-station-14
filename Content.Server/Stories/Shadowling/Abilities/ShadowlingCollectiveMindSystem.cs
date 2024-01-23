using System.Linq;
using Content.Server.Actions;
using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Content.Shared.Chat;
using Content.Shared.Stories.Shadowling;
using Robust.Server.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Stories.Shadowling;

public sealed class ShadowlingCollectiveMindSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private const string BlindnessSmokeAction = "ActionShadowlingBlindnessSmoke"; // 3 thralls
    private const string DrainThrallsAction = "ActionShadowlingDrainThralls"; // 5 thralls
    private const string SonicScreechAction = "ActionShadowlingSonicScreech"; // 7 thralls
    private const string BlackRecuperationAction = "ActionShadowlingBlackRecuperation"; // 9 thralls
    private const string AscendanceAction = "ActionShadowlingAscendance"; // 15 thralls

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
        if (!_player.TryGetSessionByEntity(uid, out var session))
            return;

        var thralls = _shadowling.GetThralls().ToArray();
        var thrallsCount = thralls.Length;

        foreach (var thrall in thralls)
        {
            _popup.PopupEntity("Вы чувствуете как нити теней пронизывают вашу душу", thrall);
        }

        List<string> newActions = new();

        if (thrallsCount >= 3 && !component.GrantedActions.Contains(BlindnessSmokeAction))
        {
            newActions.Add(BlindnessSmokeAction);
        }

        if (thrallsCount >= 5 && !component.GrantedActions.Contains(DrainThrallsAction))
        {
            newActions.Add(DrainThrallsAction);
        }

        if (thrallsCount >= 7 && !component.GrantedActions.Contains(SonicScreechAction))
        {
            newActions.Add(SonicScreechAction);
        }

        if (thrallsCount >= 9 && !component.GrantedActions.Contains(BlackRecuperationAction))
        {
            newActions.Add(BlackRecuperationAction);
        }

        if (thrallsCount >= 15 && !component.GrantedActions.Contains(AscendanceAction))
        {
            newActions.Add(AscendanceAction);
        }

        if (newActions.Count > 0)
        {
            _chat.ChatMessageToOne(
                ChatChannel.Server,
                "Были разблокированы новые способности!",
                "Были разблокированы новые способности!",
                default,
                false,
                session.Channel
            );
            foreach (var action in newActions)
            {
                if (!_prototype.TryIndex(action, out var actionPrototype))
                    continue;

                _actions.AddAction(uid, action);

                var message =
                    $"Была разблокирована способность {Loc.GetString(actionPrototype.Name)}. {Loc.GetString(actionPrototype.Description)}";
                _chat.ChatMessageToOne(ChatChannel.Unspecified, message, message, default, false, session.Channel);
            }

            _popup.PopupEntity($"У вас {thrallsCount} живых порабощённых. Новые способности разблокированы!", uid);
        }
        else
        {
            _popup.PopupEntity($"У вас {thrallsCount} живых порабощённых", uid, uid);
        }

        Dirty(uid, component);
    }
}
