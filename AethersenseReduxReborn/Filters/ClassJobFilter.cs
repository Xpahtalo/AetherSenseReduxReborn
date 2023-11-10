using AethersenseReduxReborn.DalamudUtilities;
using Dalamud.Game.ClientState.Objects.Types;

namespace AethersenseReduxReborn.Filters;

public class ClassJobFilter: IFilter
{
    public ClassJobFilterOptions Options { get; }

    public ClassJobFilter(ClassJobFilterOptions options) { Options = options; }

    public bool Passes(GameObject gameObject)
    {
        if (gameObject is not Character character){
            return false;
        }

        return Options switch {
            ClassJobFilterOptions.DisciplesOfWar           => character.Job().IsDiscipleOfWar(),
            ClassJobFilterOptions.DisciplesOfMagic         => character.Job().IsDiscipleOfMagic(),
            ClassJobFilterOptions.DisciplesOfWarOrMagic    => character.Job().IsDiscipleOfWarOrMagic(),
            ClassJobFilterOptions.Dps                      => character.Job().IsDps(),
            ClassJobFilterOptions.Tank                     => character.Job().IsTank(),
            ClassJobFilterOptions.Healer                   => character.Job().IsHealer(),
            ClassJobFilterOptions.DisciplesOfTheHand       => character.Job().IsDiscipleOfTheHand(),
            ClassJobFilterOptions.DisciplesOfTheLand       => character.Job().IsDiscipleOfTheLand(),
            ClassJobFilterOptions.DisciplesOfTheHandOrLand => character.Job().IsDiscipleOfTheHandOrLand(),
            ClassJobFilterOptions.LimitedJobs              => character.Job().IsLimitedJob(),
            ClassJobFilterOptions.Class                    => !character.Job().IsJob(),
            ClassJobFilterOptions.Job                      => character.Job().IsJob(),
            _                                              => false,
        };
    }
}

public enum ClassJobFilterOptions
{
    DisciplesOfWar,
    DisciplesOfMagic,
    DisciplesOfWarOrMagic,
    Dps,
    Tank,
    Healer,
    DisciplesOfTheHand,
    DisciplesOfTheLand,
    DisciplesOfTheHandOrLand,
    LimitedJobs,
    Class,
    Job,
}

public static class ClassJobFilterOptionsExtensions
{
    public static string DisplayName(this ClassJobFilterOptions options) =>
        options switch {
            ClassJobFilterOptions.DisciplesOfWar           => "Disciples of War",
            ClassJobFilterOptions.DisciplesOfMagic         => "Disciples of Magic",
            ClassJobFilterOptions.DisciplesOfWarOrMagic    => "Disciples of War or Magic",
            ClassJobFilterOptions.Dps                      => "DPS",
            ClassJobFilterOptions.Tank                     => "Tank",
            ClassJobFilterOptions.Healer                   => "Healer",
            ClassJobFilterOptions.DisciplesOfTheHand       => "Disciples of the Hand",
            ClassJobFilterOptions.DisciplesOfTheLand       => "Disciples of the Land",
            ClassJobFilterOptions.DisciplesOfTheHandOrLand => "Disciples of the Hand or Land",
            ClassJobFilterOptions.LimitedJobs              => "Limited Jobs",
            ClassJobFilterOptions.Class                    => "Class",
            ClassJobFilterOptions.Job                      => "Job",
            _                                              => "Unknown",
        };
}
