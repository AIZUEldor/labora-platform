namespace Labora.Domain.Exceptions;
public class AlreadyReviewedException : Exception
{
    public AlreadyReviewedException() : base("Siz bu ishni allaqachon baholagansiz.") { }
}