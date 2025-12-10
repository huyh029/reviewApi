using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class review : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriteriaSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParentCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentCode",
                        column: x => x.ParentCode,
                        principalTable: "Departments",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlows",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlows", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationObjects",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationObjects", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Classifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    CriteriaSetId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    Min = table.Column<int>(type: "integer", nullable: false),
                    Max = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classifications_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Criterias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriteriaSetId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    TypeScore = table.Column<string>(type: "text", nullable: false),
                    parentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criterias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Criterias_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Criterias_Criterias_parentId",
                        column: x => x.parentId,
                        principalTable: "Criterias",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CriteriaSetPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriteriaSetId = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaSetPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaSetPeriods_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlowsDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluationFlowCode = table.Column<string>(type: "text", nullable: false),
                    DepartmentCode = table.Column<string>(type: "text", nullable: false)
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
                        name: "FK_EvaluationFlowsDepartments_EvaluationFlows_EvaluationFlowCo~",
                        column: x => x.EvaluationFlowCode,
                        principalTable: "EvaluationFlows",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriteriaSetObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriteriaSetId = table.Column<int>(type: "integer", nullable: false),
                    EvaluationObjectCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaSetObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaSetObjects_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CriteriaSetObjects_EvaluationObjects_EvaluationObjectCode",
                        column: x => x.EvaluationObjectCode,
                        principalTable: "EvaluationObjects",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFlowsRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluationFlowCode = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFlowsRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationFlowsRoles_EvaluationFlowsRoles_ParentId",
                        column: x => x.ParentId,
                        principalTable: "EvaluationFlowsRoles",
                        principalColumn: "Id");
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

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    DepartmentCode = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentCode",
                        column: x => x.DepartmentCode,
                        principalTable: "Departments",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "Roles",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationObjectRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluationObjectCode = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriteriaSetId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ManagerId = table.Column<int>(type: "integer", nullable: true),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluationId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReplyToChatId = table.Column<int>(type: "integer", nullable: true),
                    SenderId = table.Column<int>(type: "integer", nullable: false),
                    RepliesId = table.Column<int>(type: "integer", nullable: true),
                    TypeChat = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationChats_EvaluationChats_RepliesId",
                        column: x => x.RepliesId,
                        principalTable: "EvaluationChats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EvaluationChats_EvaluationChats_ReplyToChatId",
                        column: x => x.ReplyToChatId,
                        principalTable: "EvaluationChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationChats_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationChats_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluationId = table.Column<int>(type: "integer", nullable: false),
                    CriteriaId = table.Column<int>(type: "integer", nullable: false),
                    SelfScore = table.Column<int>(type: "integer", nullable: true),
                    ManagerScore = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationScores_Criterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "Criterias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationScores_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationChatFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluationChatId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationChatFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationChatFiles_EvaluationChats_EvaluationChatId",
                        column: x => x.EvaluationChatId,
                        principalTable: "EvaluationChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classifications_CriteriaSetId",
                table: "Classifications",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Criterias_CriteriaSetId",
                table: "Criterias",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Criterias_parentId",
                table: "Criterias",
                column: "parentId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaSetObjects_CriteriaSetId",
                table: "CriteriaSetObjects",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaSetObjects_EvaluationObjectCode",
                table: "CriteriaSetObjects",
                column: "EvaluationObjectCode");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaSetPeriods_CriteriaSetId",
                table: "CriteriaSetPeriods",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentCode",
                table: "Departments",
                column: "ParentCode");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChatFiles_EvaluationChatId",
                table: "EvaluationChatFiles",
                column: "EvaluationChatId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChats_EvaluationId",
                table: "EvaluationChats",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChats_RepliesId",
                table: "EvaluationChats",
                column: "RepliesId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChats_ReplyToChatId",
                table: "EvaluationChats",
                column: "ReplyToChatId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChats_SenderId",
                table: "EvaluationChats",
                column: "SenderId");

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

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationObjectRoles_EvaluationObjectCode",
                table: "EvaluationObjectRoles",
                column: "EvaluationObjectCode");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationObjectRoles_UserId",
                table: "EvaluationObjectRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_CriteriaSetId",
                table: "Evaluations",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ManagerId",
                table: "Evaluations",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_UserId",
                table: "Evaluations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationScores_CriteriaId",
                table: "EvaluationScores",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationScores_EvaluationId",
                table: "EvaluationScores",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentCode",
                table: "Users",
                column: "DepartmentCode");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleCode",
                table: "Users",
                column: "RoleCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Classifications");

            migrationBuilder.DropTable(
                name: "CriteriaSetObjects");

            migrationBuilder.DropTable(
                name: "CriteriaSetPeriods");

            migrationBuilder.DropTable(
                name: "EvaluationChatFiles");

            migrationBuilder.DropTable(
                name: "EvaluationFlowsDepartments");

            migrationBuilder.DropTable(
                name: "EvaluationFlowsRoles");

            migrationBuilder.DropTable(
                name: "EvaluationObjectRoles");

            migrationBuilder.DropTable(
                name: "EvaluationScores");

            migrationBuilder.DropTable(
                name: "EvaluationChats");

            migrationBuilder.DropTable(
                name: "EvaluationFlows");

            migrationBuilder.DropTable(
                name: "EvaluationObjects");

            migrationBuilder.DropTable(
                name: "Criterias");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "CriteriaSets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
