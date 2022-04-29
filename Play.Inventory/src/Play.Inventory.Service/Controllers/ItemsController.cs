using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.DTOs;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
  [ApiController]
  [Route("items")]
  public class ItemsController : ControllerBase
  {
    private readonly IRepository<InventoryItem> _itemsRepository;
    private readonly IRepository<CatalogItem> _catalogItemsRepository;
    private readonly CatalogClient _catalogClient;

    public ItemsController(IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient, IRepository<CatalogItem> catalogItemsRepository)
    {
      _itemsRepository = itemsRepository;
      _catalogItemsRepository = catalogItemsRepository;
      _catalogClient = catalogClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDTO>>> GetAsync(Guid userId)
    {
      if (userId == Guid.Empty) return BadRequest();

      var inventoryItemsEntities = await _itemsRepository.GetAllAsync(item => item.UserId == userId);
      var catalogItemsEntities = await _catalogItemsRepository.GetAllAsync();

      var items = inventoryItemsEntities.Select(inventoryItem =>
      {
        var catalogItem = catalogItemsEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
        return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
      });

      return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(GrantItemsDTO grantItemsDTO)
    {
      var inventoryItem = await _itemsRepository.GetAsync(item => item.UserId == grantItemsDTO.UserId && item.CatalogItemId == grantItemsDTO.CatalogItemId);

      if (inventoryItem == null)
      {
        inventoryItem = new InventoryItem
        {
          CatalogItemId = grantItemsDTO.CatalogItemId,
          UserId = grantItemsDTO.UserId,
          Quantity = grantItemsDTO.Quantity,
          AcquiredDate = DateTimeOffset.UtcNow
        };

        await _itemsRepository.CreateAsync(inventoryItem);
      }
      else
      {
        inventoryItem.Quantity += grantItemsDTO.Quantity;
        await _itemsRepository.UpdateAsync(inventoryItem);
      }

      return Ok();
    }
  }
}