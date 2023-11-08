using Dalamud.Game.ClientState.Objects.Types;

namespace AethersenseReduxReborn.DalamudUtilities;

public static class CharacterExtensions
{
    public static Job Job(this Character character) => (Job)character.ClassJob.Id;
}
