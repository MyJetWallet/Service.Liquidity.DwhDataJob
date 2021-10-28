using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.Liquidity.DwhDataJob.Postgres.Migrations
{
    public partial class InitialCreate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastMessageId",
                schema: "liquidity_dwhdata",
                table: "balancedashboard",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMessageId",
                schema: "liquidity_dwhdata",
                table: "balancedashboard");
        }
    }
}
