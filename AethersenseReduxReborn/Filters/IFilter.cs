using Dalamud.Game.ClientState.Objects.Types;

namespace AethersenseReduxReborn.Filters;

public interface IFilter
{
    public bool Passes(GameObject gameObject);
}
