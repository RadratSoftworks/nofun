using UnityEngine.Localization.Settings;

namespace Nofun.Services.Unity
{
    public class TranslationService : ITranslationService
    {
        private const string TableName = "nofun";

        public string Translate(string key)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(TableName, key);
        }
    }
}
