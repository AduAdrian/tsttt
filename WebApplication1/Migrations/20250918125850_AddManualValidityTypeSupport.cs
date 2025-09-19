using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddManualValidityTypeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Clients_ClientId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "SmsNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Clients_ValidityType",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LastExpiryNotificationSent",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LastExpiryNotificationStatus",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ShouldReceiveSmsNotifications",
                table: "Clients");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Clients_ClientId",
                table: "Appointments",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Clients_ClientId",
                table: "Appointments");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExpiryNotificationSent",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastExpiryNotificationStatus",
                table: "Clients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldReceiveSmsNotifications",
                table: "Clients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SmsNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsProcessed = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsNotifications_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ValidityType",
                table: "Clients",
                column: "ValidityType");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_ClientId",
                table: "SmsNotifications",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_IsProcessed",
                table: "SmsNotifications",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_ScheduledFor",
                table: "SmsNotifications",
                column: "ScheduledFor");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_Status",
                table: "SmsNotifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_Status_ScheduledFor_IsProcessed",
                table: "SmsNotifications",
                columns: new[] { "Status", "ScheduledFor", "IsProcessed" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_Type",
                table: "SmsNotifications",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Clients_ClientId",
                table: "Appointments",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
