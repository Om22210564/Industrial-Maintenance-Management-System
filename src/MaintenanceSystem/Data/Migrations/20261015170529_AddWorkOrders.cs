using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_Equipment_EquipmentId",
                table: "MaintenanceRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaintenanceRequest",
                table: "MaintenanceRequest");

            migrationBuilder.RenameTable(
                name: "MaintenanceRequest",
                newName: "MaintenanceRequests");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequest_EquipmentId",
                table: "MaintenanceRequests",
                newName: "IX_MaintenanceRequests_EquipmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaintenanceRequests",
                table: "MaintenanceRequests",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintenanceRequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AssignedTo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_MaintenanceRequests_MaintenanceRequestId",
                        column: x => x.MaintenanceRequestId,
                        principalTable: "MaintenanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_MaintenanceRequestId",
                table: "WorkOrders",
                column: "MaintenanceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Equipment_EquipmentId",
                table: "MaintenanceRequests",
                column: "EquipmentId",
                principalTable: "Equipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Equipment_EquipmentId",
                table: "MaintenanceRequests");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaintenanceRequests",
                table: "MaintenanceRequests");

            migrationBuilder.RenameTable(
                name: "MaintenanceRequests",
                newName: "MaintenanceRequest");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequests_EquipmentId",
                table: "MaintenanceRequest",
                newName: "IX_MaintenanceRequest_EquipmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaintenanceRequest",
                table: "MaintenanceRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_Equipment_EquipmentId",
                table: "MaintenanceRequest",
                column: "EquipmentId",
                principalTable: "Equipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
