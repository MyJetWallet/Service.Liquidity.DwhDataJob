using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Service.Liquidity.DwhDataJob.Postgres.Migrations
{
    public partial class Remove_Balance_Dashboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "balancedashboard",
                schema: "liquidity_dwhdata");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                schema: "liquidity_dwhdata",
                table: "marketprice",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateDate",
                schema: "liquidity_dwhdata",
                table: "externalbalance",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BalanceDate",
                schema: "liquidity_dwhdata",
                table: "externalbalance",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateDate",
                schema: "liquidity_dwhdata",
                table: "convertprice",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateDate",
                schema: "liquidity_dwhdata",
                table: "commissiondashboard",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CommissionDate",
                schema: "liquidity_dwhdata",
                table: "commissiondashboard",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                schema: "liquidity_dwhdata",
                table: "marketprice",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateDate",
                schema: "liquidity_dwhdata",
                table: "externalbalance",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BalanceDate",
                schema: "liquidity_dwhdata",
                table: "externalbalance",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateDate",
                schema: "liquidity_dwhdata",
                table: "convertprice",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateDate",
                schema: "liquidity_dwhdata",
                table: "commissiondashboard",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CommissionDate",
                schema: "liquidity_dwhdata",
                table: "commissiondashboard",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "balancedashboard",
                schema: "liquidity_dwhdata",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Asset = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BalanceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BrokerBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    ClientBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    LastMessageId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_balancedashboard", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_balancedashboard_Asset_BalanceDate",
                schema: "liquidity_dwhdata",
                table: "balancedashboard",
                columns: new[] { "Asset", "BalanceDate" },
                unique: true);
        }
    }
}
