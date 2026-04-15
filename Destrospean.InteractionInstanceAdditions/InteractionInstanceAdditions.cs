using Sims3.Gameplay.Interactions;

namespace Destrospean
{
    [MonoPatcherLib.Plugin]
    public class InteractionInstanceAdditions
    {
        public delegate void InteractionInstanceAction(InteractionInstance interaction);

        [MonoPatcherLib.TypePatch(typeof(InteractionInstance))]
        public class InteractionInstancePatch
        {
            public bool RunInteractionWithoutCleanup()
            {
                InteractionInstance interaction = (InteractionInstance)(object)(this);
                if (interaction.StandardEntryCalled)
                {
                    return false;
                }
                OnInteractedStarted(interaction);
                interaction.StartInteraction();
                bool actorless = interaction.InteractionDefinition is IActorlessDefinition,
                notIntermediate = false,
                succeeded;
                try
                {
                    if (interaction.Target.GetOwnerLot() == null && interaction.InstanceActor != null && interaction.Target.LotCurrent == interaction.InstanceActor.LotHome)
                    {
                        interaction.Target.SetOwnerLot(interaction.InstanceActor.LotHome);
                    }
                    interaction.MustRun = false;
                    if (!(interaction is IImmediateInteraction))
                    {
                        interaction.InstanceActor.InteractionQueue.PushRunningInteraction(interaction);
                        notIntermediate = true;
                    }
                    if (!actorless)
                    {
                        Sims3.Gameplay.ChildAndTeenUpdates.Punishment.PunishmentPreRun(interaction.InstanceActor, interaction);
                        Sims3.Gameplay.ActorSystems.OccultImaginaryFriend.StartReactionsIfNecessary(interaction.InstanceActor, interaction);
                    }
                    if (interaction.InstanceActor != null && interaction.InstanceActor.Inventory != null && interaction.InstanceActor.Inventory.Contains(interaction.Target))
                    {
                        if (!(interaction is IRouteFromInventoryOrSelfWithoutCarrying) && !(interaction is IImmediateInteraction))
                        {
                            if (!(interaction.Target is Sims3.Gameplay.Objects.Cane))
                            {
                                interaction.InstanceActor.PopCanePostureIfNecessary();
                            }
                            if (!(interaction.Target is Sims3.Gameplay.Objects.Umbrella))
                            {
                                Sims3.Gameplay.Objects.Umbrella.PopUmbrellaPostureIfNecessary(interaction.InstanceActor, false);
                            }
                            if (!(interaction.Target is Sims3.Gameplay.Objects.Backpack))
                            {
                                interaction.InstanceActor.PopBackpackPostureIfNecessary();
                            }
                            if (!(interaction is IJetpackInteraction))
                            {
                                interaction.InstanceActor.PopJetpackPostureIfNecessary();
                            }
                        }
                        succeeded = interaction.RunFromInventory();
                    }
                    else
                    {
                        succeeded = interaction.Run();
                    }
                }
                catch (System.Exception ex)
                {
                    if (interaction.InstanceActor != null && !ex.Data.Contains("Actor"))
                    {
                        ex.Data.Add("Actor", interaction.InstanceActor.ObjectId.Value);
                    }
                    if (interaction.Target != null && !ex.Data.Contains("Target"))
                    {
                        ex.Data.Add("Target", interaction.Target.ObjectId.Value);
                    }
                    if (!ex.Data.Contains("Interaction"))
                    {
                        ex.Data.Add("Interaction", GetType().FullName);
                    }
                    if (interaction.Target != null && interaction.InstanceActor != null && interaction.InstanceActor.InteractionQueue != null && interaction.InstanceActor.InteractionQueue.TransitionInteraction == interaction && interaction.Target.IsActorUsingMe(interaction.InstanceActor))
                    {
                        interaction.Target.SetObjectToReset();
                    }
                    throw;
                }
                finally
                {
                    if (notIntermediate)
                    {
                        interaction.InstanceActor.InteractionQueue.PopRunningInteraction(interaction);
                    }
                    if (!actorless)
                    {
                        Sims3.Gameplay.ChildAndTeenUpdates.Punishment.PunishmentPostRun(interaction.InstanceActor);
                        Sims3.Gameplay.ActorSystems.OccultImaginaryFriend.StopReactionsIfNecessary(interaction.InstanceActor);
                    }
                    if (interaction.InstanceActor != null && interaction.InstanceActor.SimDescription.IsBonehilda)
                    {
                        Sims3.Gameplay.Socializing.Relationship.RemoveSimDescriptionRelationships(interaction.InstanceActor.SimDescription);
                    }
                    Sims3.Gameplay.Actors.Sim sim = interaction.Target as Sims3.Gameplay.Actors.Sim;
                    if (sim != null && sim.SimDescription.IsBonehilda)
                    {
                        Sims3.Gameplay.Socializing.Relationship.RemoveSimDescriptionRelationships(sim.SimDescription);
                    }
                }
                if (interaction.InteractionDefinition is IResortManagementInteraction || interaction.InteractionDefinition is IResortManagementStaffingInteraction || interaction.InteractionDefinition is IResortManagementCrewInteraction)
                {
                    Sims3.UI.Resort.ResortExpenseDialog.RefreshGrid();
                }
                if (interaction.InstanceActor != null && (interaction.InstanceActor.ExitReason & Sims3.Gameplay.Actors.ExitReason.RouteFailed) != 0)
                {
                    Sims3.Gameplay.EventSystem.EventTracker.SendEvent(Sims3.Gameplay.EventSystem.EventTypeId.kUsedObect, interaction.InstanceActor, interaction.Target);
                }
                try
                {
                    interaction.ValidatePostconditions(succeeded);
                    OnInteractionEnded(interaction);
                    return succeeded;
                }
                catch (System.Exception ex)
                {
                    if (interaction.InstanceActor != null && !ex.Data.Contains("Actor"))
                    {
                        ex.Data.Add("Actor", interaction.InstanceActor.ObjectId.Value);
                    }
                    if (interaction.Target != null && !ex.Data.Contains("Target"))
                    {
                        ex.Data.Add("Target", interaction.Target.ObjectId.Value);
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
                InteractionInstance interaction = (InteractionInstance)(object)(this);
                StandardEntryPreCallCallback(interaction);
                interaction.mInteractionState = InteractionInstance.InteractionState.StandardEntry;
                if (addToUseList)
                {
                    interaction.Target.AddToUseList(interaction.InstanceActor);
                }
                if (interaction.Target.LookAtTuning != null && interaction.Target.LookAtTuning.DefaultInteractionLookAtThreshold >= 0)
                {
                    interaction.InstanceActor.LookAtManager.SetInteractionLookAtThreshold(interaction.Target.LookAtTuning.DefaultInteractionLookAtThreshold);
                }
            }

            public void StandardExit(bool removeFromUseList, bool validateUseList)
            {
                InteractionInstance interaction = (InteractionInstance)(object)(this);
                interaction.mInteractionState = InteractionInstance.InteractionState.StandardExit;
                interaction.DeactivateTone();
                if (removeFromUseList && interaction.Target != null)
                {
                    interaction.Target.RemoveFromUseList(interaction.InstanceActor);
                }
                if (!validateUseList)
                {
                    interaction.mInteractionState = InteractionInstance.InteractionState.StandardExitDoNotValidateUseList;
                }
                Sims3.Gameplay.Abstracts.LookAtTuning lookAtTuning = (interaction.Target != null) ? interaction.Target.LookAtTuning : null;
                if (lookAtTuning != null && lookAtTuning.DefaultInteractionLookAtThreshold >= 0)
                {
                    interaction.InstanceActor.LookAtManager.ClearInteractionLookAt();
                }
                StandardExitPostCallCallback(interaction);
            }
        }

        public static InteractionInstanceAction OnInteractedStarted = (interaction) =>
            {
            },
        OnInteractionEnded = (interaction) =>
            {
            },
        StandardEntryPreCallCallback = (interaction) =>
            {
            },
        StandardExitPostCallCallback = (interaction) =>
            {
            };
    }
}
