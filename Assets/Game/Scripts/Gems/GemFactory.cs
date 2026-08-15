using UnityEngine;

namespace baodeag.Game
{
    public class GemFactory : MonoBehaviour
    {
        [Header("Types")]
        [SerializeField] private GemType[] gemTypes;

        private int totalWeight;
        private GemType fallbackType;

        private void Awake()
        {
            RebuildWeightCache();
        }

        private void OnValidate()
        {
            RebuildWeightCache();
        }

        public GemType GetRandomGemType()
        {
            if (gemTypes == null || gemTypes.Length == 0)
            {
                return null;
            }

            if (totalWeight <= 0)
            {
                return fallbackType;
            }

            int roll = Random.Range(0, totalWeight);
            for (int i = 0; i < gemTypes.Length; i++)
            {
                if (gemTypes[i] == null)
                {
                    continue;
                }

                roll -= Mathf.Max(0, gemTypes[i].spawnWeight);
                if (roll < 0)
                {
                    return gemTypes[i];
                }
            }

            return fallbackType;
        }

        private void RebuildWeightCache()
        {
            totalWeight = 0;
            fallbackType = null;

            if (gemTypes == null)
            {
                return;
            }

            for (int i = 0; i < gemTypes.Length; i++)
            {
                GemType type = gemTypes[i];
                if (type == null)
                {
                    continue;
                }

                fallbackType ??= type;
                totalWeight += Mathf.Max(0, type.spawnWeight);
            }
        }
    }
}
