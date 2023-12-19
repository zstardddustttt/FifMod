using System.Collections.Generic;

namespace FifMod.Base
{
    public readonly struct FifModRarity
    {
        public readonly int experimentation = 0;
        public readonly int assurance = 0;
        public readonly int vow = 0;
        public readonly int offense = 0;
        public readonly int march = 0;
        public readonly int rend = 0;
        public readonly int dine = 0;
        public readonly int titan = 0;

        public FifModRarity(params (MoonFlags flags, int value)[] rarities)
        {
            foreach (var (flags, value) in rarities)
            {
                if (flags.HasFlag(MoonFlags.Experimentation))
                    experimentation = value;
                if (flags.HasFlag(MoonFlags.Assurance))
                    assurance = value;
                if (flags.HasFlag(MoonFlags.Vow))
                    vow = value;
                if (flags.HasFlag(MoonFlags.Offense))
                    offense = value;
                if (flags.HasFlag(MoonFlags.March))
                    march = value;
                if (flags.HasFlag(MoonFlags.Rend))
                    rend = value;
                if (flags.HasFlag(MoonFlags.Dine))
                    dine = value;
                if (flags.HasFlag(MoonFlags.Titan))
                    titan = value;
            }
        }

        public int[] GetRarityOfFlags(MoonFlags flags)
        {
            var output = new List<int>();

            if (flags.HasFlag(MoonFlags.Experimentation))
                output.Add(experimentation);
            if (flags.HasFlag(MoonFlags.Assurance))
                output.Add(assurance);
            if (flags.HasFlag(MoonFlags.Vow))
                output.Add(vow);
            if (flags.HasFlag(MoonFlags.Offense))
                output.Add(offense);
            if (flags.HasFlag(MoonFlags.March))
                output.Add(march);
            if (flags.HasFlag(MoonFlags.Rend))
                output.Add(rend);
            if (flags.HasFlag(MoonFlags.Dine))
                output.Add(dine);
            if (flags.HasFlag(MoonFlags.Titan))
                output.Add(titan);

            return output.ToArray();
        }

        public static FifModRarity All(int rarity)
        {
            return new((MoonFlags.All, rarity));
        }

        public static FifModRarity All(float rarity)
        {
            return new((MoonFlags.All, (int)rarity));
        }
    }
}