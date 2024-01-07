using Content.Shared.SpaceStories.Shadowling;
using Robust.Client.Graphics;

namespace Content.Client.SpaceStories.Shadowling;

public sealed class ShadowlingHatchSystem : EntitySystem
{
    [Dependency] private readonly ILightManager _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchDoAfterEvent>(OnHatchDoAfter);
    }

    private void OnHatchDoAfter(EntityUid uid, ShadowlingComponent component, ref ShadowlingHatchDoAfterEvent ev)
    {
        _light.DrawShadows = false;
    }
}
