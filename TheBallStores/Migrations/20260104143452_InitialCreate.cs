using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheBallStores.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    MaKH = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HoTen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MatKhau = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DienThoai = table.Column<string>(type: "TEXT", unicode: false, maxLength: 15, nullable: true),
                    VaiTro = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true, defaultValueSql: "'Customer'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KhachHan__2725CF1E7C01B1ED", x => x.MaKH);
                });

            migrationBuilder.CreateTable(
                name: "KichThuocs",
                columns: table => new
                {
                    MaSize = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenSize = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KichThuo__A787E7ED598484B8", x => x.MaSize);
                });

            migrationBuilder.CreateTable(
                name: "LoaiSanPhams",
                columns: table => new
                {
                    MaLoai = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenLoai = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NhomLoai = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiSanPhams", x => x.MaLoai);
                });

            migrationBuilder.CreateTable(
                name: "Voucher",
                columns: table => new
                {
                    MaVoucher = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenChuongTrinh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LoaiGiamGia = table.Column<int>(type: "INTEGER", nullable: false),
                    GiaTriGiam = table.Column<decimal>(type: "decimal(18, 0)", nullable: false),
                    GiamToiDa = table.Column<decimal>(type: "decimal(18, 0)", nullable: true),
                    DonToiThieu = table.Column<decimal>(type: "decimal(18, 0)", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime", nullable: false),
                    SoLuongConLai = table.Column<int>(type: "INTEGER", nullable: false),
                    KichHoat = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voucher", x => x.MaVoucher);
                });

            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    MaDonHang = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NgayDat = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "datetime('now')"),
                    MaKH = table.Column<int>(type: "INTEGER", nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(18, 0)", nullable: true),
                    TrangThai = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, defaultValueSql: "'Mới đặt'"),
                    TenNguoiNhan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DiaChiGiaoHang = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SoDienThoaiNguoiNhan = table.Column<string>(type: "TEXT", unicode: false, maxLength: 15, nullable: true),
                    MaVoucher = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SoTienGiam = table.Column<decimal>(type: "decimal(18, 0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DonHangs__129584AD370F3EB8", x => x.MaDonHang);
                    table.ForeignKey(
                        name: "FK__DonHangs__MaKH__47DBAE45",
                        column: x => x.MaKH,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKH");
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    MaSP = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenSP = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    GiaBan = table.Column<decimal>(type: "decimal(18, 0)", nullable: false),
                    AnhDaiDien = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "TEXT", nullable: true),
                    MaLoai = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.MaSP);
                    table.ForeignKey(
                        name: "FK__SanPhams__MaLoai__3B75D760",
                        column: x => x.MaLoai,
                        principalTable: "LoaiSanPhams",
                        principalColumn: "MaLoai",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhSanPham",
                columns: table => new
                {
                    MaAnh = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaSp = table.Column<int>(type: "INTEGER", nullable: false),
                    DuongDanAnh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhSanPham", x => x.MaAnh);
                    table.ForeignKey(
                        name: "FK_HinhAnhSanPham_SanPham",
                        column: x => x.MaSp,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanPhamChiTiets",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaSP = table.Column<int>(type: "INTEGER", nullable: true),
                    MaSize = table.Column<int>(type: "INTEGER", nullable: true),
                    SoLuongTon = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SanPhamC__CDF0A11446FCB7A6", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK__SanPhamCh__MaSiz__403A8C7D",
                        column: x => x.MaSize,
                        principalTable: "KichThuocs",
                        principalColumn: "MaSize");
                    table.ForeignKey(
                        name: "FK__SanPhamChi__MaSP__3F466844",
                        column: x => x.MaSP,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    MaCT = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaDonHang = table.Column<int>(type: "INTEGER", nullable: true),
                    MaChiTiet = table.Column<int>(type: "INTEGER", nullable: true),
                    SoLuong = table.Column<int>(type: "INTEGER", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18, 0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChiTietD__27258E7446AD14B2", x => x.MaCT);
                    table.ForeignKey(
                        name: "FK__ChiTietDo__MaChi__4BAC3F29",
                        column: x => x.MaChiTiet,
                        principalTable: "SanPhamChiTiets",
                        principalColumn: "MaChiTiet");
                    table.ForeignKey(
                        name: "FK__ChiTietDo__MaDon__4AB81AF0",
                        column: x => x.MaDonHang,
                        principalTable: "DonHangs",
                        principalColumn: "MaDonHang");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaChiTiet",
                table: "ChiTietDonHangs",
                column: "MaChiTiet");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaDonHang",
                table: "ChiTietDonHangs",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_MaKH",
                table: "DonHangs",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPham_MaSp",
                table: "HinhAnhSanPham",
                column: "MaSp");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_MaSize",
                table: "SanPhamChiTiets",
                column: "MaSize");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhamChiTiets_MaSP",
                table: "SanPhamChiTiets",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_MaLoai",
                table: "SanPhams",
                column: "MaLoai");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "HinhAnhSanPham");

            migrationBuilder.DropTable(
                name: "Voucher");

            migrationBuilder.DropTable(
                name: "SanPhamChiTiets");

            migrationBuilder.DropTable(
                name: "DonHangs");

            migrationBuilder.DropTable(
                name: "KichThuocs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "LoaiSanPhams");
        }
    }
}
