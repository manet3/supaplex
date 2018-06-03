using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supaplex
{
    public enum GameMode
    {
        PlayMode,
        AiMode,
        CompetitionMode
    }
    public static class MainMenuParameters
    {
        public static int LevelNumb;
        public static List<string> Levels;
        public static GameMode Mode;
        public static readonly string LvlOrderFilePath = "Resources/Levels/consequence";
        public static string DefaultLevel;

        static MainMenuParameters()
        {
            DefaultLevel = null;
        }
    }
}
