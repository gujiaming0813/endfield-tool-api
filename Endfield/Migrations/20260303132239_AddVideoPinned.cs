using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endfield.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoPinned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "bilibili_videos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "bilibili_videos");
        }
    }
}
