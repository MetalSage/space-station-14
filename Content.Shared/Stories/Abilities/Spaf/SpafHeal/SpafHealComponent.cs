using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafHeal;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafHealSystem))]
public sealed partial class SpafHealComponent : Component
{
    [DataField("actionSpafHeal", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafHeal = "ActionSpafHeal";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafHealEntity")]
    public EntityUid? ActivateSpafHealEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafHeal", required: true)]
    public float HungerPerSpafHeal = 10f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "FoodMeatSpaf";
}
