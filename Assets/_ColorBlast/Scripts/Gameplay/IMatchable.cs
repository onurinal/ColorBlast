namespace ColorBlast.Gameplay
{
    public interface IMatchable
    {
        bool CanMatchWith(IMatchable other);
    }
}