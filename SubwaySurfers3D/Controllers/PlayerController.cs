using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubwaySurfers3D.Data;
using SubwaySurfers3D.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SubwaySurfers3D.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PlayerController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("login")]
        public async Task<ActionResult<PlayerDto>> LoginOrRegister([FromBody] LoginOrRegisterRequest req)
        {
            var username = string.IsNullOrWhiteSpace(req.Username) ? "RunnerPro" : req.Username.Trim();

            var player = await _db.Players
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Username.ToLower() == username.ToLower());

            if (player == null)
            {
                player = new Player
                {
                    Username = username,
                    Avatar = "jake",
                    HighScore = 0,
                    TotalCoins = 500, // Welcome bonus!
                    SelectedCharacterId = "jake",
                    SelectedBoardId = "starboard"
                };

                _db.Players.Add(player);
                await _db.SaveChangesAsync();

                // Add default unlocked items to inventory
                _db.PlayerInventories.Add(new PlayerInventory { PlayerId = player.Id, ShopItemId = "jake" });
                _db.PlayerInventories.Add(new PlayerInventory { PlayerId = player.Id, ShopItemId = "starboard" });
                await _db.SaveChangesAsync();

                // Reload inventory
                await _db.Entry(player).Collection(p => p.Inventory).LoadAsync();
            }

            var ownedItemIds = player.Inventory.Select(i => i.ShopItemId).ToList();
            if (!ownedItemIds.Contains("jake")) ownedItemIds.Add("jake");
            if (!ownedItemIds.Contains("starboard")) ownedItemIds.Add("starboard");

            return Ok(new PlayerDto
            {
                Id = player.Id,
                Username = player.Username,
                Avatar = player.Avatar,
                HighScore = player.HighScore,
                TotalCoins = player.TotalCoins,
                GamesPlayed = player.GamesPlayed,
                TotalDistance = player.TotalDistance,
                SelectedCharacterId = player.SelectedCharacterId,
                SelectedBoardId = player.SelectedBoardId,
                OwnedItemIds = ownedItemIds
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerDto>> GetProfile(int id)
        {
            var player = await _db.Players
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null) return NotFound(new { message = "Player not found" });

            var ownedItemIds = player.Inventory.Select(i => i.ShopItemId).ToList();
            if (!ownedItemIds.Contains("jake")) ownedItemIds.Add("jake");
            if (!ownedItemIds.Contains("starboard")) ownedItemIds.Add("starboard");

            return Ok(new PlayerDto
            {
                Id = player.Id,
                Username = player.Username,
                Avatar = player.Avatar,
                HighScore = player.HighScore,
                TotalCoins = player.TotalCoins,
                GamesPlayed = player.GamesPlayed,
                TotalDistance = player.TotalDistance,
                SelectedCharacterId = player.SelectedCharacterId,
                SelectedBoardId = player.SelectedBoardId,
                OwnedItemIds = ownedItemIds
            });
        }

        [HttpPost("equip")]
        public async Task<ActionResult<PlayerDto>> EquipItem([FromBody] UpdateEquipmentRequest req)
        {
            var player = await _db.Players
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == req.PlayerId);

            if (player == null) return NotFound(new { message = "Player not found" });

            var ownedItemIds = player.Inventory.Select(i => i.ShopItemId).ToList();
            ownedItemIds.Add("jake");
            ownedItemIds.Add("starboard");

            if (!string.IsNullOrEmpty(req.CharacterId))
            {
                if (ownedItemIds.Contains(req.CharacterId))
                {
                    player.SelectedCharacterId = req.CharacterId;
                }
            }

            if (!string.IsNullOrEmpty(req.BoardId))
            {
                if (ownedItemIds.Contains(req.BoardId))
                {
                    player.SelectedBoardId = req.BoardId;
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new PlayerDto
            {
                Id = player.Id,
                Username = player.Username,
                Avatar = player.Avatar,
                HighScore = player.HighScore,
                TotalCoins = player.TotalCoins,
                GamesPlayed = player.GamesPlayed,
                TotalDistance = player.TotalDistance,
                SelectedCharacterId = player.SelectedCharacterId,
                SelectedBoardId = player.SelectedBoardId,
                OwnedItemIds = ownedItemIds.Distinct().ToList()
            });
        }
    }
}
