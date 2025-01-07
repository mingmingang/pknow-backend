using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pknow_backend.Helper;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.Versioning;

namespace pknow_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UtilitiesController : ControllerBase
    {
        //readonly PolmanAstraLibrary.PolmanAstraLibrary lib = new(PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(configuration.GetConnectionString("DefaultConnection"), "PoliteknikAstra_ConfigurationKey"));

        DataTable dt = new();

        private readonly CaptchaService _captchaService;
        private readonly PolmanAstraLibrary.PolmanAstraLibrary lib;
        private readonly IConfiguration _configuration;
        public string captchaCode;

        public UtilitiesController(CaptchaService captchaService, IConfiguration configuration)
        {
            _captchaService = captchaService;
            _configuration = configuration;
            lib = new PolmanAstraLibrary.PolmanAstraLibrary(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet]
        public IActionResult GetCaptcha()
        {
            // Generate captcha code
            _captchaService.CaptchaCode = GenerateCaptchaCode();
            HttpContext.Session.SetString("CaptchaCode", _captchaService.CaptchaCode);

            Console.WriteLine($"Generated Captcha Code ayam: {HttpContext.Session.GetString("CaptchaCode")}");
            // Generate captcha image
            var captchaImage = GenerateCaptchaImage(_captchaService.CaptchaCode);

            // Return image as response
            return File(captchaImage, "image/png");
        }

        // Fungsi untuk membuat kode captcha
        private string GenerateCaptchaCode()
        {
            const string chars = "bcdfghjkmnpqrstvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Fungsi untuk membuat gambar captcha
        private byte[] GenerateCaptchaImage(string captchaCode)
        {
            using var bitmap = new Bitmap(150, 50); // Captcha image size
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);

            var font = new Font("Bell MT", 18, FontStyle.Bold);
            var brush = new SolidBrush(Color.Black);
            graphics.DrawString(captchaCode, font, brush, new PointF(20, 10));
            AddNoise(graphics, bitmap);

            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            return memoryStream.ToArray();
        }

        private void AddNoise(Graphics graphics, Bitmap bitmap)
        {
            var rand = new Random();
            int width = bitmap.Width;
            int height = bitmap.Height;

            // Menambahkan 5 garis acak
            for (int i = 0; i < 5; i++)
            {
                int x1 = rand.Next(width);
                int y1 = rand.Next(height);
                int x2 = rand.Next(width);
                int y2 = rand.Next(height);
                graphics.DrawLine(new Pen(Color.Gray, 1), x1, y1, x2, y2);
            }

            // Menambahkan titik-titik acak
            for (int i = 0; i < 100; i++)
            {
                int x = rand.Next(width);
                int y = rand.Next(height);
                bitmap.SetPixel(x, y, Color.Gray);
            }
        }


        [HttpPost]
        //[SupportedOSPlatform("windows")]
        public IActionResult Login([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());

                string userCaptchaInput = value["captcha"]?.ToString();
                string sessionCaptchaCode = _captchaService.CaptchaCode;

                Console.WriteLine($"Username: {value["username"]}");
                Console.WriteLine($"Password: {value["password"]}");
                Console.WriteLine($"Captcha: {userCaptchaInput}");
                Console.WriteLine($"Session Captcha Code: {HttpContext.Session.GetString("CaptchaCode")}");


                if (string.IsNullOrEmpty(sessionCaptchaCode) || userCaptchaInput.ToLower() != sessionCaptchaCode.ToLower())
                {
                    return BadRequest(new { error = "Captcha tidak valid." });
                }

                // Lanjutkan dengan autentikasi
                dt = lib.CallProcedure("sso_getAuthenticationKMS", EncodeData.HtmlEncodeObject(value));
                if (dt.Rows.Count == 0)
                {
                    return Ok(JsonConvert.SerializeObject(new { Status = "LOGIN FAILED" }));
                }

                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost]
        //[SupportedOSPlatform("windows")]
        public IActionResult LoginTMS([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());

                string userCaptchaInput = value["captcha"]?.ToString();
                string sessionCaptchaCode = _captchaService.CaptchaCode;

                Console.WriteLine($"Username: {value["username"]}");
                Console.WriteLine($"Password: {value["password"]}");
                Console.WriteLine($"Captcha: {userCaptchaInput}");
                Console.WriteLine($"Session Captcha Code: {HttpContext.Session.GetString("CaptchaCode")}");


                if (string.IsNullOrEmpty(sessionCaptchaCode) || userCaptchaInput.ToLower() != sessionCaptchaCode.ToLower())
                {
                    return BadRequest(new { error = "Captcha tidak valid." });
                }

                // Lanjutkan dengan autentikasi
                dt = lib.CallProcedure("sso_getAuthenticationTMS", EncodeData.HtmlEncodeObject(value));
                if (dt.Rows.Count == 0)
                {
                    return Ok(JsonConvert.SerializeObject(new { Status = "LOGIN FAILED" }));
                }

                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



        [Authorize]
        [HttpPost]
        public IActionResult RegisterGoogleUser([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                string email = value["email"]?.ToString();
                string namaLengkap = value["namaLengkap"]?.ToString();
                string gender = value["gender"]?.ToString();
                string phone = value["noTelp"]?.ToString();
                string createdBy = value["createdBy"]?.ToString();

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(namaLengkap))
                {
                    return BadRequest("Email atau nama lengkap tidak boleh kosong.");
                }

                // Membuat objek JObject untuk parameter prosedur
                var parametersCheck = new JObject
        {
            { "Email", email }
        };

                DataTable dtCheck = lib.CallProcedure("CheckUserByEmail", EncodeData.HtmlEncodeObject(parametersCheck));
                if (dtCheck.Rows.Count == 0) // If user is not registered
                {
                    var parametersInsert = new JObject
            {
                { "ext_nama_lengkap", namaLengkap },
                { "ext_username", email },
                { "ext_password", null }, // Google account doesn't require a password
                { "ext_gender", gender ?? string.Empty }, // Default value if null
                { "ext_no_telp", phone ?? string.Empty }, // Default value if null
                { "ext_created_by", createdBy ?? "Google" } // Default value if null
            };

                    DataTable dt = lib.CallProcedure("InsertNewGoogleUser", EncodeData.HtmlEncodeObject(parametersInsert));
                }
                return Ok(new { message = "Berhasil mendaftarkan akun Google." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Gagal menyimpan data pengguna.", error = ex.Message });
            }
        }







        [HttpPost]
        public IActionResult CreateJWTToken([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());

                JWTToken jwtToken = new();
                string token = jwtToken.IssueToken(
                     _configuration,
                    EncodeData.HtmlEncodeObject(value)[0],
                    EncodeData.HtmlEncodeObject(value)[1],
                    EncodeData.HtmlEncodeObject(value)[2]
                );

                return Ok(JsonConvert.SerializeObject(new { Token = token }));
            }
            catch { return BadRequest(); }
        }

        [HttpPost]
        public IActionResult CreateLogLogin([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                lib.CallProcedure("all_createLoginRecord", EncodeData.HtmlEncodeObject(value));
                dt = lib.CallProcedure("all_getLastLogin", [EncodeData.HtmlEncodeObject(value)[0], EncodeData.HtmlEncodeObject(value)[4]]);
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [Authorize]
        [HttpPost]
        public IActionResult GetDataNotifikasi([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("all_getDataNotifikasiReact", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }


        [Authorize]
        [HttpPost]
        public IActionResult GetUserLogin([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("pknow_getDataUserLogin", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [Authorize]
        [HttpPost]
        public IActionResult SetReadNotifikasi([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("pknow_setReadNotifikasi", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [Authorize]
        [HttpPost]
        public IActionResult GetDataCountingNotifikasi([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("all_getCountNotifikasiReact", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [Authorize]
        [HttpPost]
        public IActionResult createNotifikasi([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("all_createNotifikasiNew", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [Authorize]
        [HttpPost]
        public IActionResult GetListMenu([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("pknow_getListAllMenu", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [HttpPost]
        public IActionResult AllSetReadNotifikasi([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("all_setReadNotifikasiAllReact", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [HttpGet]
        public IActionResult DownloadFile(string namaFile)
        {
            try
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads");
                string filePath = Path.Combine(folderPath, namaFile);

                Console.WriteLine($"Mencoba mengakses file di: {filePath}"); // Tambahkan log

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File tidak ditemukan.");
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                string contentType = "application/octet-stream";
                return File(fileBytes, contentType, namaFile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Terjadi kesalahan: {ex.Message}");
            }
        }


    }
}
