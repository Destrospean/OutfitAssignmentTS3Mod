using System;
using System.Collections.Generic;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;

namespace Destrospean.OutfitAssignment
{
    public class Interactions
    {
        public class AssignOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/AssignOutfitToInteraction";

            class DummyComparer : IComparer<string>
            {
                public int Compare(string a, string b)
                {
                    return 1;
                }
            }

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, AssignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(actor.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.IsHuman;
                }
            }

            protected static bool TryGetEntryCallbackType(Sim target, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/EntryCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(target.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionStarted"),
                            InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:StandardEntry"),
                            InteractionInstanceTypeUtils.CallbackTypes.StandardEntry.ToString()
                        }
                    }, InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
                return true;
            }

            protected static bool TryGetExitCallbackType(Sim target, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/ExitCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(target.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionEnded"),
                            InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:StandardExit"),
                            InteractionInstanceTypeUtils.CallbackTypes.StandardExit.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Never"),
                            InteractionInstanceTypeUtils.CallbackTypes.Never.ToString()
                        }
                    }, InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
                return true;
            }

            public override bool Run()
            {
                Type[] interactionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out interactionInstanceTypes) && TryGetEntryCallbackType(Target, out entryCallbackType) && TryGetExitCallbackType(Target, out exitCallbackType))
                {
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in interactionInstanceTypes)
                    {
                        OutfitAssignment outfitAssignment;
                        if (OutfitAssignment.TryGetOutfitAssignment(Target.SimDescription, interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(Target.IsFemale, sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", Target), Target.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                        }
                    }
                    specialOutfitKey = string.IsNullOrEmpty(specialOutfitKey) ? "OutfitAssignment_" + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID() : specialOutfitKey;
                    bool outfitIsPreexisting = Actor.SimDescription.HasSpecialOutfit(specialOutfitKey);
                    if (Actor.EditSpecialOutfit(specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in interactionInstanceTypes)
                        {
                            OutfitAssignment.AssignOutfitToInteraction(Target.SimDescription, specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                    }
                    else if (!outfitIsPreexisting)
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                }
                return true;
            }
        }

        public class UnassignOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/UnassignOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, UnassignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(actor.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return OutfitAssignment.GetAllOutfitAssignments(target.SimDescription).Length > 0;
                }
            }

            public override bool Run()
            {
                Type[] interactionInstanceTypes;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out interactionInstanceTypes, Array.ConvertAll(OutfitAssignment.GetAllOutfitAssignments(Target.SimDescription), x => x.InteractionInstanceType)))
                {
                    foreach (Type interactionInstanceType in interactionInstanceTypes)
                    {
                        OutfitAssignment outfitAssignment;
                        if (OutfitAssignment.TryGetOutfitAssignment(Target.SimDescription, interactionInstanceType, out outfitAssignment) && Array.FindAll(OutfitAssignment.GetAllOutfitAssignments(Target.SimDescription), x => x.SpecialOutfitKey == outfitAssignment.SpecialOutfitKey).Length == 1)
                        {
                            Target.SimDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                        }
                        OutfitAssignment.UnassignOutfitToInteraction(Target.SimDescription, interactionInstanceType);
                    }
                }
                return true;
            }
        }
    }
}
