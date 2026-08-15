using UnityEngine;

namespace baodeag.InterviewTest
{
    public static class InterviewSaveManager
    {
        private const string ScoreKey = "InterviewTest_Score";

        public static int LoadScore()
        {
            return PlayerPrefs.GetInt(ScoreKey, 0);
        }

        public static void SaveScore(int score)
        {
            PlayerPrefs.SetInt(ScoreKey, score);
            PlayerPrefs.Save();
        }

        public static void ClearScore()
        {
            PlayerPrefs.DeleteKey(ScoreKey);
            PlayerPrefs.Save();
        }
    }
}
