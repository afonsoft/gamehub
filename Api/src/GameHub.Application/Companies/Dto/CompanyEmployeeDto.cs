using System;

namespace GameHub.Companies.Dto
{
    public class CompanyEmployeeDto
    {
        public long UserId { get; set; }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        public string Role { get; set; }

        public bool IsDefault { get; set; }

        public DateTime? JoinedAt { get; set; }
    }
}
