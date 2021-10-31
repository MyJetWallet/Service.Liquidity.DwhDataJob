using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Liquidity.DwhDataJob.Domain.Models;
using Service.Liquidity.DwhDataJob.Postgres.Models;

namespace Service.Liquidity.DwhDataJob.Postgres
{
    public class DatabaseContext : DbContext
    {
        public const string Schema = "liquidity_dwhdata";
        
        private const string MarketPriceTableName = "marketprice";
        private const string ConvertIndexPriceTableName = "convertprice";
        private const string BalanceDashboardTableName = "balancedashboard";
        private const string CommissionDashboardTableName = "commissiondashboard";
        
        private DbSet<MarketPriceEntity> MarketPriceCollection { get; set; }
        private DbSet<ConvertIndexPriceEntity> ConvertIndexPriceCollection { get; set; }
        
        private DbSet<BalanceDashboard> BalanceDashboardCollection { get; set; }
        private DbSet<CommissionDashboard> CommissionDashboardCollection { get; set; }
        
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }
        public static ILoggerFactory LoggerFactory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (LoggerFactory != null)
            {
                optionsBuilder.UseLoggerFactory(LoggerFactory).EnableSensitiveDataLogging();
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            SetMarketPriceEntity(modelBuilder);
            SetConvertIndexPriceEntity(modelBuilder);
            SetBalanceDashboardEntity(modelBuilder);
            SetCommissionDashboardEntity(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void SetCommissionDashboardEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommissionDashboard>().ToTable(CommissionDashboardTableName);
            
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<CommissionDashboard>().HasKey(e => e.Id);
            
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.Asset).HasMaxLength(64);
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.CommissionDate);
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.LastUpdateDate);
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.Commission);
            modelBuilder.Entity<CommissionDashboard>().Property(e => e.LastMessageId).HasMaxLength(64);
            
            modelBuilder.Entity<CommissionDashboard>().HasIndex(e => new {e.Asset, e.CommissionDate}).IsUnique();
        }

        private void SetBalanceDashboardEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BalanceDashboard>().ToTable(BalanceDashboardTableName);
            
            modelBuilder.Entity<BalanceDashboard>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<BalanceDashboard>().HasKey(e => e.Id);
            
            modelBuilder.Entity<BalanceDashboard>().Property(e => e.Asset).HasMaxLength(64);
            modelBuilder.Entity<BalanceDashboard>().Property(e => e.BalanceDate);
            modelBuilder.Entity<BalanceDashboard>().Property(e => e.LastUpdateDate);
            modelBuilder.Entity<BalanceDashboard>().Property(e => e.ClientBalance);
            modelBuilder.Entity<BalanceDashboard>().Property(e => e.BrokerBalance);
            modelBuilder.Entity<BalanceDashboard>().Property(e => e.LastMessageId);
            
            modelBuilder.Entity<BalanceDashboard>().HasIndex(e => new {e.Asset, e.BalanceDate}).IsUnique();
        }

        private void SetConvertIndexPriceEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConvertIndexPriceEntity>().ToTable(ConvertIndexPriceTableName);
            
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<ConvertIndexPriceEntity>().HasKey(e => e.Id);
            
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.BaseAsset).HasMaxLength(64);
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.QuotedAsset).HasMaxLength(64);
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.Price);
            modelBuilder.Entity<ConvertIndexPriceEntity>().Property(e => e.UpdateDate);
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
            modelBuilder.Entity<MarketPriceEntity>().Property(e => e.DateTime);
            modelBuilder.Entity<MarketPriceEntity>().Property(e => e.Price);
            modelBuilder.Entity<MarketPriceEntity>().Property(e => e.InstrumentStatus);
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
        
        public async Task ExecBalanceDashboardMigrationAsync(ILogger logger)
        {
            try
            {
                var path = Path.Combine(Environment.CurrentDirectory, @"Scripts/", "BalanceDashboardMigration.sql");
                using var script = new StreamReader(path);
                var scriptBody = await script.ReadToEndAsync();
                logger.LogInformation($"ExecBalanceDashboardMigrationAsync start at: {DateTime.UtcNow}.");
                await Database.ExecuteSqlRawAsync(scriptBody);
                logger.LogInformation($"ExecBalanceDashboardMigrationAsync finish at: {DateTime.UtcNow}.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public List<BalanceDashboard> GetBalanceDashboardList(DateTime balanceDate)
        {
            var dashboardList = BalanceDashboardCollection
                .Where(e => e.BalanceDate >= balanceDate.Date.AddDays(-1))
                .ToList();
            return dashboardList;
        }
        
        public async Task UpsertBalanceDashboard(IEnumerable<BalanceDashboard> dashboardList)
        {
            await BalanceDashboardCollection
                .UpsertRange(dashboardList)
                .On(e => new {e.Asset, e.BalanceDate})
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
    }
}