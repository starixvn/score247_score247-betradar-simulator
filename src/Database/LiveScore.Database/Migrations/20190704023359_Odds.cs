using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LiveScore.Database.Migrations
{
    public partial class Odds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Odds",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedTime = table.Column<DateTimeOffset>(nullable: false),
                    Value = table.Column<string>(type: "json", nullable: true),
                    MatchId = table.Column<string>(nullable: true),
                    BetTypeId = table.Column<int>(nullable: false),
                    BookmakerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odds", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Odds");
        }
    }
}
