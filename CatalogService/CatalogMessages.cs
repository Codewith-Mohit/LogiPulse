namespace CatalogService;

public record ProductDto(Guid Id, string Name, decimal Price);
public record CheckoutRequest(string DeliveryAddress, decimal TotalAmount);