namespace ColorBlast.Core
{
    public interface IMatchable
    {
        bool CanMatchWith(IMatchable other);
    }
}