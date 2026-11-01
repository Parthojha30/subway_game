using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubwaySurfers3D.Data;
using SubwaySurfers3D.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubwaySurfers3D.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ShopController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("items")]
        public async Task<ActionResult<List<ShopItem>>> GetItems()
        {
            var items = await _db.ShopItems.ToListAsync();
            return Ok(items);
        }

        [HttpPost("buy")]
        public async Task<ActionResult<BuyItemResponse>> BuyItem([FromBody] BuyItemRequest req)
        {
            var player = await _db.Players
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == req.PlayerId);

            if (player == null)
            {
                return NotFound(new BuyItemResponse { Success = false, Message = "Player not found" });
            }

            var item = await _db.ShopItems.FirstOrDefaultAsync(i => i.Id == req.ItemId);
            if (item == null)
            {
                return BadRequest(new BuyItemResponse { Success = false, Message = "Shop item not found" });
            }

            // Check if already owned
            var alreadyOwned = player.Inventory.Any(i => i.ShopItemId == req.ItemId);
            if (alreadyOwned || item.UnlockedByDefault)
            {
                return Ok(new BuyItemResponse
                {
                    Success = true,
                    Message = "Item is already owned!",
                    RemainingCoins = player.TotalCoins,
                    OwnedItemIds = player.Inventory.Select(i => i.ShopItemId).Distinct().ToList()
                });
            }

            // Check coins
            if (player.TotalCoins < item.Price)
            {
                return BadRequest(new BuyItemResponse
                {
                    Success = false,
                    Message = $"Not enough coins! Required: {item.Price}, You have: {player.TotalCoins}",
                    RemainingCoins = player.TotalCoins,
                    OwnedItemIds = player.Inventory.Select(i => i.ShopItemId).Distinct().ToList()
                });
            }

            // Deduct coins & add to inventory
            player.TotalCoins -= item.Price;
            _db.PlayerInventories.Add(new PlayerInventory
            {
                PlayerId = player.Id,
                ShopItemId = item.Id
            });

            // Automatically equip purchased item
            if (item.Category == "character") player.SelectedCharacterId = item.Id;
            else if (item.Category == "board") player.SelectedBoardId = item.Id;

            await _db.SaveChangesAsync();

            var updatedOwned = player.Inventory.Select(i => i.ShopItemId).Distinct().ToList();
            if (!updatedOwned.Contains("jake")) updatedOwned.Add("jake");
            if (!updatedOwned.Contains("starboard")) updatedOwned.Add("starboard");

            return Ok(new BuyItemResponse
            {
                Success = true,
                Message = $"Successfully purchased {item.Name}!",
                RemainingCoins = player.TotalCoins,
                OwnedItemIds = updatedOwned
            });
        }
    }
}
