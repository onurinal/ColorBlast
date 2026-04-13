namespace ColorBlast.Gameplay
{
    public interface IChainSchedular
    {
        bool IsTriggered(Block block);
        void MarkTriggered(Block block);
        void BeginEffect();
        void EndEffect();
        void SuspendGrid();
        void ResumeGrid();
    }
}