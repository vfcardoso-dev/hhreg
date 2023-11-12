namespace hhreg.business.domain.dtos;

public class MyDrakeEvent {
    public Guid Id { get; set; }
    public string? CostCenterId { get; set; }
    public string? OccurrenceId { get; set; }
    public string? RigId { get; set; }
    public string? Start { get; set; }
    public string? End { get; set; }
    public string? Justification { get; set; }
    public MyDrakeInnerEvent? Event { get; set; }
}

public class MyDrakeInnerEvent {
    public Option? Occurrence { get; set; }
    public Option? CostCenter { get; set; }
    public Option? OperationalUnit { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Reason { get; set; }
}

public class Option {
    public string? Id { get; set; }
    public string? Name { get; set; }
}