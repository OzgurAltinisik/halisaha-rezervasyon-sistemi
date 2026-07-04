using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HalisahaApp.API.Migrations
{
    /// <inheritdoc />
    public partial class IlkMigrasyon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adminler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SifreHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SonGirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adminler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sahalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SahaKodu = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcilisSaati = table.Column<int>(type: "int", nullable: false),
                    KapanisSaati = table.Column<int>(type: "int", nullable: false),
                    SaatlikUcret = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NakitDeadlineGun = table.Column<int>(type: "int", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdminId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sahalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sahalar_Adminler_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Adminler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rezervasyonlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SahaId = table.Column<int>(type: "int", nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaslangicSaati = table.Column<int>(type: "int", nullable: false),
                    BitisSaati = table.Column<int>(type: "int", nullable: false),
                    ToplamUcret = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OdemeDurumu = table.Column<int>(type: "int", nullable: false),
                    IyzicoOdemeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IptalTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IptalNedeni = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezervasyonlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rezervasyonlar_Sahalar_SahaId",
                        column: x => x.SahaId,
                        principalTable: "Sahalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Adminler",
                columns: new[] { "Id", "AdSoyad", "Email", "OlusturmaTarihi", "SifreHash", "SonGirisTarihi" },
                values: new object[] { 1, "Halısaha Yöneticisi", "admin@halisaha.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "$2a$11$example_hash_buraya_gelecek", null });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_SahaId_Tarih_BaslangicSaati",
                table: "Rezervasyonlar",
                columns: new[] { "SahaId", "Tarih", "BaslangicSaati" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sahalar_AdminId",
                table: "Sahalar",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Sahalar_SahaKodu",
                table: "Sahalar",
                column: "SahaKodu",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rezervasyonlar");

            migrationBuilder.DropTable(
                name: "Sahalar");

            migrationBuilder.DropTable(
                name: "Adminler");
        }
    }
}
