namespace LogiPulse.SharedContracts;

public record CheckoutRequestedEvent(string DeliveryAddress, decimal TotalAmount);
public record OrderPlacedEvent(Guid OrderId, string OrderNumber, string DeliveryAddress);