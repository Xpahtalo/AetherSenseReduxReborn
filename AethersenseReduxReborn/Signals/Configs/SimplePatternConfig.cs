namespace AethersenseReduxReborn.Signals.Configs;

public class SimplePatternConfig
{
    public required SimplePatternType PatternType   { get; set; }
    public required int               TotalDuration { get; set; }
    public          int               Duration1     { get; set; }
    public          int               Duration2     { get; set; }
    public          double            Intensity1    { get; set; }
    public          double            Intensity2    { get; set; }
}
