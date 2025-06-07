using UnityEngine;

namespace LuckyMultiplayer.Scripts
{
    public class Flasher : MonoBehaviour
    {
        private SpriteRenderer[] spriteRenderers;
        private Color sourceColor = Color.white;
        private Color flashColor = Color.white;

        private float timer;
        private float totalTime;

        private bool isFlashing = false;

        private void Start()
        {
            // Get all SpriteRenderers in this object and its children
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            if (spriteRenderers.Length > 0)
            {
                sourceColor = spriteRenderers[0].color;
            }
            else
            {
                Debug.LogWarning("No SpriteRenderers found on object or children.");
            }
        }

        private void Update()
        {
            if (isFlashing)
            {
                timer += Time.deltaTime;
                float t = timer / totalTime;

                Color currentColor = Color.Lerp(flashColor, sourceColor, t);

                foreach (var sr in spriteRenderers)
                {
                    sr.color = currentColor;
                    return;
                }

                if (timer >= totalTime)
                {
                    isFlashing = false;

                    // Reset to original color
                    foreach (var sr in spriteRenderers)
                    {
                        sr.color = sourceColor;
                        return;
                    }
                }
            }
        }

        public void Flash(Color color, float duration)
        {
            flashColor = color;
            totalTime = duration;
            timer = 0f;
            isFlashing = true;
        }
    }
}
