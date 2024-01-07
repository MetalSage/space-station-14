using Content.Shared.Actions;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.SpaceStories.Shadowling;
[RegisterComponent]
public sealed partial class ShadowlingThrallComponent : Component
{
    [DataField("master"), AutoNetworkedField]
    public EntityUid? Master;

    /// <summary>
    /// Когда теневой покров кончится
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField("guiseEndsAt", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan GuiseEndsAt = TimeSpan.Zero;

    /// <summary>
    /// Через сколько теневой покров окончится отсчитывая от активации
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField("guiseEndsIn", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan GuiseEndsIn = TimeSpan.FromSeconds(4);

    [ViewVariables(VVAccess.ReadWrite), DataField("actions")]
    public List<string> Actions = new()
    {
        "ActionShadowlingGuise", // Сокройте своё присутствие на короткий промежуток времени
    };

    /// <summary>
    /// Способности данные рабу
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("grantedActions")]
    public List<EntityUid> GrantedActions = new();
}

public sealed partial class ShadowlingGuiseEvent : InstantActionEvent
{
}
