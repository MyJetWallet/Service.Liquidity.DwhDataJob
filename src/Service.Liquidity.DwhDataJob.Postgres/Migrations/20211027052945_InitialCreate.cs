using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.Liquidity.DwhDataJob.Postgres.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "liquidity_dwhdata");

            migrationBuilder.CreateTable(
                name: "convertprice",
                schema: "liquidity_dwhdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseAsset = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    QuotedAsset = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Error = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_convertprice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "marketprice",
                schema: "liquidity_dwhdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrokerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    InstrumentSymbol = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    InstrumentStatus = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceMarket = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketprice", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_convertprice_BaseAsset_QuotedAsset",
                schema: "liquidity_dwhdata",
                table: "convertprice",
                columns: new[] { "BaseAsset", "QuotedAsset" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_convertprice_Error",
                schema: "liquidity_dwhdata",
                table: "convertprice",
                column: "Error");

            migrationBuilder.CreateIndex(
                name: "IX_convertprice_UpdateDate",
                schema: "liquidity_dwhdata",
                table: "convertprice",
                column: "UpdateDate");

            migrationBuilder.CreateIndex(
                name: "IX_marketprice_DateTime",
                schema: "liquidity_dwhdata",
                table: "marketprice",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_marketprice_InstrumentStatus",
                schema: "liquidity_dwhdata",
                table: "marketprice",
                column: "InstrumentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_marketprice_InstrumentSymbol",
                schema: "liquidity_dwhdata",
                table: "marketprice",
                column: "InstrumentSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_marketprice_Source_SourceMarket",
                schema: "liquidity_dwhdata",
                table: "marketprice",
                columns: new[] { "Source", "SourceMarket" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "convertprice",
                schema: "liquidity_dwhdata");

            migrationBuilder.DropTable(
                name: "marketprice",
                schema: "liquidity_dwhdata");
        }
    }
}
