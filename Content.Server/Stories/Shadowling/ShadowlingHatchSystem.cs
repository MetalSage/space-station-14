using Content.Server.Fluids.EntitySystems;
using Content.Server.Inventory;
using Content.Server.Mind;
using Content.Server.Stunnable;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.SpaceStories.Shadowling;
using Content.Shared.Standing;
using Robust.Server.GameObjects;

namespace Content.Server.SpaceStories.Shadowling;

public sealed class ShadowlingHatchSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;

    public readonly string ShadowlingPrototype = "MobShadowling";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchEvent>(OnHatch);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchDoAfterEvent>(OnHatchDoAfter);
    }

    private void OnHatch(EntityUid uid, ShadowlingComponent component, ref ShadowlingHatchEvent ev)
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
        var doAfter = new DoAfterArgs(EntityManager, uid, 30, new ShadowlingHatchDoAfterEvent(), uid)
        {
            BlockDuplicate = true,
            RequireCanInteract = false,
        };
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnHatchDoAfter(EntityUid uid, ShadowlingComponent component, ref ShadowlingHatchDoAfterEvent ev)
    {
        _standing.Stand(uid);

        if (ev.Cancelled)
            return;

        var oldMeta = MetaData(uid);

        var newUid = Spawn(ShadowlingPrototype, _transform.GetMapCoordinates(Transform(uid)));
        var newShadowling = EnsureComp<ShadowlingComponent>(newUid);
        _meta.SetEntityName(newUid, oldMeta.EntityName);
        _shadowling.SetStage(newUid, newShadowling, ShadowlingStage.Start);

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            _mind.TransferTo(mindId, newUid, mind: mind);

        QueueDel(uid);
    }
}
