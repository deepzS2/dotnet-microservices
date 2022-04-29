using System;

namespace Play.Inventory.Service.DTOs
{
  public record CatalogItemDTO(Guid Id, string Name, string Description);
  public record GrantItemsDTO(Guid UserId, Guid CatalogItemId, int Quantity);
  public record InventoryItemDTO(Guid CatalogItemId, string Name, string Description, int Quantity, DateTimeOffset AcquiredDate);
}