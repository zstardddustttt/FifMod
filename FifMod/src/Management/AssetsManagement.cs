using System.IO;
using System.Reflection;
using UnityEngine;

namespace FifMod
{
    public class FifModAssets
    {
        public readonly AssetBundle assetBundle;
        private const string START_PATH = "Assets/fifmod/";

        public FifModAssets(string assetBundle)
        {
            this.assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assetBundle));
        }

        public bool TryGetAsset<T>(string assetPath, out T asset) where T : Object
        {
            var result = assetBundle.LoadAsset<T>(START_PATH + assetPath);
            asset = result;
            return result;
        }

        public T GetAsset<T>(string assetPath) where T : Object
        {
            return assetBundle.LoadAsset<T>(START_PATH + assetPath);
        }
    }
}