using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Item da série temporal de plays por dia.
    /// </summary>
    public class PlaysOverTimeItemDto
    {
        /// <summary>Data da agregação.</summary>
        public DateTime Date { get; set; }

        /// <summary>Quantidade de plays no dia.</summary>
        public long Plays { get; set; }
    }
}
