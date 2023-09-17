using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Image))]
    public class FadedImage : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private int targetFade;
        [SerializeField] private float fadeSpeed = 10;
        private Image image;

        public void Fade()
        {
            StartCoroutine(ShowComplete());
        }

        public void SetPosition(float position)
        {
            if (image)
                image.fillAmount = 1 - position;
        }

        private void OnEnable()
        {
            image = GetComponent<Image>();
            var color = image.color;
            color[3] = 1 - targetFade;
            image.color = color;
        }

        private IEnumerator ShowComplete()
        {
            if (!image)
                yield break;

            var currentAlpha = 1f - targetFade;

            var imgColor = image.color;
            imgColor[3] = currentAlpha;
            image.color = imgColor;

            while (true)
            {
                currentAlpha = Mathf.Lerp(currentAlpha, targetFade, fadeSpeed * Time.deltaTime);
                imgColor[3] = currentAlpha;
                image.color = imgColor;
                if (Math.Abs(targetFade - currentAlpha) <= Mathf.Epsilon)
                    yield break;

                yield return null;
            }
        }
    }
}