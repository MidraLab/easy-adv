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
        [MenuItem("Tools/EasyADV/Setup")]
        private static void Setup()
        {
            Fungus.EditorUtils.FlowchartMenuItems.CreateFlowchart();
            Fungus.EditorUtils.NarrativeMenuItems.CreateSayDialog();
            Fungus.EditorUtils.NarrativeMenuItems.CreateStage();
            
            // FlowchartにUpdateScenarioFromGoogleSheetコンポーネントが追加されていなければ追加
            var flowchart = Object.FindObjectOfType<Flowchart>();
            if (flowchart == null  || flowchart.GetComponent<UpdateScenarioFromGoogleSheet>() != null)
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
            var backgroundObject = Object.Instantiate(backgroundPrefab,stageCanvas.transform);
            backgroundObject.name = "Background";
        }
    }
}