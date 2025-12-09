using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class ud2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluationObjects",
                columns: table => new
                {
                    Code = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationObjects", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationObjectRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EvaluationObjectCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationObjectRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationObjectRoles_EvaluationObjects_EvaluationObjectCode",
                        column: x => x.EvaluationObjectCode,
                        principalTable: "EvaluationObjects",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationObjectRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationObjectRoles_EvaluationObjectCode",
                table: "EvaluationObjectRoles",
                column: "EvaluationObjectCode");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationObjectRoles_UserId",
                table: "EvaluationObjectRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationObjectRoles");

            migrationBuilder.DropTable(
                name: "EvaluationObjects");
        }
    }
}
