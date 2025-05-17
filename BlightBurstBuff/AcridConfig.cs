using BepInEx.Configuration;
using BepInEx;

namespace BlightBurstBuff
{
    internal static class AcridConfig
    {
        public static ConfigEntry<int> targetBuffCount;
        public static ConfigEntry<float> procCoeff;
        public static ConfigEntry<bool> canCrit;

        public static void InitializeConfig()
        {
            var configFile = new ConfigFile(Paths.ConfigPath + "\\OakPrime.BlightBurstBuff.cfg", true);

            targetBuffCount = configFile.Bind("Main", "Blight Burst Stack Count", 3, "Stack count at which blight bursts");
            procCoeff = configFile.Bind("Main", "Proc Coefficient", 1.0f, "Proc coefficient of burst");
            canCrit = configFile.Bind("Main", "Burst Can Crit", true, "If burst damage can crit");
        }
    }
}
