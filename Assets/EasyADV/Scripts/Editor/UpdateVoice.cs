using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using EasyADV.DataModel;
using KoeiromapUnity.Core;
using KoeiromapUnity.Util;
using UnityEditor;
using UnityEngine;

namespace EasyADV.Editor
{
    /// <summary>
    ///    シナリオを更新するためのクラス
    /// </summary>
    public partial class EditScenario
    {
        private static void DeleteVoice(bool isUpdateVoice)
        {
            if (isUpdateVoice) DeleteAllVoice();
        }

        private static void DeleteAllVoice()
        {
            var voiceFiles = Directory.GetFiles(Path.Combine("Assets", _voiceFolderPath));
            foreach (var voiceFile in voiceFiles) AssetDatabase.DeleteAsset(voiceFile);
            AssetDatabase.Refresh();
        }

        private static bool IsSilentText(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;

            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return true;

            // 音を発しないセリフを表す正規表現パターン
            const string pattern = @"^[\.\!．！?？]+$";
            return Regex.IsMatch(text, pattern);
        }


        private static async UniTask<AudioClip> GetVoiceClip(string characterName, string text, int index,
            CancellationToken token)
        {
            var audioResponse = await CharacterVoice(characterName, text, token);
            var audioClip = audioResponse.voiceResult.audioClip;
            if (audioClip == null) return null;
            if (audioClip != null && !audioClip.LoadAudioData())
            {
                Debug.Log("Failed to load audio data.");
                return null;
            }

            var savePath = Path.Combine(Application.dataPath, _voiceFolderPath, $"{index}.wav");
            await AudioFileUtility.Save(audioResponse.voiceResult.audioBase64, savePath, token);
            AssetDatabase.ImportAsset(Path.Combine("Assets", _voiceFolderPath, $"{index}.wav"));
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine("Assets", _voiceFolderPath, $"{index}.wav"));
        }

        private static VoiceParam GetCharacterVoiceParam(string characterName, string text)
        {
            switch (characterName)
            {
                case "主人公":
                    return CharacterVoiceSetting.Character1(text);
                default:
                    throw new ArgumentOutOfRangeException(nameof(characterName), characterName, null);
            }
        }

        private static async UniTask<CreateVoiceResponse> CharacterVoice(string characterName, string text,
            CancellationToken token)
        {
            var response = new CreateVoiceResponse
            {
                voiceParam = GetCharacterVoiceParam(characterName, text)
            };
            response.voiceResult = await Koeiromap.GetVoice(response.voiceParam, token);
            return response;
        }
    }

    [Serializable]
    public class CreateVoiceResponse
    {
        public VoiceResult voiceResult;
        public VoiceParam voiceParam;
    }

    [Serializable]
    public class CreatedVoiceData
    {
        public int voiceIndex;
        public string text;
        public string characterName;
        public VoiceParam voiceParam;
    }
}