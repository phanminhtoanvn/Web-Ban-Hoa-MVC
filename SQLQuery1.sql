USE [master]
GO
CREATE DATABASE [QL_BanHoa]
GO

-- 2. Chọn Database để làm việc
USE [QL_BanHoa]
GO
/****** Object:  Table [dbo].[BinhLuan]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BinhLuan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MaHoa] [int] NOT NULL,
	[HoTen] [nvarchar](100) NOT NULL,
	[NoiDung] [nvarchar](500) NOT NULL,
	[SoSao] [int] NOT NULL,
	[Ngay] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblChiTietHoaDon]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblChiTietHoaDon](
	[MaHD] [int] NOT NULL,
	[MaHoa] [int] NOT NULL,
	[SoLuong] [int] NULL,
	[GiaBan] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaHD] ASC,
	[MaHoa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDanhMucHoa]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDanhMucHoa](
	[MaDM] [int] IDENTITY(1,1) NOT NULL,
	[TenDM] [nvarchar](100) NULL,
	[GhiChu] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaDM] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHinhAnh]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHinhAnh](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MaHoa] [int] NULL,
	[HinhAnh] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHoa]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHoa](
	[MaHoa] [int] IDENTITY(1,1) NOT NULL,
	[TenHoa] [nvarchar](200) NULL,
	[GiaBan] [decimal](18, 2) NULL,
	[MoTa] [nvarchar](max) NULL,
	[AnhDaiDien] [nvarchar](255) NULL,
	[DonViTinh] [nvarchar](50) NULL,
	[MauSacChuDao] [nvarchar](100) NULL,
	[MaDM] [int] NULL,
	[MaLoaiChinh] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[MaHoa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblHoaDon]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblHoaDon](
	[MaHD] [int] IDENTITY(1,1) NOT NULL,
	[MaKH] [int] NULL,
	[MaNV] [int] NULL,
	[NgayLap] [datetime] NULL,
	[TongTien] [decimal](18, 2) NULL,
	[TinhTrang] [int] NULL,
	[DiaChiGiaoHang] [nvarchar](255) NULL,
	[DaThanhToan] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[MaHD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblKhachHang]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblKhachHang](
	[MaKH] [int] IDENTITY(1,1) NOT NULL,
	[TenKH] [nvarchar](100) NULL,
	[MatKhau] [nvarchar](100) NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[NamSinh] [int] NULL,
	[Avarta] [nvarchar](255) NULL,
	[DienThoai] [nvarchar](20) NULL,
	[Email] [nvarchar](100) NULL,
	[DiaChi] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaKH] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLoaiHoaChinh]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLoaiHoaChinh](
	[MaLoaiChinh] [int] IDENTITY(1,1) NOT NULL,
	[TenLoaiChinh] [nvarchar](100) NULL,
	[MoTa] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaLoaiChinh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblNhanVien]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblNhanVien](
	[MaNV] [int] IDENTITY(1,1) NOT NULL,
	[MatKhau] [nvarchar](100) NULL,
	[TenNV] [nvarchar](100) NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[NamSinh] [int] NULL,
	[VaiTro] [int] NULL,
	[TaiKhoan] [nchar](100) NULL,
 CONSTRAINT [PK__tblNhanV__2725D70AE64377F1] PRIMARY KEY CLUSTERED 
(
	[MaNV] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTinhTrang]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTinhTrang](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TinhTrangHoaDon] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblVaiTro]    Script Date: 23/12/2025 7:32:32 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblVaiTro](
	[IDVaiTro] [int] IDENTITY(1,1) NOT NULL,
	[TenVaiTro] [nvarchar](50) NULL,
	[MoTa] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[IDVaiTro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[BinhLuan] ON 

INSERT [dbo].[BinhLuan] ([Id], [MaHoa], [HoTen], [NoiDung], [SoSao], [Ngay]) VALUES (1, 1, N'Minh Khoa', N'Hoa r�t �p, ng��i y�u t�i r�t th�ch! Giao h�ng nhanh.', 5, CAST(N'2025-11-02T10:00:00.000' AS DateTime))
INSERT [dbo].[BinhLuan] ([Id], [MaHoa], [HoTen], [NoiDung], [SoSao], [Ngay]) VALUES (2, 2, N'Lan Anh', N'L�ng hoa khai tr��ng nh�n sang tr�ng, tuy nhi�n giao h�i tr� 30 ph�t.', 4, CAST(N'2025-11-03T11:00:00.000' AS DateTime))
INSERT [dbo].[BinhLuan] ([Id], [MaHoa], [HoTen], [NoiDung], [SoSao], [Ngay]) VALUES (3, 3, N'H�u T�n', N'Gi� hoa Ly th�m l�m, m� m�nh r�t �ng �. S� �ng h� shop ti�p.', 5, CAST(N'2025-11-03T14:30:00.000' AS DateTime))
INSERT [dbo].[BinhLuan] ([Id], [MaHoa], [HoTen], [NoiDung], [SoSao], [Ngay]) VALUES (4, 8, N'B�o Tr�m', N'B� hoa h��ng d��ng h�i nh� so v�i �nh, nh�ng hoa t��i.', 4, CAST(N'2025-11-05T10:00:00.000' AS DateTime))
INSERT [dbo].[BinhLuan] ([Id], [MaHoa], [HoTen], [NoiDung], [SoSao], [Ngay]) VALUES (5, 1, N'Nguyen Hoai Phong', N'Test', 5, CAST(N'2025-12-20T23:29:19.530' AS DateTime))
INSERT [dbo].[BinhLuan] ([Id], [MaHoa], [HoTen], [NoiDung], [SoSao], [Ngay]) VALUES (6, 1, N'Nguyen Hoai Phong', N'1', 4, CAST(N'2025-12-20T23:44:15.080' AS DateTime))
INSERT [dbo].[BinhLuan] ([Id], [MaHoa], [HoTen], [NoiDung], [SoSao], [Ngay]) VALUES (7, 1, N'Nguyen Hoai Phong', N'2', 3, CAST(N'2025-12-20T23:44:25.573' AS DateTime))
SET IDENTITY_INSERT [dbo].[BinhLuan] OFF
GO
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (1, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (2, 2, 1, CAST(1200000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (2, 3, 1, CAST(750000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (3, 5, 1, CAST(800000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (4, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (4, 8, 1, CAST(350000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (5, 6, 1, CAST(1100000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (6, 7, 1, CAST(2500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (9, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (9, 2, 1, CAST(1200000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (10, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (11, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (12, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (13, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (14, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (15, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (16, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (17, 1, 2, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (20, 4, 2, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (21, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (22, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (23, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (24, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (26, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (27, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (28, 2, 1, CAST(1200000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (30, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (30, 9, 1, CAST(200000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (33, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (33, 2, 1, CAST(1200000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (34, 2, 1, CAST(1200000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (34, 3, 1, CAST(750000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (34, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (37, 1, 3, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (38, 1, 1, CAST(950000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (38, 2, 1, CAST(1200000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (39, 2, 1, CAST(1200000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (40, 3, 1, CAST(750000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (40, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (41, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (42, 3, 1, CAST(750000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (42, 4, 1, CAST(500000.00 AS Decimal(18, 2)))
INSERT [dbo].[tblChiTietHoaDon] ([MaHD], [MaHoa], [SoLuong], [GiaBan]) VALUES (43, 2, 1, CAST(1200000.00 AS Decimal(18, 2)))
GO
SET IDENTITY_INSERT [dbo].[tblDanhMucHoa] ON 

INSERT [dbo].[tblDanhMucHoa] ([MaDM], [TenDM], [GhiChu]) VALUES (1, N'Hoa sinh nh�t', N'Hoa ch�c m�ng sinh nh�t')
INSERT [dbo].[tblDanhMucHoa] ([MaDM], [TenDM], [GhiChu]) VALUES (2, N'Hoa khai tr��ng', N'Hoa ch�c m�ng khai tr��ng, c�a h�ng m�i')
INSERT [dbo].[tblDanhMucHoa] ([MaDM], [TenDM], [GhiChu]) VALUES (3, N'Hoa t�nh y�u', N'Hoa cho c�c c�p �i')
INSERT [dbo].[tblDanhMucHoa] ([MaDM], [TenDM], [GhiChu]) VALUES (4, N'Hoa chia bu�n', N'V�ng hoa, l�ng hoa vi�ng')
SET IDENTITY_INSERT [dbo].[tblDanhMucHoa] OFF
GO
SET IDENTITY_INSERT [dbo].[tblHoa] ON 

INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (1, N'B� h�ng � 99 �a', CAST(950000.00 AS Decimal(18, 2)), N'B� hoa t�nh y�u v)nh c�u v�i 99 b�ng h�ng � th�m.', N'hoa-hong-99.jpg', N'B�', N'�', 3, 1)
INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (2, N'L�ng hoa khai tr��ng Ph�t L�c', CAST(1200000.00 AS Decimal(18, 2)), N'L�ng hoa k�t h�p H��ng D��ng v� hoa Ly, ch�c m�ng kinh doanh ph�t t�i.', N'khai-truong-phat-loc.jpg', N'L�ng', N'V�ng', 2, 3)
INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (3, N'Gi� hoa Ly ch�c m�ng sinh nh�t', CAST(750000.00 AS Decimal(18, 2)), N'Gi� hoa Ly th�m ng�t, m�u s�c trang nh�, ph� h�p t�ng sinh nh�t s�p ho�c m�.', N'gio-hoa-ly.jpg', N'Gi�', N'H�ng', 1, 2)
INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (4, N'B� h�ng ph�n ng�t ng�o', CAST(500000.00 AS Decimal(18, 2)), N'B� hoa h�ng ph�n 20 b�ng, t�ng sinh nh�t b�n g�i.', N'hoa-hong-phan.jpg', N'B�', N'H�ng', 1, 1)
INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (5, N'V�ng hoa c�c tr�ng', CAST(800000.00 AS Decimal(18, 2)), N'V�ng hoa chia bu�n, t�ng m�u tr�ng tinh khi�t, th� hi�n s� k�nh vi�ng.', N'hoa-chia-buon.jpg', N'V�ng', N'Tr�ng', 4, NULL)
INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (6, N'H�p hoa tr�i tim', CAST(1100000.00 AS Decimal(18, 2)), N'Hoa h�ng � x�p h�nh tr�i tim trong h�p qu� sang tr�ng.', N'hop-hoa-tim.jpg', N'H�p', N'�', 3, 1)
INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (7, N'K� hoa khai tr��ng 2 t�ng', CAST(2500000.00 AS Decimal(18, 2)), N'K� hoa l�n 2 t�ng, k�t h�p h��ng d��ng, h�ng, ly.', N'ke-hoa-2-tang.jpg', N'K�', N'V�ng', 2, 3)
INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (8, N'B� hoa h��ng d��ng nh�', CAST(350000.00 AS Decimal(18, 2)), N'B� hoa 5 b�ng h��ng d��ng, t�ng sinh nh�t b�n b�.', N'bo-huong-duong.jpg', N'B�', N'V�ng', 1, 3)
INSERT [dbo].[tblHoa] ([MaHoa], [TenHoa], [GiaBan], [MoTa], [AnhDaiDien], [DonViTinh], [MauSacChuDao], [MaDM], [MaLoaiChinh]) VALUES (9, N'Hoa k�o t�ng b�n g�i', CAST(200000.00 AS Decimal(18, 2)), N'', N'hoakeomut-e430.jpg', N'B�', N'H�ng', 3, 4)
SET IDENTITY_INSERT [dbo].[tblHoa] OFF
GO
SET IDENTITY_INSERT [dbo].[tblHoaDon] ON 

INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (1, 1, NULL, CAST(N'2025-11-01T10:30:00.000' AS DateTime), CAST(950000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (2, 2, NULL, CAST(N'2025-11-02T14:00:00.000' AS DateTime), CAST(1950000.00 AS Decimal(18, 2)), 2, N'15 C�u Gi�y, HN', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (3, 3, NULL, CAST(N'2025-11-03T09:15:00.000' AS DateTime), CAST(800000.00 AS Decimal(18, 2)), 3, N'100 Phan Vn Tr�, B�nh Th�nh', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (4, 1, NULL, CAST(N'2025-11-04T11:00:00.000' AS DateTime), CAST(1300000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (5, 4, NULL, CAST(N'2025-11-04T15:20:00.000' AS DateTime), CAST(1100000.00 AS Decimal(18, 2)), 4, N'50 L� L�i, � N�ng', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (6, 2, NULL, CAST(N'2025-11-05T08:00:00.000' AS DateTime), CAST(2500000.00 AS Decimal(18, 2)), 2, N'15 C�u Gi�y, HN', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (9, 8, NULL, CAST(N'2025-11-11T09:34:49.133' AS DateTime), CAST(2150000.00 AS Decimal(18, 2)), 1, N'140 LTT', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (10, 1, NULL, CAST(N'2025-11-11T10:04:36.783' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (11, 1, NULL, CAST(N'2025-11-11T10:11:03.160' AS DateTime), CAST(950000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (12, 1, NULL, CAST(N'2025-11-11T10:22:20.010' AS DateTime), CAST(950000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (13, 6, NULL, CAST(N'2025-11-11T10:38:35.323' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'140 L� Tr�ng T�n, T�n Ph�, TPHCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (14, 6, NULL, CAST(N'2025-11-11T10:55:34.413' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'140 L� Tr�ng T�n, T�n Ph�, TPHCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (15, 6, NULL, CAST(N'2025-11-11T11:00:42.467' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'140 L� Tr�ng T�n, T�n Ph�, TPHCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (16, 1, NULL, CAST(N'2025-11-11T11:11:03.557' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (17, 1, NULL, CAST(N'2025-11-11T11:26:42.820' AS DateTime), CAST(1900000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (20, 1, NULL, CAST(N'2025-11-11T11:46:35.383' AS DateTime), CAST(1000000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (21, 6, NULL, CAST(N'2025-11-11T16:43:55.860' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'140 L� Tr�ng T�n, T�n Ph�, TPHCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (22, 6, NULL, CAST(N'2025-11-11T16:48:21.183' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'140 L� Tr�ng T�n, T�n Ph�, TPHCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (23, 1, NULL, CAST(N'2025-11-11T16:50:22.340' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (24, 1, NULL, CAST(N'2025-11-11T16:53:13.613' AS DateTime), CAST(950000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (25, 1, NULL, CAST(N'2025-11-11T16:53:19.077' AS DateTime), CAST(0.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (26, 1, NULL, CAST(N'2025-11-19T22:22:04.557' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (27, 1, NULL, CAST(N'2025-11-21T12:03:20.607' AS DateTime), CAST(950000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (28, 1, NULL, CAST(N'2025-11-22T22:25:16.480' AS DateTime), CAST(1200000.00 AS Decimal(18, 2)), 4, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (30, 9, NULL, CAST(N'2025-11-30T21:04:47.857' AS DateTime), CAST(1150000.00 AS Decimal(18, 2)), 4, N'B�nh Th�nh, TPHCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (33, 10, NULL, CAST(N'2025-12-09T19:27:34.427' AS DateTime), CAST(2150000.00 AS Decimal(18, 2)), 1, N'ada', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (34, 1, NULL, CAST(N'2025-12-20T17:56:06.527' AS DateTime), CAST(2450000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 0)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (37, 1, NULL, CAST(N'2025-12-20T18:09:09.107' AS DateTime), CAST(2850000.00 AS Decimal(18, 2)), 1, N'200 Nguy�n X�, HCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (38, 1, NULL, CAST(N'2025-12-20T18:55:23.493' AS DateTime), CAST(2150000.00 AS Decimal(18, 2)), 1, N'24C Nguy�n S�ng, T�y Th�nh, TPHCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (39, 1, NULL, CAST(N'2025-12-20T19:19:31.073' AS DateTime), CAST(1200000.00 AS Decimal(18, 2)), 1, N'140 L� Tr�ng T�n, T�y Th�nh, T�n Ph�, TP.HCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (40, 1, NULL, CAST(N'2025-12-20T19:44:54.957' AS DateTime), CAST(1250000.00 AS Decimal(18, 2)), 3, N'140 L� Tr�ng T�n, T�y Th�nh, T�n Ph�, TP.HCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (41, 1, NULL, CAST(N'2025-12-20T19:47:34.843' AS DateTime), CAST(500000.00 AS Decimal(18, 2)), 3, N'140 L� Tr�ng T�n, T�y Th�nh, T�n Ph�, TP.HCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (42, 1, NULL, CAST(N'2025-12-20T19:48:52.673' AS DateTime), CAST(1250000.00 AS Decimal(18, 2)), 3, N'140 L� Tr�ng T�n, T�y Th�nh, T�n Ph�, TP.HCM', 1)
INSERT [dbo].[tblHoaDon] ([MaHD], [MaKH], [MaNV], [NgayLap], [TongTien], [TinhTrang], [DiaChiGiaoHang], [DaThanhToan]) VALUES (43, 11, NULL, CAST(N'2025-12-21T07:10:14.373' AS DateTime), CAST(1200000.00 AS Decimal(18, 2)), 2, N'140 L� Tr�ng T�n, T�y Th�nh, T�n Ph�, TP.HCM', 1)
SET IDENTITY_INSERT [dbo].[tblHoaDon] OFF
GO
SET IDENTITY_INSERT [dbo].[tblKhachHang] ON 

INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (1, N'Nguy�n Vn A', N'1', N'Nam', 1995, N'avt1.jpg', N'0909123456', N'a@gmail.com', N'140 L� Tr�ng T�n, T�y Th�nh, T�n Ph�, TP.HCM')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (2, N'L� Th� B', N'abcdef', N'N�', 1998, N'avt2.jpg', N'0911222333', N'b@gmail.com', N'15 C�u Gi�y, HN')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (3, N'Tr�n Minh K', N'kieuhoa', N'Nam', 2000, N'avt3.jpg', N'0987654321', N'k@yahoo.com', N'100 Phan Vn Tr�, B�nh Th�nh')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (4, N'Ph�m Th� D', N'123456', N'N�', 1999, N'avt4.jpg', N'0912345678', N'd@gmail.com', N'50 L� L�i, � N�ng')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (5, N'� Ho�i E', N'123456', N'N�', 1993, N'avt5.jpg', N'0905111222', N'e@hotmail.com', N'22 H�ng B�ng, H� N�i')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (6, N'Tr�n Kh�nh Linh', N'123456', N'N�', 2007, NULL, N'0321249866', N'kltran0104@gmail.com', N'140 L� Tr�ng T�n, T�n Ph�, TPHCM')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (7, NULL, N'123456', NULL, NULL, NULL, N'0321249866', N'kltran0104@gmail.com', N'140 L� Tr�ng T�n, T�n Ph�, TPHCM')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (8, N'Tr�n Kh�nh Linh', N'123456', NULL, NULL, NULL, N'0321249866', N'kltran0104@gmail.com', N'140 LTT')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (9, N'Nguy�n �c T�m', N'123456', NULL, NULL, NULL, N'0123456798', N'tam123@gmail.com', N'B�nh Th�nh, TPHCM')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (10, N'Nguy�n Vn A', N'123456', NULL, NULL, NULL, N'0909123456', N'admin123@gmail.com', N'ada')
INSERT [dbo].[tblKhachHang] ([MaKH], [TenKH], [MatKhau], [GioiTinh], [NamSinh], [Avarta], [DienThoai], [Email], [DiaChi]) VALUES (11, N'Nguyen Hoai Phong', N'ae5b28d7', N'Nam', NULL, NULL, N'0357814916', N'pn64449@gmail.com', N'140 L� Tr�ng T�n, T�y Th�nh, T�n Ph�, TP.HCM')
SET IDENTITY_INSERT [dbo].[tblKhachHang] OFF
GO
SET IDENTITY_INSERT [dbo].[tblLoaiHoaChinh] ON 

INSERT [dbo].[tblLoaiHoaChinh] ([MaLoaiChinh], [TenLoaiChinh], [MoTa]) VALUES (1, N'Hoa H�ng', N'Bi�u t��ng c�a t�nh y�u')
INSERT [dbo].[tblLoaiHoaChinh] ([MaLoaiChinh], [TenLoaiChinh], [MoTa]) VALUES (2, N'Hoa Ly', N'Sang tr�ng v� thanh cao')
INSERT [dbo].[tblLoaiHoaChinh] ([MaLoaiChinh], [TenLoaiChinh], [MoTa]) VALUES (3, N'Hoa H��ng D��ng', N'T��ng tr�ng cho t��ng lai t��i s�ng')
INSERT [dbo].[tblLoaiHoaChinh] ([MaLoaiChinh], [TenLoaiChinh], [MoTa]) VALUES (4, N'Hoa K�o', N'K�o ��c l�m nh� hoa')
SET IDENTITY_INSERT [dbo].[tblLoaiHoaChinh] OFF
GO
SET IDENTITY_INSERT [dbo].[tblNhanVien] ON 

INSERT [dbo].[tblNhanVien] ([MaNV], [MatKhau], [TenNV], [GioiTinh], [NamSinh], [VaiTro], [TaiKhoan]) VALUES (1, N'admin123', N'Nguy�n Ho�i Phong', N'Nam', 1990, 1, N'admin123@gmail.com                                                                                  ')
INSERT [dbo].[tblNhanVien] ([MaNV], [MatKhau], [TenNV], [GioiTinh], [NamSinh], [VaiTro], [TaiKhoan]) VALUES (2, N'nv123', N'inh Th�nh �t', N'Nam', 1990, 2, N'nv123@gmail.com                                                                                     ')
SET IDENTITY_INSERT [dbo].[tblNhanVien] OFF
GO
SET IDENTITY_INSERT [dbo].[tblTinhTrang] ON 

INSERT [dbo].[tblTinhTrang] ([ID], [TinhTrangHoaDon]) VALUES (1, N'ang ch� x� l�')
INSERT [dbo].[tblTinhTrang] ([ID], [TinhTrangHoaDon]) VALUES (2, N'ang giao h�ng')
INSERT [dbo].[tblTinhTrang] ([ID], [TinhTrangHoaDon]) VALUES (3, N'� giao h�ng')
INSERT [dbo].[tblTinhTrang] ([ID], [TinhTrangHoaDon]) VALUES (4, N'� h�y')
SET IDENTITY_INSERT [dbo].[tblTinhTrang] OFF
GO
SET IDENTITY_INSERT [dbo].[tblVaiTro] ON 

INSERT [dbo].[tblVaiTro] ([IDVaiTro], [TenVaiTro], [MoTa]) VALUES (1, N'Admin', N'Qu�n tr� vi�n, to�n quy�n')
INSERT [dbo].[tblVaiTro] ([IDVaiTro], [TenVaiTro], [MoTa]) VALUES (2, N'Nh�n vi�n', N'Nh�n vi�n x� l� �n h�ng')
SET IDENTITY_INSERT [dbo].[tblVaiTro] OFF
GO
ALTER TABLE [dbo].[BinhLuan]  WITH CHECK ADD  CONSTRAINT [FK_BinhLuan_tblHoa] FOREIGN KEY([MaHoa])
REFERENCES [dbo].[tblHoa] ([MaHoa])
GO
ALTER TABLE [dbo].[BinhLuan] CHECK CONSTRAINT [FK_BinhLuan_tblHoa]
GO
ALTER TABLE [dbo].[tblChiTietHoaDon]  WITH CHECK ADD FOREIGN KEY([MaHoa])
REFERENCES [dbo].[tblHoa] ([MaHoa])
GO
ALTER TABLE [dbo].[tblChiTietHoaDon]  WITH CHECK ADD FOREIGN KEY([MaHD])
REFERENCES [dbo].[tblHoaDon] ([MaHD])
GO
ALTER TABLE [dbo].[tblHinhAnh]  WITH CHECK ADD FOREIGN KEY([MaHoa])
REFERENCES [dbo].[tblHoa] ([MaHoa])
GO
ALTER TABLE [dbo].[tblHoa]  WITH CHECK ADD FOREIGN KEY([MaDM])
REFERENCES [dbo].[tblDanhMucHoa] ([MaDM])
GO
ALTER TABLE [dbo].[tblHoa]  WITH CHECK ADD FOREIGN KEY([MaLoaiChinh])
REFERENCES [dbo].[tblLoaiHoaChinh] ([MaLoaiChinh])
GO
ALTER TABLE [dbo].[tblHoaDon]  WITH CHECK ADD FOREIGN KEY([MaKH])
REFERENCES [dbo].[tblKhachHang] ([MaKH])
GO
ALTER TABLE [dbo].[tblHoaDon]  WITH CHECK ADD  CONSTRAINT [FK__tblHoaDon__MaNV__5165187F] FOREIGN KEY([MaNV])
REFERENCES [dbo].[tblNhanVien] ([MaNV])
GO
ALTER TABLE [dbo].[tblHoaDon] CHECK CONSTRAINT [FK__tblHoaDon__MaNV__5165187F]
GO
ALTER TABLE [dbo].[tblHoaDon]  WITH CHECK ADD FOREIGN KEY([TinhTrang])
REFERENCES [dbo].[tblTinhTrang] ([ID])
GO
ALTER TABLE [dbo].[tblNhanVien]  WITH CHECK ADD  CONSTRAINT [FK__tblNhanVi__VaiTr__534D60F1] FOREIGN KEY([VaiTro])
REFERENCES [dbo].[tblVaiTro] ([IDVaiTro])
GO
ALTER TABLE [dbo].[tblNhanVien] CHECK CONSTRAINT [FK__tblNhanVi__VaiTr__534D60F1]
GO
USE [master]
GO
ALTER DATABASE [QL_BanHoa] SET  READ_WRITE 
GO
