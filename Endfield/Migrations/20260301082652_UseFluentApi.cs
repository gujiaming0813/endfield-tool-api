using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endfield.Api.Migrations
{
    /// <inheritdoc />
    public partial class UseFluentApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "video_tags",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "bilibili_videos",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "tag_id",
                table: "video_tag_mappings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "video_id",
                table: "video_tag_mappings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "video_tags",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "bilibili_videos",
                newName: "id");

            migrationBuilder.AlterColumn<int>(
                name: "tag_id",
                table: "video_tag_mappings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "video_id",
                table: "video_tag_mappings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);
        }
    }
}
