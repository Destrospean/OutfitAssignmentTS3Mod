using Sims3.Gameplay.Actors;
using Sims3.Gameplay.EventSystem;
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
            InteractionInstanceAdditions.ReplaceMethod(typeof(Sim).GetMethod("GetCategoryAndIndexToUse", System.Array.ConvertAll(typeof(Replacements).GetMethod("GetCategoryAndIndexToUse").GetParameters(), x => x.ParameterType)), typeof(Replacements).GetMethod("GetCategoryAndIndexToUse"));
            InteractionInstanceAdditions.ReplaceMethod(typeof(Sim).GetMethod("SwitchToOutfitWithSpin", System.Array.ConvertAll(typeof(Replacements).GetMethod("SwitchToOutfitWithSpin").GetParameters(), x => x.ParameterType)), typeof(Replacements).GetMethod("SwitchToOutfitWithSpin"));
            EventListener simAgeTransitionListener = null,
            simDescriptionDisposedListener = null,
            simInstantiatedListener = null;
            World.sOnObjectPlacedInLotEventHandler += (sender, e) =>
                {
                    World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = e as World.OnObjectPlacedInLotEventArgs;
                    if (onObjectPlacedInLotEventArgs != null)
                    {
                        AddInteractions(Sims3.Gameplay.Abstracts.GameObject.GetObject(onObjectPlacedInLotEventArgs.ObjectId) as Sims3.Gameplay.Objects.ShelvesStorage.Dresser);
                    }
                };
            World.sOnWorldLoadFinishedEventHandler += (sender, e) =>
                {
                    foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
                    {
                        AddInteractions(sim);
                    }
                    foreach (Sims3.Gameplay.Objects.ShelvesStorage.Dresser dresser in Sims3.Gameplay.Queries.GetObjects<Sims3.Gameplay.Objects.ShelvesStorage.Dresser>())
                    {
                        AddInteractions(dresser);
                    }
                    simAgeTransitionListener = EventTracker.AddListener(EventTypeId.kSimAgeTransition, evt =>
                        {
                            try
                            {
                                Sim sim = evt.TargetObject as Sim;
                                if (sim != null)
                                {
                                    OutfitAssignmentUtils.RemoveAllOutfitAssignments(sim.SimDescription, true);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                ((IScriptErrorWindow)System.AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
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
                    EventTracker.RemoveListener(simAgeTransitionListener);
                    EventTracker.RemoveListener(simDescriptionDisposedListener);
                    EventTracker.RemoveListener(simInstantiatedListener);
                    simAgeTransitionListener = null;
                    simDescriptionDisposedListener = null;
                    simInstantiatedListener = null;
                };
            InteractionInstanceAdditions.OnInteractedStarted += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || interactionInstance.InstanceActor.SimDescription.TryGetGlobalOutfitAssignment(interactionInstance, out outfitAssignment)))
                    {
                        if (interactionInstance.InstanceActor.CurrentOutfitCategory != Sims3.SimIFace.CAS.OutfitCategories.Special || interactionInstance.InstanceActor.CurrentOutfitIndex != interactionInstance.InstanceActor.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey)))
                        {
                            OutfitAssignmentUtils.PreviousOutfits.RemoveAll(x => x.SimDescription == interactionInstance.InstanceActor.SimDescription);
                            OutfitAssignmentUtils.PreviousOutfits.Add(new OutfitAssignmentUtils.Outfit
                                {
                                    Category = interactionInstance.InstanceActor.CurrentOutfitCategory,
                                    Index = interactionInstance.InstanceActor.CurrentOutfitIndex,
                                    SimDescription = interactionInstance.InstanceActor.SimDescription
                                });
                        }
                        if (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted)
                        {
                            interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                        }
                    }
                };
            InteractionInstanceAdditions.OnInteractionEnded += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || interactionInstance.InstanceActor.SimDescription.TryGetGlobalOutfitAssignment(interactionInstance, out outfitAssignment)) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded)
                    {
                        interactionInstance.InstanceActor.SwitchToPreviousOutfit();
                    }
                };
            InteractionInstanceAdditions.OnWaitForSynchronizationLevel += (interactionInstance, syncLevel) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || interactionInstance.InstanceActor.SimDescription.TryGetGlobalOutfitAssignment(interactionInstance, out outfitAssignment)))
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
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || interactionInstance.InstanceActor.SimDescription.TryGetGlobalOutfitAssignment(interactionInstance, out outfitAssignment)) && (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.OutfitChanged || outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardEntry))
                    {
                        interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                    }
                };
            InteractionInstanceAdditions.StandardExitPostCallCallback += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || interactionInstance.InstanceActor.SimDescription.TryGetGlobalOutfitAssignment(interactionInstance, out outfitAssignment)) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardExit)
                    {
                        interactionInstance.InstanceActor.SwitchToPreviousOutfit();
                    }
                };
        }

        static void AddInteractions(Sims3.Gameplay.Abstracts.GameObject gameObject)
        {
            if (gameObject != null)
            {
                gameObject.AddInteraction(Interactions.AssignOutfitCategoryToInteraction.Singleton, true);
                gameObject.AddInteraction(Interactions.AssignOutfitToInteraction.Singleton, true);
                gameObject.AddInteraction(Interactions.ConfigureOutfitAssignment.Singleton, true);
                gameObject.AddInteraction(Interactions.CopyAssignedOutfitToInteraction.Singleton, true);
                gameObject.AddInteraction(Interactions.EditAssignedOutfit.Singleton, true);
                gameObject.AddInteraction(Interactions.ExtendAssignedOutfitToInteraction.Singleton, true);
                gameObject.AddInteraction(Interactions.UnassignOutfitToInteraction.Singleton, true);
            }
            if (gameObject is Sim)
            {
                gameObject.AddInteraction(Interactions.AssignOutfitToInteraction.PartialOutfitSingleton, true);
                gameObject.AddInteraction(Interactions.CopyOutfitAssignmentToSim.Singleton, true);
            }
        }
    }
}
