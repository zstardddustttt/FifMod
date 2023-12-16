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

        public static string GetDefaultBuyNode1(string itemName)
        {
            return
$@"You have requested to order {itemName}. Amount: [variableAmount].
Total cost of items: [totalCost].

Please CONFIRM or DENY.

";
        }

        public static string GetDefaultBuyNode2(string itemName)
        {
            return
$@"Ordered [variableAmount] {itemName}. Your new balance is [playerCredits].

Our contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.

";
        }
    }
}