namespace GameCreator.Shooter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [AddComponentMenu("")]
    public abstract class BaseLineRenderer : MonoBehaviour
    {
        [Serializable]
        public class ShootingBase
        {
            public Material material;
            public LineTextureMode textureMode = LineTextureMode.Stretch;
            public LineAlignment alignement = LineAlignment.View;
            public float width = 0.1f;
        }

        // PROPERTIES: --------------------------------------------------------------------

        private const int CAP_CORNERS = 5;
        protected static Material DEFAULT_MATERIAL;
        protected static readonly Color DEFAULT_COLOR = new Color(256, 256, 256, 0.5f);

        protected LineRenderer lineRenderer;

        // PUBLIC STATIC METHODS: ---------------------------------------------------------

        public static ShootingTrailRenderer Create()
        {
            GameObject instance = new GameObject("ShootingTrailRenderer");
            return instance.AddComponent<ShootingTrailRenderer>();
        }

        // PUBLIC METHODS: ----------------------------------------------------------------

        public virtual void SetupLineRenderer(ShootingBase data, bool thinLine)
        {
            this.lineRenderer = this.InitLineRenderer();
            if (data.material != null) this.lineRenderer.material = data.material;

            this.lineRenderer.alignment = data.alignement;
            this.lineRenderer.textureMode = data.textureMode;
            this.lineRenderer.numCapVertices = CAP_CORNERS;

            this.lineRenderer.startWidth = (thinLine ? 0f : data.width);
            this.lineRenderer.endWidth = data.width;
        }

        // PRIVATE METHODS: --------------------------------------------------------------

        protected LineRenderer InitLineRenderer()
        {
            if (DEFAULT_MATERIAL == null)
            {
                DEFAULT_MATERIAL = new Material(Shader.Find("Sprites/Default"))
                {
                    color = DEFAULT_COLOR
                };
            }

            LineRenderer render = this.gameObject.AddComponent<LineRenderer>();
            render.material = DEFAULT_MATERIAL;
            render.receiveShadows = false;
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            render.useWorldSpace = true;

            return render;
        }
    }
}