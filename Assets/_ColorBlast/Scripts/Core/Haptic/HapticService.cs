namespace ColorBlast.Core
{
    public class HapticService : IHapticService
    {
        public void PlayImpact(HapticManagement.HapticTypes type) => HapticManagement.GenerateWithCooldown(type);

        public void PlaySelection() => HapticManagement.GenerateWithCooldown(HapticManagement.HapticTypes.Selection);
    }
}