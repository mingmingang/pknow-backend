using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using pknow_backend.Helper;

namespace pknow_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PengajuanKKController(IConfiguration configuration) : Controller
    {
        readonly PolmanAstraLibrary.PolmanAstraLibrary lib = new(configuration.GetConnectionString("DefaultConnection"));
        DataTable dt = new();

        [HttpPost]
        public IActionResult GetAnggotaKK([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("pknow_getDataAnggotaKeahlian", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [HttpPost]
        public IActionResult GetDetailLampiran([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("pknow_getdetaillampiran", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [HttpPost]
        //public IActionResult SaveAnggotaKK([FromBody] dynamic data)
        //{
        //    try
        //    {
        //        JObject value = JObject.Parse(data.ToString());
        //        dt = lib.CallProcedure("pknow_createAnggotaKeahlian", EncodeData.HtmlEncodeObject(value));
        //        return Ok(JsonConvert.SerializeObject(dt));
        //    }
        //    catch { return BadRequest(); }
        //}

        [HttpPost]
        public IActionResult SaveAnggotaKK([FromBody] dynamic data)
        {
            try
            {
                Console.WriteLine("Data diterima:", data); // Debug log
                JObject value = JObject.Parse(data.ToString());
                Console.WriteLine("Data setelah parse:", value); // Debug log

                dt = lib.CallProcedure("pknow_createAnggotaKeahlian", EncodeData.HtmlEncodeObject(value));
                Console.WriteLine("Hasil dari SP:", JsonConvert.SerializeObject(dt)); // Debug log

                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}"); // Debug log
                return BadRequest();
            }
        }


        [HttpPost]
        public IActionResult SaveLampiranKK([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("pknow_createLampiranKK", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }

        [HttpPost]
        public IActionResult GetRiwayat([FromBody] dynamic data)
        {
            try
            {
                JObject value = JObject.Parse(data.ToString());
                dt = lib.CallProcedure("pknow_getRiwayatPengajuan", EncodeData.HtmlEncodeObject(value));
                return Ok(JsonConvert.SerializeObject(dt));
            }
            catch { return BadRequest(); }
        }


    }
}
