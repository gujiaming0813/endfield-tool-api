using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endfield.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "video_tags",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "bilibili_videos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_video_tags_is_deleted",
                table: "video_tags",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_bilibili_videos_is_deleted",
                table: "bilibili_videos",
                column: "is_deleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_video_tags_is_deleted",
                table: "video_tags");

            migrationBuilder.DropIndex(
                name: "IX_bilibili_videos_is_deleted",
                table: "bilibili_videos");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "video_tags");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "bilibili_videos");
        }
    }
}
