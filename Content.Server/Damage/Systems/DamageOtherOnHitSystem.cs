using Content.Server.Administration.Logs;
using Content.Server.Damage.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Mobs.Components;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Server.Damage.Systems
{
    public sealed class DamageOtherOnHitSystem : EntitySystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly GunSystem _guns = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly DamageExamineSystem _damageExamine = default!;
        [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;
        [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
        [Dependency] private readonly ThrownItemSystem _thrownItem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<DamageOtherOnHitComponent, ThrowDoHitEvent>(OnDoHit);
            SubscribeLocalEvent<DamageOtherOnHitComponent, DamageExamineEvent>(OnDamageExamine);
            SubscribeLocalEvent<ItemToggleDamageOtherOnHitComponent, ItemToggledEvent>(OnItemToggle); // SpaceStories
        }

        private void OnDoHit(EntityUid uid, DamageOtherOnHitComponent component, ThrowDoHitEvent args)
        {
            var dmg = _damageable.TryChangeDamage(args.Target, component.Damage, component.IgnoreResistances, origin: args.Component.Thrower);

            // Log damage only for mobs. Useful for when people throw spears at each other, but also avoids log-spam when explosions send glass shards flying.
            if (dmg != null && HasComp<MobStateComponent>(args.Target))
                _adminLogger.Add(LogType.ThrowHit, $"{ToPrettyString(args.Target):target} received {dmg.GetTotal():damage} damage from collision");

            if (dmg is { Empty: false })
            {
                _color.RaiseEffect(Color.Red, new List<EntityUid>() { args.Target }, Filter.Pvs(args.Target, entityManager: EntityManager));
            }

            _guns.PlayImpactSound(args.Target, dmg, component.HitSound, false); // Space Stories
            if (TryComp<PhysicsComponent>(uid, out var body) && body.LinearVelocity.LengthSquared() > 0f)
            {
                var direction = body.LinearVelocity.Normalized();
                _sharedCameraRecoil.KickCamera(args.Target, direction);
            }

            // TODO: If more stuff touches this then handle it after.
            if (TryComp<PhysicsComponent>(uid, out var physics))
            {
                _thrownItem.LandComponent(args.Thrown, args.Component, physics, false);
            }
        }

        private void OnDamageExamine(EntityUid uid, DamageOtherOnHitComponent component, ref DamageExamineEvent args)
        {
            _damageExamine.AddDamageExamine(args.Message, component.Damage, Loc.GetString("damage-throw"));
        }
        private void OnItemToggle(EntityUid uid, ItemToggleDamageOtherOnHitComponent itemToggleMelee, ItemToggledEvent args) // SpaceStories
        {
            if (!TryComp(uid, out DamageOtherOnHitComponent? meleeWeapon))
                return;

            if (args.Activated)
            {
                if (itemToggleMelee.ActivatedDamage != null)
                {
                    //Setting deactivated damage to the weapon's regular value before changing it.
                    itemToggleMelee.DeactivatedDamage ??= meleeWeapon.Damage;
                    meleeWeapon.Damage = itemToggleMelee.ActivatedDamage;
                }

                meleeWeapon.HitSound = itemToggleMelee.ActivatedSoundOnHit;
            }
            else
            {
                if (itemToggleMelee.DeactivatedDamage != null)
                    meleeWeapon.Damage = itemToggleMelee.DeactivatedDamage;

                meleeWeapon.HitSound = itemToggleMelee.DeactivatedSoundOnHit;
            }
        }
    }
}
