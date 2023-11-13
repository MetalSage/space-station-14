using Content.Shared.Actions.Events; //my
using Content.Shared.Abilities.SpafFood; //my
using Content.Server.Emp;
using Content.Server.Ninja.Events;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.PowerCell;
using Content.Shared.Actions;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Ninja.Components;
using Content.Shared.Ninja.Systems;
using Content.Shared.Popups;
using Content.Shared.PowerCell.Components;
using Robust.Shared.Containers;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Server.Chat.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mobs.Systems;
using Content.Shared.Dataset;
using System.Numerics;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Shared.Atmos;
using Content.Shared.Dataset;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Pointing;
using Content.Shared.RatKing;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.Abilities.SpafFood;

public sealed class SpafFoodSystem : SharedSpafFoodSystem // Creating a system for the operation of the button
{
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafFoodComponent, SpafFoodEvent>(OnSpafFood);
    }


    private void OnSpafFood(EntityUid uid, SpafFoodComponent component, SpafFoodEvent args) // creating an action to turn a person into a man
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        float myFloat = hunger.CurrentHunger;
        string myString = myFloat.ToString();
        _popup.PopupEntity(Loc.GetString(myString), uid, uid);
    }
}
