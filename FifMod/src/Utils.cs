namespace FifMod.Utils
{
    public static class FifModUtils
    {
        public static string FormatKey(this string key)
        {
            return key switch
            {
                "leftButton" => "LMB",
                "rightButton" => "RMB",
                "middleButton" => "MMB",
                _ => key.ToUpper(),
            };
        }

        public static float PoundsToItemWeight(int pounds)
        {
            return (float)pounds / 105 + 1;
        }
    }
}