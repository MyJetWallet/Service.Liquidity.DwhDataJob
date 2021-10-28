using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.DwhDataJob.Settings
{
    public class SettingsModel
    {
        [YamlProperty("LiquidityDwhDataJob.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("LiquidityDwhDataJob.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("LiquidityDwhDataJob.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("LiquidityDwhDataJob.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("LiquidityDwhDataJob.MarketPriceJobTimerInSeconds")]
        public int MarketPriceJobTimerInSeconds { get; set; }

        [YamlProperty("LiquidityDwhDataJob.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }

        [YamlProperty("LiquidityDwhDataJob.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("LiquidityDwhDataJob.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }

        [YamlProperty("LiquidityDwhDataJob.BalanceDashboardPersistJobTimerInSeconds")]
        public int BalanceDashboardPersistJobTimerInSeconds { get; set; }
    }
}
