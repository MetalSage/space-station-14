namespace Content.Shared.SpaceStories.Shadowling;
public abstract class SharedShadowlingSystem : EntitySystem
{
    public bool IsShadowlingSlave(EntityUid uid)
    {
        return HasComp<ShadowlingThrallComponent>(uid);
    }

    public void SetStage(EntityUid uid, ShadowlingComponent component, ShadowlingStage stage)
    {
        component.Stage = stage;
        Dirty(uid, component);
    }
}
