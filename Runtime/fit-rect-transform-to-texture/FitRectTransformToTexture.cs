using BeatThat.TransformPathExt;
using BeatThat.Properties;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BeatThat.GetComponentsExt;

namespace BeatThat
{
    /// <summary>
    /// Changes the dimensions of a RectTransform to match the aspect ratio of a texture.
    /// 
    /// REQUIREMENTS:
    /// * sibling components implementing IEditsTexture and IHasUVRect. Most commonly this can be a BeatThat.RawImageTexture
    /// * The RectTransform containing this component should by default fully fill the space of its parent (e.g. anchors 0, 0, 1, 1)
    /// 
    /// </summary>
    public class FitRectTransformToTexture : UIBehaviour
    {
        public enum Policy
        {
            FitWithinParentRect = 0,
            FitAspectRatioToParentWidth = 1,
        }

        public Policy m_policy = Policy.FitWithinParentRect;

        public Policy policy { get { return m_policy; } }

        private IHasValue<Texture> hasTexture { get { return m_hasTexture ?? (m_hasTexture = GetComponent<IHasValue<Texture>>()); } }
        private IHasValue<Texture> m_hasTexture;


#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            this.rectTransform.anchorMax = Vector2.one;
            this.rectTransform.anchorMin = Vector2.zero;
            this.rectTransform.sizeDelta = Vector2.zero;
        }
#endif
        override protected void Start()
        {
            var textureProp = this.hasTexture as IHasProp<Texture>;
            if (textureProp != null)
            {
                textureProp.onValueChanged.AddListener(this.OnTextureValueChanged);
            }
            Refit();
        }

        public void Refit()
        {
            var hasT = this.hasTexture;
            if(hasT == null)
            {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("No (IHasValue<Texture>) to target at " + this.Path());
#endif
                return;
            }
            var tex = hasT.value as Texture;
            if (tex == null)
            {
                return;
            }
            Fit(this.rectTransform, tex);
        }

        private void OnTextureValueChanged(Texture t)
        {
            Refit();
        }

        override protected void OnRectTransformDimensionsChange()
        {
            if (this.changeInProgress)
            {
                return;
            }
            Refit();
        }

        public void Fit(RectTransform rt, Texture tex)
        {
            this.changeInProgress = true;
            switch (this.policy)
            {
                case Policy.FitWithinParentRect:
                    FitWithinParentRect(rt, tex);
                    break;
                case Policy.FitAspectRatioToParentWidth:
                    FitAspectRatioToParentWidth(rt, tex);
                    break;
                default:
#if UNITY_EDITOR || DEBUG_UNSTRIP
                    Debug.LogWarning("[" + Time.frameCount + "]["
                                     + this.Path() + "] unknown fit policy: " + this.policy);
#endif
                    break;
            }
            this.changeInProgress = false;
        }

        private bool changeInProgress { get; set; }

        private void FitWithinParentRect(RectTransform rt, Texture tex)
        {
            this.rectTransform.anchorMax = Vector2.one;
            this.rectTransform.anchorMin = Vector2.zero;
            this.rectTransform.sizeDelta = Vector2.zero;

            Rect rtRect = rt.rect;
            if (rtRect.width <= 0f || rtRect.height <= 0f)
            {
                return;
            }

            float rtAspect = (rtRect.height / rtRect.width);

            if (tex.width <= 0)
            {
                Debug.LogWarning("Texture has no width");
                return;
            }


            var texAspect = (float)tex.width / (float)tex.height;

            if (Mathf.Approximately(rtAspect, texAspect))
            {
                return;
            }

            if (Mathf.Approximately(texAspect, 0f))
            {
                Debug.LogWarning("Texture with aspect 0 not supported " + rt.Path());
                return;
            }

            var trimFactor = rtAspect / texAspect;

            if (Mathf.Approximately(trimFactor, 0f))
            {
                Debug.LogWarning("Trim factor 0 not supported " + this.Path());
                return;
            }

            var axis = trimFactor > 1 ?
                RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;

            var curSize = (axis == RectTransform.Axis.Horizontal) ? rtRect.width : rtRect.height;

            trimFactor = (trimFactor <= 1f) ? trimFactor : 1f / trimFactor;

            rt.SetSizeWithCurrentAnchors(axis, curSize * trimFactor);
        }

        private void FitAspectRatioToParentWidth(RectTransform rt, Texture tex)
        {
            this.rectTransform.anchorMax = Vector2.one;
            this.rectTransform.anchorMin = Vector2.zero;
            this.rectTransform.sizeDelta = Vector2.zero;

            Rect rtRect = rt.rect;
            if (rtRect.width <= 0f || tex.width <= 0)
            {
                return;
            }
            var texAspect = (float)tex.width / (float)tex.height;
            if (Mathf.Approximately(texAspect, 0f))
            {
                return;
            }
            var parentLayout = GetComponentInParent(typeof(LayoutElement)) as LayoutElement;
            if(parentLayout == null)
            {
                return;
            }
            var aspect = this.AddIfMissing<AspectRatioFitter>();
            aspect.aspectRatio = texAspect;
            parentLayout.preferredHeight = this.rectTransform.rect.height;
        }

        // Analysis disable ConvertConditionalTernaryToNullCoalescing
        private RectTransform rectTransform { get { return m_rectTransform != null ? m_rectTransform : (m_rectTransform = GetComponent<RectTransform>()); } }
        // Analysis restore ConvertConditionalTernaryToNullCoalescing
        private RectTransform m_rectTransform;
    }
}
