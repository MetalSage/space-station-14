using Content.Shared.Tag;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.Stories.FTLKey;

[RegisterComponent]
public sealed partial class FTLKeyComponent : Component
{
    [DataField("access", customTypeSerializer:typeof(PrototypeIdListSerializer<TagPrototype>))]
    public List<string>? FTLKeys = new();
}
