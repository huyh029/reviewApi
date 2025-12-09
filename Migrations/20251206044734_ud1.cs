using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class ud1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluationFlows",
                columns: table => new
                {
                    Code = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlows", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlowsDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EvaluationFlowCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    DepartmentCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlowsDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowsDepartments_Departments_DepartmentCode",
                        column: x => x.DepartmentCode,
                        principalTable: "Departments",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowsDepartments_EvaluationFlows_EvaluationFlowCode",
                        column: x => x.EvaluationFlowCode,
                        principalTable: "EvaluationFlows",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlowsRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EvaluationFlowCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    RoleCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ParentId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlowsRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowsRoles_EvaluationFlowsRoles_ParentId",
                        column: x => x.ParentId,
                        principalTable: "EvaluationFlowsRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowsRoles_EvaluationFlows_EvaluationFlowCode",
                        column: x => x.EvaluationFlowCode,
                        principalTable: "EvaluationFlows",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowsRoles_Roles_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "Roles",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowsDepartments_DepartmentCode",
                table: "EvaluationFlowsDepartments",
                column: "DepartmentCode");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowsDepartments_EvaluationFlowCode",
                table: "EvaluationFlowsDepartments",
                column: "EvaluationFlowCode");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowsRoles_EvaluationFlowCode",
                table: "EvaluationFlowsRoles",
                column: "EvaluationFlowCode");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowsRoles_ParentId",
                table: "EvaluationFlowsRoles",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFlowsRoles_RoleCode",
                table: "EvaluationFlowsRoles",
                column: "RoleCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationFlowsDepartments");

            migrationBuilder.DropTable(
                name: "EvaluationFlowsRoles");

            migrationBuilder.DropTable(
                name: "EvaluationFlows");
        }
    }
}
