using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.Liquidity.DwhDataJob.Postgres.Migrations
{
    public partial class InitialCreate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "externalbalance",
                schema: "liquidity_dwhdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BalanceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Exchange = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Asset = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_externalbalance", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_externalbalance_Asset",
                schema: "liquidity_dwhdata",
                table: "externalbalance",
                column: "Asset");

            migrationBuilder.CreateIndex(
                name: "IX_externalbalance_BalanceDate",
                schema: "liquidity_dwhdata",
                table: "externalbalance",
                column: "BalanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_externalbalance_Exchange",
                schema: "liquidity_dwhdata",
                table: "externalbalance",
                column: "Exchange");

            migrationBuilder.CreateIndex(
                name: "IX_externalbalance_Exchange_Asset_BalanceDate",
                schema: "liquidity_dwhdata",
                table: "externalbalance",
                columns: new[] { "Exchange", "Asset", "BalanceDate" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "externalbalance",
                schema: "liquidity_dwhdata");
        }
    }
}
