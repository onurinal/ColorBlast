using ColorBlast.Core;
using ColorBlast.Manager;

namespace ColorBlast.Features
{
    /// <summary>
    /// Encapsulates all sub-systems required for grid operations.
    /// Acts as a central hub for grid logic dependencies.
    /// </summary>
    public class GridSystems
    {
        public GridSpawner Spawner { get; private set; }
        public GridChecker Checker { get; private set; }
        public GridRefill Refill { get; private set; }
        public GridShuffler Shuffler { get; private set; }
        public EffectPipeline EffectPipeline { get; private set; }
        public BlockEffectFactory EffectFactory { get; private set; }

        public void Build(
            Block[,] grid,
            LevelProperties levelProperties,
            GameConfig config,
            GridManager gridManager,
            IHapticService hapticService)
        {
            Spawner = BuildSpawner(grid, levelProperties, gridManager);
            Checker = BuildChecker(grid, levelProperties, config);
            Refill = BuildRefill(grid, levelProperties, gridManager);
            Shuffler = BuildShuffler(grid, levelProperties, config, gridManager);

            var particleService = new BlockParticleService();
            var context = new EffectExecutionContext(
                grid, levelProperties, config,
                Spawner, gridManager, particleService, hapticService);

            var comboDetector = BuildComboDetector(grid, levelProperties);
            EffectFactory = new BlockEffectFactory(Checker, config, comboDetector);

            EffectPipeline = new EffectPipeline();
            EffectPipeline.Initialize(Refill, Spawner, Checker, context);
        }

        private static GridSpawner BuildSpawner(Block[,] grid, LevelProperties level, GridManager gridManager)
        {
            var spawner = new GridSpawner();
            spawner.Initialize(grid, gridManager, level);
            return spawner;
        }

        private static GridChecker BuildChecker(Block[,] grid, LevelProperties level, GameConfig config)
        {
            var checker = new GridChecker();
            checker.Initialize(grid, level, config);
            return checker;
        }

        private static GridRefill BuildRefill(Block[,] grid, LevelProperties level, GridManager gridManager)
        {
            var refill = new GridRefill();
            refill.Initialize(grid, gridManager, level);
            return refill;
        }

        private static GridShuffler BuildShuffler(Block[,] grid, LevelProperties level,
            GameConfig config, GridManager gridManager)
        {
            var shuffler = new GridShuffler();
            shuffler.Initialize(grid, level, gridManager, config);
            return shuffler;
        }

        private static ComboDetector BuildComboDetector(Block[,] grid, LevelProperties level)
        {
            var detector = new ComboDetector();
            detector.Initialize(grid, level);
            return detector;
        }
    }
}