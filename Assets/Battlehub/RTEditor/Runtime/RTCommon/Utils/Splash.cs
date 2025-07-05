using Battlehub.Utils;
using System;
using TMPro;
using UnityEngine;

namespace Battlehub.RTCommon
{
    public class Splash : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup m_progress = null;

        [SerializeField]
        private CanvasGroup m_loadingText = null;

        [SerializeField]
        private TextMeshProUGUI m_text = null;

        public void Show(Action action, string text = null, bool autoHide = true)
        {
            if (m_text != null && text != null)
            {
                m_text.text = text; 
            }

            if (m_progress != null)
            {
                //if (Run.Instance == null)
                {
                    gameObject.AddComponent<Run>();
                }

                FadeProgress(0, 1, .0f, .5f, m_progress, FloatAnimationInfo.EaseOutCubic, () =>
                {
                    action();

                    if (autoHide)
                    {
                        if (m_loadingText != null)
                        {
                            FadeProgress(1, 0, 0, 0.7f, m_loadingText, FloatAnimationInfo.EaseInCubic, () => { });
                        }

                        FadeProgress(1, 0, .7f, 0.5f, m_progress, FloatAnimationInfo.EaseInCubic, () =>
                        {
                            Destroy(gameObject);
                        });
                    }
                });
            }
            else
            {
                action();

                if (autoHide)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Hide()
        {
            if (m_progress != null)
            {
                if (m_loadingText != null)
                {
                    FadeProgress(1, 0, 0, 0.7f, m_loadingText, FloatAnimationInfo.EaseInCubic, () => { });
                }

                FadeProgress(1, 0, .7f, 0.5f, m_progress, FloatAnimationInfo.EaseInCubic, () =>
                {
                    Destroy(gameObject);
                });
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void FadeProgress(float from, float to, float delay, float duration, CanvasGroup group, Func<float, float> easing, Action done)
        {
            Run.Instance.Animation(new FloatAnimationInfo(from, to, duration, easing,
                (target, value, t, completed) =>
            {
                group.alpha = value;

                if (completed)
                {
                    done();
                }
            })
            { Delay = delay });
        }

    }

}

