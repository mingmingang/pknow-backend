namespace pknow_backend.Controllers
{
    public class CaptchaService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CaptchaService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string CaptchaCode
        {
            get
            {
                var code = _httpContextAccessor.HttpContext?.Session.GetString("CaptchaCode");
                Console.WriteLine($"[CaptchaService] Retrieving CaptchaCode: {code}");
                return code;
            }
            set
            {
                _httpContextAccessor.HttpContext?.Session.SetString("CaptchaCode", value);
                Console.WriteLine($"[CaptchaService] Storing CaptchaCode: {value}");
            }
        }
    }


}
