using Destrospean.OutfitAssignment.Interactions;
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
                    OutfitAssignmentUtils.OutfitAssignments.RemoveAll(x => x.SimDescription == null && x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix));
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
            if (gameObject is Sim)
            {
                gameObject.AddInteraction(AssignOutfitCategoryToInteraction.SimSingleton, true);
                gameObject.AddInteraction(AssignOutfitToInteraction.PartialOutfitSimSingleton, true);
                gameObject.AddInteraction(AssignOutfitToInteraction.SimSingleton, true);
                gameObject.AddInteraction(ConfigureOutfitAssignment.SimSingleton, true);
                gameObject.AddInteraction(CopyAssignedOutfitToInteraction.SimSingleton, true);
                gameObject.AddInteraction(CopyOutfitAssignmentToSim.SimSingleton, true);
                gameObject.AddInteraction(EditAssignedOutfit.SimSingleton, true);
                gameObject.AddInteraction(ExtendAssignedOutfitToInteraction.SimSingleton, true);
                gameObject.AddInteraction(UnassignOutfitToInteraction.SimSingleton, true);
            }
            else if (gameObject != null)
            {
                gameObject.AddInteraction(AssignOutfitCategoryToInteraction.GlobalOutfitSingleton, true);
                gameObject.AddInteraction(AssignOutfitCategoryToInteraction.Singleton, true);
                gameObject.AddInteraction(AssignOutfitToInteraction.GlobalOutfitSingleton, true);
                gameObject.AddInteraction(AssignOutfitToInteraction.PartialOutfitSingleton, true);
                gameObject.AddInteraction(AssignOutfitToInteraction.Singleton, true);
                gameObject.AddInteraction(ConfigureOutfitAssignment.GlobalOutfitSingleton, true);
                gameObject.AddInteraction(ConfigureOutfitAssignment.Singleton, true);
                gameObject.AddInteraction(CopyAssignedOutfitToInteraction.GlobalOutfitSingleton, true);
                gameObject.AddInteraction(CopyAssignedOutfitToInteraction.Singleton, true);
                gameObject.AddInteraction(CopyOutfitAssignmentToSim.Singleton, true);
                gameObject.AddInteraction(EditAssignedOutfit.GlobalOutfitSingleton, true);
                gameObject.AddInteraction(EditAssignedOutfit.Singleton, true);
                gameObject.AddInteraction(ExtendAssignedOutfitToInteraction.GlobalOutfitSingleton, true);
                gameObject.AddInteraction(ExtendAssignedOutfitToInteraction.Singleton, true);
                gameObject.AddInteraction(UnassignOutfitToInteraction.GlobalOutfitSingleton, true);
                gameObject.AddInteraction(UnassignOutfitToInteraction.Singleton, true);
            }
        }
    }
}
