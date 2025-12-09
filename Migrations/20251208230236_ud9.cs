using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class ud9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TypeChat",
                table: "EvaluationChats",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeChat",
                table: "EvaluationChats");
        }
    }
}
