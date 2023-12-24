using Content.Shared.Actions.Events; 
using Content.Shared.Abilities.SpafHeal; 
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.Chat.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using System;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server.Abilities.SpafHeal;

public sealed class SpafHealSystem : SharedSpafHealSystem // Creating a system for the operation of the button
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafHealComponent, SpafHealEvent>(OnSpafHeal); // Tracking events
    }


    private void OnSpafHeal(EntityUid uid, SpafHealComponent component, SpafHealEvent args) // Creating an action that the object will perform after registering the event. In this case, we create an Heal
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        // Make sure the hunger doesn't go into the negatives
        if (hunger.CurrentHunger < component.HungerPerSpafHeal)
        {
            _popup.PopupEntity(Loc.GetString("your-pathetic-appearance-needs-more-food"), uid, uid); // We inform you that there is not enough food to perform the action
            return;
        }
        args.Handled = true;
        _hunger.ModifyHunger(uid, -component.HungerPerSpafHeal, hunger); // Taking away food

        _audio.PlayPvs("/Audio/Effects/Fluids/blood2.ogg", uid, AudioParams.Default.WithVariation(0.2f).WithVolume(-4f));
        var child = Spawn(component.TransMobSpawnId, Transform(uid).Coordinates); // Creating an Heal. It will automatically create a guest role, but there is no need to prescribe it here
    }
}
