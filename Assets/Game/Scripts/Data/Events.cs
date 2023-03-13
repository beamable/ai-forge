namespace Game.Data
{
    public class Events
    {
        
    }

    public interface IContentEvent
    {
        string ContentID { get; set; }
    }

    public class CreatingItemStarted : IContentEvent
    {
        public string ContentID { get; set; }
    }

    public class CreatingItemFinished : IContentEvent
    {
        public string ContentID { get; set; }
        public bool Successful { get; set; } = true;
    }

    public class ItemSold : IContentEvent
    {
        public string ContentID { get; set; }
    }
    
    public class ButtonPressed{}
}