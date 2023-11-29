using Pathfinding;
using Pathfinding.RVO;
using Pathfinding.Util;

namespace GameCreator.Characters
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.AI;

    public class LocomotionSystemAStar : ILocomotionSystem, IAstarAI
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        private bool move = false;

        private Vector3 targetPosition;
        private TargetRotation targetRotation;

        private float stopThreshold = STOP_THRESHOLD;

        private Queue<Vector3> pathPoints = new Queue<Vector3>();
        private bool hasStopped;
        public float nextWaypointDistance = 3;
        private int currentWaypoint = 0;

        // private Queue<QueuedPath> pathPoints = new Queue<QueuedPath>();
        private UnityAction onFinishCallback;

        private Vector3 tempDestination = new Vector3(float.NaN, float.NaN,float.NaN);
        // OVERRIDE METHODS: ----------------------------------------------------------------------

        public override CharacterLocomotion.LOCOMOTION_SYSTEM Update()
        {
            base.Update();
            if (!this.move || path == null)
            {
                var defaultDirection = Vector3.up * this.characterLocomotion.verticalSpeed;
                this.characterLocomotion.characterController.Move(defaultDirection * Time.deltaTime);

                var characterTransform = this.characterLocomotion.character.transform;
                var forward = characterTransform.TransformDirection(Vector3.forward);

                var rotation = this.UpdateRotation(forward);
                this.characterLocomotion.character.transform.rotation = rotation;

                return CharacterLocomotion.LOCOMOTION_SYSTEM.CharacterController;
            }

            var controller = this.characterLocomotion.characterController;
            if (ShouldRecalculatePath) SearchPath();

            // If gravity is used depends on a lot of things.
            // For example when a non-kinematic rigidbody is used then the rigidbody will apply the gravity itself
            // Note that the gravity can contain NaN's, which is why the comparison uses !(a==b) instead of just a!=b.
            usingGravity = !(gravity == Vector3.zero) && !updatePosition;
            if (canMove)
            {
                Vector3 nextPosition;
                Quaternion nextRotation;
                MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);
                FinalizeMovement(nextPosition, nextRotation);
            }
            // reachedEndOfPath = false;
            // float distanceToWaypoint;
            // while (true)
            // {
            //     distanceToWaypoint = Vector3.Distance(controller.transform.position, path.vectorPath[currentWaypoint]);
            //     if (distanceToWaypoint < nextWaypointDistance)
            //     {
            //         if (currentWaypoint + 1 < path.vectorPath.Count)
            //         {
            //             currentWaypoint++;
            //         }
            //         else
            //         {
            //             reachedEndOfPath = true;
            //             Stopping();
            //             break;
            //         }
            //     }
            //     else
            //     {
            //         break;
            //     }
            // }
            //
            // var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;
            // Vector3 dir = (path.vectorPath[currentWaypoint] - controller.transform.position).normalized;
            //
            // float speed = this.CalculateSpeed(dir, controller.isGrounded);
            //
            // Quaternion targetRot = this.UpdateRotation(dir);
            //
            // this.UpdateAnimationConstraints(ref dir, ref targetRot);
            // var velocity = dir * speed * speedFactor;
            // // dir = Vector3.Scale(dir, HORIZONTAL_PLANE) * speed;
            // // dir += Vector3.up * this.characterLocomotion.verticalSpeed;
            //
            // controller.SimpleMove(velocity);


            float remainingDistance = (Vector3.Distance(
                Vector3.Scale(controller.transform.position, HORIZONTAL_PLANE),
                Vector3.Scale(this.targetPosition, HORIZONTAL_PLANE)
            ));
            
            if (remainingDistance <= this.stopThreshold)
            {
                this.Stopping();
            }
            else if (remainingDistance <= this.stopThreshold + SLOW_THRESHOLD)
            {
                this.Slowing(remainingDistance);
            }

            return CharacterLocomotion.LOCOMOTION_SYSTEM.CharacterController;
        }

        public override void OnDestroy()
        {
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Stopping()
        {
            if (this.characterLocomotion.navmeshAgent != null &&
                this.characterLocomotion.navmeshAgent.enabled)
            {
                this.characterLocomotion.navmeshAgent.isStopped = true;
            }

            this.FinishMovement();
            this.move = false;
            hasStopped = false;

            if (this.targetRotation.hasRotation &&
                this.characterLocomotion.faceDirection == CharacterLocomotion.FACE_DIRECTION.MovementDirection)
            {
                this.characterLocomotion.character.transform.rotation = this.targetRotation.rotation;
            }
        }

        private void Slowing(float distanceToDestination)
        {
            float tDistance = 1f - (distanceToDestination / (this.stopThreshold + SLOW_THRESHOLD));

            Transform characterTransform = this.characterLocomotion.character.transform;
            Quaternion desiredRotation = this.UpdateRotation(characterTransform.TransformDirection(Vector3.forward));

            if (this.targetRotation.hasRotation &&
                this.characterLocomotion.faceDirection == CharacterLocomotion.FACE_DIRECTION.MovementDirection)
            {
                desiredRotation = this.targetRotation.rotation;
            }

            characterTransform.rotation = Quaternion.Lerp(
                characterTransform.rotation,
                desiredRotation,
                tDistance
            );
        }

        private void Moving()
        {
            Quaternion desiredRotation = this.UpdateRotation(
                this.characterLocomotion.navmeshAgent.desiredVelocity
            );

            this.characterLocomotion.character.transform.rotation = desiredRotation;
        }

        private void FinishMovement()
        {
            if (this.onFinishCallback != null)
            {
                this.onFinishCallback.Invoke();
                this.onFinishCallback = null;
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTarget(Vector3 position, TargetRotation rotation,
            float stopThreshold, UnityAction callback = null)
        {
            hasStopped = false;
            canSearch = true;
            this.move = true;
            // destination = position;

            // this.stopThreshold = Mathf.Max(stopThreshold, STOP_THRESHOLD);
            this.onFinishCallback = callback;

            this.targetPosition = position;
            this.targetRotation = rotation ?? new TargetRotation();
            tempDestination = position;
            onSearchPath += UpdateDestination;
            OnSetTarget();
        }

        private void UpdateDestination()
        {
            destination = tempDestination;
        }
        
        public void SetTarget(Path p, TargetRotation rotation,
            float stopThreshold, UnityAction callback = null)
        {
            // Note that this needs to be set here in the constructor and not in e.g Awake
            // because it is possible that other code runs and sets the destination property
            // before the Awake method on this script runs.
            destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            path = null;
            currentWaypoint = 0;
            path = p;
            hasStopped = false;
            this.move = true;

            this.stopThreshold = Mathf.Max(stopThreshold, STOP_THRESHOLD);
            this.onFinishCallback = callback;

            this.targetPosition = path.vectorPath[path.vectorPath.Count - 1];
            this.targetRotation = rotation ?? new TargetRotation();
        }


        public void Stop(TargetRotation rotation = null, UnityAction callback = null)
        {
            this.SetTarget(
                this.characterLocomotion.characterController.transform.position,
                rotation,
                0f,
                callback
            );
        }

        public void Stop(UnityAction callback = null)
        {
            hasStopped = true;
            pathPoints.Clear();
            this.onFinishCallback = callback;
        }

        public float radius = 0.5f;

        public float height = 2;
        public float repathRate = 0.1f;
        public bool canSearch = true;
        public bool canMove = true;

        public Vector3 gravity = new Vector3(float.NaN, float.NaN, float.NaN);
        public LayerMask groundMask = -1;

        [System.Obsolete("Use the height property instead (2x this value)")]
        public float centerOffset
        {
            get { return height * 0.5f; }
            set { height = value * 2; }
        }

        float centerOffsetCompatibility = float.NaN;
        public OrientationMode orientation = OrientationMode.ZAxisForward;


        public bool enableRotation = true;

        protected Vector3 simulatedPosition;

        protected Quaternion simulatedRotation;

        public Vector3 position
        {
            get { return updatePosition ? tr.position : simulatedPosition; }
        }

        public Quaternion rotation
        {
            get { return updateRotation ? tr.rotation : simulatedRotation; }
        }

        Vector3 accumulatedMovementDelta = Vector3.zero;
        protected Vector2 velocity2D;

        /// <summary>
        /// Velocity due to gravity.
        /// Perpendicular to the movement plane.
        ///
        /// When the agent is grounded this may not accurately reflect the velocity of the agent.
        /// It may be non-zero even though the agent is not moving.
        /// </summary>
        protected float verticalVelocity;

        /// <summary>Cached Seeker component</summary>
        protected Seeker seeker;

        /// <summary>Cached Transform component</summary>
        protected Transform tr;

        /// <summary>Cached RVOController component</summary>
        protected RVOController rvoController;

        /// <summary>
        /// Plane which this agent is moving in.
        /// This is used to convert between world space and a movement plane to make it possible to use this script in
        /// both 2D games and 3D games.
        /// </summary>
        public IMovementPlane movementPlane = GraphTransform.identityTransform;

        [System.NonSerialized] public bool updatePosition = true;
        [System.NonSerialized] public bool updateRotation = true;

        protected bool usingGravity { get; set; }
        protected float lastDeltaTime;
        protected int prevFrame;

        protected Vector3 prevPosition1;

        protected Vector3 prevPosition2;

        protected Vector2 lastDeltaPosition;

        protected bool waitingForPathCalculation = false;

        protected float lastRepath = float.NegativeInfinity;

        Transform targetCompatibility;

        bool startHasRun = false;

        public Vector3 destination { get; set; }

        public Vector3 velocity
        {
            get { return lastDeltaTime > 0.000001f ? (prevPosition1 - prevPosition2) / lastDeltaTime : Vector3.zero; }
        }

        public Vector3 desiredVelocity
        {
            get
            {
                return lastDeltaTime > 0.00001f
                    ? movementPlane.ToWorld(lastDeltaPosition / lastDeltaTime, verticalVelocity)
                    : Vector3.zero;
            }
        }

        public bool isStopped { get; set; }

        public System.Action onSearchPath { get; set; }

        private bool ShouldRecalculatePath =>
            Time.time - lastRepath >= repathRate && !waitingForPathCalculation && canSearch &&
            !float.IsPositiveInfinity(destination.x);

        public float maxAcceleration = -2.5f;

        public float rotationSpeed = 360;

        public float slowdownDistance = 0.6F;

        public float pickNextWaypointDist = 2;

        public float endReachedDistance = 0.2F;

        public bool slowWhenNotFacingTarget = true;

        public CloseToDestinationMode whenCloseToDestination = CloseToDestinationMode.Stop;

        public bool constrainInsideGraph = false;

        /// <summary>Current path which is followed</summary>
        protected Path path;

        /// <summary>Helper which calculates points along the current path</summary>
        protected PathInterpolator interpolator = new PathInterpolator();

        #region IAstarAI implementation

        /// <summary>\copydoc Pathfinding::IAstarAI::remainingDistance</summary>
        public float remainingDistance
        {
            get
            {
                return interpolator.valid
                    ? interpolator.remainingDistance + movementPlane.ToPlane(interpolator.position - position).magnitude
                    : float.PositiveInfinity;
            }
        }

        /// <summary>\copydoc Pathfinding::IAstarAI::reachedDestination</summary>
        public bool reachedDestination
        {
            get
            {
                if (!reachedEndOfPath) return false;
                if (remainingDistance + movementPlane.ToPlane(destination - interpolator.endPoint).magnitude >
                    endReachedDistance) return false;

                // Don't do height checks in 2D mode
                if (orientation != OrientationMode.YAxisForward)
                {
                    // Check if the destination is above the head of the character or far below the feet of it
                    float yDifference;
                    movementPlane.ToPlane(destination - position, out yDifference);
                    var h = tr.localScale.y * height;
                    if (yDifference > h || yDifference < -h * 0.5) return false;
                }

                return true;
            }
        }

        public bool reachedEndOfPath { get; protected set; }

        public bool hasPath
        {
            get { return interpolator.valid; }
        }

        public bool pathPending
        {
            get { return waitingForPathCalculation; }
        }

        public Vector3 steeringTarget
        {
            get { return interpolator.valid ? interpolator.position : position; }
        }

        float IAstarAI.radius
        {
            get { return radius; }
            set { radius = value; }
        }

        float IAstarAI.height
        {
            get { return height; }
            set { height = value; }
        }

        float IAstarAI.maxSpeed
        {
            get { return characterLocomotion.runSpeed; }
            set { characterLocomotion.runSpeed = value; }
        }

        bool IAstarAI.canSearch
        {
            get { return canSearch; }
            set { canSearch = value; }
        }

        bool IAstarAI.canMove
        {
            get { return canMove; }
            set { canMove = value; }
        }

        #endregion

        public void GetRemainingPath(List<Vector3> buffer, out bool stale)
        {
            buffer.Clear();
            buffer.Add(position);
            if (!interpolator.valid)
            {
                stale = true;
                return;
            }

            stale = false;
            interpolator.GetRemainingPath(buffer);
        }

        /// <summary>
        /// Looks for any attached components like RVOController and CharacterController etc.
        ///
        /// This is done during <see cref="OnEnable"/>. If you are adding/removing components during runtime you may want to call this function
        /// to make sure that this script finds them. It is unfortunately prohibitive from a performance standpoint to look for components every frame.
        /// </summary>
        public void FindComponents()
        {
            tr = characterLocomotion.character.transform;
            seeker = characterLocomotion.character.GetComponent<Seeker>();
            rvoController = characterLocomotion.character.GetComponent<RVOController>();
        }

        /// <summary>Called when the component is enabled</summary>
        protected void OnSetTarget()
        {
            FindComponents();
            // Make sure we receive callbacks when paths are calculated
            seeker.pathCallback += OnPathComplete;
            Init();
        }

        void Init()
        {
            // Clamp the agent to the navmesh (which is what the Teleport call will do essentially. Though only some movement scripts require this, like RichAI).
            // The Teleport call will also make sure some variables are properly initialized (like #prevPosition1 and #prevPosition2)
            // Teleport(position, false);
            lastRepath = float.NegativeInfinity;
            if (ShouldRecalculatePath) SearchPath();
        }

        /// <summary>\copydoc Pathfinding::IAstarAI::Teleport</summary>
        public virtual void Teleport(Vector3 newPosition, bool clearPath = true)
        {
            reachedEndOfPath = false;
            if (clearPath) ClearPath();
            prevPosition1 = prevPosition2 = simulatedPosition = newPosition;
            if (updatePosition) tr.position = newPosition;
            if (rvoController != null) rvoController.Move(Vector3.zero);
            if (clearPath) SearchPath();
        }

        protected void CancelCurrentPathRequest()
        {
            waitingForPathCalculation = false;
            // Abort calculation of the current path
            if (seeker != null) seeker.CancelCurrentPathRequest();
        }

        private void OnDisable()
        {
            onSearchPath -= UpdateDestination;
            ClearPath();

            // Make sure we no longer receive callbacks when paths complete
            seeker.pathCallback -= OnPathComplete;

            velocity2D = Vector3.zero;
            accumulatedMovementDelta = Vector3.zero;
            verticalVelocity = 0f;
            lastDeltaTime = 0;


            // Release current path so that it can be pooled
            if (path != null) path.Release(this);
            path = null;
            interpolator.SetPath(null);
        }

        /// <summary>\copydoc Pathfinding::IAstarAI::MovementUpdate</summary>
        public void MovementUpdate(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
        {
            lastDeltaTime = deltaTime;
            MovementUpdateInternal(deltaTime, out nextPosition, out nextRotation);
        }

        /// <summary>Called during either Update or FixedUpdate depending on if rigidbodies are used for movement or not</summary>
        protected void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
        {
            float currentAcceleration = maxAcceleration;

            // If negative, calculate the acceleration from the max speed
            if (currentAcceleration < 0) currentAcceleration *= -characterLocomotion.runSpeed;

            if (updatePosition)
            {
                // Get our current position. We read from transform.position as few times as possible as it is relatively slow
                // (at least compared to a local variable)
                simulatedPosition = tr.position;
            }

            if (updateRotation) simulatedRotation = tr.rotation;

            var currentPosition = simulatedPosition;

            // Update which point we are moving towards
            interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
            var dir = movementPlane.ToPlane(steeringTarget - currentPosition);

            // Calculate the distance to the end of the path
            float distanceToEnd = dir.magnitude + Mathf.Max(0, interpolator.remainingDistance);

            // Check if we have reached the target
            var prevTargetReached = reachedEndOfPath;
            reachedEndOfPath = distanceToEnd <= endReachedDistance && interpolator.valid;
            if (!prevTargetReached && reachedEndOfPath) OnTargetReached();
            float slowdown;

            // Normalized direction of where the agent is looking
            var forwards = movementPlane.ToPlane(simulatedRotation *
                                                 (orientation == OrientationMode.YAxisForward
                                                     ? Vector3.up
                                                     : Vector3.forward));

            // Check if we have a valid path to follow and some other script has not stopped the character
            if (interpolator.valid && !isStopped)
            {
                // How fast to move depending on the distance to the destination.
                // Move slower as the character gets closer to the destination.
                // This is always a value between 0 and 1.
                slowdown = distanceToEnd < slowdownDistance ? Mathf.Sqrt(distanceToEnd / slowdownDistance) : 1;

                if (reachedEndOfPath && whenCloseToDestination == CloseToDestinationMode.Stop)
                {
                    // Slow down as quickly as possible
                    velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
                }
                else
                {
                    velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir,
                                      dir.normalized * characterLocomotion.runSpeed,
                                      velocity2D, currentAcceleration, rotationSpeed, characterLocomotion.runSpeed,
                                      forwards) *
                                  deltaTime;
                }
            }
            else
            {
                slowdown = 1;
                // Slow down as quickly as possible
                velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
            }

            velocity2D = MovementUtilities.ClampVelocity(velocity2D, characterLocomotion.runSpeed, slowdown,
                slowWhenNotFacingTarget && enableRotation, forwards);

            ApplyGravity(deltaTime);

            if (rvoController != null && rvoController.enabled)
            {
                var rvoTarget = currentPosition +
                                movementPlane.ToWorld(Vector2.ClampMagnitude(velocity2D, distanceToEnd), 0f);
                rvoController.SetTarget(rvoTarget, velocity2D.magnitude, characterLocomotion.runSpeed);
            }

            // Set how much the agent wants to move during this frame
            var delta2D = lastDeltaPosition =
                CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);
            nextPosition = currentPosition + movementPlane.ToWorld(delta2D, verticalVelocity * lastDeltaTime);
            CalculateNextRotation(slowdown, out nextRotation);
        }

        public void OnTargetReached()
        {
            this.FinishMovement();
            this.move = false;
            this.canMove = true;
            hasStopped = false;

            if (this.targetRotation.hasRotation &&
                this.characterLocomotion.faceDirection == CharacterLocomotion.FACE_DIRECTION.MovementDirection)
            {
                this.characterLocomotion.character.transform.rotation = this.targetRotation.rotation;
            }
            
            OnDisable();
        }

        protected virtual void CalculateNextRotation(float slowdown, out Quaternion nextRotation)
        {
            if (lastDeltaTime > 0.00001f && enableRotation)
            {
                Vector2 desiredRotationDirection;
                if (rvoController != null && rvoController.enabled)
                {
                    var actualVelocity = lastDeltaPosition / lastDeltaTime;
                    desiredRotationDirection = Vector2.Lerp(velocity2D, actualVelocity,
                        4 * actualVelocity.magnitude / (characterLocomotion.runSpeed + 0.0001f));
                }
                else
                {
                    desiredRotationDirection = velocity2D;
                }

                var currentRotationSpeed = rotationSpeed * Mathf.Max(0, (slowdown - 0.3f) / 0.7f);
                nextRotation = SimulateRotationTowards(desiredRotationDirection, currentRotationSpeed * lastDeltaTime);
            }
            else
            {
                // TODO: simulatedRotation
                nextRotation = rotation;
            }
        }

        protected virtual void CalculatePathRequestEndpoints(out Vector3 start, out Vector3 end)
        {
            start = GetFeetPosition();
            end = destination;
        }

        /// <summary>\copydoc Pathfinding::IAstarAI::SearchPath</summary>
        public void SearchPath()
        {
            if (float.IsPositiveInfinity(destination.x)) return;
            if (onSearchPath != null) onSearchPath();

            lastRepath = Time.time;
            waitingForPathCalculation = true;

            seeker.CancelCurrentPathRequest();

            Vector3 start, end;
            CalculatePathRequestEndpoints(out start, out end);

            // Alternative way of requesting the path
            //ABPath p = ABPath.Construct(start, end, null);
            //seeker.StartPath(p);

            // This is where we should search to
            // Request a path to be calculated from our current position to the destination
            seeker.StartPath(start, end);
        }

        public virtual Vector3 GetFeetPosition()
        {
            return position;
        }

        protected void OnPathComplete(Path newPath)
        {
            ABPath p = newPath as ABPath;

            if (p == null)
                throw new System.Exception("This function only handles ABPaths, do not use special path types");

            waitingForPathCalculation = false;

            p.Claim(this);

            if (p.error)
            {
                p.Release(this);
                return;
            }

            // Release the previous path.
            if (path != null) path.Release(this);

            // Replace the old path
            path = p;

            // Make sure the path contains at least 2 points
            if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
            interpolator.SetPath(path.vectorPath);

            var graph = path.path.Count > 0 ? AstarData.GetGraph(path.path[0]) as ITransformedGraph : null;
            movementPlane = graph != null
                ? graph.transform
                : (orientation == OrientationMode.YAxisForward
                    ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 270, 90), Vector3.one))
                    : GraphTransform.identityTransform);

            // Reset some variables
            reachedEndOfPath = false;

            interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint) * 0.5f);
            interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

            interpolator.MoveToCircleIntersection2D(position, pickNextWaypointDist, movementPlane);

            var distanceToEnd = remainingDistance;
            if (distanceToEnd <= endReachedDistance)
            {
                reachedEndOfPath = true;
                OnTargetReached();
            }
        }

        protected void ClearPath()
        {
            CancelCurrentPathRequest();
            interpolator.SetPath(null);
            reachedEndOfPath = false;
        }

        public void SetPath(Path path)
        {
            if (path == null)
            {
                CancelCurrentPathRequest();
                ClearPath();
            }
            else if (path.PipelineState == PathState.Created)
            {
                // Path has not started calculation yet
                lastRepath = Time.time;
                waitingForPathCalculation = true;
                seeker.CancelCurrentPathRequest();
                seeker.StartPath(path);
            }
            else if (path.PipelineState == PathState.Returned)
            {
                if (seeker.GetCurrentPath() != path) seeker.CancelCurrentPathRequest();
                else
                    throw new System.ArgumentException(
                        "If you calculate the path using seeker.StartPath then this script will pick up the calculated path anyway as it listens for all paths the Seeker finishes calculating. You should not call SetPath in that case.");

                OnPathComplete(path);
            }
            else
            {
                throw new System.ArgumentException(
                    "You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
            }
        }

        protected void ApplyGravity(float deltaTime)
        {
            // Apply gravity
            if (usingGravity)
            {
                float verticalGravity;
                velocity2D += movementPlane.ToPlane(deltaTime * (float.IsNaN(gravity.x) ? Physics.gravity : gravity),
                    out verticalGravity);
                verticalVelocity += verticalGravity;
            }
            else
            {
                verticalVelocity = 0;
            }
        }

        protected Vector2 CalculateDeltaToMoveThisFrame(Vector2 position, float distanceToEndOfPath, float deltaTime)
        {
            if (rvoController != null && rvoController.enabled)
            {
                return movementPlane.ToPlane(
                    rvoController.CalculateMovementDelta(movementPlane.ToWorld(position, 0), deltaTime));
            }

            // Direction and distance to move during this frame
            return Vector2.ClampMagnitude(velocity2D * deltaTime, distanceToEndOfPath);
        }

        public Quaternion SimulateRotationTowards(Vector3 direction, float maxDegrees)
        {
            return SimulateRotationTowards(movementPlane.ToPlane(direction), maxDegrees);
        }

        protected Quaternion SimulateRotationTowards(Vector2 direction, float maxDegrees)
        {
            if (direction != Vector2.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementPlane.ToWorld(direction, 0),
                    movementPlane.ToWorld(Vector2.zero, 1));
                if (orientation == OrientationMode.YAxisForward) targetRotation *= Quaternion.Euler(90, 0, 0);
                return Quaternion.RotateTowards(simulatedRotation, targetRotation, maxDegrees);
            }

            return simulatedRotation;
        }

        public virtual void Move(Vector3 deltaPosition)
        {
            accumulatedMovementDelta += deltaPosition;
        }

        public virtual void FinalizeMovement(Vector3 nextPosition, Quaternion nextRotation)
        {
            if (enableRotation) FinalizeRotation(nextRotation);
            FinalizePosition(nextPosition);
        }

        void FinalizeRotation(Quaternion nextRotation)
        {
            simulatedRotation = nextRotation;
            if (updateRotation)
            {
                tr.rotation = nextRotation;
            }
        }

        void FinalizePosition(Vector3 nextPosition)
        {
            // Use a local variable, it is significantly faster
            Vector3 currentPosition = simulatedPosition;
            bool positionDirty1 = false;

            if (characterLocomotion.characterController != null && characterLocomotion.characterController.enabled &&
                updatePosition)
            {
                tr.position = currentPosition;
                var controller = characterLocomotion.characterController;
                
                Vector3 targetPos = Vector3.Scale(nextPosition, HORIZONTAL_PLANE);
                targetPos += Vector3.up * controller.transform.position.y;
                Vector3 targetDirection = (targetPos - controller.transform.position).normalized;

                float speed = this.CalculateSpeed(targetDirection, controller.isGrounded);
                Quaternion targetRot = this.UpdateRotation(targetDirection);

                this.UpdateAnimationConstraints(ref targetDirection, ref targetRot);

                targetDirection = Vector3.Scale(targetDirection, HORIZONTAL_PLANE) * speed;
                targetDirection += Vector3.up * this.characterLocomotion.verticalSpeed;

                controller.Move(targetDirection * Time.deltaTime);
                controller.transform.rotation = targetRot;
                
                // var dir = (nextPosition - currentPosition).normalized;
                // var tarRot = this.UpdateRotation(dir);
                // this.UpdateAnimationConstraints(ref dir, ref tarRot);
                // characterLocomotion.characterController.Move(
                //     (nextPosition - currentPosition) + accumulatedMovementDelta);
                // TODO: Add this into the clampedPosition calculation below to make RVO better respond to physics
                currentPosition = tr.position;
                if (characterLocomotion.characterController.isGrounded) verticalVelocity = 0;
            }

            // Clamp the position to the navmesh after movement is done
            bool positionDirty2 = false;
            currentPosition = ClampToNavmesh(currentPosition, out positionDirty2);

            if ((positionDirty1 || positionDirty2) && updatePosition)
            {
                tr.position = currentPosition;
            }

            accumulatedMovementDelta = Vector3.zero;
            simulatedPosition = currentPosition;
            UpdateVelocity();
        }

        protected void UpdateVelocity()
        {
            var currentFrame = Time.frameCount;

            if (currentFrame != prevFrame) prevPosition2 = prevPosition1;
            prevPosition1 = position;
            prevFrame = currentFrame;
        }


        static NNConstraint cachedNNConstraint = NNConstraint.Default;

        protected Vector3 ClampToNavmesh(Vector3 position, out bool positionChanged)
        {
            if (constrainInsideGraph)
            {
                cachedNNConstraint.tags = seeker.traversableTags;
                cachedNNConstraint.graphMask = seeker.graphMask;
                cachedNNConstraint.distanceXZ = true;
                var clampedPosition = AstarPath.active.GetNearest(position, cachedNNConstraint).position;

                // We cannot simply check for equality because some precision may be lost
                // if any coordinate transformations are used.
                var difference = movementPlane.ToPlane(clampedPosition - position);
                float sqrDifference = difference.sqrMagnitude;
                if (sqrDifference > 0.001f * 0.001f)
                {
                    velocity2D -= difference * Vector2.Dot(difference, velocity2D) / sqrDifference;

                    if (rvoController != null && rvoController.enabled)
                    {
                        rvoController.SetCollisionNormal(difference);
                    }

                    positionChanged = true;
                    return position + movementPlane.ToWorld(difference);
                }
            }

            positionChanged = false;
            return position;
        }

        protected Vector3 RaycastPosition(Vector3 position, float lastElevation)
        {
            RaycastHit hit;
            float elevation;

            movementPlane.ToPlane(position, out elevation);
            float rayLength = tr.localScale.y * height * 0.5f + Mathf.Max(0, lastElevation - elevation);
            Vector3 rayOffset = movementPlane.ToWorld(Vector2.zero, rayLength);

            if (Physics.Raycast(position + rayOffset, -rayOffset, out hit, rayLength, groundMask,
                QueryTriggerInteraction.Ignore))
            {
                verticalVelocity *= System.Math.Max(0, 1 - 5 * lastDeltaTime);
                return hit.point;
            }

            return position;
        }

        public static readonly Color ShapeGizmoColor = new Color(240 / 255f, 213 / 255f, 30 / 255f);

        protected void Reset()
        {
            ResetShape();
        }

        [System.Obsolete("Use the destination property or the AIDestinationSetter component instead")]
        public Transform target
        {
            get
            {
                var setter = characterLocomotion.character.GetComponent<AIDestinationSetter>();
                return setter != null ? setter.target : null;
            }
            set
            {
                targetCompatibility = null;
                var setter = characterLocomotion.character.GetComponent<AIDestinationSetter>();
                if (setter == null)
                    setter = characterLocomotion.character.gameObject.AddComponent<AIDestinationSetter>();
                setter.target = value;
                destination = value != null
                    ? value.position
                    : new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            }
        }

        void ResetShape()
        {
            var cc = characterLocomotion.characterController;

            if (cc != null)
            {
                radius = cc.radius;
                height = Mathf.Max(radius * 2, cc.height);
            }
        }
    }
}