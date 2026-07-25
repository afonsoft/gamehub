using System;

namespace GameHub.Developer.Dto
{
    public class DeveloperBillingProfileDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public string TaxId { get; set; }
        public string Address { get; set; }
        public string PaymentMethodPlaceholder { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPendingReview { get; set; }
    }
}
