using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/* Unit Task UI script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    //the task UI attributes that get assigned by the UI manager
    public struct TaskUIAttributes
    {
        public int ID;
        public TaskTypes type;
        public TaskLauncher taskLauncher;
        public Sprite icon;
        public Entity source;
        public List<Entity> sourceList;
        public bool unitComponentTask;
        public Color color;
    }

    [RequireComponent(typeof(Button))]
	public class TaskUI : MonoBehaviour {

        private TaskUIAttributes attributes;

        //type of the task UI component:
        /* 
         * idle -> normal task in the task panel
         * inProgress -> in progress task that needs to keep displaying the progress of its pending task
         * multipleSelection -> a task for a unit that needs to monitor and display its health in the multiple selection panel
         * */
        public enum Types {idle, inProgress, multipleSelectionIndiv, multipleSelectionMul};
        private Types type = Types.idle;

        //empty and full progress bars for pending tasks only.
        [SerializeField]
        private ProgressBarUI progressBar = new ProgressBarUI();

        //the task UI button components
        Image image;
        Button button;
        [SerializeField]
        private Text label = null; //in a multipleSelectioMul type, this displays the amount of selected entities of the type represented by this task

        GameManager gameMgr;

        //initialize the task UI:
        public void Init (GameManager gameMgr)
        {
            this.gameMgr = gameMgr;

            image = GetComponent<Image>();
            button = GetComponent<Button>();

            progressBar.Init(); //initialize the progress bar
        }
        
        //reloads the attributes of the task UI
        public void Reload(TaskUIAttributes attributes, Types type)
        {
            this.attributes = attributes;
            this.type = type;

            if (attributes.source && attributes.source as FactionEntity == null) //if there's a source and it is not a faction entity (i.e: resource)
                this.type = Types.idle; //force type to be idle

            switch(this.type)
            {
                case Types.idle:
                case Types.multipleSelectionMul:
                    progressBar.Toggle(false);
                    break;
                    
                //if it's an in progress task or a multiple selection task then show the progress bars
                case Types.multipleSelectionIndiv:
                case Types.inProgress:

                    progressBar.Toggle(true);
                    //default size and position of the progress bar:
                    progressBar.Update(0.0f);
                    break;
            }

            image.sprite = attributes.icon; //set the image icon

            if(attributes.color != Color.clear) //make sure a valid color is entered (not one that's fully transparent)
                image.color = attributes.color; //set the icon's color

            //enable the task UI button
            this.enabled = true;
            image.enabled = true;
            button.enabled = true;

            //only if this a multiple selection task for multiple entities, then show their amount
            label.text = (type == Types.multipleSelectionMul) ? attributes.sourceList.Count.ToString() : "";
        }

        public void Disable()
        {
            this.enabled = false;
            image.enabled = false;
            button.enabled = false;
            progressBar.Toggle(false);
            label.text = "";
        }

        private void Update()
        {
            float nextBarLength = 0.0f;

            switch(type)
            {
                case Types.idle:
                case Types.multipleSelectionMul:
                    return;

                case Types.inProgress:
                    if (attributes.ID != 0) //if this is not the first in progress task
                        return; //do not proceed

                    //update the progress bar to show the pending task progress:
                    nextBarLength = attributes.taskLauncher.GetPendingTaskProgress(attributes.ID);
                    break;
                case Types.multipleSelectionIndiv: //if this a multiple selection task used for one entity instance
                    //update the progress bar to show the selected unit's health:
                    nextBarLength = (attributes.source as FactionEntity).EntityHealthComp.CurrHealth / (float)(attributes.source as FactionEntity).EntityHealthComp.MaxHealth;
                    break;
            }

            progressBar.Update(nextBarLength);
        }

        //method to launch the task:
        public void OnTaskClick ()
		{
            gameMgr.TaskMgr.AddTask(attributes, true);
		}
       
        //this method is called whenever the mouse hovers over a task button: 
        public void ShowTaskInfo()
        {
            if (!enabled) //if this component is not enabled do not show task tooltip
                return;

            ResourceInput[] requiredResources = new ResourceInput[0];
            ResourceInput[] completeResources = new ResourceInput[0];

            string message = "";

            switch (attributes.type) //display a different message depending on the type of the task
            {
                case TaskTypes.deselectIndiv: //deselect a currently selected unit
                case TaskTypes.deselectMul:
                    return; //do not show anything

                case TaskTypes.generateResource: //generating resource task.

                    ResourceGenerator.Generator generator = (attributes.source as Building).GeneratorComp.GetGenerator(attributes.ID);
                    message += "Maximum amount reached! Click to collect " + generator.GetCurrAmount().ToString() + " of " + generator.GetResourceName();
                    break;
                case TaskTypes.APCEject: //APC release task.

                    message += "Eject unit: " + (attributes.source as FactionEntity).APCComp.GetStoredUnit(attributes.ID).GetName() + " from the " + (attributes.source as FactionEntity).GetName() + ".";
                    break;
                case TaskTypes.APCEjectAll: //APC release task.

                    message += "Eject all units inside the " + (attributes.source as FactionEntity).GetName() + ".";
                    break;
                case TaskTypes.APCCall: //apc calling units

                    message += "Call units to get into the " + (attributes.source as FactionEntity).GetName() + ".";
                    break;
                case TaskTypes.placeBuilding:

                    //Get the building associated with this task ID:
                    Building currentBuilding = gameMgr.PlacementMgr.GetBuilding(attributes.ID);

                    message += currentBuilding.GetName() + ": " + currentBuilding.GetDescription();

                    requiredResources = currentBuilding.GetResources();

                    if (currentBuilding.GetRequiredBuildings().Count > 0) //if the building requires other buildings to be placed
                    {
                        message += "\n<b>Required Buildings:</b>";
                        foreach(Building.RequiredBuilding b in currentBuilding.GetRequiredBuildings())
                            message += " " + b.GetName() + " -";

                        message = message.Substring(0, message.Length - " -".Length);
                    }
                    break;

                case TaskTypes.attackTypeSelection:

                    message += "Switch attack type.";
                    break;
                case TaskTypes.toggleWander:

                    message += "Toggle wandering.";
                    break;
                case TaskTypes.cancelPendingTask:

                    message += "Cancel pending task.";
                    break;
                case TaskTypes.movement:

                    message += "Move unit.";
                    break;
                case TaskTypes.build:

                    message += "Construct building.";
                    break;
                case TaskTypes.collectResource:

                    message += "Collect resource.";
                    break;
                case TaskTypes.attack:

                    message += "Attack enemy unit/building.";
                    break;
                case TaskTypes.heal:

                    message += "Heal friendly unit.";
                    break;

                case TaskTypes.convert:

                    message += "Convert enemy unit.";
                    break;
                default:
                    if(attributes.taskLauncher != null)
                    {
                        requiredResources = attributes.taskLauncher.GetTask(attributes.ID).GetRequiredResources();
                        completeResources = attributes.taskLauncher.GetTask(attributes.ID).GetCompleteResources();
                        message += attributes.taskLauncher.GetTask(attributes.ID).GetDescription();
                    }
                    break;

            }

            //display the required and complete resources:
            AddResourceInfo(requiredResources, "Required Resources", ref message);
            AddResourceInfo(completeResources, "Complete Resources", ref message);

            //show the task info on the tooltip:
            gameMgr.UIMgr.ShowTooltip(message);
        }

        //a method that hides the task info tooltip panel
        public void HideTaskInfo ()
        {
            gameMgr.UIMgr.HideTooltip();
        }

        //method used to add resource info to a message to be displayed later in the tooltip
        private void AddResourceInfo (ResourceInput[] resources, string title, ref string message)
        {
            if (resources.Length > 0)
            {
                message += $"\n<b>{title}:</b>";

                foreach (ResourceInput r in resources)
                    message += " " + r.Name + ": " + r.Amount.ToString() + " -";

                message = message.Substring(0, message.Length - " -".Length);
            }
        }
    }
}