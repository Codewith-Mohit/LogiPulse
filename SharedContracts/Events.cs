namespace LogiPulse.SharedContracts;

public record CheckoutRequestedEvent(string DeliveryAddress, decimal TotalAmount);