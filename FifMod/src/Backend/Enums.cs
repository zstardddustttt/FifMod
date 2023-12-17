using System;

namespace FifMod
{
    [Flags]
    public enum MoonFlags
    {
        None = 1 << 0,
        Experimentation = 1 << 1,
        Assurance = 1 << 2,
        Vow = 1 << 3,
        Offense = 1 << 4,
        March = 1 << 5,
        Rend = 1 << 6,
        Dine = 1 << 7,
        Titan = 1 << 8,
        Abandoned = Experimentation,
        Deserted = Assurance | Offense,
        Forested = Vow | March,
        Easy = Experimentation | Assurance | Vow,
        Intermediate = Offense | March,
        Expert = Rend | Dine | Titan,
        All = Experimentation | Assurance | Vow | Offense | March | Rend | Dine | Titan
    }

    [Flags]
    public enum EnemySpawnFlags
    {
        None = 1 << 0,
        Facility = 1 << 1,
        Mansion = 1 << 2,
        Outside = 1 << 3,
        Daytime = 1 << 4,
        Default = Facility | Mansion,
        All = Facility | Mansion | Outside | Daytime
    }

    [Flags]
    public enum MapObjectSpawnFlags
    {
        None = 1 << 0,
        Facility = 1 << 1,
        Mansion = 1 << 2,
        Outside = 1 << 3,
        Default = Facility | Mansion,
        All = Facility | Mansion | Outside
    }

    [Flags]
    public enum ScrapSpawnFlags
    {
        None = 1 << 0,
        Facility = 1 << 1,
        Mansion = 1 << 2,
        All = Facility | Mansion
    }
}