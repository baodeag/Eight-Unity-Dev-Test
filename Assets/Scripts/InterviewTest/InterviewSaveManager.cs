using UnityEngine;

namespace baodeag.InterviewTest
{
    public static class InterviewSaveManager
    {
        private const string ScoreKey = "InterviewTest_Score";
        private const string GemCountKey = "InterviewTest_GemCount";

        public static int LoadScore()
        {
            return PlayerPrefs.GetInt(ScoreKey, 0);
        }

        public static void SaveScore(int score)
        {
            PlayerPrefs.SetInt(ScoreKey, score);
            PlayerPrefs.Save();
        }

        public static int LoadGemCount()
        {
            return PlayerPrefs.GetInt(GemCountKey, 0);
        }

        public static void SaveGemCount(int gemCount)
        {
            PlayerPrefs.SetInt(GemCountKey, gemCount);
            PlayerPrefs.Save();
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
