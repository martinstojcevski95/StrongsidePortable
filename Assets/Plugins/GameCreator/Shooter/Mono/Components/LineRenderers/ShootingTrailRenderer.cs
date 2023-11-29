namespace GameCreator.Shooter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("")]
    public class ShootingTrailRenderer : BaseLineRenderer
    {
        [Serializable]
        public class ShootingTrail : BaseLineRenderer.ShootingBase
        {
            public bool useShootingTrail = true;
            public float duration = 0.1f;

            [NonSerialized] public Vector3 position1;
            [NonSerialized] public Vector3 position2;
        }

        // PROPERTIES: --------------------------------------------------------------------

        private float startTime = 0f;
        private float totalDuration = 1f;
        private float startAlpha = 1f;

        private bool retract = true;
        private Vector3 positionA;
        private Vector3 positionB;

        // PUBLIC STATIC METHODS: ---------------------------------------------------------

        public static ShootingTrailRenderer Create(ShootingTrail data)
        {
            GameObject instance = new GameObject("ShootingTrailRenderer");
            ShootingTrailRenderer trail = instance.AddComponent<ShootingTrailRenderer>();
            trail.SetupShootingTrail(data);
            return trail;
        }

        public static ShootingTrailRenderer Create(
            ShootingTrail data,
            TrajectoryRenderer.TrajectoryResult trajectory)
        {
            GameObject instance = new GameObject("ShootingTrailRenderer");
            ShootingTrailRenderer trail = instance.AddComponent<ShootingTrailRenderer>();
            trail.SetupShootingTrail(data);

            trail.lineRenderer.positionCount = trajectory.count;
            trail.lineRenderer.SetPositions(trajectory.points);

            trail.retract = (trajectory.count == 2);
            if (trail.retract)
            {
                trail.positionA = trajectory.points[0];
                trail.positionB = trajectory.points[1];
            }

            return trail;
        }

        // PRIVATE METHODS: --------------------------------------------------------------

        private void SetupShootingTrail(ShootingTrail data)
        {
            this.SetupLineRenderer(data, true);
            this.lineRenderer.positionCount = 2;
            this.lineRenderer.SetPositions(new Vector3[]
            {
                data.position1,
                data.position2
            });

            this.positionA = data.position1;
            this.positionB = data.position2;

            this.startAlpha = this.lineRenderer.material.color.a;
            this.startTime = Time.time;

            float distance = Vector3.Magnitude(this.positionB - this.positionA);
            this.totalDuration = distance * data.duration;
        }

        private void Update()
        {
            float t = (Time.time - this.startTime) / this.totalDuration;

            this.lineRenderer.material.color = new Color(
                this.lineRenderer.endColor.r,
                this.lineRenderer.endColor.g,
                this.lineRenderer.endColor.b,
                Mathf.Lerp(this.startAlpha, 0f, t)
            );

            if (retract)
            {
                Vector3 position = Vector3.Lerp(this.positionA, positionB, t);
                this.lineRenderer.SetPosition(0, position);
            }

            if (t >= 1) Destroy(gameObject);
        }
    }
}