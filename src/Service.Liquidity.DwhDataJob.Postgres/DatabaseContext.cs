using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Liquidity.DwhDataJob.Postgres.Models;

namespace Service.Liquidity.DwhDataJob.Postgres
{
    public class DatabaseContext : DbContext
    {
        public const string Schema = "liquidity_dwhdata";
        
        private const string MarketPriceTableName = "marketprice";
        
        private DbSet<MarketPriceEntity> MarketPriceCollection { get; set; }
        
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
            
            base.OnModelCreating(modelBuilder);
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
        
        public async Task UpsertMarketPrice(List<MarketPriceEntity> prices)
        {
            await MarketPriceCollection
                .UpsertRange(prices)
                .On(e => new {e.Source, e.SourceMarket})
                .RunAsync();
        }
    }
}