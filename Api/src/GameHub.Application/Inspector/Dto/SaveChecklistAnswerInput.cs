using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Inspector.Dto
{
    public class SaveChecklistAnswerInput
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        [StringLength(64)]
        public string QuestionId { get; set; }

        [StringLength(2000)]
        public string Answer { get; set; }
    }
}
