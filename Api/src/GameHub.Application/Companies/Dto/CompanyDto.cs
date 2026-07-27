using System;
using Abp.Application.Services.Dto;

namespace GameHub.Companies.Dto
{
    public class CompanyDto : EntityDto<int>
    {
        public string TenancyName { get; set; }

        public string Name { get; set; }

        public string PrimaryContactEmail { get; set; }

        public string Country { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreationTime { get; set; }

        public int EmployeeCount { get; set; }
    }
}
