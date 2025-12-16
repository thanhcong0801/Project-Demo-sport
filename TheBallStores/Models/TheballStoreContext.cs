using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TheBallStores.Models
{
    public partial class TheballStoreContext : DbContext
    {
        public TheballStoreContext()
        {
        }

        public TheballStoreContext(DbContextOptions<TheballStoreContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; } = null!;
        public virtual DbSet<DonHang> DonHangs { get; set; } = null!;
        public virtual DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public virtual DbSet<KichThuoc> KichThuocs { get; set; } = null!;
        public virtual DbSet<LoaiSanPham> LoaiSanPhams { get; set; } = null!;
        public virtual DbSet<SanPham> SanPhams { get; set; } = null!;
        public virtual DbSet<SanPhamChiTiet> SanPhamChiTiets { get; set; } = null!;
        public virtual DbSet<HinhAnhSanPham> HinhAnhSanPhams { get; set; } = null!; // THÊM DbSet Ảnh
        public virtual DbSet<Voucher> Vouchers { get; set; } = null!; // THÊM DbSet Voucher


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CẤU HÌNH ChiTietDonHang (Giữ nguyên cấu hình cũ)
            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.HasKey(e => e.MaCt)
                    .HasName("PK__ChiTietD__27258E7446AD14B2");

                entity.Property(e => e.MaCt).HasColumnName("MaCT");

                entity.Property(e => e.DonGia).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.MaChiTietNavigation)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaChiTiet)
                    .HasConstraintName("FK__ChiTietDo__MaChi__4BAC3F29");

                entity.HasOne(d => d.MaDonHangNavigation)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaDonHang)
                    .HasConstraintName("FK__ChiTietDo__MaDon__4AB81AF0");
            });

            // CẤU HÌNH DonHang (Giữ nguyên cấu hình cũ + thêm Voucher)
            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.HasKey(e => e.MaDonHang)
                    .HasName("PK__DonHangs__129584AD370F3EB8");

                entity.Property(e => e.DiaChiGiaoHang).HasMaxLength(200);

                entity.Property(e => e.MaKh).HasColumnName("MaKH");

                entity.Property(e => e.NgayDat)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SoDienThoaiNguoiNhan)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.TenNguoiNhan).HasMaxLength(100);

                entity.Property(e => e.TongTien).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.TrangThai)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("(N'Mới đặt')");

                // Cấu hình Voucher
                entity.Property(e => e.MaVoucher).HasMaxLength(50);
                entity.Property(e => e.SoTienGiam).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.MaKhNavigation)
                    .WithMany(p => p.DonHangs)
                    .HasForeignKey(d => d.MaKh)
                    .HasConstraintName("FK__DonHangs__MaKH__47DBAE45");
            });

            // CẤU HÌNH KhachHang (Giữ nguyên cấu hình cũ)
            modelBuilder.Entity<KhachHang>(entity =>
            {
                entity.HasKey(e => e.MaKh)
                    .HasName("PK__KhachHan__2725CF1E7C01B1ED");

                entity.Property(e => e.MaKh).HasColumnName("MaKH");

                entity.Property(e => e.DiaChi).HasMaxLength(200);

                entity.Property(e => e.DienThoai)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.HoTen).HasMaxLength(100);

                entity.Property(e => e.MatKhau).HasMaxLength(100);

                entity.Property(e => e.VaiTro)
                    .HasMaxLength(20)
                    .HasDefaultValueSql("('Customer')");
            });

            // CẤU HÌNH KichThuoc (Giữ nguyên cấu hình cũ)
            modelBuilder.Entity<KichThuoc>(entity =>
            {
                entity.HasKey(e => e.MaSize)
                    .HasName("PK__KichThuo__A787E7ED598484B8");

                entity.Property(e => e.TenSize).HasMaxLength(20);
            });

            // CẤU HÌNH LoaiSanPham (Cập nhật cấu hình mới)
            modelBuilder.Entity<LoaiSanPham>(entity =>
            {
                entity.ToTable("LoaiSanPhams");
                entity.HasKey(e => e.MaLoai);
                entity.Property(e => e.TenLoai).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NhomLoai).HasMaxLength(50);
            });

            // CẤU HÌNH SanPham (Cập nhật cấu hình mới và FK an toàn)
            modelBuilder.Entity<SanPham>(entity =>
            {
                entity.ToTable("SanPhams");
                entity.HasKey(e => e.MaSp);
                entity.Property(e => e.MaSp).HasColumnName("MaSP");
                entity.Property(e => e.TenSp).IsRequired().HasMaxLength(200).HasColumnName("TenSP");
                entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)").IsRequired();
                entity.Property(e => e.AnhDaiDien).HasMaxLength(200);
                entity.Property(e => e.MoTa);

                // FK an toàn: Khi danh mục bị xóa, MaLoai sẽ set về NULL
                entity.HasOne(d => d.MaLoaiNavigation)
                    .WithMany(p => p.SanPhams)
                    .HasForeignKey(d => d.MaLoai)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__SanPhams__MaLoai__3B75D760");
            });

            // CẤU HÌNH SanPhamChiTiet (Giữ nguyên cấu hình cũ)
            modelBuilder.Entity<SanPhamChiTiet>(entity =>
            {
                entity.HasKey(e => e.MaChiTiet)
                    .HasName("PK__SanPhamC__CDF0A11446FCB7A6");

                entity.Property(e => e.MaSp).HasColumnName("MaSP");

                entity.Property(e => e.SoLuongTon).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.MaSizeNavigation)
                    .WithMany(p => p.SanPhamChiTiets)
                    .HasForeignKey(d => d.MaSize)
                    .HasConstraintName("FK__SanPhamCh__MaSiz__403A8C7D");

                entity.HasOne(d => d.MaSpNavigation)
                    .WithMany(p => p.SanPhamChiTiets)
                    .HasForeignKey(d => d.MaSp)
                    .HasConstraintName("FK__SanPhamChi__MaSP__3F466844");
            });

            // CẤU HÌNH HinhAnhSanPham (MỚI - Đã thêm FK Cascade)
            modelBuilder.Entity<HinhAnhSanPham>(entity =>
            {
                entity.ToTable("HinhAnhSanPham");
                entity.HasKey(e => e.MaAnh);
                entity.Property(e => e.DuongDanAnh).IsRequired().HasMaxLength(255);

                // Quan hệ với SanPham: Xóa sản phẩm sẽ xóa ảnh liên quan
                entity.HasOne(d => d.MaSpNavigation)
                    .WithMany(p => p.HinhAnhSanPhams)
                    .HasForeignKey(d => d.MaSp)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_HinhAnhSanPham_SanPham");
            });

            // CẤU HÌNH VOUCHER (MỚI)
            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.ToTable("Voucher");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}