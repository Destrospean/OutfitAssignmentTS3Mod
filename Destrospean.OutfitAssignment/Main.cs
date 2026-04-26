using Sims3.Gameplay.Actors;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Tuning = Sims3.Gameplay.Destrospean.OutfitAssignment;

namespace Destrospean.OutfitAssignment
{
    public class Main
    {
        [Tunable]
        protected static bool kInstantiator;

        static Main()
        {
            InteractionInstanceTypeUtils.InitInteractionInstanceTypes();
            EventListener simDescriptionDisposedListener = null,
            simInstantiatedListener = null;
            World.sOnWorldLoadFinishedEventHandler += (sender, e) =>
                {
                    foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
                    {
                        AddInteractions(sim);
                    }
                    simDescriptionDisposedListener = EventTracker.AddListener(EventTypeId.kSimDescriptionDisposed, evt =>
                        {
                            try
                            {
                                Sim sim = evt.TargetObject as Sim;
                                if (sim != null)
                                {
                                    OutfitAssignmentUtils.RemoveAllOutfitAssignments(sim.SimDescription);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                ((IScriptErrorWindow)System.AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                    simInstantiatedListener = EventTracker.AddListener(EventTypeId.kSimInstantiated, evt =>
                        {
                            try
                            {
                                Sim sim = evt.TargetObject as Sim;
                                if (sim != null)
                                {
                                    AddInteractions(sim);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                ((IScriptErrorWindow)System.AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                };
            World.sOnWorldQuitEventHandler += (sender, e) =>
                {
                    EventTracker.RemoveListener(simDescriptionDisposedListener);
                    EventTracker.RemoveListener(simInstantiatedListener);
                    simDescriptionDisposedListener = null;
                    simInstantiatedListener = null;
                };
            InteractionInstanceAdditions.OnInteractedStarted += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment))
                    {
                        switch (outfitAssignment.EntryCallbackType)
                        {
                            case InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted:
                                interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                                break;
                            case InteractionInstanceTypeUtils.CallbackTypes.OutfitChanged:
                                Sims3.SimIFace.CAS.SimOutfit initialOutfit = interactionInstance.InstanceActor.CurrentOutfit;
                                AlarmHandle[] alarms = new AlarmHandle[1];
                                alarms[0] = interactionInstance.InstanceActor.AddAlarmRepeating(Tuning.kOutfitChangedCheckInterval, TimeUnit.Seconds, () =>
                                    {
                                        if (interactionInstance.InstanceActor.CurrentInteraction != interactionInstance)
                                        {
                                            interactionInstance.InstanceActor.RemoveAlarm(alarms[0]);
                                            return;
                                        }
                                        if (initialOutfit != interactionInstance.InstanceActor.CurrentOutfit)
                                        {
                                            interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment, false);
                                            interactionInstance.InstanceActor.RemoveAlarm(alarms[0]);
                                        }
                                    }, outfitAssignment.SpecialOutfitKey, AlarmType.DeleteOnReset);
                                break;
                        }
                    }
                };
            InteractionInstanceAdditions.OnInteractionEnded += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded)
                    {
                        interactionInstance.InstanceActor.SwitchToPreviousOutfit();
                    }
                };
            InteractionInstanceAdditions.OnWaitForSynchronizationLevel += (interactionInstance, syncLevel) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment))
                    {
                        switch (syncLevel)
                        {
                            case Sim.SyncLevel.Committed:
                                if (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCommitted)
                                {
                                    interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                                }
                                break;
                            case Sim.SyncLevel.Completed:
                                if (outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCompleted)
                                {
                                    interactionInstance.InstanceActor.SwitchToPreviousOutfit();
                                }
                                break;
                            case Sim.SyncLevel.Routed:
                                if (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.SyncLevelRouted)
                                {
                                    interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                                }
                                break;
                        }
                    }
                };
            InteractionInstanceAdditions.StandardEntryPreCallCallback += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) && outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardEntry)
                    {
                        interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                    }
                };
            InteractionInstanceAdditions.StandardExitPostCallCallback += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardExit)
                    {
                        interactionInstance.InstanceActor.SwitchToPreviousOutfit();
                    }
                };
        }

        static void AddInteractions(Sim sim)
        {
            if (sim != null)
            {
                sim.AddInteraction(Interactions.AssignOutfitToInteraction.Singleton, true);
                sim.AddInteraction(Interactions.ConfigureOutfitAssignment.Singleton, true);
                sim.AddInteraction(Interactions.CopyAssignedOutfitToInteraction.Singleton, true);
                sim.AddInteraction(Interactions.EditAssignedOutfit.Singleton, true);
                sim.AddInteraction(Interactions.ExtendAssignedOutfitToInteraction.Singleton, true);
                sim.AddInteraction(Interactions.UnassignOutfitToInteraction.Singleton, true);
            }
        }
    }
}
