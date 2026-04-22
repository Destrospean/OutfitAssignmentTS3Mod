using Sims3.Gameplay.Actors;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;

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
                    if (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment))
                    {
                        switch (outfitAssignment.EntryCallbackType)
                        {
                            case InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted:
                                interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                                break;
                            case InteractionInstanceTypeUtils.CallbackTypes.OutfitChanged:
                                Sims3.SimIFace.CAS.OutfitCategories initialOutfitCategory = interactionInstance.InstanceActor.CurrentOutfitCategory;
                                int initialOutfitIndex = interactionInstance.InstanceActor.CurrentOutfitIndex,
                                specialOutfitIndex = outfitAssignment.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey));
                                if (initialOutfitCategory != Sims3.SimIFace.CAS.OutfitCategories.Special || initialOutfitIndex != specialOutfitIndex)
                                {
                                    OutfitAssignmentUtils.PreviousOutfits.RemoveAll(x => x.SimDescription == outfitAssignment.SimDescription);
                                    OutfitAssignmentUtils.PreviousOutfits.Insert(0, new OutfitAssignmentUtils.Outfit
                                        {
                                            Category = initialOutfitCategory,
                                            Index = initialOutfitIndex,
                                            SimDescription = outfitAssignment.SimDescription
                                        });
                                }
                                AlarmHandle[] alarms = new AlarmHandle[1];
                                alarms[0] = interactionInstance.InstanceActor.AddAlarmRepeating(1, TimeUnit.Minutes, () =>
                                    {
                                        if (interactionInstance.InstanceActor.CurrentInteraction != interactionInstance)
                                        {
                                            interactionInstance.InstanceActor.RemoveAlarm(alarms[0]);
                                            return;
                                        }
                                        if (initialOutfitCategory != interactionInstance.InstanceActor.CurrentOutfitCategory || initialOutfitIndex != interactionInstance.InstanceActor.CurrentOutfitIndex)
                                        {
                                            interactionInstance.InstanceActor.SwitchToOutfitWithoutSpin(Sims3.SimIFace.CAS.OutfitCategories.Special, specialOutfitIndex);
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
                    if (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded)
                    {
                        interactionInstance.InstanceActor.SwitchToPreviousOutfit();
                    }
                };
            InteractionInstanceAdditions.OnWaitForSynchronizationLevel += (interactionInstance, syncLevel) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment))
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
                                if (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCompleted)
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
                    if (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) && outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardEntry)
                    {
                        interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                    }
                };
            InteractionInstanceAdditions.StandardExitPostCallCallback += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardExit)
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
                sim.AddInteraction(Interactions.ExtendAssignedOutfitToInteraction.Singleton, true);
                sim.AddInteraction(Interactions.UnassignOutfitToInteraction.Singleton, true);
            }
        }
    }
}
