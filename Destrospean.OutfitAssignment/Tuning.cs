namespace Sims3.Gameplay.Destrospean
{
    public class OutfitAssignment
    {
        [Sims3.SimIFace.Tunable, Sims3.SimIFace.TunableComment("Interval (in in-game seconds) to check if the outfit has changed for interactions with assigned outfits with the entry type being \"Outfit Changed\"")]
        public static float kOutfitChangedCheckInterval;
    }
}
