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
            Common.InitInteractionInstanceTypes();
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
            InteractionInstanceAdditions.OnInteractedStarted += (interaction) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interaction.InstanceActor.SimDescription, interaction, out outfitAssignment) && outfitAssignment.EntryCallbackType == InteractionInstanceCallbackTypes.InteractionStarted)
                    {
                        outfitAssignment.PreviousOutfitCategory = interaction.InstanceActor.CurrentOutfitCategory;
                        outfitAssignment.PreviousOutfitIndex = interaction.InstanceActor.CurrentOutfitIndex;
                        interaction.InstanceActor.SwitchToOutfitWithSpin(Sims3.SimIFace.CAS.OutfitCategories.Special, outfitAssignment.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.InteractionInstanceType.FullName)));
                    }
                };
            InteractionInstanceAdditions.OnInteractionEnded += (interaction) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interaction.InstanceActor.SimDescription, interaction, out outfitAssignment) && outfitAssignment.ExitCallbackType == InteractionInstanceCallbackTypes.InteractionEnded)
                    {
                        interaction.InstanceActor.SwitchToOutfitWithSpin(outfitAssignment.PreviousOutfitCategory, outfitAssignment.PreviousOutfitIndex);
                    }
                };
            InteractionInstanceAdditions.StandardEntryPreCallCallback += (interaction) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interaction.InstanceActor.SimDescription, interaction, out outfitAssignment) && outfitAssignment.EntryCallbackType == InteractionInstanceCallbackTypes.StandardEntry)
                    {
                        outfitAssignment.PreviousOutfitCategory = interaction.InstanceActor.CurrentOutfitCategory;
                        outfitAssignment.PreviousOutfitIndex = interaction.InstanceActor.CurrentOutfitIndex;
                        interaction.InstanceActor.SwitchToOutfitWithSpin(Sims3.SimIFace.CAS.OutfitCategories.Special, outfitAssignment.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.InteractionInstanceType.FullName)));
                    }
                };
            InteractionInstanceAdditions.StandardExitPostCallCallback += (interaction) =>
                {
                    OutfitAssignment outfitAssignment;
                    if (OutfitAssignment.TryGetOutfitAssignment(interaction.InstanceActor.SimDescription, interaction, out outfitAssignment) && outfitAssignment.ExitCallbackType == InteractionInstanceCallbackTypes.StandardExit)
                    {
                        interaction.InstanceActor.SwitchToOutfitWithSpin(outfitAssignment.PreviousOutfitCategory, outfitAssignment.PreviousOutfitIndex);
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
