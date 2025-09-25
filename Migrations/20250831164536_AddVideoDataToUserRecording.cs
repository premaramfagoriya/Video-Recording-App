using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoDataToUserRecording : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "UserRecordings",
                newName: "FileName");

            migrationBuilder.AddColumn<byte[]>(
                name: "VideoData",
                table: "UserRecordings",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoData",
                table: "UserRecordings");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "UserRecordings",
                newName: "FilePath");
        }
    }
}
