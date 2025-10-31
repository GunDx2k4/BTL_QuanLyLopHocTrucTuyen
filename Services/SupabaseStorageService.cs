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

            // 🧩 Làm sạch tên file gốc (loại bỏ dấu, ký tự đặc biệt, khoảng trắng)
            string safeFileName = RemoveVietnamese(file.FileName);
            safeFileName = Path.GetFileNameWithoutExtension(safeFileName);
            safeFileName = new string(safeFileName
                .Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
                .ToArray());
            safeFileName = safeFileName.Replace(" ", "_").ToLower();

            // 🧩 Tạo tên file duy nhất
            string ext = Path.GetExtension(file.FileName);
            string uniqueName = $"{safeFileName}_{Guid.NewGuid()}{ext}";

            string supabasePath = $"{folderName}/{uniqueName}";

            // 🔹 Tạo thư mục tạm
            string tempFolder = Path.Combine(Path.GetTempPath(), "SupabaseUploads");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string tempFilePath = Path.Combine(tempFolder, $"{Guid.NewGuid()}{ext}");

            // 🔹 Ghi file thật vào thư mục tạm
            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                // 🔹 Upload file lên Supabase
                await bucket.Upload(tempFilePath, supabasePath, new Supabase.Storage.FileOptions
                {
                    ContentType = file.ContentType,
                    Upsert = false
                });

                // 🔹 Xóa file tạm
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

            try
            {
                var storage = _client.Storage;
                var bucket = storage.From(_bucketName);

                // ✅ Lấy phần đường dẫn sau /object/public/<bucketName>/
                var uri = new Uri(fileUrl);
                string pathAfterBucket = uri.AbsolutePath;

                // Tìm vị trí của bucket name và cắt phần sau nó
                int index = pathAfterBucket.IndexOf($"/{_bucketName}/");
                if (index >= 0)
                {
                    pathAfterBucket = pathAfterBucket.Substring(index + _bucketName.Length + 2); // +2 vì có "//"
                }

                // Giải mã URL (trường hợp có %20, %2F,...)
                pathAfterBucket = Uri.UnescapeDataString(pathAfterBucket);

                Console.WriteLine($"🗑️ Đang xóa file Supabase: {pathAfterBucket}");
                await bucket.Remove(new List<string> { pathAfterBucket });

                Console.WriteLine("✅ File đã được xóa khỏi Supabase!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi DeleteFileAsync: {ex.Message}");
            }
        }

        private string RemoveVietnamese(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            string formD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var ch in formD)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D');
        }


    }
}
