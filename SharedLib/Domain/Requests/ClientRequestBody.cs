using SharedLib.Domain.Enums;

namespace SharedLib.Domain.Requests
{
    public class ClientRequestBody
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public AccountTypeEnum Type { get; set; }
        public decimal Balance { get; set; } = 0.0m;
    }
}
