using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddSimplifiedClientsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Clients_ClientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Name",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ClientId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Clients",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clients",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Clients",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Clients",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Clients",
                type: "TEXT",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Clients",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Appointments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Name",
                table: "Clients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClientId",
                table: "Appointments",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Clients_ClientId",
                table: "Appointments",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
