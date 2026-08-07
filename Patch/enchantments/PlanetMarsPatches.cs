// PengoTarot/Patches/PlanetMarsPatches.cs
#nullable enable

namespace PengoTarot.Patches
{
    /// <summary>
    /// All PlanetMars sync logic (card generation, card transformation, orb channeling, summoning)
    /// is now handled via proper Hook overrides in <see cref="PengoTarot.Powers.PlanetMarsPower"/>.
    /// No Harmony patches are needed.
    /// </summary>
    public static class PlanetMarsPatches
    {
    }
}