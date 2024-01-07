using Robust.Shared.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Stories.FTLKey;

[RegisterComponent]
public sealed partial class FTLAccessConsoleComponent : Component
{
    public const string ConsoleFTLKeySlotA = "ShuttleConsole-FTLKeySlotA";
    public const string ConsoleFTLKeySlotB = "ShuttleConsole-FTLKeySlotB";


    [DataField("FTLKeySlotA")]
    public ItemSlot FTLKeySlotA = new();

    [DataField("FTLKeySlotB")]
    public ItemSlot FTLKeySlotB = new();

    [DataField("KeyA", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? KeyA;

    [DataField("KeyB", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? KeyB;

    [ViewVariables] public EntityUid FTLKeyA = EntityUid.Invalid;
    [ViewVariables] public EntityUid FTLKeyB = EntityUid.Invalid;

}

