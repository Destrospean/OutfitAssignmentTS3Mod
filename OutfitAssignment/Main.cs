using System;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.EventSystem;
using Sims3.SimIFace;

namespace Destrospean.OutfitAssignment
{
    [MonoPatcherLib.Plugin]
    public class Main
    {
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
                                    OutfitAssignment.RemoveAllOutfitAssignments(sim.SimDescription);
                                }
                            }
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
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
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
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
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interactionInstance.InstanceActor.SimDescription, interactionInstance, out outfitAssignment) && outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted)
                    {
                        OutfitAssignment.SwitchSimToAssignedOutfit(interactionInstance.InstanceActor, outfitAssignment);
                    }
                };
            InteractionInstanceAdditions.OnInteractionEnded += (interactionInstance) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interactionInstance.InstanceActor.SimDescription, interactionInstance, out outfitAssignment) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded)
                    {
                        interactionInstance.InstanceActor.SwitchToOutfitWithSpin(outfitAssignment.PreviousOutfitCategory, outfitAssignment.PreviousOutfitIndex);
                    }
                };
            InteractionInstanceAdditions.OnWaitForSynchronizationLevel += (interactionInstance, syncLevel) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interactionInstance.InstanceActor.SimDescription, interactionInstance, out outfitAssignment))
                    {
                        switch (syncLevel)
                        {
                            case Sim.SyncLevel.Committed:
                                if (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCommitted)
                                {
                                    OutfitAssignment.SwitchSimToAssignedOutfit(interactionInstance.InstanceActor, outfitAssignment);
                                }
                                break;
                            case Sim.SyncLevel.Completed:
                                if (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCompleted)
                                {
                                    interactionInstance.InstanceActor.SwitchToOutfitWithSpin(outfitAssignment.PreviousOutfitCategory, outfitAssignment.PreviousOutfitIndex);
                                }
                                break;
                            case Sim.SyncLevel.Routed:
                                if (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.SyncLevelRouted)
                                {
                                    OutfitAssignment.SwitchSimToAssignedOutfit(interactionInstance.InstanceActor, outfitAssignment);
                                }
                                break;
                        }
                    }
                };
            InteractionInstanceAdditions.StandardEntryPreCallCallback += (interactionInstance) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interactionInstance.InstanceActor.SimDescription, interactionInstance, out outfitAssignment) && outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardEntry)
                    {
                        OutfitAssignment.SwitchSimToAssignedOutfit(interactionInstance.InstanceActor, outfitAssignment);
                    }
                };
            InteractionInstanceAdditions.StandardExitPostCallCallback += (interactionInstance) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interactionInstance.InstanceActor.SimDescription, interactionInstance, out outfitAssignment) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardExit)
                    {
                        interactionInstance.InstanceActor.SwitchToOutfitWithSpin(outfitAssignment.PreviousOutfitCategory, outfitAssignment.PreviousOutfitIndex);
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
