using Content.Server.Chat.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Inventory;
using Content.Server.Mind;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Stunnable;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.SpaceStories.Shadowling;
using Content.Shared.Standing;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server.SpaceStories.Shadowling;

public sealed class ShadowlingAscendanceSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public readonly string AscendedPrototype = "MobAscendance";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAscendanceEvent>(OnAscendance);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAscendanceDoAfterEvent>(OnAscendanceDoAfter);
    }

    private void OnAscendance(EntityUid uid, ShadowlingComponent component, ref ShadowlingAscendanceEvent ev)
    {
        if (!TryComp<TransformComponent>(uid, out var transform))
            return;

        ev.Handled = true;

        var solution = new Solution();
        solution.AddReagent("ShadowlingSmokeReagent", 300);

        var smokeEnt = Spawn("Smoke", transform.Coordinates);
        _smoke.StartSmoke(smokeEnt, solution, 30, 12);
        _standing.Down(uid);

        _stun.TryStun(uid, TimeSpan.FromSeconds(30), true);

        var slots = _inventory.GetSlotEnumerator(uid, SlotFlags.All);
        while (slots.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } contained) continue;

            _transform.DropNextTo(contained, uid);
        }
        var doAfter = new DoAfterArgs(EntityManager, uid, 30, new ShadowlingAscendanceDoAfterEvent(), uid)
        {
            RequireCanInteract = false,
            BlockDuplicate = true,
        };
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnAscendanceDoAfter(EntityUid uid, ShadowlingComponent component, ref ShadowlingAscendanceDoAfterEvent ev)
    {
        _standing.Stand(uid);

        if (ev.Cancelled)
            return;

        var oldMeta = MetaData(uid);

        var newUid = Spawn(AscendedPrototype, _transform.GetMapCoordinates(Transform(uid)));
        var newShadowling = Comp<ShadowlingComponent>(newUid);
        _meta.SetEntityName(newUid, oldMeta.EntityName);
        _shadowling.SetStage(newUid, newShadowling, ShadowlingStage.Ascended);

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            _mind.TransferTo(mindId, newUid, mind: mind);

        QueueDel(uid);

        var announcementString = "Тени одержали вверх, тенеморф превознёсся, вас уже ничего не спасёт...";
        _chat.DispatchGlobalAnnouncement(announcementString, "Неизвестный голос", false, colorOverride: Color.FromName("red"));
        var centComAnnouncementString = "Станция, говорит Центральное Командование. Сканерами дальнего действия было зафиксировано превознесение тенеморфа. К вам будет направлен экстренный эвакуационный шаттл. Станция, держитесь!";
        _chat.DispatchGlobalAnnouncement(centComAnnouncementString);

        var audioParams = new AudioParams(-5f, 1, "Master", SharedAudioSystem.DefaultSoundRange, 1, 1, false, 0f);
        _audio.PlayGlobal("/Audio/Stories/Misc/tear_of_veil.ogg", Filter.Broadcast(), true, audioParams);
        _roundEnd.RequestRoundEnd(TimeSpan.FromMinutes(3), newUid, false);
    }
}
