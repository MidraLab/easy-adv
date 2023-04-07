using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EasyADV.DataModel;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyADV.Util
{
    public class GetScenarioFromSheet
    {
        /// <summary>
        ///     ゲーム情報をスプレッドシートから取得
        /// </summary>
        /// <param name="sheetURL"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static async UniTask<T> GetGameInfo<T>(string sheetURL, string sheetName)
        {
            var request = UnityWebRequest.Get($"{sheetURL}?sheetName={sheetName}");
            await request.SendWebRequest();
            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log("fail to get card info from google sheet");
            }
            else
            {
                var json = request.downloadHandler.text;
                Debug.Log("json: " + json);
                var data = JsonUtility.FromJson<T>(json);
                return data;
            }

            return default;
        }

        public static string ConvertFungusTextFormat(ScenarioDataList data)
        {
            var fungusText = string.Empty;
            fungusText += ConvertDialogue(data);
            fungusText += CovertCharacter(data);

            return fungusText;
        }

        private static string ConvertDialogue(ScenarioDataList dataList)
        {
            var fungusText = string.Empty;
            foreach (var (info, index) in dataList.gameInfo.Select((info, index) => (info, index)))
            {
                fungusText += $"#{info.command.ToUpper()}.Flowchart.{index + 1}.{info.characterName}\n";
                fungusText += $"{info.content}\n";
                fungusText += "\n";
            }

            return fungusText;
        }

        private static string CovertCharacter(ScenarioDataList dataList)
        {
            var fungusText = string.Empty;
            var characterList = new List<string>();
            foreach (var info in dataList.gameInfo.Where(info => !characterList.Contains(info.characterName)))
            {
                characterList.Add(info.characterName);
                fungusText += $"#CHARACTER.{info.characterName}\n";
                fungusText += $"{info.characterName}\n";
                fungusText += "\n";
            }

            return fungusText;
        }
    }
}