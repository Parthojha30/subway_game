using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubwaySurfers3D.Controllers;
using SubwaySurfers3D.Data;
using SubwaySurfers3D.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SubwaySurfers3D.Tests
{
    public class PlayerControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task LoginOrRegister_NewUser_CreatesPlayerWithDefaultItems()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var controller = new PlayerController(db);
            var request = new LoginOrRegisterRequest { Username = "SpeedyRunner" };

            // Act
            var response = await controller.LoginOrRegister(request);

            // Assert
            var result = Assert.IsType<OkObjectResult>(response.Result);
            var dto = Assert.IsType<PlayerDto>(result.Value);

            Assert.Equal("SpeedyRunner", dto.Username);
            Assert.Equal(500, dto.TotalCoins);
            Assert.Contains("jake", dto.OwnedItemIds);
            Assert.Contains("starboard", dto.OwnedItemIds);
        }

        [Fact]
        public async Task GetProfile_ExistingPlayer_ReturnsOkWithProfile()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var player = new Player
            {
                Username = "TrackMaster",
                HighScore = 1500,
                TotalCoins = 1200,
                SelectedCharacterId = "jake",
                SelectedBoardId = "starboard"
            };
            db.Players.Add(player);
            await db.SaveChangesAsync();

            var controller = new PlayerController(db);

            // Act
            var response = await controller.GetProfile(player.Id);

            // Assert
            var result = Assert.IsType<OkObjectResult>(response.Result);
            var dto = Assert.IsType<PlayerDto>(result.Value);

            Assert.Equal("TrackMaster", dto.Username);
            Assert.Equal(1500, dto.HighScore);
        }

        [Fact]
        public async Task GetProfile_InvalidId_ReturnsNotFound()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var controller = new PlayerController(db);

            // Act
            var response = await controller.GetProfile(9999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(response.Result);
        }
    }
}
