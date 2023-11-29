namespace GameCreator.Shooter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("")]
    public class TrajectoryRenderer : BaseLineRenderer
    {
        [Serializable]
        public class Trajectory : BaseLineRenderer.ShootingBase
        {
            public Mode mode = Mode.Curve;
            public Vector3 offset = Vector3.forward;

            public float minVelocity = 10f;
            public float maxVelocity = 20f;
            public float resolution = 0.1f;

            [HideInInspector] public float maxDistance = 10f;
            [HideInInspector] public LayerMask layerMask = -1;
            [HideInInspector] public CharacterShooter shooter;
        }

        public class TrajectoryResult
        {
            public Vector3[] points;
            public int count;
            public RaycastHit hit;
        }

        public enum Mode
        {
            Straight,
            Curve
        }

        // PROPERTIES: --------------------------------------------------------------------

        public float charge = 0f;
        private Trajectory trajectory = new Trajectory();

        // PUBLIC STATIC METHODS: ---------------------------------------------------------

        public static TrajectoryRenderer Create(Trajectory trajectory, Transform parent)
        {
            GameObject instance = new GameObject("ArcLineRenderer");
            TrajectoryRenderer arc = instance.AddComponent<TrajectoryRenderer>();
            arc.transform.parent = parent;
            arc.transform.localPosition = Vector3.zero;
            arc.transform.localRotation = Quaternion.identity;
            arc.transform.localScale = Vector3.one;

            arc.trajectory = trajectory;
            arc.SetupTrajectory(arc.trajectory);

            return arc;
        }

        public static TrajectoryResult GetTrajectory(
            Transform transform, Trajectory trajectory, float charge)
        {
            switch (trajectory.mode)
            {
                case Mode.Straight:
                    return CalculateStraightPositions(transform, trajectory, charge);

                case Mode.Curve:
                    return CalculateCurvePositions(transform, trajectory, charge);
            }

            return null;
        }

        // PRIVATE METHODS: --------------------------------------------------------------

        private void SetupTrajectory(Trajectory data)
        {
            this.SetupLineRenderer(data, false);
            this.UpdateTrajectory();
        }

        private void Update()
        {
            this.UpdateTrajectory();
        }

        private void UpdateTrajectory()
        {
            if (this.trajectory.shooter != null)
            {
                this.charge = this.trajectory.shooter.GetCharge();
            }

            TrajectoryResult result = GetTrajectory(
                transform,
                this.trajectory,
                this.charge
            );

            this.lineRenderer.positionCount = result.count;
            this.lineRenderer.SetPositions(result.points);
        }

        private static TrajectoryResult CalculateStraightPositions(Transform transform, Trajectory pTrajectory, float pCharge)
        {
            Vector3 origin = transform.position;
            TrajectoryResult result = new TrajectoryResult()
            {
                points = new Vector3[]
                {
                    origin + transform.TransformDirection(pTrajectory.offset),
                    origin + transform.TransformDirection(Vector3.forward * pTrajectory.maxDistance)
                },
                count = 2,
                hit = new RaycastHit()
            };

            RaycastHit raycastHit;
            bool linecast = Physics.Linecast(
                result.points[0],
                result.points[1],
                out raycastHit,
                pTrajectory.layerMask,
                QueryTriggerInteraction.Ignore
            );

            if (linecast)
            {
                result.points[1] = raycastHit.point;
                result.hit = raycastHit;
            }

            return result;
        }

        private static TrajectoryResult CalculateCurvePositions(Transform transform, Trajectory pTrajectory, float pCharge)
        {
            List<Vector3> points = new List<Vector3>();

            RaycastHit raycastHit = new RaycastHit();

            float v = Mathf.Lerp(pTrajectory.minVelocity, pTrajectory.maxVelocity, pCharge);
            Vector3 initVelocity = transform.TransformDirection(Vector3.forward) * v;

            Vector3 prevPosition = transform.position;

            int maxCount = (int)(pTrajectory.maxDistance / pTrajectory.resolution);
            float distanceAmount = 0f;

            for (int i = 0; i < maxCount; ++i)
            {
                float t = i * pTrajectory.resolution;
                points.Add(CalculateCurvePoint(
                    transform.position + transform.TransformDirection(pTrajectory.offset),
                    initVelocity,
                    t
                ));

                bool linecast = Physics.Linecast(
                    prevPosition,
                    points[i],
                    out raycastHit,
                    pTrajectory.layerMask,
                    QueryTriggerInteraction.Ignore
                );

                if (linecast)
                {
                    points[i] = raycastHit.point;
                    break;
                }

                distanceAmount += (points[i] - prevPosition).magnitude;
                if (distanceAmount > pTrajectory.maxDistance) break;

                prevPosition = points[i];
            }

            TrajectoryResult result = new TrajectoryResult()
            {
                points = points.ToArray(),
                count = points.Count,
                hit = raycastHit
            };

            return result;
        }

        private static Vector3 CalculateCurvePoint(Vector3 initPosition, Vector3 initVelocity, float t)
        {
            return (
                initPosition +
                initVelocity * t +
                Physics.gravity * t * t * 0.5f
            );
        }
    }
}