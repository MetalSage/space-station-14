using Content.Shared.Actions;
using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.SpaceStories.Shadowling;
[RegisterComponent]
public sealed partial class ShadowlingComponent : Component
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    /// <summary>
    /// Связанные рабы
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField("slaves")]
    public List<EntityUid> Slaves = new();

    /// <summary>
    /// Активен ли теневой шаг
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField("inShadowWalk")]
    public bool InShadowWalk = false;

    /// <summary>
    /// Когда теневой шаг окончится
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField("shadowWalkEndsAt", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan ShadowWalkEndsAt = TimeSpan.Zero;

    /// <summary>
    /// Через сколько теневой шаг окончится отсчитывая от активации
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField("shadowWalkEndsIn", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan ShadowWalkEndsIn = TimeSpan.FromSeconds(3);

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("icyVeinsReagentId", customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string IcyVeinsReagentId = "IceOil";

    /// <summary>
    /// Какая связь у существа с силой.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("stage")]
    public ShadowlingStage Stage
    {
        get => _stage;
        set
        {
            _stage = value;
            Actions.TryGetValue(value, out var toGrant);
            var ev = new ShadowlingStageChangeEvent(Owner, value, toGrant);
            _entityManager.EventBus.RaiseLocalEvent(Owner, ref ev, true);
        }
    }
    private ShadowlingStage _stage = ShadowlingStage.Beginning;

    [ViewVariables(VVAccess.ReadWrite), DataField("enthrallablePrototypes", customTypeSerializer: typeof(PrototypeIdListSerializer<BodyPrototype>))]
    public List<string> EnthrallablePrototypes = new()
    {
        "Arachnid",
        "Diona",
        "Dwarf",
        "Human",
        "Moth",
        "Reptilian",
        "Slime"
    };

    [ViewVariables(VVAccess.ReadWrite), DataField("veilBlacklist", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> VeilBlacklist = new()
    {
        "APCConstructed",
        "APCBasic",
        "APCHighCapacity",
        "APCHyperCapacity",
        "APCSuperCapacity"
    };

    /// <summary>
    /// Способности у существа в зависимости от типа силы.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("actions")]
    public Dictionary<ShadowlingStage, List<string>> Actions = new()
    {
        {
            ShadowlingStage.Beginning,
            new() {
                "ActionShadowlingHatch" // Раскрыться
            }
        },
        {
            ShadowlingStage.Start,
            new() {
                "ActionShadowlingEnthrall", // Поработить
                "ActionShadowlingGlare", // Вспышка
                "ActionShadowlingVeil", // No more lights (TM)
                "ActionShadowlingShadowWalk", // Шаг тени
                "ActionShadowlingIcyVeins", // Стынущие жилы, вводит ледяное масло
                "ActionShadowlingCollectiveMind", // Узнать количество рабов, если их достаточно, то повысить уровень
                "ActionShadowlingRapidReHatch", // Позволяет тенеморфу восстановится
                // "ActionShadowlingDestroyEngines", // Принести в жертву члена экипажа для задержания эвака на 10 минут
            }
        },
        {
            ShadowlingStage.Basic,
            new () {
                "ActionShadowlingEnthrall", // Поработить
                "ActionShadowlingGlare", // Вспышка
                "ActionShadowlingVeil", // No more lights (TM)
                "ActionShadowlingShadowWalk", // Шаг тени
                "ActionShadowlingIcyVeins", // Стынущие жилы, вводит ледяное масло
                "ActionShadowlingCollectiveMind", // Узнать количество рабов, если их достаточно, то повысить уровень
                "ActionShadowlingRapidReHatch", // Позволяет тенеморфу восстановится
                // "ActionShadowlingDestroyEngines", // Принести в жертву члена экипажа для задержания эвака на 10 минут
                "ActionShadowlingSonicScreech", // Оглушает всех в округе, выбивает окна
            }
        },
        {
            ShadowlingStage.Medium,
            new () {
                "ActionShadowlingEnthrall", // Поработить
                "ActionShadowlingGlare", // Вспышка
                "ActionShadowlingVeil", // No more lights (TM)
                "ActionShadowlingShadowWalk", // Шаг тени
                "ActionShadowlingIcyVeins", // Стынущие жилы, вводит ледяное масло
                "ActionShadowlingCollectiveMind", // Узнать количество рабов, если их достаточно, то повысить уровень
                "ActionShadowlingRapidReHatch", // Позволяет тенеморфу восстановится
                // "ActionShadowlingDestroyEngines", // Принести в жертву члена экипажа для задержания эвака на 10 минут
                "ActionShadowlingSonicScreech", // Оглушает всех в округе, выбивает окна
                "ActionShadowlingBlindnessSmoke", // Непроглядный чёрный дым, позволяет тенеморфам и треллам регенировать
            }
        },
        {
            ShadowlingStage.High,
            new() {
                "ActionShadowlingEnthrall", // Поработить
                "ActionShadowlingGlare", // Вспышка
                "ActionShadowlingVeil", // No more lights (TM)
                "ActionShadowlingShadowWalk", // Шаг тени
                "ActionShadowlingIcyVeins", // Стынущие жилы, вводит ледяное масло
                "ActionShadowlingCollectiveMind", // Узнать количество рабов, если их достаточно, то повысить уровень
                "ActionShadowlingRapidReHatch", // Позволяет тенеморфу восстановится
                // "ActionShadowlingDestroyEngines", // Принести в жертву члена экипажа для задержания эвака на 10 минут
                "ActionShadowlingSonicScreech", // Оглушает всех в округе, выбивает окна
                "ActionShadowlingBlindnessSmoke", // Непроглядный чёрный дым, позволяет тенеморфам и треллам регенировать
                "ActionShadowlingBlackRecuperation", // Чёрная медицина, воскрешает раба, если тот в крите/мёртв, делает живого раба низшим тенелингом
            }
        },
        {
            ShadowlingStage.Final,
            new() {
                "ActionShadowlingEnthrall", // Поработить
                "ActionShadowlingGlare", // Вспышка
                "ActionShadowlingVeil", // No more lights (TM)
                "ActionShadowlingShadowWalk", // Шаг тени
                "ActionShadowlingIcyVeins", // Стынущие жилы, вводит ледяное масло
                "ActionShadowlingCollectiveMind", // Узнать количество рабов, если их достаточно, то повысить уровень
                "ActionShadowlingRapidReHatch", // Позволяет тенеморфу восстановится
                // "ActionShadowlingDestroyEngines", // Принести в жертву члена экипажа для задержания эвака на 10 минут
                "ActionShadowlingSonicScreech", // Оглушает всех в округе, выбивает окна
                "ActionShadowlingBlindnessSmoke", // Непроглядный чёрный дым, позволяет тенеморфам и треллам регенировать
                "ActionShadowlingBlackRecuperation", // Чёрная медицина, воскрешает раба, если тот в крите/мёртв, делает живого раба низшим тенелингом
                "ActionShadowlingAscendance" // Стань богом, уничтожь всё живое
            }
        },
        {
            ShadowlingStage.Ascended,
            new() {
                "ActionShadowlingHypnosis", // Гипноз
                "ActionShadowlingGlare", // Вспышка
                "ActionShadowlingVeil", // No more lights (TM)
                "ActionShadowlingPlaneShift", // Смещение, теневой шаг, но без ограничений
                "ActionShadowlingIcyVeins", // Стынущие жилы, вводит ледяное масло
                "ActionShadowlingCollectiveMind", // Узнать количество рабов, если их достаточно, то повысить уровень
                "ActionShadowlingRapidReHatch", // Позволяет тенеморфу восстановится
                // "ActionShadowlingDestroyEngines", // Принести в жертву члена экипажа для задержания эвака на 10 минут
                "ActionShadowlingSonicScreech", // Оглушает всех в округе, выбивает окна
                "ActionShadowlingBlindnessSmoke", // Непроглядный чёрный дым, позволяет тенеморфам и треллам регенировать
                "ActionShadowlingBlackRecuperation", // Чёрная медицина, воскрешает раба, если тот в крите/мёртв, делает живого раба низшим тенелингом
                "ActionShadowlingAnnihilate", // Вы стали богом, вы можете заставить разорваться членов экипажа просто силой мысли
                "ActionShadowlingLightningStorm", // Грозовой шторм, уничтожьте электричеством всё в округе
            }
        },
        {
            ShadowlingStage.Lower,
            new() {
                "ActionShadowlingShadowWalk", // Теневой шаг, прямо как у истинного тенелинга
            }
        }
    };

    /// <summary>
    /// Способности данные тенеморфу
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("grantedActions")]
    public List<EntityUid> GrantedActions = new();
}

public enum ShadowlingStage : byte
{
    Lower, // Slave with a little piece of power of a true shadowling

    Beginning, // You can only hatch yourself
    Start, // Almost all common powers
    Basic, // Starting shadowling but with 3 slaves
    Medium, // Basic shadowling but with 5 slaves
    High, // Medium shadowling but with 9 slaves
    Final, // You're able to become an ascended (requires 15 slaves)
    Ascended, // Absolute power!
}

[ByRefEvent]
public readonly record struct ShadowlingStageChangeEvent(EntityUid Uid, ShadowlingStage ShadowlingStage, List<string>? NewActions);

public sealed partial class ShadowlingHatchEvent : InstantActionEvent
{
}

/// <summary>
/// Is relayed at the end of the sericulturing doafter.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class ShadowlingHatchDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed partial class ShadowlingEnthrallEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingHypnosisEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingGlareEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingVeilEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingShadowWalkEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingIcyVeinsEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingCollectiveMindEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingRapidReHatchEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingDestroyEnginesEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingSonicScreechEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingBlindnessSmokeEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingBlackRecuperationEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingAnnihilateEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingLightningStormEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingPlaneShiftEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingGuiseEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingAscendanceEvent : InstantActionEvent
{
}
// <summary>
/// Is relayed at the end of the sericulturing doafter.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class ShadowlingAscendanceDoAfterEvent : SimpleDoAfterEvent
{
}
