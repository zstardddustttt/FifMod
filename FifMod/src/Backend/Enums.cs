using System;

namespace FifMod
{
    [Flags]
    public enum MoonFlags
    {
        None = 1 << 0,
        ExperimentationLevel = 1 << 1,
        AssuranceLevel = 1 << 2,
        VowLevel = 1 << 3,
        OffenseLevel = 1 << 4,
        MarchLevel = 1 << 5,
        RendLevel = 1 << 6,
        DineLevel = 1 << 7,
        TitanLevel = 1 << 8,
        All = ExperimentationLevel | AssuranceLevel | VowLevel | OffenseLevel | MarchLevel | RendLevel | DineLevel | TitanLevel
    }

    public enum SpawnMode
    {
        Facility,
        Outside,
        Daytime
    }
}