using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;

namespace JTD
{
    public class JTD : PhysicsGame
    {
        private Image grass = LoadImage("grass.png");

        private Image cannon1 = LoadImage("cannon1"),
            cannon2 = LoadImage("cannon2.png"),
            cannon3 = LoadImage("cannon3.png"),
            cannon4 = LoadImage("cannon4.png");

        private Image enemy1 = LoadImage("enemy1.png"),
            enemy2 = LoadImage("enemy2.png"),
            enemy3 = LoadImage("enemy3.png"),
            enemy4 = LoadImage("enemy4.png");

        private Image castle = LoadImage("castle.png");
        private ScoreList pointlist;
        private IntMeter enemyKillCount;
        private SortedList<char, Vector> route;
        private object[,] cannons;
        private object[,] enemies;
        private int level;
        private int enemiesAlive;
        private bool pointsAdded;
        private IntMeter money;
        private Target target;

        /// <summary>
        /// Game initialization
        /// </summary>
        public override void Begin()
        {
            ClearAll();

            IsPaused = false;

            level = 1;
            enemiesAlive = 0;
            cannonSelected = 1;

            pointlist = new ScoreList(10, false, 0);

            enemies = new object[,]
            {
                {3, 100, 30, enemy1},
                {5, 40, 40, enemy2},
                {7, 60, 50, enemy3},
                {9, 20, 70, enemy4}
            }; //lifepoints, speed, value, texture

            cannons = new object[,]
            {
                {300, 1, 1.0, cannon1, null, Color.Black},
                {500, 3, 0.1, cannon2, 2, Color.Red},
                {900, 5, 0.3, cannon3, null, Color.LimeGreen},
                {1000, 8, 0.6, cannon4, null, Color.Blue}
            }; //Price, damage, speed, texture, burst speed, ammo color

            SetWindowSize(1000, 600);

            Level.Background.Image = grass;
            CreateLevel();
            Controllers();
            CreateMoneyCounter();
            ShowCannons();
            SelectCannon(cannonSelected);
            KillCounter();

            Wave();

            Camera.ZoomToAllObjects();

            pointlist = DataStorage.TryLoad(pointlist, "points.xml");
            pointsAdded = false;
        }

        /// <summary>
        /// Set controllers
        /// </summary>
        public void Controllers()
        {
            IsMouseVisible = true;

            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Quit");
            Mouse.Listen(MouseButton.Left, ButtonState.Pressed, MouseHandler, "Build cannon");
            Keyboard.Listen(Key.D1, ButtonState.Pressed, SelectCannon, "Select 1st cannon", 1);
            Keyboard.Listen(Key.D2, ButtonState.Pressed, SelectCannon, "Select 2nd cannon", 2);
            Keyboard.Listen(Key.D3, ButtonState.Pressed, SelectCannon, "Select 3rd cannon", 3);
            Keyboard.Listen(Key.D4, ButtonState.Pressed, SelectCannon, "Select 4th cannon", 4);
#if DEBUG
            Keyboard.Listen(Key.Enter, ButtonState.Pressed, delegate { money.Value += 10000; }, "Debugmoney", 4);
#endif
        }

        /// <summary>
        /// Moneycounter.
        /// </summary>
        public void CreateMoneyCounter()
        {
            money = new IntMeter(1000);

            Label rahaLaskuri = new Label();
            rahaLaskuri.X = Screen.Left + 70;
            rahaLaskuri.Y = Screen.Top - 20;
            rahaLaskuri.TextColor = Color.Black;
            rahaLaskuri.Color = Color.White;
            rahaLaskuri.IntFormatString = "Money: {0:D3}";

            rahaLaskuri.BindTo(money);
            Add(rahaLaskuri);
        }

        /// <summary>
        /// Killcounter
        /// </summary>
        public void KillCounter()
        {
            enemyKillCount = new IntMeter(0);

            Label tappoLaskuri = new Label();
            tappoLaskuri.X = Level.Right + 30;
            tappoLaskuri.Y = Level.Top + 70;
            tappoLaskuri.TextColor = Color.Black;
            tappoLaskuri.Color = Color.White;
            tappoLaskuri.IntFormatString = "Kills: {0}";

            tappoLaskuri.BindTo(enemyKillCount);
            Add(tappoLaskuri);
        }

        /// <summary>
        /// Create a new enemy wave.
        /// </summary>
        public void Wave()
        {
            Timer ajastin = new Timer();
            ajastin.Interval = 0.5;
            ajastin.Timeout += CreateEnemy;
            ajastin.Start(level + 2);
        }

        /// <summary>
        /// Create a new random enemy with properties based on current level
        /// </summary> 
        public void CreateEnemy()
        {
            int i = RandomGen.NextInt(0, 4);
            Enemy enemy = new Enemy(15, 15, (int) enemies[i, 0] * level, (int) enemies[i, 2], (Image) enemies[i, 3],
                (int) enemies[i, 1], route);
            enemy.Position = route.Values[0];

            Add(enemy);
            AddCollisionHandler(enemy, "Target", delegate
            {
                Explosion explosion = new Explosion(50);
                explosion.Position = enemy.Position;
                //Add(explosion); //TODO: nullpointer crash
                target.Health.Value -= 100;
                enemy.Destroy();
            });
            enemy.Destroyed += delegate { Kill(enemy); };

            enemiesAlive++;
        }

        /// <summary>
        /// After enemy dies, get money based on its value
        /// </summary>
        /// <param name="Vihu"></param>
        public void Kill(Enemy enemy)
        {
            money.Value += enemy.Value;
            enemyKillCount.Value++;

            enemiesAlive--;

            if (enemiesAlive == 0)
            {
                Wave();
                level++;
            }
        }

        /// <summary>
        /// Create the level
        /// </summary>
        public void CreateLevel()
        {
            route = new SortedList<char, Vector>();

            TileMap tiles = TileMap.FromLevelAsset("level.txt");
            tiles.SetTileMethod('#', CreatePath);
            tiles.SetTileMethod('+', CreateNoBuildArea);
            for (char merkki = 'A'; merkki <= 'Z'; merkki++)
            {
                tiles.SetTileMethod(merkki, CreateCorner, merkki);
            }

            tiles.Execute(20, 20);
            CreateTarget();
        }

        /// <summary>
        /// Create an area where you cannot build
        /// </summary>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void CreateNoBuildArea(Vector position, double width, double height)
        {
            GameObject noBuild = new GameObject(width, height);
            noBuild.Position = position;
            noBuild.Tag = "NoBuild";
            noBuild.Color = Color.Transparent;
            Add(noBuild, -1);
        }

        /// <summary>
        /// Create a path in which enemies travel.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void CreatePath(Vector position, double width, double height)
        {
            GameObject polku = new GameObject(width, height);
            polku.Position = position;
            polku.Tag = "Path";
            Add(polku, -1);
        }

        /// <summary>
        /// Create a cornerpiece where enemies direction changes.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="character"></param>
        public void CreateCorner(Vector position, double width, double height, char character)
        {
            route.Add(character, position);
            CreatePath(position, width, height);
        }

        /// <summary>
        /// Target which you must protect.
        /// </summary>
        public void CreateTarget()
        {
            target = new Target(50, 70, 500, castle);
            target.Position = route.Values[route.Count - 1]; // + new Vector(15, 10);

            Add(target);

            target.Destroyed += delegate { End(); };
        }

        /// <summary>
        /// You lost!
        /// </summary>
        public void End()
        {
            MultiSelectWindow menu =
                new MultiSelectWindow("You lost!", "Try again", "Highscores", "Quit");
            Add(menu);
            menu.AddItemHandler(0, Begin);
            menu.AddItemHandler(1, Points);
            menu.AddItemHandler(2, Exit);
            IsPaused = true;
        }

        /// <summary>
        /// Highscore window.
        /// </summary>
        public void Points()
        {
            HighScoreWindow pisteIkkuna;
            if (pointsAdded == false)
            {
                pisteIkkuna = new HighScoreWindow("Enemies killed",
                    "Yay, you killed %p! enemies. What is your name:", pointlist, enemyKillCount.Value);
                pointsAdded = true;
            }
            else
            {
                pisteIkkuna = new HighScoreWindow("Enemies killed", pointlist);
            }

            pisteIkkuna.Closed += delegate
            {
                DataStorage.Save(pointlist, "points.xml");
                End();
                pointsAdded = true;
            };
            Add(pisteIkkuna);
        }

        /// <summary>
        /// Handles mouse actions.
        /// </summary>
        public void MouseHandler()
        {
            List<GameObject> cannons = GetObjectsWithTag("Cannon");
            List<GameObject> buttons = GetObjectsWithTag("Buttons");
            List<GameObject> route = GetObjectsWithTag("Path");

            bool button = false;
            bool cannon = false;
            bool path = false;
            bool somethingClicked = false;

            // is mouse on a cannon?
            foreach (Cannon t in cannons)
            {
                cannon = Mouse.IsCursorOn(t);
                if (cannon)
                {
                    somethingClicked = true;
                    t.Upgrade(money);
                }
            }

            // cannon selector
            int position = 0;
            foreach (GameObject n in buttons)
            {
                button = Mouse.IsCursorOn(n);
                position++;

                if (button)
                {
                    somethingClicked = true;
                    SelectCannon(position);
                }
            }

            // is mouse on a route
            foreach (GameObject p in route)
            {
                path = Mouse.IsCursorOn(p);
                if (path)
                {
                    somethingClicked = true;
                }
            }

            if (!somethingClicked)
            {
                BuildCannon();
            }
        }

        /// <summary>
        /// Create a tower on mouses location
        /// </summary>
        public void BuildCannon()
        {
            if (money.Value >= (int) cannons[cannonSelected, 0])
            {
                Cannon cannon = new Cannon((int) cannons[cannonSelected, 0], (int) cannons[cannonSelected, 1],
                    (double) cannons[cannonSelected, 2], (Image) cannons[cannonSelected, 3]);
                cannon.Level = 0;
                cannon.AmmoColor = (Color) cannons[cannonSelected, 5];

                cannon.Position = Mouse.PositionOnWorld;

                if (cannons[cannonSelected, 4] == null)
                {
                    cannon.ShootTimer = new Timer();
                    cannon.ShootTimer.Interval = (double) cannons[cannonSelected, 2];
                    cannon.ShootTimer.Timeout += delegate { cannon.Shoot(); };
                    cannon.ShootTimer.Start();
                }
                else
                {
                    cannon.BurstTimer = new Timer();
                    cannon.BurstTimer.Interval = (int) cannons[cannonSelected, 4];
                    cannon.BurstTimer.Timeout += delegate { cannon.BurstFire((double) cannons[cannonSelected, 2]); };
                    cannon.BurstTimer.Start();
                    cannon.BurstFire((double) cannons[cannonSelected,
                        2]); // TODO: Fix game crashing if burstcannon is updated before it fires for the first time
                }

                Add(cannon, +1);
                money.Value -= (int) cannons[cannonSelected, 0];

                // Tower aiming timer
                cannon.TurnTimer = new Timer();
                cannon.TurnTimer.Interval = 0.1;
                cannon.TurnTimer.Timeout += delegate { cannon.Aim(); };
                cannon.TurnTimer.Start();
            }
        }

        /// <summary>
        /// What cannon is selected for building
        /// </summary>
        private int cannonSelected = 1;

        private GameObject cannonSelection;

        public void SelectCannon(int cannon)
        {
            cannonSelected = cannon - 1;
            cannonSelection.X = Level.Left + cannonSelected * 10 + 10;
            cannonSelection.Y = Level.Top + 20;
        }

        /// <summary>
        /// Show buildable cannons on the top left corner
        /// </summary>
        public void ShowCannons()
        {
            int cannons = this.cannons.GetLength(0);

            cannonSelection = ShowSelection();

            for (int i = 0; i < cannons; i++)
            {
                GameObject button = new GameObject(10, 10, Shape.Rectangle);
                button.X = Level.Left + i * 10 + 10;
                button.Y = Level.Top + 20;
                button.Image = (Image) this.cannons[i, 3];
                button.Tag = "Button";
                Add(button, 3);
            }
        }

        /// <summary>
        /// Shows selected cannon with a yellow square
        /// </summary>
        public GameObject ShowSelection()
        {
            GameObject selection = new GameObject(10, 10, Shape.Rectangle);
            selection.Color = Color.Yellow;
            Add(selection, 2);
            return selection;
        }

        /// <summary>
        /// Finds nearest enemy to a given cannon
        /// </summary>
        /// <returns>Nearest enemy</returns>
        /// <param name="cannon">cannon</param>
        public PhysicsObject FindEnemy(Cannon cannon)
        {
            PhysicsObject nearestEnemy = null;

            double shortestDist = double.MaxValue;

            foreach (PhysicsObject enemy in GetObjectsWithTag("Enemy"))
            {
                double dist = Vector.Distance(enemy.Position, cannon.Position);

                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }
    }
}