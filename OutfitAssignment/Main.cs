using System;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
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
                        outfitAssignment.PreviousOutfitCategory = interactionInstance.InstanceActor.CurrentOutfitCategory;
                        outfitAssignment.PreviousOutfitIndex = interactionInstance.InstanceActor.CurrentOutfitIndex;
                        interactionInstance.InstanceActor.SwitchToOutfitWithSpin(Sims3.SimIFace.CAS.OutfitCategories.Special, outfitAssignment.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey)));
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
            InteractionInstanceAdditions.StandardEntryPreCallCallback += (interactionInstance) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interactionInstance.InstanceActor.SimDescription, interactionInstance, out outfitAssignment) && outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardEntry)
                    {
                        outfitAssignment.PreviousOutfitCategory = interactionInstance.InstanceActor.CurrentOutfitCategory;
                        outfitAssignment.PreviousOutfitIndex = interactionInstance.InstanceActor.CurrentOutfitIndex;
                        interactionInstance.InstanceActor.SwitchToOutfitWithSpin(Sims3.SimIFace.CAS.OutfitCategories.Special, outfitAssignment.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey)));
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
                sim.AddInteraction(Interactions.UnassignOutfitToInteraction.Singleton, true);
            }
        }
    }
}
