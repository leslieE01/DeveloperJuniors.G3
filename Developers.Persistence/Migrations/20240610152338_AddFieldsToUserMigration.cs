﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Developers.Persistence.Migrations;

/// <inheritdoc />
public partial class AddFieldsToUserMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Discriminator",
            table: "AspNetUsers",
            type: "nvarchar(21)",
            maxLength: 21,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "AspNetUsers",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LastName",
            table: "AspNetUsers",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Discriminator",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "FirstName",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "LastName",
            table: "AspNetUsers");
    }
}
