using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace showcase.Data.Migrations
{
    public partial class AddedBlogEntryAndTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Images",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateTable(
                name: "BlogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateUploaded = table.Column<DateTimeOffset>(nullable: false),
                    Html = table.Column<string>(nullable: true),
                    ImageId = table.Column<int>(nullable: true),
                    ImagePlacement = table.Column<int>(nullable: false),
                    Markdown = table.Column<string>(nullable: true),
                    ShortDescription = table.Column<string>(nullable: true),
                    ShowFooter = table.Column<bool>(nullable: false),
                    ShowOnList = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    TitlePlacement = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogEntries_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlogEntryId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PortfolioEntryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_BlogEntries_BlogEntryId",
                        column: x => x.BlogEntryId,
                        principalTable: "BlogEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tags_PortfolioEntries_PortfolioEntryId",
                        column: x => x.PortfolioEntryId,
                        principalTable: "PortfolioEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogEntries_ImageId",
                table: "BlogEntries",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_BlogEntryId",
                table: "Tags",
                column: "BlogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_PortfolioEntryId",
                table: "Tags",
                column: "PortfolioEntryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "BlogEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Images",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
