namespace Timer;
public class KimaiSheetEntry
{
	public int Id { get; set; }
	public DateTime Begin { get; set; }
	public DateTime End { get; set; }
	public int Activity { get; set; }
	public int Project { get; set; }
	public string Description { get; set; } = "";
	public float FixedRate { get; set; }
	public float HourlyRate { get; set; }
	public int User { get; set; }
	public bool Exported { get; set; } = false;
	public bool Billable { get; set; } = false;
}