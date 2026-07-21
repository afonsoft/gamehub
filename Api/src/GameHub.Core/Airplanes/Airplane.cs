using Abp.Auditing;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameHub.Airplanes
{
    [Table("EafAirplanes")]
    [Audited]
    public class Airplane : FullAuditedEntity, IMayHaveTenant
    {
        public const int MaxModelLength = 256;

        public int? TenantId { get; set; }

        [Required]
        public virtual string Number { get; set; }

        [Required]
        [StringLength(MaxModelLength)]
        public virtual string Model { get; set; }
    }
}