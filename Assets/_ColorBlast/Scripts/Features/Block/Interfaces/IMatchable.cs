namespace ColorBlast.Features
{
    public interface IMatchable
    {
        bool CanMatchWith(IMatchable block);
    }
}