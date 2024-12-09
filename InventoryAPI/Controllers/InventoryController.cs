using InventoryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {

        private readonly InventoryRepository _inventoryRepository;
        private readonly ILogger<InventoryController> _logger;
        private readonly RabbitMQService _rabbitMQService;

        /*
        public InventoryController(ILogger<InventoryController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _inventoryRepository = new InventoryRepository(configuration);
        }
        */

        public InventoryController(ILogger<InventoryController> logger, InventoryRepository inventoryRepository, RabbitMQService rabbitMQService)
        {
            _logger = logger;
            _inventoryRepository = inventoryRepository;
            _rabbitMQService = rabbitMQService;
        }

        // With name probaj za poimenovanje
        [HttpGet(Name = "GetAllItems")]
        public async Task<IEnumerable<Inventory>> GetAllItems()
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Get all inventory items: ",
                Timestamp = DateTime.UtcNow,
                Url = "getAllItems",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });

            var items = await _inventoryRepository.GetAllItemsAsync();
           return items;
        }

        [HttpGet("{itemId}", Name = "GetItemById")]
        public async Task<ActionResult<Inventory>> GetItemById(string itemId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Get inventory item by id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "getItemById",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            var item = await _inventoryRepository.GetItemByIdAsync(itemId);

            if (item == null)
            {
                return NotFound("Item not found");
            }

            return item;
        }

        [Authorize]
        [HttpPost(Name = "AddItem")]
        public async Task<IActionResult> AddItem([FromBody] Inventory item)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Add item to invenotry: " + item,
                Timestamp = DateTime.UtcNow,
                Url = "AddItem",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            await _inventoryRepository.AddItemAsync(item);
            return CreatedAtRoute("GetItemById", new { itemId = item.Id.ToString() }, item);
        }

        [Authorize]
        [HttpPut("{itemId}", Name = "UpdateItem")]
        public async Task<IActionResult> UpdateItem(string itemId, [FromBody] Inventory updatedItem)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Update invenotry item by id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "updateItem",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            var existingItem = await _inventoryRepository.GetItemByIdAsync(itemId);

            if (existingItem == null)
            {
                return NotFound("Item not found");
            }

            updatedItem.Id = existingItem.Id;
            await _inventoryRepository.UpdateItemAsync(updatedItem);

            return Ok(updatedItem);
        }

        [Authorize]
        [HttpDelete("{itemId}", Name = "DeleteItem")]
        public async Task<IActionResult> DeleteItem(string itemId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Delete inventory item by id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "DeleteItem",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            var existingItem = await _inventoryRepository.GetItemByIdAsync(itemId);

            if (existingItem == null)
            {
                return NotFound("Item not found");
            }

            await _inventoryRepository.DeleteItemAsync(itemId);

            return NoContent();
        }

        [HttpPost("{itemId}/addquantity/{quantityToAdd}", Name = "AddItemQunatityById")]
        public async Task<IActionResult> AddQuantity(string itemId, int quantityToAdd)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Add inventory item quantity by id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "itemid/addquantity/quantity",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            try
            {
                await _inventoryRepository.AddQuantityAsync(itemId, quantityToAdd);
                return Ok($"Added {quantityToAdd} to quantity of item {itemId}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{itemId}/subtractquantity/{quantityToSubtract}", Name = "SubtractItemQuantityById")]
        public async Task<IActionResult> SubtractQuantity(string itemId, int quantityToSubtract)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Subtract inventory quantity item by id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "itemid/subtractquantity/quantity",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            try
            {
                await _inventoryRepository.SubtractQuantityAsync(itemId, quantityToSubtract);
                return Ok($"Subtracted {quantityToSubtract} from quantity of item {itemId}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("{itemId}/changeprice/{newPrice}", Name = "ChangeItemPriceById")]
        public async Task<IActionResult> ChangePrice(string itemId, double newPrice)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Update invenotry item by id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "updateItem",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            try
            {
                await _inventoryRepository.ChangePriceAsync(itemId, newPrice);
                return Ok($"Changed price of item {itemId} to {newPrice}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{itemId}/comments", Name = "GetComments")]
        public async Task<ActionResult<List<Comment>>> GetComments(string itemId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Get comments by item id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "itemid/comments",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            var comments = await _inventoryRepository.GetCommentsAsync(itemId);
            return comments;
        }

        [HttpGet("{itemId}/ratings", Name = "GetRatings")]
        public async Task<ActionResult<List<Rating>>> GetRatings(string itemId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Get ratings by item id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "itemid/ratings",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "InventoryAPI",
                LogType = "Info"
            });
            var ratings = await _inventoryRepository.GetRatingsAsync(itemId);
            return ratings;
        }

    }
}