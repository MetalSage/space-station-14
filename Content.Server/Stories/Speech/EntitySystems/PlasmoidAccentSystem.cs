using Content.Server.Speech;
using Content.Server.Stories.Speech.Components;
using Robust.Shared.Random;
using System.Text.RegularExpressions;

namespace Content.Server.Stories.Speech.EntitySystems;

public sealed class PlasmoidAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlasmoidAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, PlasmoidAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // Б - П
        message = Regex.Replace(message, "Б", "П");

        // б - п
        message = Regex.Replace(message, "б", "п");

        // К - Х
        message = Regex.Replace(message, "К", "Х");

        // к - х
        message = Regex.Replace(message, "к", "х");

        // Г - Х
        message = Regex.Replace(message, "Г", "Х");

        // г - х
        message = Regex.Replace(message, "г", "х");

        // В - Ф
        message = Regex.Replace(message, "В", "Ф");

        // в - ф
        message = Regex.Replace(message, "в", "ф");

        // Д - Т
        message = Regex.Replace(message, "Д", "Т");

        // д - т
        message = Regex.Replace(message, "д", "т");

        // Ж - Ш
        message = Regex.Replace(message, "Ж", "Ш");

        // ж - ш
        message = Regex.Replace(message, "ж", "ш");

        args.Message = message;
    }
}

