using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace RTSEngine
{
    [System.Serializable]
    public class TaskPanelUI
    {
        [SerializeField]
        private TaskUI taskUIPrefab = null; //the main task UI prefab from which all task buttons will be created.

        //Task panel categories:
        [SerializeField]
        private GridLayoutGroup[] taskPanelCategories = new GridLayoutGroup[0]; //a list of grid layout groups that present task panel categories.
        //if you want to have one task panel category then have one element only in the array.
        //the ID of each task panel category is its index in this array

        //tasks attributes used for each task panel category and the multiple selection panel
        public class TaskList
        {
            public List<TaskUI> list; //the actual list of the tasks
            public int used; //the amount of currently used tasks
            public int capacity; //the amount of total available tasks
            public Transform parent; //the parent transform of all tasks in the list
        }
        private TaskList[] taskLists = new TaskList[0]; //each task panel has its own list of tasks.

        //In progress tasks:
        [SerializeField]
        private GridLayoutGroup inProgressTaskPanel = null;

        //initilaize the task panel UI component
        GameManager gameMgr;

        public void Init(GameManager gameMgr)
        {
            this.gameMgr = gameMgr;

            //initialise the task list for task panel categories, pending tasks and multiple selection:
            taskLists = new TaskList[taskPanelCategories.Length+2];
            for (int i = 0; i < taskLists.Length; i++)
                taskLists[i] = new TaskList()
                {
                    list = new List<TaskUI>(),
                    used = 0,
                    capacity = 0,
                    //set the parent of the tasks list (either one of the task panel categories or pending task) or the mutiple selection panel for the last task list
                    parent = (i < taskPanelCategories.Length) ? taskPanelCategories[i].transform 
                    : ((i == taskLists.Length-2) ? inProgressTaskPanel.transform : gameMgr.UIMgr.GetMultipleSelectionPanel().transform)
                };

            //custom events to update/hide UI elements:
            CustomEvents.UnitWanderToggled += OnUnitWanderToggled;

            CustomEvents.AttackSwitch += OnAttackSwitch;

            CustomEvents.APCAddUnit += OnAPCUpdated;
            CustomEvents.APCRemoveUnit += OnAPCUpdated;

            CustomEvents.BuildingPlaced += OnBuildingPlacementStopped;
            CustomEvents.BuildingStopPlacement += OnBuildingPlacementStopped;
            CustomEvents.BuildingStartPlacement += OnBuildingPlacementStarted;

            CustomEvents.ResourceGeneratorFull += OnResourceGeneratorUpdated;
            CustomEvents.ResourceGeneratorCollected += OnResourceGeneratorUpdated;

            CustomEvents.TaskLaunched += OnTaskLauncherStatusUpdated;
            CustomEvents.TaskCanceled += OnTaskLauncherStatusUpdated;
            CustomEvents.TaskCompleted += OnTaskLauncherStatusUpdated;
        }

        //called to disable this component
        public void Disable ()
        {
            //stop listening to the custom events
            CustomEvents.UnitWanderToggled -= OnUnitWanderToggled;

            CustomEvents.AttackSwitch -= OnAttackSwitch;

            CustomEvents.APCAddUnit -= OnAPCUpdated;
            CustomEvents.APCRemoveUnit -= OnAPCUpdated;

            CustomEvents.BuildingPlaced -= OnBuildingPlacementStopped;
            CustomEvents.BuildingStopPlacement -= OnBuildingPlacementStopped;
            CustomEvents.BuildingStartPlacement -= OnBuildingPlacementStarted;

            CustomEvents.ResourceGeneratorFull -= OnResourceGeneratorUpdated;
            CustomEvents.ResourceGeneratorCollected -= OnResourceGeneratorUpdated;

            CustomEvents.TaskLaunched -= OnTaskLauncherStatusUpdated;
            CustomEvents.TaskCanceled -= OnTaskLauncherStatusUpdated;
            CustomEvents.TaskCompleted -= OnTaskLauncherStatusUpdated;
        }

        //called each time a unit wandering behavior is toggled
        private void OnUnitWanderToggled (Unit unit)
        {
            //show wander tasks only if this one unit is selected
            if (SelectionManager.IsSelected(unit.GetSelection(), true, true))
                Update();
        }

        //called each time a faction entity switches its attack:
        private void OnAttackSwitch (AttackEntity attackEntity, FactionEntity target, Vector3 targetPosition)
        {
            //only if the source faction entity is the only player entity selected
            if (SelectionManager.IsSelected(attackEntity.FactionEntity.GetSelection(), true, true))
                Update();
        }

        //called each time a unit is added/removed to/from an APC
        private void OnAPCUpdated (APC apc, Unit unit)
        {
            //show APC tasks only if the apc is the only entity selected
            if (SelectionManager.IsSelected(apc.FactionEntity.GetSelection(), true, true))
                Update();
        }

        //called each time a building placement stops or when a building is placed
        private void OnBuildingPlacementStopped (Building building)
        {
            if (building.FactionID == GameManager.PlayerFactionID) //if the building belongs to the local player
                Update(); //update tasks to re-display builder units tasks
        }

        //called each time a building placement starts
        private void OnBuildingPlacementStarted (Building building)
        {
            if (building.FactionID == GameManager.PlayerFactionID) //if this is the player faction
            {
                gameMgr.UIMgr.HideTooltip();
                Hide();
            }

        }

        //called each time a player owned resource generator is either full or collected
        private void OnResourceGeneratorUpdated (ResourceGenerator resourceGenerator, int generatorID)
        {
            if(SelectionManager.IsSelected(resourceGenerator.building.GetSelection(), true, true)) //if the resource generator's building is the only selected entity
            {
                gameMgr.UIMgr.HideTooltip(); //hide the tooltip in case resource have been collected and the task is gone
                Update();
            }
        }

        //called each time a task launcher status is updated (task added, cancelled or completed)
        private void OnTaskLauncherStatusUpdated (TaskLauncher taskLauncher, int taskID, int taskQueueID)
        {
            //only show the task launcher tasks if the task launcher is the only player entity selected
            if(SelectionManager.IsSelected(taskLauncher.FactionEntity.GetSelection(), true, true))
            {
                Update();

                if (taskLauncher.GetTask(taskID).IsAvailable == false) //if the task is no longer available
                    gameMgr.UIMgr.HideTooltip(); //hide the tooltip
            }
        }

        public TaskUI Add (TaskUIAttributes attributes, int categoryIndex, TaskUI.Types type = TaskUI.Types.idle)
        {
            //if the task type is multiple selection, get the last element in the tasks list array, else get the task lists category index.
            TaskList currTaskList = (type == TaskUI.Types.multipleSelectionIndiv || type == TaskUI.Types.multipleSelectionMul) ? taskLists[taskLists.Length - 1]
                : ((type == TaskUI.Types.inProgress) ? taskLists[taskLists.Length - 2] : taskLists[categoryIndex]);

            TaskUI nextTask = (currTaskList.used < currTaskList.capacity) ? currTaskList.list[currTaskList.used] : null; 
            if(nextTask == null) //if all tasks are used and none is available
            {
                nextTask = Object.Instantiate(taskUIPrefab.gameObject).GetComponent<TaskUI>(); //create and init new task UI
                nextTask.Init(gameMgr);

                nextTask.transform.SetParent(currTaskList.parent, true); //set its parent
                nextTask.transform.localScale = Vector3.one;

                currTaskList.list.Add(nextTask); //add a new task to the list
                currTaskList.capacity++; //increment capacity
            }

            currTaskList.used++; //increment amount of used tasks.

            nextTask.Reload(attributes, type); //initialize the task.

            return nextTask;
        }

        //a method that hides all task panel and in progress task panel tasks or the multiple selection tasks
        public void Hide(bool multipleSelection = false)
        {
            //determine the start and finish values of the for loop counter depending on whether we want to hide multiple selection or normal tasks
            //multiple selection -> only last element of the taskLists array
            //task panel tasks/in progress tasks -> rest of the elements
            int start = (multipleSelection) ? taskLists.Length-1 : 0;
            int finish = (multipleSelection) ? taskLists.Length : taskLists.Length - 1;
            for (int i = start; i < finish; i++)
            {
                foreach (TaskUI task in taskLists[i].list)
                    task.Disable(); //hide task

                taskLists[i].used = 0; //reset the used tasks counter
            }
        }

        //update the task panel:
        public void Update ()
        {
            Hide(); //hide currently active tasks

            List<Unit> selectedUnits = gameMgr.SelectionMgr.Selected.GetEntitiesList(EntityTypes.unit, false, true).Cast<Unit>().ToList(); //get selected units from player faction
            List<Building> selectedBuildings = gameMgr.SelectionMgr.Selected.GetEntitiesList(EntityTypes.building, false, true).Cast<Building>().ToList(); //get selected buildings from player faction

            if(selectedUnits.Count + selectedBuildings.Count == 1) //if only one faction entity is selected
            {
                FactionEntity factionEntity = selectedUnits.Count == 1 ? selectedUnits[0] as FactionEntity : selectedBuildings[0] as FactionEntity; //get it

                if (factionEntity.EntityHealthComp.IsDead() == true) //if dead, then do not show any tasks
                    return;

                UpdateAPCTasks(factionEntity, factionEntity.APCComp); //show APC tasks only if one faction entity is selected
                UpdateTaskLauncherTasks(factionEntity, factionEntity.TaskLauncherComp); //show task launcher tasks only if one faction entity is selected
                UpdateMultipleAttackTasks(factionEntity, factionEntity.MultipleAttackMgr); //show the multiple attack component only if one faction entity is selected
            }

            AttackEntity attackComp = null; //when != null, then all selected units/buildings have an attack component

            if(selectedBuildings.Count > 0) //more than one building is selected
            {
                foreach(Building building in selectedBuildings) //see if all buildings are placed and built and all have an attack component
                {
                    if (building.Placed == false || building.IsBuilt == false || building.HealthComp == false) //if one of the buildings is not built or placed
                        return;

                    attackComp = building.AttackComp;

                    if (attackComp == null) //no attack component detected here, do not continue
                        break;
                }

                if (selectedBuildings.Count == 1 && selectedUnits.Count == 0) //only one building selected
                    UpdateResourceGeneratorTasks(selectedBuildings[0], selectedBuildings[0].GeneratorComp); //show the resource generator tasks
            }

            if(selectedUnits.Count > 0) //units are selected
            {
                //see if all selected units have the following components
                Builder builderComp = selectedUnits[0].BuilderComp;
                ResourceCollector collectorComp = selectedUnits[0].CollectorComp;
                Healer healerComp = selectedUnits[0].HealerComp;
                Converter converterComp = selectedUnits[0].ConverterComp;
                attackComp = selectedUnits[0].AttackComp;

                //make sure all selected units have the same components
                foreach(Unit u in selectedUnits)
                {
                    if (u.HealthComp.IsDead() == true) //if one of the unit is dead
                        return; //do not continue

                    if (u.BuilderComp == null)
                        builderComp = null;
                    if (u.CollectorComp == null)
                        collectorComp = null;
                    if (u.HealerComp == null)
                        healerComp = null;
                    if (u.ConverterComp == null)
                        converterComp = null;
                    if (u.AttackComp == null)
                        attackComp = null;
                }

                UpdateUnitComponentTask(selectedUnits[0], selectedUnits[0].MovementComp.taskUI, TaskTypes.movement);

                if (builderComp != null)
                {
                    UpdateUnitComponentTask(selectedUnits[0], builderComp.taskUI, TaskTypes.build);

                    if(selectedBuildings.Count == 0) //only if no buildings are selected can we show the buildings to place
                        UpdateBuilderTasks(selectedUnits[0], builderComp);
                }
                if (collectorComp != null)
                    UpdateUnitComponentTask(selectedUnits[0], collectorComp.taskUI, TaskTypes.collectResource);
                if (healerComp != null)
                    UpdateUnitComponentTask(selectedUnits[0], healerComp.taskUI, TaskTypes.heal);
                if (converterComp != null)
                    UpdateUnitComponentTask(selectedUnits[0], converterComp.taskUI, TaskTypes.convert);

                if (selectedUnits.Count == 1 && selectedBuildings.Count == 0) //if there's only one unit and no buildings selected
                    UpdateWanderTasks(selectedUnits[0], selectedUnits[0].WanderComp);
            }

            if (attackComp != null) //if all selected buildings and units have an attack component
                UpdateUnitComponentTask(attackComp.FactionEntity, attackComp.taskUI, TaskTypes.attack);
        }

        public void UpdateAPCTasks (FactionEntity sourceEntity, APC APCComp)
        {
            if (sourceEntity == null || APCComp == null)
                return;

            if (APCComp.IsEmpty() == false) //if there are units stored inside the APC
            {
                if (APCComp.CanEject(true) == true) //if we're allowed to eject all units at once
                    Add(new TaskUIAttributes {
                        type = TaskTypes.APCEjectAll,
                        source = sourceEntity,
                        icon = APCComp.GetEjectAllUnitsIcon() },
                        APCComp.GetEjectTaskCategory(true));

                if (APCComp.CanEject(false) == true && APCComp.GetCount() > 0) //if we're allowed to eject single units and there are actual units stored.
                    for (int unitID = 0; unitID < APCComp.GetCount(); unitID++)
                        Add(new TaskUIAttributes
                        {
                            ID = unitID,
                            source = sourceEntity,
                            type = TaskTypes.APCEject,
                            icon = APCComp.GetStoredUnit(unitID).GetIcon()
                        },
                        APCComp.GetEjectTaskCategory(false));
            }
            if (APCComp.IsFull() == false && APCComp.CanCallUnits() == true) //if there are still free slots and the APC can call units
                Add(new TaskUIAttributes
                {
                    type = TaskTypes.APCCall,
                    source = sourceEntity,
                    icon = APCComp.GetCallUnitsIcon()
                },
                 APCComp.GetCallUnitsTaskCategory());
        }

        public void UpdateTaskLauncherTasks (FactionEntity sourceEntity, TaskLauncher taskLauncher)
        {
            if (taskLauncher == null || taskLauncher.Initiated == false || taskLauncher.GetTasksCount() == 0) //if the task launcher is invalid or the source can't manage a task
                return;
            
            for(int taskID = 0; taskID < taskLauncher.GetTasksCount(); taskID++) //go through all tasks
            {
                FactionEntityTask task = taskLauncher.GetTask(taskID);
                if (task.IsEnabled() == true)
                {
                    TaskUI taskUI = Add(new TaskUIAttributes
                    {
                        ID = taskID,
                        type = task.GetTaskType(),
                        icon = task.GetIcon(),
                        source = sourceEntity,
                        taskLauncher = taskLauncher,
                        //if this is a unit creation task, check if it has reached its limit and change task ui image color accordinly
                        color = task.GetTaskType() == TaskTypes.createUnit && sourceEntity.FactionMgr.HasReachedLimit(task.UnitCode, task.UnitCategory) == true ? Color.red : Color.white

                    }, task.GetTaskPanelCategory());

                }
            }

            UpdateInProgressTasks(taskLauncher); //show the in progress tasks
        }

        public void UpdateInProgressTasks (TaskLauncher taskLauncher)
        {
            if (taskLauncher == null || taskLauncher.GetTaskQueueCount() == 0) //if the task launcher is invalid or there are no tasks in the queue
                return;

            for(int progressTaskID = 0; progressTaskID < taskLauncher.GetTaskQueueCount(); progressTaskID++)
            {
                Add(new TaskUIAttributes
                {
                    ID = progressTaskID,
                    type = TaskTypes.cancelPendingTask,
                    taskLauncher = taskLauncher,
                    source = taskLauncher.FactionEntity,
                    icon = taskLauncher.GetPendingTaskIcon(progressTaskID)
                }, 0, TaskUI.Types.inProgress);
            }
        }

        public void UpdateMultipleAttackTasks (FactionEntity sourceEntity, MultipleAttackManager multipleAttackComp)
        {
            if (multipleAttackComp == null)
                return;

            for(int attackID = 0; attackID < multipleAttackComp.AttackEntities.Length; attackID++) //go through all the attack entities
            {
                AttackEntity attackComp = multipleAttackComp.AttackEntities[attackID];
                if(!attackComp.IsLocked && !attackComp.IsActive()) //as long as the attack entity is not active and not locked, show a task to activate it:
                {
                    TaskUI taskUI = Add(new TaskUIAttributes
                    {
                        ID = attackID,
                        type = TaskTypes.attackTypeSelection,
                        icon = attackComp.GetIcon(),
                        source = sourceEntity
                    },  multipleAttackComp.GetTaskPanelCategory());

                    if(attackComp.CoolDownActive == true) //if the attack type is in cool down mode
                    {
                        Color nextColor = taskUI.GetComponent<Image>().color;
                        nextColor.a = 0.5f; //make it semi-transparent to indicate cooldown to player
                        taskUI.GetComponent<Image>().color = nextColor;
                    }
                }
            }
        }

        public void UpdateUnitComponentTask (FactionEntity sourceEntity, UnitComponentTask unitCompTask, TaskTypes type)
        {
            if (unitCompTask.enabled == false)
                return;

            Add(new TaskUIAttributes
            {
                source = sourceEntity,
                type = type,
                icon = unitCompTask.icon,
                unitComponentTask = true
            },  unitCompTask.panelCategory);
        }

        public void UpdateResourceGeneratorTasks (FactionEntity sourceEntity, ResourceGenerator generatorComp)
        {
            if (sourceEntity == null || generatorComp == null)
                return;

            for(int generatorID = 0; generatorID < generatorComp.GetGeneratorsLength(); generatorID++)
            {
                ResourceGenerator.Generator generator = generatorComp.GetGenerator(generatorID);
                if(generator.IsMaxAmountReached() == true) //only display the resource collection task if the maximum amount is reached
                {
                    Add(new TaskUIAttributes
                    {
                        ID = generatorID,
                        icon = generator.GetTaskIcon(),
                        source = sourceEntity,
                        type = TaskTypes.generateResource
                    },  generatorComp.GetTaskPanelCategory());

                }
            }
        }

        public void UpdateBuilderTasks (Unit sourceUnit, Builder builderComp)
        {
            if (sourceUnit == null || builderComp == null)
                return;

            int buildingID = -1;
            foreach(Building building in gameMgr.PlacementMgr.GetBuildings()) //go through all the placeable buildings 
            { 
                buildingID++;

                if (!builderComp.CanBuild(building)) //if the next building can't be constructed by the selected builders
                    continue; //move to then next one

                TaskUI taskUI = Add(new TaskUIAttributes
                {
                    ID = buildingID,
                    icon = building.GetIcon(),
                    source = sourceUnit,
                    type = TaskTypes.placeBuilding,
                    //if the building type has reached its faction limit then show it with the color red
                    color = sourceUnit.FactionMgr.HasReachedLimit(building.GetCode(), building.GetCategory()) ? Color.red : Color.white
                },  building.GetTaskPanelCategory());

            }
        }

        public void UpdateWanderTasks (Unit sourceUnit, Wander wanderComp)
        {
            if (sourceUnit == null || wanderComp == null)
                return;

            Add(new TaskUIAttributes
            {
                type = TaskTypes.toggleWander,
                icon = wanderComp.GetIcon(),
                source = sourceUnit
            },  wanderComp.GetTaskPanelCategory());
        }
    }
}
