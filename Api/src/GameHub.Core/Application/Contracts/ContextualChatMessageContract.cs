using System;

namespace GameHub.Contracts
{
    /// <summary>
    /// Contrato para mensagens de chat contextual compartilhadas entre serviços.
    /// </summary>
    public class ContextualChatMessageContract
    {
        /// <summary>
        /// Identificador único da mensagem.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Texto da mensagem.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Identificador do usuário remetente.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Identificador da conversa para agrupamento contextual.
        /// </summary>
        public Guid? ConversationId { get; set; }

        /// <summary>
        /// Identificador opcional do jogo ao qual a mensagem está relacionada.
        /// </summary>
        public Guid? GameId { get; set; }

        /// <summary>
        /// Identificador opcional da partida à qual a mensagem está relacionada.
        /// </summary>
        public Guid? MatchId { get; set; }

        /// <summary>
        /// Tipo de contexto onde a mensagem foi produzida (por exemplo, <c>lobby</c>, <c>match</c> ou <c>team</c>).
        /// </summary>
        public string ContextType { get; set; }

        /// <summary>
        /// Chave de idempotência gerada pelo cliente para evitar mensagens duplicadas em retries.
        /// </summary>
        public string ClientMessageId { get; set; }

        /// <summary>
        /// Data e hora de criação da mensagem.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
