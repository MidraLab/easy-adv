using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using EasyADV.DataModel;
using EasyADV.FungusExtension;
using EasyADV.Scripts;
using EasyADV.Util;
using Fungus;
using Fungus.EditorUtils;
using UnityEditor;
using UnityEngine;
using Command = EasyADV.FungusExtension.Command;
using Menu = Fungus.Menu;

namespace EasyADV.Editor
{
    [CustomEditor(typeof(UpdateScenarioFromGoogleSheet))]
    public class EditScenario : UnityEditor.Editor
    {
        private const string SheetName = ScenarioSheetData.MainSheet;

        private const int BlockName = 1;
        private static List<Block> _blocks = new();
        private static Flowchart _flowchart;
        private static string _characterImageFolderPath;
        private static string _backgroundImageFolderPath;
        private static string _bgmFolderPath;
        private static bool _isUpdateVoiceProperty;
        private static int _voiceStartIndex;
        private static string _voiceFolderPath;
        private readonly Dictionary<string, Character> _characterListInHierarchy = new();
        private CancellationTokenSource _cancellationTokenSource;
        private CommandSelectorPopupWindowContent _commandSelectorPopupWindowContent;
        private int _menuCount = 1;

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        public override async void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // プロパティを最新の状態にする
            serializedObject.Update();

            // serializedObjectからSampleComponentのプロパティを取得
            _characterImageFolderPath = Application.dataPath + "/EasyADV/CharacterImage";
            _backgroundImageFolderPath = Application.dataPath + "/EasyADV/BackgroundImage";
            _bgmFolderPath = Application.dataPath + "/EasyADV/Sound";

            if (GUILayout.Button(new GUIContent("Update Scenario")))
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var scenarioData = await DownloadScenarioData(SheetName);
                CacheComponents();
                DeleteAllCommand();
                UpdateCharacterInfo(scenarioData);
                await UpdateConversationBlock(scenarioData, BlockName);
                Debug.Log("Update Scenario");
            }
        }

        /// <summary>
        /// コンポーネントをキャッシュする
        /// </summary>
        private static void CacheComponents()
        {
            _flowchart = FindObjectOfType<Flowchart>();
            _blocks = _flowchart.GetComponents<Block>().ToList();
        }

        /// <summary>
        /// キャラクターの情報を更新する
        /// </summary>
        /// <param name="scenarioDataList"></param>
        private void UpdateCharacterInfo(ScenarioDataList scenarioDataList)
        {
            var characters = FindObjectsOfType<Character>().ToList();
            foreach (var character in characters) DestroyImmediate(character.gameObject);

            foreach (var scenarioData in scenarioDataList.gameInfo.Where(scenarioData =>
                         !_characterListInHierarchy.ContainsKey(scenarioData.characterName)))
            {
                if (string.IsNullOrEmpty(scenarioData.characterName)) continue;
                var characterObject = FlowchartMenuItems.SpawnPrefab("Character");
                var fungusCharacter = characterObject.GetComponent<Character>();
                fungusCharacter.NameColor = Color.grey;
                _characterListInHierarchy.Add(scenarioData.characterName, fungusCharacter);
                characterObject.name = scenarioData.characterName;
                fungusCharacter.SetStandardText(scenarioData.characterName);
                var characterImagePath = Path.Combine(_characterImageFolderPath, scenarioData.characterImage);
                fungusCharacter.Portraits.Add(AssetDatabase.LoadAssetAtPath<Sprite>(characterImagePath));
            }
        }

        /// <summary>
        /// シナリオデータをダウンロードする
        /// </summary>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private static async UniTask<ScenarioDataList> DownloadScenarioData(string sheetName)
        {
            return await GetScenarioFromSheet.GetGameInfo<ScenarioDataList>(ScenarioSheetData.SheetURL, sheetName);
        }

        /// <summary>
        ///     Google SheetのCommand数だけFlowchartのCommandを更新する
        /// </summary>
        private async UniTask UpdateConversationBlock(ScenarioDataList scenarioDataList, int blockName)
        {
            var targetBlock = _blocks.FirstOrDefault(x => x.BlockName == blockName.ToString());
            _menuCount = int.Parse(targetBlock.BlockName);

            var blockIndex = blockName;
            foreach (var (scenarioData, index) in scenarioDataList.gameInfo.Select((info, index) => (info, index)))
            {
                if (scenarioData.command == "If")
                {
                    blockIndex++;
                    targetBlock = _blocks.First(x => x.BlockName == blockIndex.ToString());
                }

                await AddBlock(scenarioData, targetBlock, index);
            }
        }

        /// <summary>
        /// Flowchartにブロック(コマンド)を追加する
        /// </summary>
        /// <param name="scenarioData"></param>
        /// <param name="block"></param>
        /// <param name="blockIndex"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async UniTask AddBlock(ScenarioData scenarioData, Block block, int blockIndex)
        {
            if (string.IsNullOrEmpty(scenarioData.command)) return;
            var commandType = Command.GetCommandType(scenarioData.command);
            switch (commandType)
            {
                case CommandEnum.Say or CommandEnum.If:
                    var commandSay = block.gameObject.AddComponent<Say>();
                    commandSay.SetStandardText(scenarioData.content);
                    commandSay._Character = _characterListInHierarchy
                        .FirstOrDefault(x => x.Key == scenarioData.characterName).Value;
                    commandSay.ItemId = blockIndex + 1;

                    block.CommandList.Add(commandSay);
                    break;
                //このコマンドは要修正
                case CommandEnum.SetBackgroundImage:
                    var commandShowImage = block.gameObject.AddComponent<CallMethod>();
                    var background = FindObjectOfType<SetBackgroundImage>();
                    var characterImage = Path.Combine(_backgroundImageFolderPath, scenarioData.backgroundImage);
                    background.SetImage(AssetDatabase.LoadAssetAtPath<Sprite>(characterImage));
                    commandShowImage.ItemId = blockIndex + 1;
                    block.CommandList.Add(commandShowImage);
                    break;
                case CommandEnum.HideCharacter:
                    var commandHideCharacter = block.gameObject.AddComponent<Portrait>();
                    commandHideCharacter.Display = DisplayType.Hide;
                    commandHideCharacter.ItemId = blockIndex + 1;
                    commandHideCharacter._Character = _characterListInHierarchy[scenarioData.characterName];
                    block.CommandList.Add(commandHideCharacter);
                    break;
                case CommandEnum.ShowCharacter:
                    var commandShowCharacter = block.gameObject.AddComponent<Portrait>();
                    commandShowCharacter.Display = DisplayType.Show;
                    commandShowCharacter.ItemId = blockIndex + 1;
                    commandShowCharacter._Character = _characterListInHierarchy[scenarioData.characterName];
                    commandShowCharacter._Portrait = _characterListInHierarchy[scenarioData.characterName].Portraits[0];
                    block.CommandList.Add(commandShowCharacter);
                    break;
                case CommandEnum.PlayBGM:
                    var commandPlayBgm = block.gameObject.AddComponent<PlayMusic>();
                    commandPlayBgm.ItemId = blockIndex + 1;
                    commandPlayBgm.MusicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                        Path.Combine(_bgmFolderPath, scenarioData.sound));
                    block.CommandList.Add(commandPlayBgm);
                    break;
                case CommandEnum.StopBGM:
                    var commandStopBgm = block.gameObject.AddComponent<StopMusic>();
                    commandStopBgm.ItemId = blockIndex + 1;
                    block.CommandList.Add(commandStopBgm);
                    break;
                case CommandEnum.Menu:
                    _menuCount++;
                    var commandMenu = block.gameObject.AddComponent<Menu>();
                    commandMenu.SetStandardText(scenarioData.content);
                    commandMenu.SetMenuDialog = FindObjectOfType<MenuDialog>();
                    commandMenu.ItemId = blockIndex + 1;
                    var targetBlock = _blocks.FirstOrDefault(x => x.BlockName == _menuCount.ToString());
                    commandMenu.SetTargetBlock(targetBlock);
                    block.CommandList.Add(commandMenu);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Flowchartのブロックを削除する
        /// </summary>
        private static void DeleteAllCommand()
        {
            foreach (var (deleteBlock, _) in _blocks.Select((info, index) => (info, index)))
            {
                foreach (var command in deleteBlock.CommandList) DestroyImmediate(command);
                deleteBlock.CommandList.Clear();
            }
        }
    }
}