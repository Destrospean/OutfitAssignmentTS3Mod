using Sims3.Gameplay.Actors;
using Sims3.Gameplay.EventSystem;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Tuning = Sims3.Gameplay.Destrospean.OutfitAssignment;

namespace Destrospean.OutfitAssignment
{
    public class Main
    {
        [Tunable]
        protected static bool kInstantiator;

        public class SimPatch
        {
            public void GetCategoryAndIndexToUse(Sim.ClothesChangeReason reason, ref OutfitCategories category, out int index)
            {
                Sim sim = (Sim)(object)this;
                index = -1;
                if (sim.SimDescription.IsVisuallyPregnant && category == OutfitCategories.Outerwear && sim.SimDescription.GetOutfitCount(category) < 1)
                {
                    sim.CreateRandomOuterwear();
                    index = 0;
                    return;
                }
                if ((sim.SimDescription.IsVisuallyPregnant || sim.SimDescription.GetOutfitCount(category) > 0) && category != 0)
                {
                    index = category == OutfitCategories.Career ? sim.SimDescription.CareerOutfitIndex : Tuning.kPickRandomOutfitIndex ? Sims3.Gameplay.Core.RandomUtil.GetInt(sim.SimDescription.GetOutfitCount(category) - 1) : 0;
                    if (sim.BuffManager != null && sim.BuffManager.TransformBuffInst != null)
                    {
                        sim.BuffManager.TransformBuffInst.GenerateTransformOutfit(sim.SimDescription.GetOutfit(category, index));
                    }
                    return;
                }
                index = Tuning.kPickRandomOutfitIndex ? Sims3.Gameplay.Core.RandomUtil.GetInt(sim.SimDescription.GetOutfitCount(category) - 1) : 0;
                switch (category)
                {
                    case OutfitCategories.Naked:
                    case OutfitCategories.Everyday:
                    case OutfitCategories.Formalwear:
                    case OutfitCategories.Sleepwear:
                    case OutfitCategories.Swimwear:
                    case OutfitCategories.Athletic:
                    case OutfitCategories.Career:
                        {
                            SimBuilder simBuilder = new SimBuilder
                                {
                                    TextureSize = 1024,
                                    UseCompression = true
                                };
                            SimOutfit newOutfit = null;
                            Sims3.Gameplay.CAS.OutfitUtils.MakeCategoryOutfitUsingEveryday(simBuilder, category, sim.SimDescription, ref newOutfit);
                            simBuilder.Dispose();
                            break;
                        }
                    case OutfitCategories.Outerwear:
                        if (GameUtils.IsInstalled(ProductVersion.EP8))
                        {
                            sim.CreateRandomOuterwear();
                        }
                        else
                        {
                            category = OutfitCategories.Everyday;
                        }
                        break;
                    case OutfitCategories.Special:
                        if (reason == Sim.ClothesChangeReason.GoingToBed)
                        {
                            int tempIndex;
                            if (Sims3.Gameplay.Situations.SlumberParty.ShouldWearSlumberPartyPajama(sim, out tempIndex))
                            {
                                index = tempIndex;
                            }
                        }
                        break;
                    default:
                        category = OutfitCategories.Everyday;
                        break;
                }
            }

            public bool SwitchToOutfitWithSpin(Sim.SwitchOutfitHelper spin, bool mirrored)
            {
                Sim sim = (Sim)(object)this;
                OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                if (!OutfitAssignmentUtils.TimeToChangeBackList.Contains(sim.SimDescription))
                {
                    bool isGlobal = false;
                    if (sim.SimDescription.TryGetOutfitAssignment(sim.CurrentInteraction, out outfitAssignment) || (isGlobal = OutfitAssignmentUtils.TryGetOutfitAssignment(null, sim.CurrentInteraction, out outfitAssignment)))
                    {
                        SimOutfit outfit;
                        if (isGlobal && Sims3.Gameplay.CAS.OutfitUtils.TryApplyUniformToOutfit(sim.CurrentOutfit, OutfitAssignmentUtils.GlobalAssignedOutfits[outfitAssignment.SpecialOutfitKey], sim.SimDescription, outfitAssignment.SpecialOutfitKey, out outfit))
                        {
                            if (sim.SimDescription.HasSpecialOutfit(outfitAssignment.SpecialOutfitKey))
                            {
                                sim.SimDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                            }
                            sim.SimDescription.AddSpecialOutfit(outfit, outfitAssignment.SpecialOutfitKey);
                        }
                        OutfitCategories outfitCategory = outfitAssignment.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) ? (OutfitCategories)System.Enum.Parse(typeof(OutfitCategories), outfitAssignment.SpecialOutfitKey.Substring(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix.Length)) : OutfitCategories.Special;
                        sim.SimDescription.CreateOutfitForCategoryIfNecessary(outfitCategory);
                        spin = new Sim.SwitchOutfitHelper(sim, outfitCategory, outfitAssignment.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) ? Tuning.kPickRandomOutfitIndex ? Sims3.Gameplay.Core.RandomUtil.GetInt(sim.SimDescription.GetOutfitCount(outfitCategory) - 1) : 0 : sim.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey)));
                    }
                }
                spin.Start();
                spin.Wait(true);
                if (spin.WillChange)
                {
                    if (sim.SimDescription.ChildOrAbove)
                    {
                        if (mirrored && !sim.SimDescription.IsHorse)
                        {
                            mirrored = false;
                        }
                        if (sim.Posture is Sims3.Gameplay.Objects.Hoverboard.RidingHoverboardPosture)
                        {
                            spin.AddScriptEventHandler(sim.Posture.CurrentStateMachine);
                            sim.Posture.CurrentStateMachine.RequestState("x", "ChangeClothes");
                            sim.Posture.CurrentStateMachine.RequestState("x", "Hold");
                        }
                        else
                        {
                            StateMachineClient stateMachineClient = StateMachineClient.Acquire(sim, "solo_generic");
                            if (string.IsNullOrEmpty(spin.OverrideAnimation))
                            {
                                stateMachineClient.SetParameter("AnimationName", "a_clothesChange" + (mirrored ? "_mirrored" : "") + (sim.IsHuman ? "" : "_x"), sim.IsHuman ? ProductVersion.BaseGame : ProductVersion.EP5);
                            }
                            else
                            {
                                stateMachineClient.SetParameter("AnimationName", spin.OverrideAnimation, spin.OverrideProductVersion);
                            }
                            spin.AddScriptEventHandler(stateMachineClient);
                            stateMachineClient.SetActor("x", sim);
                            stateMachineClient.EnterState("x", "Enter");
                            stateMachineClient.RequestState("x", "Play Animation");
                            stateMachineClient.RequestState("x", "Exit");
                        }
                    }
                    else
                    {
                        spin.ChangeOutfit();
                    }
                    return true;
                }
                return false;
            }
        }

        static Main()
        {
            InteractionInstanceTypeUtils.InitInteractionInstanceTypes();
            InteractionInstanceAdditions.ReplaceMethod(typeof(Sim).GetMethod("GetCategoryAndIndexToUse", System.Array.ConvertAll(typeof(SimPatch).GetMethod("GetCategoryAndIndexToUse").GetParameters(), x => x.ParameterType)), typeof(SimPatch).GetMethod("GetCategoryAndIndexToUse"));
            InteractionInstanceAdditions.ReplaceMethod(typeof(Sim).GetMethod("SwitchToOutfitWithSpin", System.Array.ConvertAll(typeof(SimPatch).GetMethod("SwitchToOutfitWithSpin").GetParameters(), x => x.ParameterType)), typeof(SimPatch).GetMethod("SwitchToOutfitWithSpin"));
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
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || OutfitAssignmentUtils.TryGetOutfitAssignment(null, interactionInstance, out outfitAssignment)))
                    {
                        if (interactionInstance.InstanceActor.CurrentOutfitCategory != OutfitCategories.Special || interactionInstance.InstanceActor.CurrentOutfitIndex != outfitAssignment.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey)))
                        {
                            OutfitAssignmentUtils.PreviousOutfits.RemoveAll(x => x.SimDescription == outfitAssignment.SimDescription);
                            OutfitAssignmentUtils.PreviousOutfits.Add(new OutfitAssignmentUtils.Outfit
                                {
                                    Category = interactionInstance.InstanceActor.CurrentOutfitCategory,
                                    Index = interactionInstance.InstanceActor.CurrentOutfitIndex,
                                    SimDescription = outfitAssignment.SimDescription
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
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || OutfitAssignmentUtils.TryGetOutfitAssignment(null, interactionInstance, out outfitAssignment)) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded)
                    {
                        interactionInstance.InstanceActor.SwitchToPreviousOutfit();
                    }
                };
            InteractionInstanceAdditions.OnWaitForSynchronizationLevel += (interactionInstance, syncLevel) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || OutfitAssignmentUtils.TryGetOutfitAssignment(null, interactionInstance, out outfitAssignment)))
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
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || OutfitAssignmentUtils.TryGetOutfitAssignment(null, interactionInstance, out outfitAssignment)) && outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardEntry)
                    {
                        interactionInstance.InstanceActor.SwitchToAssignedOutfit(outfitAssignment);
                    }
                };
            InteractionInstanceAdditions.StandardExitPostCallCallback += (interactionInstance) =>
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (interactionInstance != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription != null && (interactionInstance.InstanceActor.SimDescription.TryGetOutfitAssignment(interactionInstance, out outfitAssignment) || OutfitAssignmentUtils.TryGetOutfitAssignment(null, interactionInstance, out outfitAssignment)) && outfitAssignment.ExitCallbackType == InteractionInstanceTypeUtils.CallbackTypes.StandardExit)
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
        }
    }
}
