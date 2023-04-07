using KoeiromapUnity.Core;

namespace EasyADV.DataModel
{
    public static class CharacterVoiceSetting
    {
        /// <summary>
        ///     sample
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static VoiceParam Character1(string text)
        {
            return new VoiceParam
            {
                text = text,
                speaker_x = 0.50f,
                speaker_y = 0.20f,
                style = "talk",
                seed = "15"
            };
        }
    }
}