using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Content.Shared.Tag;
using Content.Server.Shuttles.Systems;
using Content.Shared.Stories.ConsoleLock;

namespace Content.Server.Stories.FTLKey
{
    public sealed class FTLAccessConsoleSystem : EntitySystem
    {
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly ShuttleConsoleSystem _shuttleConsoleSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<FTLAccessConsoleComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<FTLAccessConsoleComponent, ComponentRemove>(OnComponentRemove);

            SubscribeLocalEvent<FTLAccessConsoleComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
            SubscribeLocalEvent<FTLAccessConsoleComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
            SubscribeLocalEvent<FTLAccessConsoleComponent, AnchorStateChangedEvent>(OnAnchorChange);

            //Locking
            SubscribeLocalEvent<FTLAccessConsoleComponent, ConsoleLockToggledEvent>(OnLockToggled);
        }

        private void OnComponentInit(EntityUid uid, FTLAccessConsoleComponent consl, ComponentInit args)
        {
            if (consl.KeyA != null)
                consl.FTLKeySlotA.StartingItem = consl.KeyA;
            if (consl.KeyB != null)
                consl.FTLKeySlotB.StartingItem = consl.KeyB;

            _itemSlotsSystem.AddItemSlot(uid, FTLAccessConsoleComponent.ConsoleFTLKeySlotA, consl.FTLKeySlotA);
            _itemSlotsSystem.AddItemSlot(uid, FTLAccessConsoleComponent.ConsoleFTLKeySlotB, consl.FTLKeySlotB);

            UpdateAccess(uid, consl);
        }

        private void OnComponentRemove(EntityUid uid, FTLAccessConsoleComponent consl, ComponentRemove args)
        {
            RemoveAccess(uid, consl);

            _itemSlotsSystem.RemoveItemSlot(uid, consl.FTLKeySlotA);
            _itemSlotsSystem.RemoveItemSlot(uid, consl.FTLKeySlotB);
        }

        private void OnItemInserted(EntityUid uid, FTLAccessConsoleComponent consl, EntInsertedIntoContainerMessage args)
        {
            UpdateAccess(uid, consl);
        }

        private void OnItemRemoved(EntityUid uid, FTLAccessConsoleComponent consl, EntRemovedFromContainerMessage args)
        {
            UpdateAccess(uid, consl, args.Entity);
        }

        private void OnAnchorChange(EntityUid uid, FTLAccessConsoleComponent consl, AnchorStateChangedEvent args)
        {
            if (args.Anchored) UpdateAccess(uid, consl);
            else RemoveAccess(uid, consl);
        }

        private void OnLockToggled(EntityUid uid, FTLAccessConsoleComponent consl, ConsoleLockToggledEvent args)
        {
            if (args.Locked)
            {
                _itemSlotsSystem.SetLock(uid, consl.FTLKeySlotA, true);
                _itemSlotsSystem.SetLock(uid, consl.FTLKeySlotB, true);
            }
            else
            {
                _itemSlotsSystem.SetLock(uid, consl.FTLKeySlotA, false);
                _itemSlotsSystem.SetLock(uid, consl.FTLKeySlotB, false);
            }
        }


        private void UpdateAccess(EntityUid uid, FTLAccessConsoleComponent consl, EntityUid? removing = null)
        {
            if (consl.FTLKeySlotA.ContainerSlot is not null && consl.FTLKeySlotA.ContainerSlot.ContainedEntity is not null)
                consl.FTLKeyA = (EntityUid) consl.FTLKeySlotA.ContainerSlot.ContainedEntity;
            else consl.FTLKeyA = EntityUid.Invalid;
            if (consl.FTLKeySlotB.ContainerSlot is not null && consl.FTLKeySlotB.ContainerSlot.ContainedEntity is not null)
                consl.FTLKeyB = (EntityUid) consl.FTLKeySlotB.ContainerSlot.ContainedEntity;
            else consl.FTLKeyB = EntityUid.Invalid;

            RemoveAccess(uid, consl);


            // Getting tag component of shuttle
            var trans = _entityManager.GetComponent<TransformComponent>(uid);

            if (trans.GridUid is null)
            {
                return;
            }

            var tagComponent = EnsureComp<TagComponent>((EntityUid) trans.GridUid);

            if (tagComponent is null) return;

            if (consl.FTLKeyA != EntityUid.Invalid)
                AddFTLTags(tagComponent, _entityManager.GetComponent<FTLKeyComponent>(consl.FTLKeyA));
            if (consl.FTLKeyB != EntityUid.Invalid)
                AddFTLTags(tagComponent, _entityManager.GetComponent<FTLKeyComponent>(consl.FTLKeyB));

            UpdateConsole(uid);
        }

        private void RemoveAccess(EntityUid uid, FTLAccessConsoleComponent consl)
        {
            var trans = _entityManager.GetComponent<TransformComponent>(uid);
            if (trans.GridUid is null)
            {
                return;
            }

            // Remake Tag Component
            RemComp<TagComponent>((EntityUid) trans.GridUid);
            EnsureComp<TagComponent>((EntityUid) trans.GridUid);
        }

        private void AddFTLTags(TagComponent tagComponent, FTLKeyComponent keycomp)
        {
            if (keycomp.FTLKeys is null) return;
            _tagSystem.AddTags(tagComponent, keycomp.FTLKeys);
        }

        private void RemoveFTLTags(TagComponent tagComponent, FTLKeyComponent keycomp)
        {
            if (keycomp.FTLKeys is null) return;
            _tagSystem.RemoveTags(tagComponent, keycomp.FTLKeys);
        }


        private TagComponent? GetTagComp(EntityUid uid)
        {
            var trans = _entityManager.GetComponent<TransformComponent>(uid);
            if (trans.GridUid is null)
            {
                return null;
            }
            return EnsureComp<TagComponent>((EntityUid) trans.GridUid);
        }

        private void UpdateConsole(EntityUid uid)
        {
            _shuttleConsoleSystem.RefreshShuttleConsoles(uid);
        }
    }
}
