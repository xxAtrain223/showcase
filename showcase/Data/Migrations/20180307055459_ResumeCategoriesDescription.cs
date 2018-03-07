using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace showcase.Data.Migrations
{
    public partial class ResumeCategoriesDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ResumeCategories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ResumeCategories");
        }
    }
}
