namespace ColorBlast.Core
{
    public interface IHapticService
    {
        void PlayImpact(HapticManagement.HapticTypes type);
        void PlaySelection();
    }
}