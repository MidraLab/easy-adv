using System;
using System.Collections.Generic;

namespace EasyADV.DataModel
{
    /// <summary>
    ///    シナリオデータ
    /// </summary>
    [Serializable]
    public class ScenarioData
    {
        public string backgroundImage;
        public string characterImage;
        public string characterName;
        public string command;
        public string content;
        public string sound;
    }

    /// <summary>
    ///   シナリオデータリスト
    /// </summary>
    [Serializable]
    public class ScenarioDataList
    {
        public List<ScenarioData> gameInfo;
    }
}