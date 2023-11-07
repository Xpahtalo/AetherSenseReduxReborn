namespace AethersenseReduxReborn.Signals.Configs;

public class SimplePatternConfig
{
    public SimplePatternType PatternType   { get; set; }
    public int               TotalDuration { get; set; }
    public int               Duration1     { get; set; }
    public int               Duration2     { get; set; }
    public double            Intensity1    { get; set; }
    public double            Intensity2    { get; set; }

    public static SimplePatternConfig DefaultConstantPattern() =>
        new() {
            TotalDuration = 250,
            Intensity1    = 1,
        };
}
