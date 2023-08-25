namespace AntiLag
{
    public class Config
    {
        public bool enabled = false;
        public float clearIntevalMS = 0;
        public bool disableHalOnEvents = false;
        public int itemAmountToKeep = 0;
        public int itemAmountToKeepOnEvents = 0;
        public bool syncTilesOnIntervalToo = false;
        public int baseTimeUntilClearLag = 0;

        public static Config DefaultConfig()
        {
            Config vConf = new Config
            {
                enabled = true,
                clearIntevalMS = 3000.0f,
                disableHalOnEvents = false,
                itemAmountToKeep = 20,
                itemAmountToKeepOnEvents = 50,
                syncTilesOnIntervalToo = true,
                baseTimeUntilClearLag = 1000
            };

            return vConf;
        }
    }
}
