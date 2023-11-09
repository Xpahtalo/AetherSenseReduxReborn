namespace AethersenseReduxReborn.Signals.Configs;

public enum CombineType
{
    Average,
    Max,
    Minimum,
}

public static class CombineTypeExtensions
{
    public static string DisplayName(this CombineType combineType) =>
        combineType switch {
            CombineType.Average => "Average",
            CombineType.Max     => "Max",
            CombineType.Minimum => "Minimum",
            _                   => "Unknown",
        };
}
