namespace DOMAIN_LAYER.Entity.Trip;

public class TripDay
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int Day { get; set; }
    public string? Location { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public string? ImgUrl { get; set; }
    public Trip Trip { get; set; }
}
