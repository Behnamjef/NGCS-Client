using NGCS.MultiBuild;

public interface IMultiBuildConfig
{
    public MarketType GetCurrentMarketType();
    public PlatformConfig GetCurrentConfig();
}