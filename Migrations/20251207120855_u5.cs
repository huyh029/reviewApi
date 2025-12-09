using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class u5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationFlowsRoles_EvaluationFlowsRoles_ParentId",
                table: "EvaluationFlowsRoles");

            migrationBuilder.AlterColumn<int>(
                name: "ParentId",
                table: "EvaluationFlowsRoles",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationFlowsRoles_EvaluationFlowsRoles_ParentId",
                table: "EvaluationFlowsRoles",
                column: "ParentId",
                principalTable: "EvaluationFlowsRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationFlowsRoles_EvaluationFlowsRoles_ParentId",
                table: "EvaluationFlowsRoles");

            migrationBuilder.AlterColumn<int>(
                name: "ParentId",
                table: "EvaluationFlowsRoles",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationFlowsRoles_EvaluationFlowsRoles_ParentId",
                table: "EvaluationFlowsRoles",
                column: "ParentId",
                principalTable: "EvaluationFlowsRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
