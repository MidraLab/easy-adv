using UnityEngine;

namespace EasyADV.Scripts
{
    public class UpdateScenarioFromGoogleSheet : MonoBehaviour
    {
        [SerializeField] private string characterImageFolderPath;
        [SerializeField] private string backgroundImageFolderPath;
        [SerializeField] private string bgmFolderPath;
        [SerializeField] private bool isUpdateVoice;
        [SerializeField] private int voiceStartIndex;
        [SerializeField] private string voiceFolderPath;
    }
}