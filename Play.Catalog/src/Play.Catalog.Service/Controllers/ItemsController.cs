using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.DTOs;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
  [ApiController]
  [Route("items")]
  public class ItemsController : ControllerBase
  {
    private readonly IRepository<Item> _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public ItemsController(IRepository<Item> repository, IPublishEndpoint publishEndpoint)
    {
      _repository = repository;
      _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDTO>>> GetAsync()
    {
      var items = (await _repository.GetAllAsync()).Select(i => i.AsDTO());
      return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDTO>> GetByIdAsync(Guid id)
    {
      var item = await _repository.GetAsync(id);

      if (item == null) return NotFound();
      else return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDTO>> Post([FromBody] CreateItemDTO createItemDTO)
    {
      var item = new Item
      {
        Name = createItemDTO.Name,
        Description = createItemDTO.Description,
        Price = createItemDTO.Price,
        CreatedAt = DateTimeOffset.UtcNow
      };

      await _repository.CreateAsync(item);

      await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

      return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, UpdateItemDTO updateItemDTO)
    {
      var existingItem = await _repository.GetAsync(id);

      if (existingItem == null) return NotFound();

      existingItem.Name = updateItemDTO.Name;
      existingItem.Description = updateItemDTO.Description;
      existingItem.Price = updateItemDTO.Price;

      await _repository.UpdateAsync(existingItem);
      await _publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      var itemExist = await _repository.GetAsync(id);

      if (itemExist == null) return NotFound();

      await _repository.DeleteAsync(id);
      await _publishEndpoint.Publish(new CatalogItemDeleted(id));

      return NoContent();
    }
  }
}