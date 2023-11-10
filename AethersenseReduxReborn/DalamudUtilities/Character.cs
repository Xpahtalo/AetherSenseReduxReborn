using Dalamud.Game.ClientState.Objects.Types;

namespace AethersenseReduxReborn.DalamudUtilities;

public static class CharacterExtensions
{
    public static Job Job(this Character character)
    {
        var jobId = character.ClassJob.Id;
        if (jobId < JobExtensions.DefinedJobCount){
            return (Job)jobId;
        }
        return DalamudUtilities.Job.Unknown;
    }
}
