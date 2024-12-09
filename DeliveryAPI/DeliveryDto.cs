// DeliveryDto.cs (DTOs)
public class DeliveryCreateDto
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string? PaymentId { get; set; }
    public string? Address { get; set; }
    public DateTime DeliveryTime { get; set; }
    public double GeoX { get; set; }
    public double GeoY { get; set; }
}

public class DeliveryUpdateDto
{
    public string? Address { get; set; }
    public double GeoX { get; set; }
    public double GeoY { get; set; }
}