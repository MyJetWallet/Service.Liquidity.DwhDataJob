using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyJetWallet.Sdk.Postgres;
using Service.Liquidity.DwhDataJob.Domain.Models;
using Service.Liquidity.DwhDataJob.Postgres.Models;

namespace Service.Liquidity.DwhDataJob.Postgres
{
    public class DatabaseContext : MyDbContext
    {
        public const string Schema = "liquidity_dwhdata";
        
        private const string MarketPriceTableName = "marketprice";
        private const string ConvertIndexPriceTableName = "convertprice";
        private const string CommissionDashboardTableName = "commissiondashboard";
        private const string ExternalBalanceTableName = "externalbalance";
        
        private DbSet<MarketPriceEntity> MarketPriceCollection { get; set; }
        private DbSet<ConvertIndexPriceEntity> ConvertIndexPriceCollection { get; set; }
        
        private DbSet<CommissionDashboard> CommissionDashboardCollection { get; set; }
        private DbSet<ExternalBalanceEntity> ExternalBalanceCollection { get; set; }
        
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            SetMarketPriceEntity(modelBuilder);
            SetConvertIndexPriceEntity(modelBuilder);
            SetCommissionDashboardEntity(modelBuilder);
            SetExternalBalanceEntity(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void SetExternalBalanceEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalBalanceEntity>().ToTable(ExternalBalanceTableName);
            
            modelBuilder.Entity<ExternalBalanceEntity>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<ExternalBalanceEntity>().HasKey(e => e.Id);
            
            modelBuilder.Entity<ExternalBalanceEntity>().Property(e => e.Asset).HasMaxLength(64);
            modelBuilder.Entity<ExternalBalanceEntity>().Property(e => e.Exchange).HasMaxLength(64);
            
            modelBuilder.Entity<ExternalBalanceEntity>().HasIndex(e => new {e.Exchange, e.Asset, e.BalanceDate}).IsUnique();
            modelBuilder.Entity<ExternalBalanceEntity>().HasIndex(e => e.Exchange);
            modelBuilder.Entity<ExternalBalanceEntity>().HasIndex(e => e.Asset);
            modelBuilder.Entity<ExternalBalanceEntity>().HasIndex(e => e.BalanceDate);
        }

        private void SetCommissionDashboardEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommissionDashboard>().ToTable(CommissionDashboardTableName);
            
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<CommissionDashboard>().HasKey(e => e.Id);
            
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.Asset).HasMaxLength(64);
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.LastMessageId).HasMaxLength(64);
            
            modelBuilder.Entity<CommissionDashboard>().HasIndex(e => new {e.Asset, e.CommissionDate}).IsUnique();
        }

        

        private void SetConvertIndexPriceEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConvertIndexPriceEntity>().ToTable(ConvertIndexPriceTableName);
            
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<ConvertIndexPriceEntity>().HasKey(e => e.Id);
            
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.BaseAsset).HasMaxLength(64);
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.QuotedAsset).HasMaxLength(64);
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.Error).HasMaxLength(256);
            
            modelBuilder.Entity<ConvertIndexPriceEntity>().HasIndex(e => new {e.BaseAsset, e.QuotedAsset}).IsUnique();
            modelBuilder.Entity<ConvertIndexPriceEntity>().HasIndex(e => e.UpdateDate);
            modelBuilder.Entity<ConvertIndexPriceEntity>().HasIndex(e => e.Error);
        }

        private void SetMarketPriceEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MarketPriceEntity>().ToTable(MarketPriceTableName);
            
            modelBuilder.Entity<MarketPriceEntity>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<MarketPriceEntity>().HasKey(e => e.Id);
            
            modelBuilder.Entity<MarketPriceEntity>().Property(e => e.BrokerId).HasMaxLength(64);
            modelBuilder.Entity<MarketPriceEntity>().Property(e => e.InstrumentSymbol).HasMaxLength(64);
            modelBuilder.Entity<MarketPriceEntity>().Property(e => e.Source).HasMaxLength(64);
            modelBuilder.Entity<MarketPriceEntity>().Property(e => e.SourceMarket).HasMaxLength(64);
            
            modelBuilder.Entity<MarketPriceEntity>().HasIndex(e => new {e.Source, e.SourceMarket}).IsUnique();
            modelBuilder.Entity<MarketPriceEntity>().HasIndex(e => e.DateTime);
            modelBuilder.Entity<MarketPriceEntity>().HasIndex(e => e.InstrumentStatus);
            modelBuilder.Entity<MarketPriceEntity>().HasIndex(e => e.InstrumentSymbol);
        }
        
        public async Task UpsertMarketPrice(IEnumerable<MarketPriceEntity> prices)
        {
            await MarketPriceCollection
                .UpsertRange(prices)
                .On(e => new {e.Source, e.SourceMarket})
                .RunAsync();
        }
        public async Task UpsertConvertPrice(IEnumerable<ConvertIndexPriceEntity> prices)
        {
            await ConvertIndexPriceCollection
                .UpsertRange(prices)
                .On(e => new {e.BaseAsset, e.QuotedAsset})
                .RunAsync();
        }

        public List<CommissionDashboard> GetCommissionDashboardList(DateTime commissionDate)
        {
            var dashboardList = CommissionDashboardCollection
                .Where(e => e.CommissionDate >= commissionDate.Date.AddDays(-1))
                .ToList();
            return dashboardList;
        }
        
        public async Task UpsertCommissionDashboard(IEnumerable<CommissionDashboard> dashboardList)
        {
            await CommissionDashboardCollection
                .UpsertRange(dashboardList)
                .On(e => new {e.Asset, e.CommissionDate})
                .RunAsync();
        }

        public async Task UpsertExternalBalances(IEnumerable<ExternalBalanceEntity> allBalances)
        {
            await ExternalBalanceCollection
                .UpsertRange(allBalances)
                .On(e => new {e.Exchange, e.Asset, e.BalanceDate})
                .RunAsync();
        }
    }
}