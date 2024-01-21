using System.Linq;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using System.Numerics;
using Robust.Client.ResourceManagement;
using Robust.Client.Utility;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Graphics.RSI;
using Serilog;

namespace Content.Client.StatusIcon;

public sealed class StatusIconOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly StatusIconSystem _statusIcon;
    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    internal StatusIconOverlay()
    {
        IoCManager.InjectDependencies(this);

        _sprite = _entity.System<SpriteSystem>();
        _transform = _entity.System<TransformSystem>();
        _statusIcon = _entity.System<StatusIconSystem>();
        _shader = _prototype.Index<ShaderPrototype>("unshaded").Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        var eyeRot = args.Viewport.Eye?.Rotation ?? default;

        var xformQuery = _entity.GetEntityQuery<TransformComponent>();
        var scaleMatrix = Matrix3.CreateScale(new Vector2(1, 1));
        var rotationMatrix = Matrix3.CreateRotation(-eyeRot);

        handle.UseShader(_shader);

        var query = _entity.AllEntityQueryEnumerator<StatusIconComponent, SpriteComponent, TransformComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite, out var xform, out var meta))
        {
            if (xform.MapID != args.MapId)
                continue;

            var bounds = comp.Bounds ?? sprite.Bounds;

            var worldPos = _transform.GetWorldPosition(xform, xformQuery);

            if (!bounds.Translated(worldPos).Intersects(args.WorldAABB))
                continue;

            var icons = _statusIcon.GetStatusIcons(uid, meta);
            if (icons.Count == 0)
                continue;

            if (!_statusIcon.IsVisible(uid))
                continue;

            var worldMatrix = Matrix3.CreateTranslation(worldPos);
            Matrix3.Multiply(scaleMatrix, worldMatrix, out var scaledWorld);
            Matrix3.Multiply(rotationMatrix, scaledWorld, out var matty);
            handle.SetTransform(matty);

            var countL = 0;
            var countR = 0;
            var accOffsetL = 0;
            var accOffsetR = 0;
            icons.Sort();

            Texture? texture = null;
            foreach (var proto in icons)
            {
                switch (proto.Icon)
                {
                    case SpriteSpecifier.Rsi rsi:
                        var rsiActual = _resourceCache.GetResource<RSIResource>("/Textures/" + rsi.RsiPath.ToString()).RSI;

                        if (!rsiActual.TryGetState(rsi.RsiState, out var state))
                        {
                            return;
                        }

                        var frames = state.GetFrames(RsiDirection.South);
                        var delays = state.GetDelays();
                        var totalDelay = delays.Sum();
                        var time = _timing.RealTime.TotalSeconds % totalDelay;
                        var delaySum = 0f;

                        for (var i = 0; i < delays.Length; i++)
                        {
                            var delay = delays[i];
                            delaySum += delay;

                            if (time > delaySum)
                                continue;

                            texture = frames[i];
                            break;
                        }

                        texture ??= _sprite.Frame0(proto.Icon);
                        break;
                    case SpriteSpecifier.Texture spriteTexture:
                        texture = spriteTexture.GetTexture(_resourceCache);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                float yOffset;
                float xOffset;

                // the icons are ordered left to right, top to bottom.
                // extra icons that don't fit are just cut off.
                if (proto.LocationPreference == StatusIconLocationPreference.Left ||
                    proto.LocationPreference == StatusIconLocationPreference.None && countL <= countR)
                {
                    if (accOffsetL + texture.Height > sprite.Bounds.Height * EyeManager.PixelsPerMeter)
                        break;
                    accOffsetL += texture.Height;
                    yOffset = (bounds.Height + sprite.Offset.Y) / 2f - (float) accOffsetL / EyeManager.PixelsPerMeter;
                    xOffset = -(bounds.Width + sprite.Offset.X) / 2f;

                    countL++;
                }
                else
                {
                    if (accOffsetR + texture.Height > sprite.Bounds.Height * EyeManager.PixelsPerMeter)
                        break;
                    accOffsetR += texture.Height;
                    yOffset = (bounds.Height + sprite.Offset.Y) / 2f - (float) accOffsetR / EyeManager.PixelsPerMeter;
                    xOffset = (bounds.Width + sprite.Offset.X) / 2f - (float) texture.Width / EyeManager.PixelsPerMeter;

                    countR++;
                }

                var position = new Vector2(xOffset, yOffset);
                handle.DrawTexture(texture, position);
            }
        }

        handle.UseShader(null);
    }
}
