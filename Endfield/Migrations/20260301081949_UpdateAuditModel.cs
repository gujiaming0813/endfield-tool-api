using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endfield.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "video_tags",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "created_by",
                table: "video_tags",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_name",
                table: "video_tags",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "video_tags",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "updated_by",
                table: "video_tags",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_name",
                table: "video_tags",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "bilibili_videos",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "bilibili_videos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "created_by",
                table: "bilibili_videos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_name",
                table: "bilibili_videos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "updated_by",
                table: "bilibili_videos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_name",
                table: "bilibili_videos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "comment",
                table: "video_tags");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "video_tags");

            migrationBuilder.DropColumn(
                name: "created_name",
                table: "video_tags");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "video_tags");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "video_tags");

            migrationBuilder.DropColumn(
                name: "updated_name",
                table: "video_tags");

            migrationBuilder.DropColumn(
                name: "comment",
                table: "bilibili_videos");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "bilibili_videos");

            migrationBuilder.DropColumn(
                name: "created_name",
                table: "bilibili_videos");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "bilibili_videos");

            migrationBuilder.DropColumn(
                name: "updated_name",
                table: "bilibili_videos");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "bilibili_videos",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);
        }
    }
}
