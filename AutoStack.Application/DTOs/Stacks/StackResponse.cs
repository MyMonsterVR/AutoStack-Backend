namespace AutoStack.Application.DTOs.Stacks;

public class StackResponse
{
    public string Name { get; set; }
    public string Description { get; set; }
    public StackType Type { get; set; }
    public int Downloads { get; set; }
    public List<StackInfoResponse> StackInfo { get; set; }
}