using Sims3.SimIFace;

namespace Destrospean.OutfitAssignment
{
    public class Tasks
    {
        public delegate void Action();

        public class TaskGenericAction : Task
        {
            Action[] mActions = null;

            public TaskGenericAction(params Action[] actions)
            {
                mActions = actions;
            }

            public override void Simulate()
            {
                try
                {
                    if (mActions != null)
                    {
                        foreach (Action action in mActions)
                        {
                            action();
                        }
                    }
                }
                catch
                {
                }
                finally 
                {
                    Simulator.DestroyObject(ObjectId);
                }
            }

            public void Start()
            {
                Simulator.AddObject(this);
            }

            public static void Start(params Action[] actions)
            {
                Simulator.AddObject(new TaskGenericAction(actions));
            }
        }
    }
}
