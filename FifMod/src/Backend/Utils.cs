using UnityEngine;

namespace FifMod.Utils
{
    public static class FifModBackendUtils
    {
        public static TerminalKeyword CreateTerminalKeyword(string word, bool isVerb = false, CompatibleNoun[] compatibleNouns = null, TerminalNode specialKeywordResult = null, TerminalKeyword defaultVerb = null, bool accessTerminalObjects = false)
        {

            TerminalKeyword keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            keyword.name = word;
            keyword.word = word;
            keyword.isVerb = isVerb;
            keyword.compatibleNouns = compatibleNouns;
            keyword.specialKeywordResult = specialKeywordResult;
            keyword.defaultVerb = defaultVerb;
            keyword.accessTerminalObjects = accessTerminalObjects;
            return keyword;
        }

        public static TerminalNode GetDefaultItemInfo(string itemName)
        {
            var output = ScriptableObject.CreateInstance<TerminalNode>();
            output.name = $"{itemName.Replace(" ", "-")}InfoNode";
            output.displayText = $"[No information about this object was found.]\n\n";
            output.clearPreviousText = true;
            output.maxCharactersToType = 25;

            return output;
        }

        public static TerminalNode GetDefaultBuyNode1(string itemName)
        {
            var output = ScriptableObject.CreateInstance<TerminalNode>();
            output.name = $"{itemName.Replace(" ", "-")}BuyNode1";
            output.displayText = GetDefaultBuyNode1Text(itemName);
            output.clearPreviousText = true;
            output.maxCharactersToType = 35;

            return output;
        }

        public static TerminalNode GetDefaultBuyNode2(string itemName)
        {
            var output = ScriptableObject.CreateInstance<TerminalNode>();
            output.name = $"{itemName.Replace(" ", "-")}BuyNode2";
            output.displayText = GetDefaultBuyNode2Text(itemName);
            output.clearPreviousText = true;
            output.maxCharactersToType = 15;

            return output;
        }

        public static string GetDefaultBuyNode1Text(string itemName)
        {
            return
$@"You have requested to order {itemName}. Amount: [variableAmount].
Total cost of items: [totalCost].

Please CONFIRM or DENY.

";
        }

        public static string GetDefaultBuyNode2Text(string itemName)
        {
            return
$@"Ordered [variableAmount] {itemName}. Your new balance is [playerCredits].

Our contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.

";
        }
    }
}