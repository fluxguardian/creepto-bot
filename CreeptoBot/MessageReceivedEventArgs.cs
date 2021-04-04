namespace StrategyTester
{
    public record  MessageReceivedEventArgs 
    { 
    
        public Message Message { get; init; }
    }
}