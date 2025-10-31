using Supabase;
using System.Globalization;
using System.Text;

using Microsoft.Extensions.Configuration;

namespace BTL_QuanLyLopHocTrucTuyen.Services
{
    public class SupabaseStorageService
    {
        private readonly Client _client;
        private readonly string _bucketName = "web-class-online"; // ⚡ Phải trùng 100% tên bucket trong Supabase

        public SupabaseStorageService(IConfiguration config)
        {
            string url = config["Supabase:Url"];
            string key = config["Supabase:AnonKey"];

            _client = new Client(url, key);
            _client.InitializeAsync().Wait(); // đồng bộ hóa khởi tạo (vì DI không hỗ trợ async)
        }

        /// <summary>
        /// 📤 Upload file lên Supabase Storage
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string folderName = "assignments")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Không có file để upload.");

            var storage = _client.Storage;
            var bucket = storage.From(_bucketName);

            // 🧩 Chuẩn hoá tên file (xóa dấu, khoảng trắng, ký tự đặc biệt)
            string cleanName = NormalizeFileName(Path.GetFileNameWithoutExtension(file.FileName));
            string extension = Path.GetExtension(file.FileName);
            string supabasePath = $"{folderName}/{cleanName}_{Guid.NewGuid()}{extension}";

            // 🔹 Tạo thư mục tạm trong hệ thống (AppData\Local\Temp\SupabaseUploads)
            string tempFolder = Path.Combine(Path.GetTempPath(), "SupabaseUploads");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string tempFilePath = Path.Combine(tempFolder, $"{Guid.NewGuid()}{extension}");

            // 🔹 Ghi file upload vào file tạm
            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                // ✅ Đúng thứ tự: Upload(local_path, remote_path, options)
                await bucket.Upload(tempFilePath, supabasePath, new Supabase.Storage.FileOptions
                {
                    ContentType = file.ContentType,
                    Upsert = false
                });

                try { System.IO.File.Delete(tempFilePath); } catch { }

                string publicUrl = bucket.GetPublicUrl(supabasePath);
                Console.WriteLine($"✅ Uploaded to Supabase: {publicUrl}");
                return publicUrl;
}

            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi upload Supabase: {ex.Message}");
                Console.WriteLine($"⚠️ tempFilePath hiện tại: {tempFilePath}");
                throw;
            }
        }



        /// <summary>
        /// 🗑️ Xóa file khỏi Supabase qua public URL
        /// </summary>
        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return;

            var storage = _client.Storage;
            var bucket = storage.From(_bucketName);

            // Lấy phần đường dẫn tương đối sau tên bucket
            string baseUrl = bucket.GetPublicUrl("");
            string relativePath = fileUrl.Replace(baseUrl, "").TrimStart('/');

            await bucket.Remove(new List<string> { relativePath });
            Console.WriteLine($"🗑️ Đã xóa file Supabase: {relativePath}");
        }
        private static string NormalizeFileName(string fileName)
        {
            // Xóa dấu tiếng Việt
            string normalized = fileName.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            normalized = sb.ToString().Normalize(NormalizationForm.FormC);

            // Chuyển về chữ thường, thay khoảng trắng bằng "_"
            normalized = normalized.ToLower()
                                .Replace(" ", "_");

            // Loại bỏ ký tự không hợp lệ trong tên file
            foreach (char ch in Path.GetInvalidFileNameChars())
                normalized = normalized.Replace(ch, '_');

            return normalized;
        }


    }
}
