namespace OrderAPIs;

public record CreateOrderRequest(string DeliveryAddress);
public record OrderDto(System.Guid Id, string OrderNumber, string DeliveryAddress, string Status);
public record OrderPlacedEvent(System.Guid OrderId, string OrderNumber, string DeliveryAddress);
