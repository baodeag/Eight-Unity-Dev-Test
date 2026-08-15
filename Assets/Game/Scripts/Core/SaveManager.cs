using UnityEngine;

namespace baodeag.Game
{
    public static class SaveManager
    {
        private const string ScoreKey = "Score";
        private const string GemCountKey = "GemCount";

        public static int LoadScore()
        {
            return PlayerPrefs.GetInt(ScoreKey, 0);
        }

        public static int LoadGemCount()
        {
            return PlayerPrefs.GetInt(GemCountKey, 0);
        }

        public static void SaveProgress(int score, int gemCount)
        {
            PlayerPrefs.SetInt(ScoreKey, score);
            PlayerPrefs.SetInt(GemCountKey, gemCount);
            PlayerPrefs.Save();
        }

        public static void ClearScore()
        {
            PlayerPrefs.DeleteKey(ScoreKey);
            PlayerPrefs.DeleteKey(GemCountKey);
            PlayerPrefs.Save();
        }
    }
}
