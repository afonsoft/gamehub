using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Chat
{
    /// <summary>
    /// A message sent from a game context.
    /// </summary>
    public class SendGameChatMessageInput
    {
        /// <summary>Game identifier from the host context.</summary>
        [Required]
        public Guid GameId { get; set; }

        /// <summary>Conversation identifier in the form match:{id} or user:{tenant}:{id}.</summary>
        [Required]
        [StringLength(128)]
        public string ConversationId { get; set; } = string.Empty;

        /// <summary>Message text supplied by the game.</summary>
        [Required]
        [StringLength(500)]
        public string Text { get; set; } = string.Empty;

        /// <summary>Client-generated idempotency key.</summary>
        [Required]
        [StringLength(128)]
        public string ClientMessageId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of a contextual chat send.
    /// </summary>
    public class GameChatMessageResult
    {
        /// <summary>Whether this request created a message.</summary>
        public bool Accepted { get; set; }

        /// <summary>Whether the request was already processed for this user.</summary>
        public bool Duplicate { get; set; }

        /// <summary>Echo of the client idempotency key.</summary>
        public string ClientMessageId { get; set; } = string.Empty;
    }
}
