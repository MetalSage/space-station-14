using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Rotation;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.Standing
{
    public sealed class StandingStateSystem : EntitySystem
    {
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!; // Stories-Crawling

        // If StandingCollisionLayer value is ever changed to more than one layer, the logic needs to be edited.
        private const int StandingCollisionLayer = (int) CollisionGroup.MidImpassable;

        // Stories-Crawling-Start
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<StandingStateComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiersEvent);
            SubscribeLocalEvent<StandingStateComponent, DownDoAfterEvent>(OnDownDoAfterEvent);
            SubscribeLocalEvent<StandingStateComponent, StandDoAfterEvent>(OnStandDoAfterEvent);
        }
        // Stories-Crawling-End

        public bool IsDown(EntityUid uid, StandingStateComponent? standingState = null)
        {
            if (!Resolve(uid, ref standingState, false))
                return false;

            return !standingState.Standing;
        }

        // Stories-Crawling-Start
        public bool CanCrawl(EntityUid uid, StandingStateComponent? standingState = null)
        {
            if (!Resolve(uid, ref standingState, false))
                return false;

            return standingState.CanCrawl;
        }
        // Stories-Crawling-End

        public bool Down(EntityUid uid, bool playSound = true, bool dropHeldItems = true, bool canStandUp = true,
            StandingStateComponent? standingState = null,
            AppearanceComponent? appearance = null,
            HandsComponent? hands = null)
        {
            // TODO: This should actually log missing comps...
            if (!Resolve(uid, ref standingState, false))
                return false;

            // Optional component.
            Resolve(uid, ref appearance, ref hands, false);

            if (!standingState.Standing)
                return true;

            // This is just to avoid most callers doing this manually saving boilerplate
            // 99% of the time you'll want to drop items but in some scenarios (e.g. buckling) you don't want to.
            // We do this BEFORE downing because something like buckle may be blocking downing but we want to drop hand items anyway
            // and ultimately this is just to avoid boilerplate in Down callers + keep their behavior consistent.
            if (dropHeldItems && hands != null)
            {
                RaiseLocalEvent(uid, new DropHandItemsEvent(), false);
            }

            var msg = new DownAttemptEvent();
            RaiseLocalEvent(uid, msg, false);

            if (msg.Cancelled)
                return false;

            standingState.Standing = false;
            standingState.CanStandUp = canStandUp;
            Dirty(uid, standingState); // Stories-Crawling
            RaiseLocalEvent(uid, new DownedEvent(), false);
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid); // Stories-Crawling

            // Seemed like the best place to put it
            _appearance.SetData(uid, RotationVisuals.RotationState, RotationState.Horizontal, appearance);

            // Change collision masks to allow going under certain entities like flaps and tables
            if (TryComp(uid, out FixturesComponent? fixtureComponent))
            {
                foreach (var (key, fixture) in fixtureComponent.Fixtures)
                {
                    if ((fixture.CollisionMask & StandingCollisionLayer) == 0)
                        continue;

                    standingState.ChangedFixtures.Add(key);
                    _physics.SetCollisionMask(uid, key, fixture, fixture.CollisionMask & ~StandingCollisionLayer, manager: fixtureComponent);
                }
            }

            // check if component was just added or streamed to client
            // if true, no need to play sound - mob was down before player could seen that
            if (standingState.LifeStage <= ComponentLifeStage.Starting)
                return true;

            if (playSound)
            {
                _audio.PlayPredicted(standingState.DownSound, uid, uid);
            }

            return true;
        }

        public bool Stand(EntityUid uid,
            StandingStateComponent? standingState = null,
            AppearanceComponent? appearance = null,
            bool force = false)
        {
            // TODO: This should actually log missing comps...
            if (!Resolve(uid, ref standingState, false))
                return false;

            // Optional component.
            Resolve(uid, ref appearance, false);

            if (standingState.Standing)
                return true;

            if (!force)
            {
                var msg = new StandAttemptEvent();
                RaiseLocalEvent(uid, msg, false);

                if (msg.Cancelled)
                    return false;
            }

            standingState.Standing = true;
            Dirty(uid, standingState);
            RaiseLocalEvent(uid, new StoodEvent(), false);
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid); // Stories-Crawling

            _appearance.SetData(uid, RotationVisuals.RotationState, RotationState.Vertical, appearance);

            if (TryComp(uid, out FixturesComponent? fixtureComponent))
            {
                foreach (var key in standingState.ChangedFixtures)
                {
                    if (fixtureComponent.Fixtures.TryGetValue(key, out var fixture))
                        _physics.SetCollisionMask(uid, key, fixture, fixture.CollisionMask | StandingCollisionLayer, fixtureComponent);
                }
            }
            standingState.ChangedFixtures.Clear();

            return true;
        }

        // Stories-Crawling-Start
        private void OnStandDoAfterEvent(EntityUid uid, StandingStateComponent standing, ref StandDoAfterEvent ev)
        {
            if (ev.Cancelled)
                return;

            Stand(uid, standingState: standing);
        }

        private void OnDownDoAfterEvent(EntityUid uid, StandingStateComponent standing, ref DownDoAfterEvent ev)
        {
            if (ev.Cancelled)
                return;

            Down(uid, standingState: standing);
        }

        private void OnRefreshMovementSpeedModifiersEvent(EntityUid uid, StandingStateComponent standing, ref RefreshMovementSpeedModifiersEvent ev)
        {
            if (standing.Standing)
                return;

            ev.ModifySpeed(standing.CrawlingSpeedModifier, standing.CrawlingSpeedModifier);
        }
        // Stories-Crawling-End
    }

    public sealed class DropHandItemsEvent : EventArgs
    {
    }

    /// <summary>
    /// Subscribe if you can potentially block a down attempt.
    /// </summary>
    public sealed class DownAttemptEvent : CancellableEntityEventArgs
    {
    }

    /// <summary>
    /// Subscribe if you can potentially block a stand attempt.
    /// </summary>
    public sealed class StandAttemptEvent : CancellableEntityEventArgs
    {
    }

    /// <summary>
    /// Raised when an entity becomes standing
    /// </summary>
    public sealed class StoodEvent : EntityEventArgs
    {
    }

    /// <summary>
    /// Raised when an entity is not standing
    /// </summary>
    public sealed class DownedEvent : EntityEventArgs
    {
    }

    // Stories-Crawling-Start
    [Serializable, NetSerializable]
    public sealed partial class DownDoAfterEvent : SimpleDoAfterEvent
    {
    }

    [Serializable, NetSerializable]
    public sealed partial class StandDoAfterEvent : SimpleDoAfterEvent
    {
    }
    // Stories-Crawling-End
}
