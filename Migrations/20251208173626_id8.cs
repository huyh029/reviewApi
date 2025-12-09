using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class id8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Users_ManagerId",
                table: "Evaluations");

            migrationBuilder.AlterColumn<int>(
                name: "ManagerId",
                table: "Evaluations",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Users_ManagerId",
                table: "Evaluations",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Users_ManagerId",
                table: "Evaluations");

            migrationBuilder.AlterColumn<int>(
                name: "ManagerId",
                table: "Evaluations",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Users_ManagerId",
                table: "Evaluations",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
