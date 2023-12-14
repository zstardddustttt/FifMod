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
    }
}