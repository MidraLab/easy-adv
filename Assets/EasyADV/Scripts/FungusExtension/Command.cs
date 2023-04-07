using System;

namespace EasyADV.FungusExtension
{
    public static class Command
    {
        public static CommandEnum GetCommandType(string commandString)
        {
            return (CommandEnum)Enum.Parse(typeof(CommandEnum), commandString);
        }
    }

    public enum CommandEnum
    {
        Say,
        ShowCharacter,
        HideCharacter,
        SetBackgroundImage,
        PlayBGM,
        StopBGM,
        If,
        Menu
    }
}