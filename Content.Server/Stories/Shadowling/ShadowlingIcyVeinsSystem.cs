using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.SpaceStories.Shadowling;

namespace Content.Server.SpaceStories.Shadowling;

public sealed class ShadowlingIcyVeinsSystem : EntitySystem
{
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly SolutionContainerSystem _solution = default!;
    [Dependency] private readonly ChemistrySystem _chemistry = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingIcyVeinsEvent>(OnIcyVeinsEvent);
    }

    private void OnIcyVeinsEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingIcyVeinsEvent ev)
    {
        ev.Handled = true;
        var bodies = _shadowling.GetEntitiesAroundShadowling<BodyComponent>(uid, 15);
        var solution = new Solution();
        solution.AddReagent(component.IcyVeinsReagentId, 10);

        foreach (var entity in bodies)
        {
            if (!_solution.TryGetInjectableSolution(entity, out var targetSolution))
                continue;

            _solution.AddSolution(entity, targetSolution, solution);
        }
    }
}
