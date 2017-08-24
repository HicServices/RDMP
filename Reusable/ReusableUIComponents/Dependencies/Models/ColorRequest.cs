namespace ReusableUIComponents.Dependencies.Models
{
    public class ColorRequest
    {
        public ColorRequest(bool isHighlighted)
        {
            IsHighlighted = isHighlighted;
        }

        public bool IsHighlighted { get; set; }
    }
}