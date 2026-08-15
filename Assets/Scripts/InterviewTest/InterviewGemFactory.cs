using UnityEngine;

namespace baodeag.InterviewTest
{
    public class InterviewGemFactory : MonoBehaviour
    {
        [Header("Types")]
        [SerializeField] private InterviewGemType[] gemTypes;

        public InterviewGemType GetRandomGemType()
        {
            if (gemTypes == null || gemTypes.Length == 0)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < gemTypes.Length; i++)
            {
                if (gemTypes[i] != null)
                {
                    totalWeight += Mathf.Max(0, gemTypes[i].spawnWeight);
                }
            }

            if (totalWeight <= 0)
            {
                return gemTypes[0];
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

            return gemTypes[0];
        }
    }
}
