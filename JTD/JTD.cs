using System;
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
        private SortedList<char, Vector> route;
        private List<Cannon> cannons;
        private object[,] enemies;
        private bool pointsAdded;
        private Target target;

        /// <summary>
        /// Game initialization
        /// </summary>
        public override void Begin()
        {
            ClearAll();
            
            IsPaused = false;

            GameManager.CannonSelected = 1;

            GameManager.Level = 1;
            GameManager.EnemiesAlive = 0;
            GameManager.Money = new IntMeter(1000);
            GameManager.KillCount = new IntMeter(0);
            
            enemies = new object[,]
            {
                {3, 100, 30, enemy1},
                {5, 40, 40, enemy2},
                {7, 60, 50, enemy3},
                {9, 20, 70, enemy4}
            }; //lifepoints, speed, value, texture

            /*
            CannonTemplate[] cannonTemplates = new[]
            {
                new CannonTemplate {Price = 300, Damage = 1, Interval = 1, Image = cannon1, AmmoColor = Color.Black, ShootAction = (c) => c.Shoot()},
                new CannonTemplate {Price = 500, Damage = 3, Interval = 2, Image = cannon2, AmmoColor = Color.Red, ShootAction = (c) => {
                    Timer t = new Timer(0.1);
                    t.Timeout += c.Shoot;
                    t.Start(3);
                }},
                new CannonTemplate {Price = 900, Damage = 5, Interval = 0.3, Image = cannon3, AmmoColor = Color.LimeGreen, ShootAction = (c) => c.Shoot()},
                new CannonTemplate {Price = 1000, Damage = 8, Interval = 0.6, Image = cannon4, AmmoColor = Color.Blue, ShootAction = (c) => c.Shoot()}
            };
*/
            cannons = CannonReader.Read();
            /*cannons = new object[,]
            {
                {300, 1, 1.0, cannon1, null, Color.Black},
                {500, 3, 2.0, cannon2, 0.1, Color.Red},
                {900, 5, 0.3, cannon3, null, Color.LimeGreen},
                {1000, 8, 0.6, cannon4, null, Color.Blue}
            }; //Price, damage, speed, texture, burst speed, ammo color
*/
            SetWindowSize(1000, 600);

            Level.Background.Image = grass;
            CreateLevel();
            Controllers();
            CreateMoneyCounter();
            ShowCannons();
            SelectCannon(GameManager.CannonSelected);
            KillCounter();

            Wave();

            Camera.ZoomToAllObjects();

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
            Keyboard.Listen(Key.Enter, ButtonState.Pressed, delegate { GameManager.Money.Value += 10000; }, "Debugmoney", 4);
#endif
        }

        /// <summary>
        /// Moneycounter.
        /// </summary>
        public void CreateMoneyCounter()
        {
            Label moneycounter = new Label();
            moneycounter.X = Screen.Left + 70;
            moneycounter.Y = Screen.Top - 20;
            moneycounter.TextColor = Color.Black;
            moneycounter.Color = Color.White;
            moneycounter.IntFormatString = "Money: {0:D3}";

            moneycounter.BindTo(GameManager.Money);
            Add(moneycounter);
        }

        /// <summary>
        /// Killcounter
        /// </summary>
        public void KillCounter()
        {
            Label killcounter = new Label();
            killcounter.X = Level.Right + 30;
            killcounter.Y = Level.Top + 70;
            killcounter.TextColor = Color.Black;
            killcounter.Color = Color.White;
            killcounter.IntFormatString = "Kills: {0}";

            killcounter.BindTo(GameManager.KillCount);
            Add(killcounter);
        }

        /// <summary>
        /// Create a new enemy wave.
        /// </summary>
        public void Wave()
        {
            Timer wavetimer = new Timer();
            wavetimer.Interval = 0.5;
            wavetimer.Timeout += CreateEnemy;
            wavetimer.Start(GameManager.Level + 2);
        }

        /// <summary>
        /// Create a new random enemy with properties based on current level
        /// </summary> 
        public void CreateEnemy()
        {
            int i = RandomGen.NextInt(0, 4);
            Enemy enemy = new Enemy(15, 15, (int) enemies[i, 0] * GameManager.Level, (int) enemies[i, 2], (Image) enemies[i, 3],
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

            GameManager.EnemiesAlive++;
        }

        /// <summary>
        /// After enemy dies, get money based on its value
        /// </summary>
        /// <param name="Vihu"></param>
        public void Kill(Enemy enemy)
        {
            GameManager.Money.Value += enemy.Value;
            GameManager.KillCount.Value++;

            GameManager.EnemiesAlive--;

            if (GameManager.EnemiesAlive == 0)
            {
                Wave();
                GameManager.Level++;
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
            ScoreList pointlist = new ScoreList(10, false, 0);
            pointlist = DataStorage.TryLoad(pointlist, "points.xml");
            
            HighScoreWindow pointWindow;
            if (pointsAdded == false)
            {
                pointWindow = new HighScoreWindow("Enemies killed",
                    "Yay, you killed %p! enemies. What is your name:", pointlist, GameManager.KillCount.Value);
                pointsAdded = true;
            }
            else
            {
                pointWindow = new HighScoreWindow("Enemies killed", pointlist);
            }

            pointWindow.Closed += delegate
            {
                DataStorage.Save(pointlist, "points.xml");
                End();
                pointsAdded = true;
            };
            Add(pointWindow);
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
                    t.Upgrade(GameManager.Money);
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
            Cannon cannon = cannons[GameManager.CannonSelected];
            if (GameManager.Money.Value >= cannon.Price)
            {
                
                cannon.Position = Mouse.PositionOnWorld;

                cannon.ShootTimer = new Timer();
                cannon.ShootTimer.Interval = cannon.Speed;
                
                
                    cannon.ShootTimer.Timeout += delegate { cannon.Shoot(); };
               
                cannon.ShootTimer.Start();

                Add(cannon, +1);
                GameManager.Money.Value -= cannon.Price;

                // Tower aiming timer
                cannon.TurnTimer = new Timer();
                cannon.TurnTimer.Interval = 0.1;
                cannon.TurnTimer.Timeout += delegate { cannon.Aim(); };
                cannon.TurnTimer.Start();
            }
        }

        private GameObject cannonSelection;

        public void SelectCannon(int cannon)
        {
            GameManager.CannonSelected = cannon - 1;
            cannonSelection.X = Level.Left + GameManager.CannonSelected * 10 + 10;
            cannonSelection.Y = Level.Top + 20;
        }

        /// <summary>
        /// Show buildable cannons on the top left corner
        /// </summary>
        public void ShowCannons()
        {
            cannonSelection = ShowSelection();

            for (int i = 0; i < cannons.Count; i++)
            {
                GameObject button = new GameObject(10, 10, Shape.Rectangle);
                button.X = Level.Left + i * 10 + 10;
                button.Y = Level.Top + 20;
                button.Image = cannons[i].Image;
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