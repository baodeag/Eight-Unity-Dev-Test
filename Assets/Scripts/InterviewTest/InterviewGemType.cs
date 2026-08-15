using UnityEngine;

namespace baodeag.InterviewTest
{
    [CreateAssetMenu(fileName = "Interview Gem Type", menuName = "Interview Test/Gem Type")]
    public class InterviewGemType : ScriptableObject
    {
        [Header("Identity")]
        public string gemName = "Common Gem";
        public int scoreValue = 1;
        public int spawnWeight = 1;

        [Header("Visual")]
        public Material material;
        public Color lightColor = Color.cyan;
        public Sprite uiIcon;
    }
}
