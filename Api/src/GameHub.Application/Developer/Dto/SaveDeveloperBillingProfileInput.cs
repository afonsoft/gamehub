using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto
{
    public class SaveDeveloperBillingProfileInput
    {
        [Required]
        public Guid TeamId { get; set; }

        [StringLength(64)]
        public string TaxId { get; set; }

        [StringLength(512)]
        public string Address { get; set; }

        [StringLength(64)]
        public string PaymentMethodPlaceholder { get; set; }
    }
}
