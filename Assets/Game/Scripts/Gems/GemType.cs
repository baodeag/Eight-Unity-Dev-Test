using UnityEngine;

namespace baodeag.Game
{
    [CreateAssetMenu(fileName = "Gem Type", menuName = "Game/Gem Type")]
    public class GemType : ScriptableObject
    {
        [Header("Identity")]
        public string gemName = "Common Gem";
        public int scoreValue = 1;
        public int spawnWeight = 1;

        [Header("Visual")]
        public Material material;
        public Color lightColor = Color.cyan;
        public Sprite uiIcon;

        private void OnValidate()
        {
            scoreValue = Mathf.Max(1, scoreValue);
            spawnWeight = Mathf.Max(0, spawnWeight);
        }
    }
}
