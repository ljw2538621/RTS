using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

/* Unit Movement created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [RequireComponent(typeof(Unit))]
    public class UnitMovement : MonoBehaviour
    {
        private Unit unit; //the main unit's component

        [SerializeField]
        private bool canMove = true; //can this unit move when ordered by the player?
        public bool CanMove() { return canMove; }

        [SerializeField]
        private bool canFly = false; //when true, the unit will be able to fly over the terrain/map.
        public bool CanFly() { return canFly; }

        [SerializeField]
        private bool useNavAgent = false; //when true, the NavAgent component will be used to move the unit

        [SerializeField, Tooltip("Enable to reset unit's movement when it receives a new command while movement is already active.")]
        private bool resetPendingMovement = false;

        private bool isMoving = false; //Is the player currently moving?
        public bool IsMoving() { return isMoving || pendingMovement; }

        private bool pendingMovement = false; //when the unit is awaiting for the movement manager to assign its movement
        private MovementManager.MovementTask pendingMovementTask; //in case pending movement is enabled, this stores the pending movement task

        //the next two fields allow to avoid having a unit stuck.
        private float mvtCheckTimer; //timer to check whether the unit is moving towards its current target or not.
        private Vector3 lastPosition; //saves the last player's position to compare it later and see if the unit has actually moved.

        private NavMeshAgent navAgent; //Navigation Agent component attached to the unit's object.

        public LayerMask GetAgentAreaMask() { return navAgent.areaMask; } //return the area mask set in the nav agent component, this presents on which areas the unit can and can't move.
        public float GetAgentRadius() { return navAgent.radius; } //get the navigation agent radius which presents the size that the unit is supposed to occupy on the navmesh

        private NavMeshPath navPath; //we'll be using the navigation agent to compute the path and store it here then move the unit manually
        private Queue<Vector3> navPathCornerQueue; //after computing a valid and complete path, the path's corners will be added to this queue.

        private Vector3 finalDestination; //the target destination the unit is moving towards.
        private float stoppingDistance; //the current stopping distance of the unit

        private Vector3 currentDestination; //the current corner that the unit is moving towards in the computed path
        private Vector3 currentDirection; //the current direction of the unit when moving from one corner to another in the path.
        private IEnumerator heightCheckCoroutine; //so that we don't sample the height every frame (since it's very expensive), we do it a couple of times per second in a coroutine

        public bool DestinationReached { set; get; } //when the target his destination, this is set to true.

        private NavMeshObstacle navObstacle; //Navigation Obstacle component that's attached to the unit's object (in case avoidance is enabled).

        //Speed:
        [SerializeField]
        private float speed = 10.0f; //The unit's movement speed.
        private float maxSpeed; //the unit's current max speed.
        public float CurrentSpeed { private set; get; } //the current unit's speed when it's moving
        public float GetMaxSpeed() { return maxSpeed; }
        public void SetMaxSpeed(float newSpeed) {
            maxSpeed = newSpeed;
            if (useNavAgent) //update speed on NavMeshAgent component if that is used
                navAgent.speed = newSpeed;
        }

        //how fast will the unit reach the max speed
        [SerializeField]
        private float acceleration = 10.0f;

        //when building the movement's path, sometimes the built-in navmesh will produce some corners that are too close to each other...
        //...this value determines how far should each corner of the movement path should be from the next one.
        [SerializeField]
        private float minCornerDistance = 0.5f;

        //Movement targets:
        private APC targetAPC; //if the unit is moving towards a APC, it will be held here.
        private Vector3 lastAPCInteractionTarget; //holds the interaction position of the APC when the movement path has been calculated to move towards the APC
        private Portal targetPortal; //if the unit is moving towards a portal, it will be held here.

        //Rotation:
        [SerializeField]
        private bool canMoveRotate = true; //can the unit rotate and move at the same time? 
        bool facingNextDestination = false; //when the unit faces its next target for the first time and is ready to move towards, this is enabled until the next destination in the path is assigned
        [SerializeField]
        private float minMoveAngle = 40.0f; //the close this value to 0.0f, the closer must the unit face its next destination in its path to move

        [SerializeField]
        private bool canIdleRotate = true; //can the unit rotate when not moving?
        [SerializeField]
        private float idleAngularSpeed = 2.0f; //The angular speed of the unit when it is not moving.
        [SerializeField]
        private float mvtAngularSpeed = 2.0f; //How fast does the rotation update?
        private Quaternion rotationTarget; //What is the unit currently looking at?
        private Vector3 lookAtTarget; //Where should the unit look at as soon as it stops moving?
        private Transform lookAtTransform; //the object that this unit should be look at when not moving, if it exists.

        //Target Position Collider:
        [SerializeField]
        private Collider targetPositionCollider = null; //a collider component that represents the position that the unit occupies when idle/will occupy in the future when moving.
        //the following are the methods that can access the target positions collider attributes:
        public void DestroyTargetPositionCollider() { Destroy(targetPositionCollider.gameObject); }
        public void TriggerTargetPositionCollider(bool enable)
        {
            targetPositionCollider.enabled = enable;
        }
        public void UpdateTargetPositionCollider(Vector3 newPosition) { targetPositionCollider.transform.position = newPosition; }
        public void SetTargetPositionColliderParent(Transform parent) { targetPositionCollider.transform.SetParent(parent, true); }

        [SerializeField]
        public UnitComponentTask taskUI = new UnitComponentTask(); //the task that will appear on the task panel when a unit of this component is seelcted

        //Audio:
        [SerializeField]
        private AudioClip mvtOrderAudio = null; //Audio played when the unit is ordered to move.
        public AudioClip GetMvtOrderAudio() { return mvtOrderAudio; }
        [SerializeField]
        private AudioClip mvtAudio = null; //Audio clip played when the unit is moving.
        [SerializeField]
        private AudioClip invalidMvtPathAudio = null; //When the movement path is invalid, this audio is played.

        private GameManager gameMgr;

        public void Init(GameManager gameMgr, Unit unit)
        {
            this.gameMgr = gameMgr;
            this.unit = unit;

            //get the nav agent and nav obstacle components:
            navAgent = GetComponent<NavMeshAgent>();
            navObstacle = GetComponent<NavMeshObstacle>();
            navPath = new NavMeshPath(); //initiate the path:

            Assert.IsNotNull(navAgent, "[UnitMovement] NavMeshAgent must be attached to the unit in order to make it movable.");
            navAgent.enabled = false; //disable it by default
            if (!useNavAgent)
                navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; //always set to none as avoidance will be handled by the nav obstacle
            //if we'll be manually controlling the unit's position, we don't need the nav agent component to update the position
            navAgent.updatePosition = useNavAgent;
            if (useNavAgent) //if we're using the NavAgent to move the unit then set the NavAgent fields:
            {
                navAgent.speed = speed;
                navAgent.acceleration = acceleration;
                navAgent.angularSpeed = mvtAngularSpeed;
                mvtAngularSpeed = idleAngularSpeed;
            }

            if (gameMgr.MvtMgr.IsAvoidanceEnabled() == true) //if avoidance is enabled
            {
                Assert.IsNotNull(navObstacle, "[UnitMovement] Movement avoidance is enabled but the unit doesn't have a NavMeshObstacle component attached to it.");
                navObstacle.enabled = false; //dsiable by default
                navObstacle.carving = true; //enable carving
                navObstacle.carveOnlyStationary = true; //carve only stationary 
            }
            else if (navObstacle != null) //if the nav obstacle is attached but avoidance is disabled or this is a flying unit
                Destroy(navObstacle); //destroy the nav obstacle

            //apply the speed modifier to both the speed and acceleration values
            speed *= this.gameMgr.GetSpeedModifier();
            acceleration *= this.gameMgr.GetSpeedModifier();

            targetPositionCollider.transform.SetParent(null, true); //release the target position collider

            isMoving = false; //the unit is initially not moving.
            pendingMovement = false; //and not in pending movement

            maxSpeed = speed; //set default value for maxSpeed.
            CurrentSpeed = 0.0f; //default value for the current speed
        }

        void FixedUpdate()
        {
            if (unit.HealthComp.IsDead()) //if the unit is already dead
                return; //do not update movement

            if (isMoving == false || (canMoveRotate == false && facingNextDestination == false))
            {
                //deceleration (when either the unit is not moving or rotating to face next destination)
                if (CurrentSpeed > 0.0f)
                    CurrentSpeed -= acceleration * Time.deltaTime;

                if (isMoving == false && canIdleRotate == true && rotationTarget != Quaternion.identity) //can the unit rotate when idle (and the unit is not moving) + there's a valid rotation target
                {
                    if (lookAtTransform != null) //if there's a target object to look at
                        rotationTarget = RTSHelper.GetLookRotation(transform, lookAtTransform.position); //keep updating the rotation target as the target object might keep changing position

                    transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget, Time.deltaTime * idleAngularSpeed); //smoothly update the unit's rotation
                }
            }

            if (isMoving == true && unit.IsInMvtState()) //if the unit is currently moving
            {
                if (navPath == null) //if the unit's path is invalid
                    Stop(); //stop the unit movement.

                else //valid path
                {
                    //only if either the unit can move and rotate at the same time or it can't move and rotate and it's still hasn't faced its next destination in the path
                    if (canMoveRotate == false || facingNextDestination == true)
                    {
                        if (mvtCheckTimer > 0) //movement check timer -> making sure the unit is not stuck at its current position
                            mvtCheckTimer -= Time.deltaTime;
                        if (mvtCheckTimer < 0) //the movement check duration is hardcoded to 2 seconds, while this is only a temporary solution for the units getting stuck issue, a more optimal solution will be soon presented
                        {
                            if (Vector3.Distance(transform.position, lastPosition) <= 0.1f) //if the time passed and we still in the same position (unit is stuck) then stop the movement
                            {
                                Stop();
                                unit.CancelJob(Unit.jobType.all); //cancel all unit jobs.
                            }
                            ReloadMvtCheck();
                        }
                    }

                    MoveAlongPath(); //move the unit along its path using this component

                    //if the unit is currently moving towards an APC, check whether the APC has moved by the stopping distance
                    if (targetAPC != null && Vector3.Distance(lastAPCInteractionTarget, targetAPC.GetInteractionPosition()) > stoppingDistance)
                    {
                        APC nextAPC = targetAPC;
                        Stop(); //stop current movement

                        //we need to recalculate the unit's path so it can move towards the new APC's interaction position
                        nextAPC.Move(unit, false);
                        return;
                    }

                    if (DestinationReached == false) //check if the unit has reached its target position or not
                        DestinationReached = Vector3.Distance(transform.position, finalDestination) <= stoppingDistance;
                }

                if (DestinationReached == true)
                {
                    APC nextAPC = targetAPC;
                    Portal nextPortal = targetPortal;

                    Stop(); //stop the unit mvt

                    if (nextAPC != null) //if the unit is looking to get inside a APC
                    {
                        nextAPC.Add(unit); //get in the APC
                        lookAtTransform = null; //so that the unit won't look at the APC when leaving it
                    }
                    else if (nextPortal != null) //if the unit is moving to get inside a portal
                        nextPortal.Add(unit); //go through the portal
                }
            }
        }

        //a method that attempts to calculate a path using the navigation agent component when given a target position
        public bool CalculatePath(Vector3 targetPosition)
        {
            //disable the nav obstacle component and enable the nav agent component so a path can be computed.
            navObstacle.enabled = false;
            navAgent.enabled = true;
            navAgent.CalculatePath(targetPosition, navPath); //calculate the path here

            if (navPath != null && navPath.status == NavMeshPathStatus.PathComplete) //if the generated path is valid and is fully complete
            {
                if (!useNavAgent) //if we're not using the NavAgent component to move the unit
                    GeneratePathCornerQueue(); //generate the new corners queue
                return true;
            }
            return false;
        }

        //converts a valid path into a corners queue
        public void GeneratePathCornerQueue()
        {
            navPathCornerQueue = new Queue<Vector3>();
            //build the movement corners queue but filter out corners that are too close to each other
            Vector3 lastCorner = Vector3.zero;
            foreach (Vector3 corner in navPath.corners)
            {
                //if we have already a corner in the current path queue
                if (navPathCornerQueue.Count > 0 && Vector3.Distance(corner, lastCorner) <= minCornerDistance)
                    continue;

                navPathCornerQueue.Enqueue(corner);
                lastCorner = corner;
            }

            finalDestination = navPath.corners[navPath.corners.Length - 1]; //the final destination is set to the last corner in the path.

            GetNextCorner(); //set the first corner's info
        }

        //a method that updates the current destination and direction to the next corner in the computed path
        private void GetNextCorner()
        {
            if (navPathCornerQueue.Count > 0) //if there are more corners left in the path
            {
                currentDestination = navPathCornerQueue.Dequeue();
                currentDirection = (currentDestination - transform.position).normalized;
                facingNextDestination = false;
            }
        }

        //moving the unit along its computed path
        private void MoveAlongPath()
        {
            if (!useNavAgent || navAgent.isStopped) //only if not using the NavAgent component for movement or when the nav agent is stopped.
                //update the rotation as long as the unit is moving to look at the next corner in the path queue.
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    RTSHelper.GetLookRotation(transform, currentDestination),
                    Time.deltaTime * mvtAngularSpeed);

            //if the unit can't move before it faces a certain angle towards its next destination in the path
            if(canMoveRotate == false && facingNextDestination == false)
            {
                //keep checking if the angle between the unit and its next destination
                Vector3 lookAt = currentDestination - transform.position;
                lookAt.y = 0.0f;

                //as long as the angle is still over the min allowed movement angle, then do not proceed to keep moving
                if (Vector3.Angle(transform.forward, lookAt) > minMoveAngle)
                {
                    if (useNavAgent && !navAgent.isStopped) //using NavAgent movement? stop it if it's not already stopped
                        navAgent.isStopped = true;
                    return;
                }
                else
                    facingNextDestination = true;
            }

            if (useNavAgent) //not using the NavAgent to move the unit, stop here since the NavAgent component will handle actual movement
            {
                if (currentDestination != navAgent.steeringTarget) //if the next corner/destination on path has been updated
                {
                    facingNextDestination = false; //to trigger checking for correct rotation properties
                    currentDestination = navAgent.steeringTarget; //assign new corner in path
                }

                if (navAgent.isStopped) //using NavAgent movement? start mvt if it has been stopped
                    navAgent.isStopped = false;
                return;
            }

            float currentDistance = (transform.position - currentDestination).sqrMagnitude; //compute the distance between the current unit's position and the next corner in the path

            //if this is the last corner or the player's distance to the next corner reaches a min value, move to the next corner, if not keep moving the player towards the current corner.
            if (currentDistance > stoppingDistance || navPathCornerQueue.Count == 0)
            {
                //acceleration:
                CurrentSpeed = CurrentSpeed >= maxSpeed ? maxSpeed : CurrentSpeed + acceleration * Time.deltaTime;

                //move the unit on the x and z axis using the assigned speed
                transform.position += new Vector3(currentDirection.x * CurrentSpeed * Time.deltaTime, 0.0f, currentDirection.z * CurrentSpeed * Time.deltaTime);
            }
            else
                GetNextCorner();
        }

        //a method that reloads the movement check attributes:
        private void ReloadMvtCheck()
        {
            mvtCheckTimer = 2.0f; //launch the timer
            lastPosition = transform.position; //set this is as the last registered position.
        }

        //a method that stops the unit's movement.
        /// <summary>
        /// Stops the current unit's movement.
        /// </summary>
        /// <param name="prepareNextMovement">When true, not all movement settings will be reset since a new movement command will be followed. Default value: false.</param>
        public void Stop(bool prepareNextMovement = false)
        {
            DisablePendingMovement(true); 
            AudioManager.Stop(unit.AudioSourceComp); //stop the movement audio from playing

            if (isMoving == false) //if the unit is not moving already then stop here
                return;

            isMoving = false; //marked as not moving

            SetMaxSpeed(speed); //set the movement speed to the default one in case it was changed by the Attack on Escape component.

            //unit doesn't have a target APC or Portal to move to anymore
            targetAPC = null;
            targetPortal = null;

            if (!resetPendingMovement && prepareNextMovement) //if we're preparing for another movement command that will follow this call here, then no need to reset some of the params
                return;

            if (!useNavAgent) //if we're using the NavAgent to move the unit, it will handle keeping the height of the unit in check
                StopCoroutine(heightCheckCoroutine);
            else
                navAgent.isStopped = true; //using the NavAgent component? then stop updating the unit's position.

            rotationTarget = RTSHelper.GetLookRotation(transform, lookAtTarget); //update the rotation target using the registered lookAt position.

            UpdateTargetPositionCollider(transform.position); //set the target position's collider to the current unit position since the movement has stopped.

            if (!unit.HealthComp.IsDead()) //if the unit is not dead
                unit.SetAnimState(UnitAnimState.idle); //get into idle state
        }

        //called from the movement manager to enable pending movement
        public void EnablePendingMovement (MovementManager.MovementTask movementTask)
        {
            Stop(true); //stop previous movement to prepare for next one

            DestinationReached = false; //mark destination as not-reached
            TriggerTargetPositionCollider(true); //enable the target position collider here.
            UpdateTargetPositionCollider(movementTask.targetPosition); //set the target position collider at the target position

            //enable pending movement and store the movement task
            pendingMovement = true;
            pendingMovementTask = movementTask;

            CustomEvents.OnUnitMoveAttempt(unit);
        }

        //called to disable pending movement
        public void DisablePendingMovement (bool removeTask)
        {
            if (removeTask == true && pendingMovement == true) //if pending movement was enabled
                gameMgr.MvtMgr.RemoveMovementTask(pendingMovementTask); //remove the pending movement task from the movement manager queue

            pendingMovement = false; //movement is not pending anymore
        }

        //called when after all retries, the movement manager still can't find a valid path for the unit's movement
        public void OnInvalidPath(bool playAudio, bool toIdle)
        {
            if (toIdle == true) //if this invalid path calculation moves the unit into idle state
            {
                Stop(); //if the unit was moving, stop it.
                unit.CancelJob(Unit.jobType.all); //stop all unit's current jobs.
            }
             
            if (playAudio && GameManager.PlayerFactionID == unit.FactionID) //if the local player owns this unit and we can play the invalid path audio
                AudioManager.Play(gameMgr.GetGeneralAudioSource(), invalidMvtPathAudio);
        }

        //Called when a valid and complete path is calculated to start the unit's movement
        public void OnPathComplete(Vector3 targetPosition, Entity target, Vector3 lookAtTarget, float stoppingDistance, InputMode targetMode)
        {
            targetPortal = null;
            targetAPC = null;

            if (InputMode.portal == targetMode) //if the unit is orderd to move towards a portal
                targetPortal = (target as Building).PortalComp; //set target portal
            else if (InputMode.APC == targetMode) //if the unit is orderd to move towards a APC
            {
                targetAPC = (target as FactionEntity).APCComp;
                lastAPCInteractionTarget = targetAPC.GetInteractionPosition();
            }

            //movement settings:
            ReloadMvtCheck();

            this.stoppingDistance = stoppingDistance; //set the movement stopping distance.

            if (!useNavAgent) //only if we're using this component to move the unit
            {
                navAgent.enabled = false; //disable the nav agent component.
                navObstacle.enabled = true; //enable the nav mesh obstacle.
            }

            isMoving = true; //player is now marked as moving
            DestinationReached = false; //destination is not reached by default

            if (unit.GetCurrAnimatorState() == UnitAnimState.moving) //if the unit was already moving, then lock changing the animator state briefly
                unit.LockAnimState = true;

            List<Unit.jobType> jobsToCancel = new List<Unit.jobType>(); //holds the jobs that will be later cancelled
            jobsToCancel.AddRange(new Unit.jobType[] { Unit.jobType.attack, Unit.jobType.healing, Unit.jobType.converting, Unit.jobType.building, Unit.jobType.collecting });

            if (unit.AttackComp && targetMode == InputMode.attack && unit.AttackComp.CanEngageTarget(target as FactionEntity) == ErrorMessage.none) //if the unit is set to attack the target object
                jobsToCancel.Remove(Unit.jobType.attack);

            if (target && target.gameObject.activeInHierarchy == true) //if the unit is moving towards an active object
            {
                Resource targetResource = target as Resource;
                Building targetBuilding = target as Building;
                Unit targetUnit = target as Unit;

                if (targetBuilding && targetBuilding.FactionID == unit.FactionID) //if the target object is a building that belongs to the unit's faction
                {
                    if (unit.BuilderComp != null && unit.BuilderComp.GetTarget() == targetBuilding) //is the unit going to construct this building?
                        jobsToCancel.Remove(Unit.jobType.building);
                    else if (unit.CollectorComp != null && unit.CollectorComp.IsDroppingOff() == true) //is the unit dropping off resources?
                        jobsToCancel.Remove(Unit.jobType.collecting);
                }
                else if (targetUnit) //if the target object is a unit
                {
                    if (unit.HealerComp && targetUnit.FactionID == unit.FactionID && targetAPC == null) //same faction and unit is not going towards a APC -> healing
                    {
                        jobsToCancel.Remove(Unit.jobType.healing);
                        this.stoppingDistance = unit.HealerComp.GetStoppingDistance(); //set the stopping distance to be the max healing distance
                    }
                    else if (unit.ConverterComp && unit.ConverterComp.GetTarget() == targetUnit) //different faction but unit is going for conversion
                    {
                        jobsToCancel.Remove(Unit.jobType.converting);
                        this.stoppingDistance = unit.ConverterComp.GetStoppingDistance(); //set the stopping distance to be the max converting distance
                    }
                }
                else if (targetResource && targetResource == unit.CollectorComp.GetTarget()) //if the target object is a resource and the unit is going to collect it
                    jobsToCancel.Remove(Unit.jobType.collecting);

            }

            unit.CancelJob(jobsToCancel.ToArray()); //cancel the jobs that need to be stopped

            unit.LockAnimState = false; //unlock animation state and play the movement anim
            unit.SetAnimState(UnitAnimState.moving);

            //we're using the UnitMovement component to move units
            if (!useNavAgent)
            {
                if (heightCheckCoroutine == null) //if there's no active height check coroutine
                {
                    heightCheckCoroutine = HeightCheck(gameMgr.MvtMgr.GetHeightCheckReload()); //Start the height check coroutine

                    StartCoroutine(heightCheckCoroutine);
                }

                if (targetMode == InputMode.unitEscape && unit.EscapeComp != null) //if the unit is supposed to perform an attack escape & it has a valid escape component
                {
                    SetMaxSpeed(unit.EscapeComp.GetSpeed());
                }

                //if the current speed is below zero, reset it
                if (CurrentSpeed < 0.0f)
                    CurrentSpeed = 0.0f;
            }
            //if we're usig the NavAgent component to move the unit
            else
            {
                navAgent.isStopped = false; //enable NavAgent movement

                navAgent.stoppingDistance = this.stoppingDistance; //set the stopping distance

                navAgent.SetPath(navPath); //assign the calculated path

                //set final path destination and first corner on path
                finalDestination = navAgent.destination; //set the target destination
                currentDestination = navAgent.steeringTarget; //set the current target destination corner

                facingNextDestination = false; //trigger checking for correct rotation properties.
            }

            UpdateRotationTarget(useNavAgent && !target ? finalDestination : lookAtTarget, target?.transform); //set the rotation target at the destination

            AudioManager.Play(unit.AudioSourceComp, mvtAudio, true);
        }

        //active when the unit is moving, it samples the height of the terrain where the unit is and updates it
        private IEnumerator HeightCheck(float waitTime)
        {
            while (true)
            {
                yield return new WaitForSeconds(waitTime);
                transform.position = new Vector3(
                        transform.position.x,
                        gameMgr.TerrainMgr.SampleHeight(transform.position, GetAgentRadius(), GetAgentAreaMask()),
                        transform.position.z);
            }
        }

        //update the rotation settings of the unit:
        public void UpdateRotationTarget (Vector3 lookAtTarget, Transform lookAtTransform)
        {
            this.lookAtTarget = lookAtTarget;
            this.lookAtTransform = lookAtTransform;
        }
    }
}
