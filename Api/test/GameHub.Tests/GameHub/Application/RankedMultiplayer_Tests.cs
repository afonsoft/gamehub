using System;
using GameHub.Multiplayer;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class RankedMultiplayer_Tests
    {
        [Fact]
        public void Dado_RatingInicial_Quando_AplicarDerrota_Entao_RatingDiminuiSemFicarNegativo()
        {
            var rating = new PlayerRating { Rating = 1000 };

            rating.ApplyResult(0, 1, 32);

            rating.Rating.ShouldBe(968);
            rating.Losses.ShouldBe(1);
            rating.GamesPlayed.ShouldBe(1);
        }

        [Fact]
        public void Dado_RatingBaixo_Quando_AplicarDerrotas_Entao_RatingNaoFicaNegativo()
        {
            var rating = new PlayerRating { Rating = 10 };

            rating.ApplyResult(0, 1, 32);

            rating.Rating.ShouldBe(0);
        }

        [Fact]
        public void Dado_FilaRanqueada_Quando_CriarEntrada_Entao_PreservaSnapshotDeRating()
        {
            var entry = new RankedQueueEntry
            {
                Id = Guid.NewGuid(),
                RatingSnapshot = 1234,
                Status = RankedQueueStatus.Waiting
            };

            entry.Status.ShouldBe(RankedQueueStatus.Waiting);
            entry.RatingSnapshot.ShouldBe(1234);
        }
    }
}
