using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace showcase.Data.Migrations
{
    public partial class AddedPortfolioEntryAndImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AltText = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Html = table.Column<string>(nullable: true),
                    ImageId = table.Column<int>(nullable: true),
                    Markdown = table.Column<string>(nullable: true),
                    ShortDescription = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioEntries_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioEntries_ImageId",
                table: "PortfolioEntries",
                column: "ImageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioEntries");

            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
