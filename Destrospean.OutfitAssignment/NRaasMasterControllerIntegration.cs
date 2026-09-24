using System;
using Destrospean.Utils;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace.CAS;

namespace Sims3.Gameplay.Destrospean.Utils
{
    public class NRaasMasterControllerIntegration
    {
        [Sims3.SimIFace.Tunable]
        static bool kIntegrateNRaasMasterController = true;

        public static void Init()
        {
            OutfitExtensions.EditSpecialOutfitFunc editSpecialOutfit = OutfitExtensions.EditSpecialOutfit;
            OutfitExtensions.EditSpecialOutfit = (sim, specialOutfitKey) =>
                {
                    if (kIntegrateNRaasMasterController)
                    {
                        SimDescription simDescription = sim.SimDescription;
                        if (!simDescription.HasSpecialOutfit(specialOutfitKey))
                        {
                            simDescription.AddSpecialOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), specialOutfitKey);
                        }
                        OutfitCategories previousOutfitCategory = sim.CurrentOutfitCategory;
                        int previousOutfitIndex = sim.CurrentOutfitIndex;
                        simDescription.AddOutfit(simDescription.GetSpecialOutfit(specialOutfitKey), OutfitCategories.Everyday, 0);
                        simDescription.RemoveSpecialOutfit(specialOutfitKey);
                        sim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday, 0);
                        CASLogic casLogic = CASLogic.GetSingleton();
                        new NRaas.MasterControllerSpace.Sims.Stylist().Perform(new NRaas.CommonSpace.Options.GameHitParameters<Sims3.Gameplay.Abstracts.GameObject>(sim, sim, Sims3.SimIFace.GameObjectHit.NoHit));
                        casLogic.ShowUI += OutfitExtensions.OnShowUI;
                        while (Sims3.Gameplay.GameStates.NextInWorldStateId != 0)
                        {
                            NRaas.SpeedTrap.Sleep();
                        }
                        casLogic.ShowUI -= OutfitExtensions.OnShowUI;
                        simDescription.AddSpecialOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), specialOutfitKey);
                        simDescription.RemoveOutfit(OutfitCategories.Everyday, 0, true);
                        sim.SwitchToOutfitWithoutSpin(previousOutfitCategory, previousOutfitIndex);
                        return !CASChangeReporter.Instance.CasCancelled;
                    }
                    return editSpecialOutfit(sim, specialOutfitKey);
                };
        }
    }
}

