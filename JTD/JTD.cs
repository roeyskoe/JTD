using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;
using System.Linq;
using System.Text.Json;
using JTD.GUI;
using Jypeli.Controls;

namespace JTD
{
    public class JTD : PhysicsGame
    {
        private SortedList<char, Vector> route;
        private Dictionary<string,CannonTemplate> cannons;
        private Dictionary<string, EnemyTemplate> enemies;
        private bool pointsAdded;
        Label debugLabel;
        Widget mousevisualizer;

        /// <summary>
        /// Game initialization
        /// </summary>
        public override void Begin()
        {
            ClearAll();

            IsPaused = false;

            GameManager.Level = 1;
            GameManager.EnemiesAlive = 0;
            GameManager.Money = new IntMeter(1000);
            GameManager.KillCount = new IntMeter(0);
            GameManager.Images = new Images();
            GameManager.ListenContext = ControlContext;
            GameManager.DebugText = "";
            GameManager.Grid = new Grid();
            GameManager.Grid.CellSize = new Vector(20, 20);
            
            cannons = JsonSerializer.Deserialize<Dictionary<string,CannonTemplate>>(System.IO.File.ReadAllText("Content/cannons/CannonDefinitions.json"));
            enemies = JsonSerializer.Deserialize<Dictionary<string, EnemyTemplate>>(System.IO.File.ReadAllText("Content/enemies/EnemyDefinitions.json"));

            foreach (var item in cannons.Keys)
            {
                CannonTemplate c = cannons[item];
                c.Name = item;
                cannons[item] = c; // TODO: Simpler way without modifying json?
            }

            GameManager.CannonSelected = cannons.First().Value.Name;

            SetWindowSize(2000, 1200);
            
            CreateLevel();
            Controllers();
            CreateMoneyCounter();
            KillCounter();

            Camera.ZoomToAllObjects();
            Level.Size = Screen.Size;
            Level.Background.Image = GameManager.Images["grass.png"];
            Level.Background.TileToLevel();

            Wave();

            pointsAdded = false;

            Cannonselector cs = new Cannonselector(cannons.Values.ToList());
            cs.X = Screen.Left + cs.Width/1.5;
            cs.Y = Screen.Bottom + cs.Height;
            Add(cs);

            mousevisualizer = new Widget(20, 20);
            mousevisualizer.Color = Color.Transparent;
            mousevisualizer.BorderColor = Color.LightGray;
            Add(mousevisualizer);

            debugLabel = new Label();
            debugLabel.Y = 100;
            Add(debugLabel);
        }
        protected override void Update(Time time)
        {
            debugLabel.Text = GameManager.DebugText;
            mousevisualizer.Position = GameManager.Grid.SnapToLines(Mouse.PositionOnScreen);
            base.Update(time);
        }

        /// <summary>
        /// Set controllers
        /// </summary>
        public void Controllers()
        {
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Quit");
            Mouse.Listen(MouseButton.Left, ButtonState.Pressed, MouseHandler, "Build cannon").InContext(this);
            /*
            Keyboard.Listen(Key.D1, ButtonState.Pressed, SelectCannon, "Select 1st cannon", 1);
            Keyboard.Listen(Key.D2, ButtonState.Pressed, SelectCannon, "Select 2nd cannon", 2);
            Keyboard.Listen(Key.D3, ButtonState.Pressed, SelectCannon, "Select 3rd cannon", 3);
            Keyboard.Listen(Key.D4, ButtonState.Pressed, SelectCannon, "Select 4th cannon", 4);
            */
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
            killcounter.X = Screen.Right - 40;
            killcounter.Y = Screen.Top - 20;
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
            EnemyTemplate nextEnemy = enemies[RandomGen.SelectOne(enemies.Keys.ToArray())];
            Enemy enemy = new Enemy(nextEnemy, GameManager.Level, route);
            enemy.Position = route.Values[0];

            Add(enemy);
            
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
            tiles.SetTileMethod('*', CreateWall);
            for (char merkki = 'A'; merkki <= 'Z'; merkki++)
            {
                tiles.SetTileMethod(merkki, CreateCorner, merkki);
            }
            tiles.Execute(20, 20);
            CreateTarget();
        }

        private void CreateWall(Vector position, double width, double height)
        {
            GameObject g = new GameObject(width, height);
            g.Position = position;
            Add(g);
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
            Target target = new Target(50, 70, 500, GameManager.Images["castle.png"]);
            target.Position = route.Values[route.Count - 1]; // + new Vector(15, 10);

            Add(target);

            target.Destroyed += delegate { End(); };

            GameManager.Target = target;
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
            CannonTemplate selected = cannons[GameManager.CannonSelected];
            
            if (GameManager.Money.Value >= selected.Price)
            {
                Cannon cannon = new Cannon(selected);
                cannon.Position = GameManager.Grid.SnapToLines(Mouse.PositionOnWorld);

                Add(cannon, +1);
                GameManager.Money.Value -= cannon.Price;

                // Tower aiming timer
                cannon.TurnTimer = new Timer();
                cannon.TurnTimer.Interval = 0.1;
                cannon.TurnTimer.Timeout += delegate { cannon.Aim(); };
                cannon.TurnTimer.Start();
            }
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