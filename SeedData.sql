-- =============================================
-- LAMP STORE - DỮ LIỆU MẪU ĐÈN NGỦ (Docker DB)
-- =============================================

USE LampStoreDB;
GO

-- =============================================
-- 1. CATEGORIES
-- =============================================
DECLARE @CatDenNguThu UNIQUEIDENTIFIER = 'A1B2C3D4-1111-1111-1111-000000000001';
DECLARE @CatDenNguKhungGo UNIQUEIDENTIFIER = 'A1B2C3D4-1111-1111-1111-000000000002';
DECLARE @CatDenNguCamBien UNIQUEIDENTIFIER = 'A1B2C3D4-1111-1111-1111-000000000003';
DECLARE @CatDenNguThuyTinh UNIQUEIDENTIFIER = 'A1B2C3D4-1111-1111-1111-000000000004';
DECLARE @CatDenNguLed UNIQUEIDENTIFIER = 'A1B2C3D4-1111-1111-1111-000000000005';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @CatDenNguThu)
INSERT INTO Categories (Id, Name, Description, ImageUrl, IsDisplayed, CreatedAt)
VALUES 
(@CatDenNguThu, N'Đèn ngủ thú', N'Đèn ngủ hình thú cưng dễ thương, phù hợp cho trẻ em và trang trí phòng ngủ', NULL, 1, GETUTCDATE()),
(@CatDenNguKhungGo, N'Đèn ngủ khung gỗ', N'Đèn ngủ khung gỗ tự nhiên, phong cách vintage ấm áp', NULL, 1, GETUTCDATE()),
(@CatDenNguCamBien, N'Đèn ngủ cảm biến', N'Đèn ngủ thông minh tích hợp cảm biến chuyển động và ánh sáng', NULL, 1, GETUTCDATE()),
(@CatDenNguThuyTinh, N'Đèn ngủ thủy tinh', N'Đèn ngủ thủy tinh nghệ thuật, ánh sáng lung linh huyền ảo', NULL, 1, GETUTCDATE()),
(@CatDenNguLed, N'Đèn ngủ LED', N'Đèn ngủ LED tiết kiệm năng lượng, nhiều màu sắc bắt mắt', NULL, 1, GETUTCDATE());

-- =============================================
-- 2. TAGS
-- =============================================
DECLARE @TagBanChay UNIQUEIDENTIFIER = 'B1B2C3D4-2222-2222-2222-000000000001';
DECLARE @TagMoi UNIQUEIDENTIFIER = 'B1B2C3D4-2222-2222-2222-000000000002';
DECLARE @TagGiamGia UNIQUEIDENTIFIER = 'B1B2C3D4-2222-2222-2222-000000000003';
DECLARE @TagCaoCap UNIQUEIDENTIFIER = 'B1B2C3D4-2222-2222-2222-000000000004';
DECLARE @TagTreEm UNIQUEIDENTIFIER = 'B1B2C3D4-2222-2222-2222-000000000005';

IF NOT EXISTS (SELECT 1 FROM Tags WHERE Id = @TagBanChay)
INSERT INTO Tags (Id, Name, Description, CreatedAt)
VALUES 
(@TagBanChay, N'Bán chạy', N'Sản phẩm bán chạy nhất', GETUTCDATE()),
(@TagMoi, N'Mới', N'Sản phẩm mới nhất', GETUTCDATE()),
(@TagGiamGia, N'Giảm giá', N'Sản phẩm đang được giảm giá', GETUTCDATE()),
(@TagCaoCap, N'Cao cấp', N'Sản phẩm cao cấp chất lượng', GETUTCDATE()),
(@TagTreEm, N'Trẻ em', N'Phù hợp cho trẻ em', GETUTCDATE());

-- =============================================
-- 3. PRODUCTS
-- =============================================
DECLARE @P1 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000001';
DECLARE @P2 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000002';
DECLARE @P3 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000003';
DECLARE @P4 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000004';
DECLARE @P5 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000005';
DECLARE @P6 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000006';
DECLARE @P7 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000007';
DECLARE @P8 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000008';
DECLARE @P9 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000009';
DECLARE @P10 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000010';
DECLARE @P11 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000011';
DECLARE @P12 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000012';
DECLARE @P13 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000013';
DECLARE @P14 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000014';
DECLARE @P15 UNIQUEIDENTIFIER = 'C1C2C3D4-3333-3333-3333-000000000015';

IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = @P1)
INSERT INTO Products (Id, Name, Description, ReviewCount, Tags, ViewCount, Favorites, SellCount, CategoryId, Status, CreatedAt)
VALUES 
(@P1, N'Đèn ngủ hình mèo Kawaii', N'Đèn ngủ silicon hình mèo dễ thương với thiết kế Kawaii Nhật Bản. Chất liệu silicon mềm mịn, an toàn cho trẻ em. Ánh sáng vàng ấm dịu nhẹ, 3 mức độ sáng.', 24, N'đèn ngủ,mèo,kawaii,trẻ em', 1520, 89, 156, @CatDenNguThu, 1, GETUTCDATE()),
(@P2, N'Đèn ngủ hình gấu Teddy', N'Đèn ngủ hình gấu bông Teddy Bear đáng yêu, chất liệu nhựa ABS cao cấp. Ánh sáng LED 7 màu tự chuyển đổi. Tích hợp loa bluetooth.', 18, N'đèn ngủ,gấu,teddy,bluetooth', 980, 67, 98, @CatDenNguThu, 1, GETUTCDATE()),
(@P3, N'Đèn ngủ hình thỏ trắng', N'Đèn ngủ silicon hình thỏ trắng tinh tế. Tai thỏ mềm có thể bẻ cong tạo dáng. Đèn LED đổi 16 màu với remote điều khiển từ xa.', 31, N'đèn ngủ,thỏ,silicon,remote', 2100, 142, 220, @CatDenNguThu, 1, GETUTCDATE()),
(@P4, N'Đèn ngủ khung gỗ thông Bắc Âu', N'Đèn ngủ khung gỗ thông tự nhiên phong cách Scandinavian. Chụp đèn vải lanh cao cấp, tạo ánh sáng ấm áp. Kích thước 15x15x28cm.', 15, N'đèn ngủ,gỗ thông,bắc âu', 750, 45, 67, @CatDenNguKhungGo, 1, GETUTCDATE()),
(@P5, N'Đèn ngủ gỗ tre đan thủ công', N'Đèn ngủ gỗ tre đan thủ công bởi nghệ nhân Việt Nam. Hoa văn tinh xảo. Ánh sáng xuyên qua khe tre tạo hiệu ứng bóng đổ tuyệt đẹp.', 22, N'đèn ngủ,gỗ tre,thủ công', 1200, 88, 134, @CatDenNguKhungGo, 1, GETUTCDATE()),
(@P6, N'Đèn ngủ khung gỗ walnut vintage', N'Đèn ngủ khung gỗ walnut cao cấp phong cách retro vintage. Gỗ walnut nhập khẩu Bắc Mỹ, vân gỗ tự nhiên sang trọng.', 9, N'đèn ngủ,gỗ walnut,vintage', 560, 34, 42, @CatDenNguKhungGo, 1, GETUTCDATE()),
(@P7, N'Đèn ngủ cảm biến chuyển động', N'Đèn ngủ tự động bật/tắt khi phát hiện chuyển động trong bán kính 3m. Cảm biến hồng ngoại PIR. Ánh sáng vàng ấm 3000K. Pin sạc USB dùng 30 ngày.', 42, N'đèn ngủ,cảm biến,chuyển động', 3200, 198, 380, @CatDenNguCamBien, 1, GETUTCDATE()),
(@P8, N'Đèn ngủ cảm biến ánh sáng tự động', N'Đèn ngủ tự động bật khi trời tối và tắt khi có ánh sáng. Cắm trực tiếp vào ổ điện. Công suất 0.5W tiết kiệm điện tối đa.', 35, N'đèn ngủ,cảm biến ánh sáng,tự động', 2800, 165, 310, @CatDenNguCamBien, 1, GETUTCDATE()),
(@P9, N'Đèn ngủ cảm ứng chạm đổi màu', N'Đèn ngủ cảm ứng chạm điều khiển bằng tay. Chạm nhẹ để bật/tắt, giữ để điều chỉnh độ sáng. Đổi 7 màu RGB. Thân kim loại mạ chrome.', 28, N'đèn ngủ,cảm ứng,chạm,đổi màu', 1900, 120, 175, @CatDenNguCamBien, 1, GETUTCDATE()),
(@P10, N'Đèn ngủ thủy tinh Murano Ý', N'Đèn ngủ thủy tinh nghệ thuật Murano nhập khẩu từ Ý. Thổi thủy tinh thủ công với vân màu xoáy đặc trưng. Đế đồng mạ vàng sang trọng.', 7, N'đèn ngủ,thủy tinh,murano,cao cấp', 420, 28, 19, @CatDenNguThuyTinh, 1, GETUTCDATE()),
(@P11, N'Đèn ngủ thủy tinh bong bóng Galaxy', N'Đèn ngủ thủy tinh hình cầu với hiệu ứng Galaxy 3D bên trong. Công nghệ khắc laser 3D. Đế gỗ LED RGB 16 màu. Remote điều khiển.', 56, N'đèn ngủ,thủy tinh,galaxy,3d', 4500, 320, 450, @CatDenNguThuyTinh, 1, GETUTCDATE()),
(@P12, N'Đèn ngủ lọ thủy tinh đom đóm', N'Đèn ngủ lọ thủy tinh với dây đèn LED siêu nhỏ tạo hiệu ứng đom đóm lấp lánh. Nắp gỗ sồi tự nhiên. Pin sạc USB dùng 20 giờ.', 38, N'đèn ngủ,thủy tinh,đom đóm', 2600, 187, 280, @CatDenNguThuyTinh, 1, GETUTCDATE()),
(@P13, N'Đèn ngủ LED dải Neon uốn dẻo', N'Đèn ngủ LED dải neon silicon uốn dẻo tạo hình tùy ý. LED SMD2835 siêu sáng, tuổi thọ 50.000 giờ. Kết nối app điều khiển. Dài 3m.', 19, N'đèn ngủ,LED,neon,uốn dẻo', 1100, 76, 95, @CatDenNguLed, 1, GETUTCDATE()),
(@P14, N'Đèn ngủ LED Mặt Trăng 3D', N'Đèn ngủ LED hình mặt trăng in 3D từ dữ liệu NASA. Bề mặt chi tiết. 3 tông màu: trắng ấm, trắng lạnh, vàng. Có 3 kích thước.', 47, N'đèn ngủ,LED,mặt trăng,3D', 3800, 245, 390, @CatDenNguLed, 1, GETUTCDATE()),
(@P15, N'Đèn ngủ LED Sunset chiếu hoàng hôn', N'Đèn ngủ LED chiếu hiệu ứng hoàng hôn lên tường. Góc chiếu xoay 360 độ. 4 bộ lọc màu. USB powered. Dùng trang trí và chụp ảnh.', 33, N'đèn ngủ,LED,sunset,hoàng hôn', 2200, 156, 210, @CatDenNguLed, 1, GETUTCDATE());

-- =============================================
-- 4. PRODUCT VARIANTS (Table: ProductVariants)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM ProductVariants WHERE ProductId = @P1)
INSERT INTO ProductVariants (Id, ProductId, Price, DiscountPrice, Stock, Materials, Weight, SKU, CreatedAt)
VALUES 
(NEWID(), @P1, 189000, 149000, 50, N'Silicon, LED', 0.2, 'DEN-MEO-001', GETUTCDATE()),
(NEWID(), @P2, 259000, 219000, 35, N'Nhựa ABS, LED, Loa bluetooth', 0.35, 'DEN-GAU-001', GETUTCDATE()),
(NEWID(), @P3, 219000, 179000, 45, N'Silicon, LED RGB', 0.25, 'DEN-THO-001', GETUTCDATE()),
(NEWID(), @P4, 350000, 299000, 25, N'Gỗ thông, vải lanh, LED E14', 0.8, 'DEN-GO-THONG-001', GETUTCDATE()),
(NEWID(), @P5, 420000, 380000, 15, N'Gỗ tre tự nhiên, LED', 0.6, 'DEN-GO-TRE-001', GETUTCDATE()),
(NEWID(), @P6, 890000, 790000, 10, N'Gỗ walnut, kính mờ', 1.2, 'DEN-GO-WALNUT-001', GETUTCDATE()),
(NEWID(), @P7, 159000, 129000, 80, N'Nhựa ABS, LED, cảm biến PIR', 0.15, 'DEN-CB-CD-001', GETUTCDATE()),
(NEWID(), @P8, 89000, 69000, 120, N'Nhựa PC, LED', 0.08, 'DEN-CB-AS-001', GETUTCDATE()),
(NEWID(), @P9, 289000, 249000, 40, N'Kim loại mạ chrome, LED RGB', 0.45, 'DEN-CU-CHAM-001', GETUTCDATE()),
(NEWID(), @P10, 2500000, 2200000, 5, N'Thủy tinh Murano, đồng mạ vàng', 1.5, 'DEN-TT-MURANO-001', GETUTCDATE()),
(NEWID(), @P11, 320000, 269000, 60, N'Thủy tinh, gỗ, LED RGB', 0.5, 'DEN-TT-GALAXY-001', GETUTCDATE()),
(NEWID(), @P12, 199000, 169000, 55, N'Thủy tinh, gỗ sồi, LED micro', 0.3, 'DEN-TT-DOMDOM-001', GETUTCDATE()),
(NEWID(), @P13, 350000, 299000, 30, N'Silicon, LED SMD2835', 0.4, 'DEN-LED-NEON-001', GETUTCDATE()),
(NEWID(), @P14, 280000, 239000, 45, N'PLA 3D Print, LED', 0.3, 'DEN-LED-MOON-S', GETUTCDATE()),
(NEWID(), @P14, 380000, 329000, 30, N'PLA 3D Print, LED', 0.5, 'DEN-LED-MOON-M', GETUTCDATE()),
(NEWID(), @P14, 520000, 459000, 20, N'PLA 3D Print, LED', 0.8, 'DEN-LED-MOON-L', GETUTCDATE()),
(NEWID(), @P15, 249000, 199000, 40, N'Nhựa ABS, LED, bộ lọc màu', 0.35, 'DEN-LED-SUNSET-001', GETUTCDATE());

-- =============================================
-- 5. PRODUCT TAGS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM ProductTags WHERE ProductId = @P1 AND TagId = @TagTreEm)
INSERT INTO ProductTags (Id, ProductId, TagId, CreatedAt)
VALUES
(NEWID(), @P1, @TagTreEm, GETUTCDATE()),
(NEWID(), @P1, @TagBanChay, GETUTCDATE()),
(NEWID(), @P2, @TagTreEm, GETUTCDATE()),
(NEWID(), @P2, @TagMoi, GETUTCDATE()),
(NEWID(), @P3, @TagTreEm, GETUTCDATE()),
(NEWID(), @P3, @TagBanChay, GETUTCDATE()),
(NEWID(), @P4, @TagMoi, GETUTCDATE()),
(NEWID(), @P5, @TagBanChay, GETUTCDATE()),
(NEWID(), @P5, @TagCaoCap, GETUTCDATE()),
(NEWID(), @P6, @TagCaoCap, GETUTCDATE()),
(NEWID(), @P7, @TagBanChay, GETUTCDATE()),
(NEWID(), @P7, @TagGiamGia, GETUTCDATE()),
(NEWID(), @P8, @TagGiamGia, GETUTCDATE()),
(NEWID(), @P8, @TagBanChay, GETUTCDATE()),
(NEWID(), @P9, @TagMoi, GETUTCDATE()),
(NEWID(), @P10, @TagCaoCap, GETUTCDATE()),
(NEWID(), @P11, @TagBanChay, GETUTCDATE()),
(NEWID(), @P11, @TagGiamGia, GETUTCDATE()),
(NEWID(), @P12, @TagBanChay, GETUTCDATE()),
(NEWID(), @P13, @TagMoi, GETUTCDATE()),
(NEWID(), @P14, @TagBanChay, GETUTCDATE()),
(NEWID(), @P14, @TagMoi, GETUTCDATE()),
(NEWID(), @P15, @TagGiamGia, GETUTCDATE()),
(NEWID(), @P15, @TagMoi, GETUTCDATE());

-- Verify
SELECT 'Categories' AS T, COUNT(*) AS C FROM Categories
UNION ALL SELECT 'Products', COUNT(*) FROM Products
UNION ALL SELECT 'ProductVariants', COUNT(*) FROM ProductVariants
UNION ALL SELECT 'Tags', COUNT(*) FROM Tags
UNION ALL SELECT 'ProductTags', COUNT(*) FROM ProductTags;

PRINT N'Done!';
GO
