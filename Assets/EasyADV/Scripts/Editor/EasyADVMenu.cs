using System.IO;
using EasyADV.FungusExtension;
using EasyADV.Scripts;
using Fungus;
using UnityEditor;
using UnityEngine;

namespace EasyADV.Editor
{
    /// <summary>
    ///    EasyADVのメニューを追加するクラス
    /// </summary>
    public class EasyADVMenu
    {
        /// <summary>
        ///   背景画像のフォルダ名
        /// </summary>
        private const string BackgroundImageFolderName = "BackgroundImage";

        /// <summary>
        ///  キャラクター画像のフォルダ名
        /// </summary>
        private const string CharacterImageFolderName = "CharacterImage";

        /// <summary>
        /// サウンドのフォルダ名
        /// </summary>
        private const string SoundFolderName = "Sound";

        [MenuItem("Tools/EasyADV/Setup")]
        private static void Setup()
        {
            SetupHierarchyObject();
            SetupMaterialFolder();
        }

        /// <summary>
        /// 素材フォルダを作成する
        /// </summary>
        private static void SetupMaterialFolder()
        {
            var folderRoot = Application.dataPath + "/EasyADV/";
            
            // 背景画像フォルダがなければ作成
            if (!Directory.Exists(folderRoot + BackgroundImageFolderName))
            {
                Directory.CreateDirectory(folderRoot + BackgroundImageFolderName);
                CreateGitKeep(Path.Combine(folderRoot, BackgroundImageFolderName));
            }

            // キャラクター画像フォルダがなければ作成
            if (!Directory.Exists(folderRoot + CharacterImageFolderName))
            {
                Directory.CreateDirectory(folderRoot + CharacterImageFolderName);
                CreateGitKeep(Path.Combine(folderRoot, CharacterImageFolderName));
            }

            // サウンドフォルダがなければ作成
            if (!Directory.Exists(folderRoot + SoundFolderName))
            {
                Directory.CreateDirectory(folderRoot + SoundFolderName);
                CreateGitKeep(Path.Combine(folderRoot, SoundFolderName));
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Fungus・EasyADVを使用するためのオブジェクトを作成する
        /// </summary>
        private static void SetupHierarchyObject()
        {
            if (Object.FindObjectOfType<Flowchart>() == null)
            {
                Fungus.EditorUtils.FlowchartMenuItems.CreateFlowchart();
            }

            if(Object.FindObjectOfType<Stage>() == null)
            {
                Fungus.EditorUtils.NarrativeMenuItems.CreateStage();
            }

            if (Object.FindObjectOfType<SayDialog>() == null)
            {
                Fungus.EditorUtils.NarrativeMenuItems.CreateSayDialog();
            }

            // FlowchartにUpdateScenarioFromGoogleSheetコンポーネントが追加されていなければ追加
            var flowchart = Object.FindObjectOfType<Flowchart>();
            if (flowchart == null || flowchart.GetComponent<UpdateScenarioFromGoogleSheet>() != null)
            {
                return;
            }

            flowchart.gameObject.AddComponent<UpdateScenarioFromGoogleSheet>();

            // ステージ内に背景画 切り替え用のオブジェクトとコンポーネントの追加
            var stageCanvas = Object.FindObjectOfType<Stage>().GetComponentInChildren<Canvas>();
            if (stageCanvas.GetComponentInChildren<SetBackgroundImage>() != null)
            {
                return;
            }

            var backgroundPrefab =
                AssetDatabase.LoadAssetAtPath<SetBackgroundImage>("Assets/EasyADV/Prefabs/Background.prefab");
            var backgroundObject = Object.Instantiate(backgroundPrefab, stageCanvas.transform);
            backgroundObject.name = "Background";
        }
        
        /// <summary>
        /// Gitの管理下に置くためのファイルを作成する
        /// </summary>
        /// <param name="path"></param>
        private static void CreateGitKeep(string path)
        {
            var gitKeepPath = path + "/.gitkeep";
            if (File.Exists(gitKeepPath))
            {
                return;
            }

            File.Create(gitKeepPath);
        }
    }
}