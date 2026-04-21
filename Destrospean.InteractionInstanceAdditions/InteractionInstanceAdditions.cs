using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;

namespace Destrospean
{
    [MonoPatcherLib.Plugin]
    public class InteractionInstanceAdditions
    {
        public delegate void InteractionInstanceAction(InteractionInstance interactionInstance);

        public delegate void WaitForSynchronizationLevelAction(InteractionInstance interactionInstance, Sim.SyncLevel syncLevel);

        [MonoPatcherLib.TypePatch(typeof(InteractionInstance))]
        public class InteractionInstancePatch
        {
            public bool RunInteractionWithoutCleanup()
            {
                InteractionInstance interactionInstance = (InteractionInstance)(object)this;
                if (interactionInstance.StandardEntryCalled)
                {
                    return false;
                }
                OnInteractedStarted(interactionInstance);
                interactionInstance.StartInteraction();
                bool actorless = interactionInstance.InteractionDefinition is IActorlessDefinition,
                notIntermediate = false,
                succeeded;
                try
                {
                    if (interactionInstance.Target.GetOwnerLot() == null && interactionInstance.InstanceActor != null && interactionInstance.Target.LotCurrent == interactionInstance.InstanceActor.LotHome)
                    {
                        interactionInstance.Target.SetOwnerLot(interactionInstance.InstanceActor.LotHome);
                    }
                    interactionInstance.MustRun = false;
                    if (!(interactionInstance is IImmediateInteraction))
                    {
                        interactionInstance.InstanceActor.InteractionQueue.PushRunningInteraction(interactionInstance);
                        notIntermediate = true;
                    }
                    if (!actorless)
                    {
                        Sims3.Gameplay.ChildAndTeenUpdates.Punishment.PunishmentPreRun(interactionInstance.InstanceActor, interactionInstance);
                        Sims3.Gameplay.ActorSystems.OccultImaginaryFriend.StartReactionsIfNecessary(interactionInstance.InstanceActor, interactionInstance);
                    }
                    if (interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.Inventory != null && interactionInstance.InstanceActor.Inventory.Contains(interactionInstance.Target))
                    {
                        if (!(interactionInstance is IRouteFromInventoryOrSelfWithoutCarrying) && !(interactionInstance is IImmediateInteraction))
                        {
                            if (!(interactionInstance.Target is Sims3.Gameplay.Objects.Cane))
                            {
                                interactionInstance.InstanceActor.PopCanePostureIfNecessary();
                            }
                            if (!(interactionInstance.Target is Sims3.Gameplay.Objects.Umbrella))
                            {
                                Sims3.Gameplay.Objects.Umbrella.PopUmbrellaPostureIfNecessary(interactionInstance.InstanceActor, false);
                            }
                            if (!(interactionInstance.Target is Sims3.Gameplay.Objects.Backpack))
                            {
                                interactionInstance.InstanceActor.PopBackpackPostureIfNecessary();
                            }
                            if (!(interactionInstance is IJetpackInteraction))
                            {
                                interactionInstance.InstanceActor.PopJetpackPostureIfNecessary();
                            }
                        }
                        succeeded = interactionInstance.RunFromInventory();
                    }
                    else
                    {
                        succeeded = interactionInstance.Run();
                    }
                }
                catch (System.Exception ex)
                {
                    if (interactionInstance.InstanceActor != null && !ex.Data.Contains("Actor"))
                    {
                        ex.Data.Add("Actor", interactionInstance.InstanceActor.ObjectId.Value);
                    }
                    if (interactionInstance.Target != null && !ex.Data.Contains("Target"))
                    {
                        ex.Data.Add("Target", interactionInstance.Target.ObjectId.Value);
                    }
                    if (!ex.Data.Contains("Interaction"))
                    {
                        ex.Data.Add("Interaction", GetType().FullName);
                    }
                    if (interactionInstance.Target != null && interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.InteractionQueue != null && interactionInstance.InstanceActor.InteractionQueue.TransitionInteraction == interactionInstance && interactionInstance.Target.IsActorUsingMe(interactionInstance.InstanceActor))
                    {
                        interactionInstance.Target.SetObjectToReset();
                    }
                    throw;
                }
                finally
                {
                    if (notIntermediate)
                    {
                        interactionInstance.InstanceActor.InteractionQueue.PopRunningInteraction(interactionInstance);
                    }
                    if (!actorless)
                    {
                        Sims3.Gameplay.ChildAndTeenUpdates.Punishment.PunishmentPostRun(interactionInstance.InstanceActor);
                        Sims3.Gameplay.ActorSystems.OccultImaginaryFriend.StopReactionsIfNecessary(interactionInstance.InstanceActor);
                    }
                    if (interactionInstance.InstanceActor != null && interactionInstance.InstanceActor.SimDescription.IsBonehilda)
                    {
                        Sims3.Gameplay.Socializing.Relationship.RemoveSimDescriptionRelationships(interactionInstance.InstanceActor.SimDescription);
                    }
                    Sim sim = interactionInstance.Target as Sim;
                    if (sim != null && sim.SimDescription.IsBonehilda)
                    {
                        Sims3.Gameplay.Socializing.Relationship.RemoveSimDescriptionRelationships(sim.SimDescription);
                    }
                }
                if (interactionInstance.InteractionDefinition is IResortManagementInteraction || interactionInstance.InteractionDefinition is IResortManagementStaffingInteraction || interactionInstance.InteractionDefinition is IResortManagementCrewInteraction)
                {
                    Sims3.UI.Resort.ResortExpenseDialog.RefreshGrid();
                }
                if (interactionInstance.InstanceActor != null && (interactionInstance.InstanceActor.ExitReason & ExitReason.RouteFailed) != 0)
                {
                    Sims3.Gameplay.EventSystem.EventTracker.SendEvent(Sims3.Gameplay.EventSystem.EventTypeId.kUsedObect, interactionInstance.InstanceActor, interactionInstance.Target);
                }
                try
                {
                    interactionInstance.ValidatePostconditions(succeeded);
                    OnInteractionEnded(interactionInstance);
                    return succeeded;
                }
                catch (System.Exception ex)
                {
                    if (interactionInstance.InstanceActor != null && !ex.Data.Contains("Actor"))
                    {
                        ex.Data.Add("Actor", interactionInstance.InstanceActor.ObjectId.Value);
                    }
                    if (interactionInstance.Target != null && !ex.Data.Contains("Target"))
                    {
                        ex.Data.Add("Target", interactionInstance.Target.ObjectId.Value);
                    }
                    if (!ex.Data.Contains("Interaction"))
                    {
                        ex.Data.Add("Interaction", GetType().FullName);
                    }
                    throw;
                }
            }

            public void StandardEntry(bool addToUseList)
            {
                InteractionInstance interactionInstance = (InteractionInstance)(object)this;
                StandardEntryPreCallCallback(interactionInstance);
                interactionInstance.mInteractionState = InteractionInstance.InteractionState.StandardEntry;
                if (addToUseList)
                {
                    interactionInstance.Target.AddToUseList(interactionInstance.InstanceActor);
                }
                if (interactionInstance.Target.LookAtTuning != null && interactionInstance.Target.LookAtTuning.DefaultInteractionLookAtThreshold >= 0)
                {
                    interactionInstance.InstanceActor.LookAtManager.SetInteractionLookAtThreshold(interactionInstance.Target.LookAtTuning.DefaultInteractionLookAtThreshold);
                }
            }

            public void StandardExit(bool removeFromUseList, bool validateUseList)
            {
                InteractionInstance interactionInstance = (InteractionInstance)(object)this;
                interactionInstance.mInteractionState = InteractionInstance.InteractionState.StandardExit;
                interactionInstance.DeactivateTone();
                if (removeFromUseList && interactionInstance.Target != null)
                {
                    interactionInstance.Target.RemoveFromUseList(interactionInstance.InstanceActor);
                }
                if (!validateUseList)
                {
                    interactionInstance.mInteractionState = InteractionInstance.InteractionState.StandardExitDoNotValidateUseList;
                }
                Sims3.Gameplay.Abstracts.LookAtTuning lookAtTuning = (interactionInstance.Target != null) ? interactionInstance.Target.LookAtTuning : null;
                if (lookAtTuning != null && lookAtTuning.DefaultInteractionLookAtThreshold >= 0)
                {
                    interactionInstance.InstanceActor.LookAtManager.ClearInteractionLookAt();
                }
                StandardExitPostCallCallback(interactionInstance);
            }
        }

        [MonoPatcherLib.TypePatch(typeof(Sim))]
        public class SimPatch
        {
            public bool WaitForSynchronizationLevelWithSim(Sim targetSim, Sim.SyncLevel desiredSynchLevel, ExitReason exitReasonInterrupt, float giveupTime, SyncLoopCallbackFunction loopCallback, float notifySimMinutes, bool performSocializeWithTest)
            {
                Sim sim = (Sim)(object)this;
                DateAndTime dateAndTime = SimClock.CurrentTime(),
                previousDateAndTime = dateAndTime;
                Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback = null;
                sim.SynchronizationSleeping = sim.SynchronizationLevel >= desiredSynchLevel;
                while (!sim.IsAtSynchronizationLevelWith(targetSim, desiredSynchLevel))
                {
                    if (SimClock.ElapsedTime(TimeUnit.Minutes, dateAndTime) >= giveupTime)
                    {
                        sim.OnWaitForSynchronizationLevelWithSimFailed(targetSim);
                        return false;
                    }
                    if (sim.HasExitReason(exitReasonInterrupt))
                    {
                        sim.OnWaitForSynchronizationLevelWithSimFailed(targetSim);
                        return false;
                    }
                    if (performSocializeWithTest)
                    {
                        InteractionTestResult result = Sims3.Gameplay.Socializing.SocialInteractionA.Definition.CanSocializeWithSyncCheck(null, sim, targetSim, sim.IsInAutonomousInteraction(), ref greyedOutTooltipCallback, true, true);
                        if (!IUtil.IsPass(result))
                        {
                            sim.OnWaitForSynchronizationLevelWithSimFailed(targetSim);
                            return false;
                        }
                    }
                    if (loopCallback != null && SimClock.ElapsedTime(TimeUnit.Minutes, previousDateAndTime) >= notifySimMinutes)
                    {
                        if (!loopCallback())
                        {
                            sim.OnWaitForSynchronizationLevelWithSimFailed(targetSim);
                            return false;
                        }
                        previousDateAndTime = SimClock.CurrentTime();
                    }
                    Sims3.SimIFace.Simulator.Sleep(0);
                }
                sim.SynchronizationSleeping = false;
                ExitReason exitReason = exitReasonInterrupt;
                if (sim.SynchronizationRole == Sim.SyncRole.Receiver)
                {
                    exitReason &= ExitReason.SynchronizationFailed;
                }
                if (sim.HasExitReason(exitReason))
                {
                    sim.OnWaitForSynchronizationLevelWithSimFailed(targetSim);
                    return false;
                }
                if (!sim.IsAtSynchronizationLevelWith(targetSim, desiredSynchLevel))
                {
                    sim.OnWaitForSynchronizationLevelWithSimFailed(targetSim);
                    return false;
                }
                OnWaitForSynchronizationLevel(sim.CurrentInteraction, desiredSynchLevel);
                return true;
            }
        }

        public static InteractionInstanceAction OnInteractedStarted = (interactionInstance) =>
            {
            },
        OnInteractionEnded = (interactionInstance) =>
            {
            },
        StandardEntryPreCallCallback = (interactionInstance) =>
            {
            },
        StandardExitPostCallCallback = (interactionInstance) =>
            {
            };

        public static WaitForSynchronizationLevelAction OnWaitForSynchronizationLevel = (interactionInstance, syncLevel) =>
            {
            };
    }
}
