using System.Diagnostics.CodeAnalysis;

namespace AethersenseReduxReborn.DalamudUtilities;

public enum Job: uint
{
    Unknown       = 0,
    Gladiator     = 1,
    Pugilist      = 2,
    Marauder      = 3,
    Lancer        = 4,
    Archer        = 5,
    Conjurer      = 6,
    Thaumaturge   = 7,
    Carpenter     = 8,
    Blacksmith    = 9,
    Armorer       = 10,
    Goldsmith     = 11,
    Leatherworker = 12,
    Weaver        = 13,
    Alchemist     = 14,
    Culinarian    = 15,
    Miner         = 16,
    Botanist      = 17,
    Fisher        = 18,
    Paladin       = 19,
    Monk          = 20,
    Warrior       = 21,
    Dragoon       = 22,
    Bard          = 23,
    WhiteMage     = 24,
    BlackMage     = 25,
    Arcanist      = 26,
    Summoner      = 27,
    Scholar       = 28,
    Rogue         = 29,
    Ninja         = 30,
    Machinist     = 31,
    DarkKnight    = 32,
    Astrologian   = 33,
    Samurai       = 34,
    RedMage       = 35,
    BlueMage      = 36,
    Gunbreaker    = 37,
    Dancer        = 38,
    Reaper        = 39,
    Sage          = 40,
    NewJob1       = 41,
    NewJob2       = 42,
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class JobExtensions
{
    public static uint DefinedJobCount => 43;

    public static bool IsDiscipleOfTheLand(this Job job) =>
        job switch {
            Job.Miner    => true,
            Job.Botanist => true,
            Job.Fisher   => true,
            _            => false,
        };

    public static bool IsDiscipleOfTheHand(this Job job) =>
        job switch {
            Job.Carpenter     => true,
            Job.Blacksmith    => true,
            Job.Armorer       => true,
            Job.Goldsmith     => true,
            Job.Leatherworker => true,
            Job.Weaver        => true,
            Job.Alchemist     => true,
            Job.Culinarian    => true,
            _                 => false,
        };

    public static bool IsTank(this Job job) =>
        job switch {
            Job.Gladiator  => true,
            Job.Marauder   => true,
            Job.Paladin    => true,
            Job.Warrior    => true,
            Job.DarkKnight => true,
            Job.Gunbreaker => true,
            _              => false,
        };

    public static bool IsHealer(this Job job) =>
        job switch {
            Job.Conjurer    => true,
            Job.WhiteMage   => true,
            Job.Scholar     => true,
            Job.Astrologian => true,
            Job.Sage        => true,
            _               => false,
        };

    public static bool IsMeleeDps(this Job job) =>
        job switch {
            Job.Pugilist => true,
            Job.Lancer   => true,
            Job.Rogue    => true,
            Job.Monk     => true,
            Job.Dragoon  => true,
            Job.Ninja    => true,
            Job.Samurai  => true,
            Job.Reaper   => true,
            _            => false,
        };

    public static bool IsPhysicalRangedDps(this Job job) =>
        job switch {
            Job.Archer    => true,
            Job.Bard      => true,
            Job.Machinist => true,
            Job.Dancer    => true,
            _             => false,
        };

    public static bool IsMagicalRangedDps(this Job job) =>
        job switch {
            Job.Thaumaturge => true,
            Job.BlackMage   => true,
            Job.Arcanist    => true,
            Job.Summoner    => true,
            Job.RedMage     => true,
            Job.BlueMage    => true,
            _               => false,
        };

    public static bool IsDps(this Job job) => job.IsMeleeDps() || job.IsPhysicalRangedDps() || job.IsMagicalRangedDps();

    public static bool IsPhysicalDps(this Job job) => job.IsMeleeDps() || job.IsPhysicalRangedDps();

    public static bool IsDiscipleOfWar(this Job job) => job.IsTank() || job.IsMeleeDps() || job.IsPhysicalRangedDps();

    public static bool IsDiscipleOfMagic(this Job job) => job.IsHealer() || job.IsMagicalRangedDps();

    public static bool IsDiscipleOfWarOrMagic(this Job job) => job.IsDiscipleOfWar() || job.IsDiscipleOfMagic();

    public static bool IsDiscipleOfTheHandOrLand(this Job job) => job.IsDiscipleOfTheHand() || job.IsDiscipleOfTheLand();

    public static bool IsJob(this Job job) =>
        job switch {
            Job.Paladin     => true,
            Job.Monk        => true,
            Job.Warrior     => true,
            Job.Dragoon     => true,
            Job.Bard        => true,
            Job.WhiteMage   => true,
            Job.BlackMage   => true,
            Job.Summoner    => true,
            Job.Scholar     => true,
            Job.Ninja       => true,
            Job.Machinist   => true,
            Job.DarkKnight  => true,
            Job.Astrologian => true,
            Job.Samurai     => true,
            Job.RedMage     => true,
            Job.BlueMage    => true,
            Job.Gunbreaker  => true,
            Job.Dancer      => true,
            Job.Reaper      => true,
            Job.Sage        => true,
            Job.NewJob1     => true,
            Job.NewJob2     => true,
            _               => false,
        };

    public static bool IsLimitedJob(this Job job) =>
        job switch {
            Job.BlueMage => true,
            _            => false,
        };
}
