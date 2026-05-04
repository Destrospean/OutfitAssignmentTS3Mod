using Sims3.Gameplay.Actors;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Tuning = Sims3.Gameplay.Destrospean.OutfitAssignment;

namespace Destrospean.OutfitAssignment
{
    public class Replacements
    {
        public void GetCategoryAndIndexToUse(Sim.ClothesChangeReason reason, ref OutfitCategories category, out int index)
        {
            Sim sim = (Sim)(object)this;
            index = -1;
            if (category == sim.CurrentOutfitCategory)
            {
                index = sim.CurrentOutfitIndex;
                return;
            }
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
                        using (SimBuilder simBuilder = new SimBuilder
                            {
                                TextureSize = 1024,
                                UseCompression = true
                            })
                        {
                            SimOutfit newOutfit = null;
                            Sims3.Gameplay.CAS.OutfitUtils.MakeCategoryOutfitUsingEveryday(simBuilder, category, sim.SimDescription, ref newOutfit);
                        }
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
                    if (isGlobal)
                    {
                        sim.AddAssignedOutfit(outfitAssignment.SpecialOutfitKey);
                    }
                    OutfitCategories outfitCategory = outfitAssignment.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) ? (OutfitCategories)System.Enum.Parse(typeof(OutfitCategories), outfitAssignment.SpecialOutfitKey.Substring(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix.Length)) : OutfitCategories.Special;
                    if (outfitCategory == 0)
                    {
                        return false;
                    }
                    sim.SimDescription.CreateOutfitForCategoryIfNecessary(outfitCategory);
                    spin = new Sim.SwitchOutfitHelper(sim, outfitCategory, outfitAssignment.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) ? Tuning.kPickRandomOutfitIndex ? Sims3.Gameplay.Core.RandomUtil.GetInt(sim.SimDescription.GetOutfitCount(outfitCategory) - 1) : 0 : sim.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey)));
                }
            }
            spin.Start();
            spin.Wait(true);
            if (!spin.WillChange)
            {
                return false;
            }
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
    }
}
