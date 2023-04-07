using System;
using System.Collections.Generic;

namespace EasyADV.DataModel
{
    [Serializable]
    public class ScenarioData
    {
        public string backgroundImage;
        public string characterImage;
        public string characterName;
        public string command;
        public string content;
        public string description;
        public string sound;
    }

    [Serializable]
    public class ScenarioDataList
    {
        public List<ScenarioData> gameInfo;
    }
}