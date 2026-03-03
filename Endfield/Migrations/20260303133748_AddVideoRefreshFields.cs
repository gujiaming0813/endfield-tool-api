using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endfield.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoRefreshFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastRefreshTime",
                table: "bilibili_videos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefreshRetryCount",
                table: "bilibili_videos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RefreshStatus",
                table: "bilibili_videos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRefreshTime",
                table: "bilibili_videos");

            migrationBuilder.DropColumn(
                name: "RefreshRetryCount",
                table: "bilibili_videos");

            migrationBuilder.DropColumn(
                name: "RefreshStatus",
                table: "bilibili_videos");
        }
    }
}
