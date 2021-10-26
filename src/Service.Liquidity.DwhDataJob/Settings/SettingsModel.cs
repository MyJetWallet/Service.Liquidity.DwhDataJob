using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.DwhDataJob.Settings
{
    public class SettingsModel
    {
        [YamlProperty("Liquidity.DwhDataJob.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("Liquidity.DwhDataJob.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("Liquidity.DwhDataJob.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}
