using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;

public class JTD : PhysicsGame
{
    private Image nurmikko = LoadImage("Nurmikko");

    private Image tykki1 = LoadImage("Tykki1"),
        tykki2 = LoadImage("Tykki2"),
        tykki3 = LoadImage("Tykki3"),
        tykki4 = LoadImage("Tykki4");

    private Image vihu1 = LoadImage("Vihu1"),
        vihu2 = LoadImage("Vihu2"),
        vihu3 = LoadImage("Vihu3"),
        vihu4 = LoadImage("Vihu4");

    private Image jyfl = LoadImage("Linna");
    private ScoreList pistelista;
    private IntMeter tapettujaVihollisia;
    private SortedList<char, Vector> reitti;
    private object[,] tykit;
    private object[,] viholliset;
    private int taso;
    private int vihollisia;
    private bool lisatty;
    private IntMeter raha;
    private Target kohde;

    /// <summary>
    /// Kutsuu kaikkia pelin eri toimintoja.
    /// </summary>
    public override void Begin()
    {
        ClearAll();

        IsPaused = false;

        taso = 1;
        vihollisia = 0;
        valittuTykki = 1;

        pistelista = new ScoreList(10, false, 0);

        viholliset = new object[,]
        {
            {3, 100, 30, vihu1},
            {5, 40, 40, vihu2},
            {7, 60, 50, vihu3},
            {9, 20, 70, vihu4}
        }; //Elämää, nopeus, arvo, tekstuuri

        tykit = new object[,]
        {
            {300, 1, 1.0, tykki1, null, Color.Black},
            {500, 3, 0.1, tykki2, 2, Color.Red},
            {900, 5, 0.3, tykki3, null, Color.LimeGreen},
            {1000, 8, 0.6, tykki4, null, Color.Blue}
        }; //Hinta, vahinko, nopeus, tekstuuri, burstin nopeus, ammuksen väri

        SetWindowSize(1000, 600);

        Level.Background.Image = nurmikko;
        LuoKentta();
        Ohjaimet();
        LuoRahaLaskuri();
        NaytaTykit();
        ValitseTykki(valittuTykki);
        TappoLaskuri();

        Aalto();

        Camera.ZoomToAllObjects();

        pistelista = DataStorage.TryLoad(pistelista, "pisteet.xml");
        lisatty = false;
    }

    /// <summary>
    /// Aliohjelma joka määrittelee kaikki ohjaimet.
    /// </summary>
    public void Ohjaimet()
    {
        IsMouseVisible = true;

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Mouse.Listen(MouseButton.Left, ButtonState.Pressed, Hiiri, "Rakenna torni");
        Keyboard.Listen(Key.D1, ButtonState.Pressed, ValitseTykki, "Valitse 1. tykki", 1);
        Keyboard.Listen(Key.D2, ButtonState.Pressed, ValitseTykki, "Valitse 2. tykki", 2);
        Keyboard.Listen(Key.D3, ButtonState.Pressed, ValitseTykki, "Valitse 3. tykki", 3);
        Keyboard.Listen(Key.D4, ButtonState.Pressed, ValitseTykki, "Valitse 4. tykki", 4);
#if DEBUG
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, delegate { raha.Value += 10000; }, "Debugmoney", 4);
#endif
    }

    /// <summary>
    /// Rahalaskuri.
    /// </summary>
    public void LuoRahaLaskuri()
    {
        raha = new IntMeter(1000);

        Label rahaLaskuri = new Label();
        rahaLaskuri.X = Screen.Left + 70;
        rahaLaskuri.Y = Screen.Top - 20;
        rahaLaskuri.TextColor = Color.Black;
        rahaLaskuri.Color = Color.White;
        rahaLaskuri.IntFormatString = "Rahaa: {0:D3}";

        rahaLaskuri.BindTo(raha);
        Add(rahaLaskuri);
    }

    /// <summary>
    /// Näyttää kuinka monta vihollista on tapettu
    /// </summary>
    public void TappoLaskuri()
    {
        tapettujaVihollisia = new IntMeter(0);

        Label tappoLaskuri = new Label();
        tappoLaskuri.X = Level.Right + 30;
        tappoLaskuri.Y = Level.Top + 70;
        tappoLaskuri.TextColor = Color.Black;
        tappoLaskuri.Color = Color.White;
        tappoLaskuri.IntFormatString = "Tappoja: {0}";

        tappoLaskuri.BindTo(tapettujaVihollisia);
        Add(tappoLaskuri);
    }

    /// <summary>
    /// Luo vihollisaallon.
    /// </summary>
    public void Aalto()
    {
        Timer ajastin = new Timer();
        ajastin.Interval = 0.5;
        ajastin.Timeout += LuoVihollinen;
        ajastin.Start(taso + 2); //Luodaan 5 vihollista
    }

    /// <summary>
    /// Luodaan vihollinen, ja sille aivot
    /// </summary> 
    public void LuoVihollinen()
    {
        //Arvotaan nyt satunnaiset viholliset, tulevaisuudessa voisi olla vaikka aaltokohtaisesti
        //määritetty mitä tulee (ja millä ominaisuuksilla?).
        int i = RandomGen.NextInt(0, 4);
        Enemy vihu = new Enemy(15, 15, (int) viholliset[i, 0] * taso, (int) viholliset[i, 2], (Image) viholliset[i, 3],
            (int) viholliset[i, 1], reitti);
        vihu.Position = reitti.Values[0];

        Add(vihu);
        AddCollisionHandler(vihu, "JYFL", delegate
        {
            Explosion rajahdys = new Explosion(50);
            rajahdys.Position = vihu.Position;
            //Add(rajahdys); //TODO: nullpointer crash
            kohde.Elamalaskuri.Value -= 100;
            vihu.Destroy();
        });
        vihu.Destroyed += delegate { Tappo(vihu); };

        vihollisia++;
    }

    /// <summary>
    /// Antaa rahaa tapetun vihollisen arvon verran.
    /// </summary>
    /// <param name="Vihu"></param>
    public void Tappo(Enemy vihu)
    {
        raha.Value += vihu.Arvo;
        tapettujaVihollisia.Value++;

        vihollisia--;

        if (vihollisia == 0)
        {
            Aalto();
            taso++;
        }
    }

    /// <summary>
    /// Luodaan kenttä.
    /// </summary>
    public void LuoKentta()
    {
        reitti = new SortedList<char, Vector>();

        TileMap ruudut = TileMap.FromLevelAsset("Kenttä");
        ruudut.SetTileMethod('#', LuoPolku);
        ruudut.SetTileMethod('+', Rakennuskieltoalue);
        for (char merkki = 'A'; merkki <= 'Z'; merkki++)
        {
            ruudut.SetTileMethod(merkki, LuoKulma, merkki);
        }

        ruudut.Execute(20, 20);
        LuoKohde();
    }

    /// <summary>
    /// Alue jolle ei voi rakentaa tykkejä, mutta ei näy pelikentällä.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    public void Rakennuskieltoalue(Vector paikka, double leveys, double korkeus)
    {
        GameObject polku = new GameObject(leveys, korkeus);
        polku.Position = paikka;
        polku.Tag = "Polku";
        polku.Color = Color.Transparent;
        Add(polku, -1);
    }

    /// <summary>
    /// Luodaan polku jota pitkin oliot kulkevat.
    /// </summary>
    /// <param name="paikka">Paikka.</param>
    /// <param name="leveys">Leveys.</param>
    /// <param name="korkeus">Korkeus.</param>
    public void LuoPolku(Vector paikka, double leveys, double korkeus)
    {
        GameObject polku = new GameObject(leveys, korkeus);
        polku.Position = paikka;
        polku.Tag = "Polku";
        Add(polku, -1);
    }

    /// <summary>
    /// Luodaan reitin kulma, jossa oli kääntyy kohti seuraavaa kulmaa.
    /// </summary>
    /// <param name="paikka">Paikka.</param>
    /// <param name="leveys">Kulmaan tulevan "reittipalan" leveys.</param>
    /// <param name="korkeus">Kulmaan tulevan "reittipalan" korkeus.</param>
    /// <param name="merkki">Taulukossa oleva merkki</param>
    public void LuoKulma(Vector paikka, double leveys, double korkeus, char merkki)
    {
        reitti.Add(merkki, paikka);
        LuoPolku(paikka, leveys, korkeus);
    }

    /// <summary>
    /// Suojeltava kohde.
    /// </summary>
    public void LuoKohde()
    {
        kohde = new Target(50, 70, 500, jyfl);
        kohde.Position = reitti.Values[reitti.Count - 1] + new Vector(15, 10);

        Add(kohde);

        kohde.Destroyed += delegate { Loppu(); };
    }

    /// <summary>
    /// Hävisit pelin. Aina ei voi voittaa
    /// </summary>
    public void Loppu()
    {
        MultiSelectWindow valikko =
            new MultiSelectWindow("Hävisit pelin!", "Aloita alusta", "Parhaat pisteet", "Lopeta");
        Add(valikko);
        valikko.AddItemHandler(0, Begin);
        valikko.AddItemHandler(1, Pisteet);
        valikko.AddItemHandler(2, Exit);
        IsPaused = true;
    }

    /// <summary>
    /// Parhaiden pelaajien pisteet. Mitä enemmän rahaa sinulla oli jäljellä, sitä korkeammalle pääset.
    /// </summary>
    public void Pisteet()
    {
        HighScoreWindow pisteIkkuna;
        if (lisatty == false)
        {
            pisteIkkuna = new HighScoreWindow("Tapettuja vihollisia",
                "Onneksi olkoon, tapoit %p! vihollista. Syötä nimesi:", pistelista, tapettujaVihollisia.Value);
            lisatty = true;
        }
        else
        {
            pisteIkkuna = new HighScoreWindow("Parhaat pisteet", pistelista);
        }

        pisteIkkuna.Closed += delegate
        {
            DataStorage.Save(pistelista, "pisteet.xml");
            Loppu();
            lisatty = true;
        };
        Add(pisteIkkuna);
    }

    /// <summary>
    /// Hiiren klikkauksia seuraava aliohjelma.
    /// </summary>
    public void Hiiri()
    {
        //Luodaan listat pelialueella olevista objekteista.
        List<GameObject> tykit = GetObjectsWithTag("Tykki");
        List<GameObject> nappulat = GetObjectsWithTag("Nappula");
        List<GameObject> reitti = GetObjectsWithTag("Polku");
        bool nappula = false;
        bool tykki = false;
        bool polku = false;
        bool jotainKlikattu = false;

        //Verrataan hiiren sijaintia listojen objekteihin.
        foreach (Cannon t in tykit)
        {
            tykki = Mouse.IsCursorOn(t);
            if (tykki)
            {
                jotainKlikattu = true;
                raha.Value = t.PaivitaTykki(raha.Value);
            }
        }

        int sijainti = 0;
        foreach (GameObject n in nappulat)
        {
            nappula = Mouse.IsCursorOn(n);
            sijainti++;

            if (nappula)
            {
                jotainKlikattu = true;
                ValitseTykki(sijainti);
            }
        }

        foreach (GameObject p in reitti)
        {
            polku = Mouse.IsCursorOn(p);
            if (polku)
            {
                jotainKlikattu = true;
            }
        }

        if (!jotainKlikattu)
        {
            RakennaTykki();
        }
    }

    /// <summary>
    /// Luo tornin hiiren sijaintiin.
    /// </summary>
    public void RakennaTykki()
    {
        if (raha.Value >= (int) tykit[valittuTykki, 0])
        {
            Cannon torni = new Cannon((int) tykit[valittuTykki, 0], (int) tykit[valittuTykki, 1],
                (double) tykit[valittuTykki, 2], (Image) tykit[valittuTykki, 3]);
            torni.Versio = 0;
            torni.AmmuksenVari = (Color) tykit[valittuTykki, 5];

            torni.Position = Mouse.PositionOnWorld;

            if (tykit[valittuTykki, 4] == null)
            {
                torni.AmpumisAjastin = new Timer();
                torni.AmpumisAjastin.Interval = (double) tykit[valittuTykki, 2];
                torni.AmpumisAjastin.Timeout += delegate { torni.Ammu(); };
                torni.AmpumisAjastin.Start();
            }
            else
            {
                torni.Burst = new Timer();
                torni.Burst.Interval = (int) tykit[valittuTykki, 4];
                torni.Burst.Timeout += delegate { torni.BurstFire((double) tykit[valittuTykki, 2]); };
                torni.Burst.Start();
                torni.BurstFire((double) tykit[valittuTykki,
                    2]); // peli kaatuu jos tykin päivittää ennen kuin se on ampunut kertaakaan.
            }

            Add(torni, +1);
            raha.Value -= (int) tykit[valittuTykki, 0];

            //Luodaan ajastin joka kääntää tornin osoittamaan kohti kohdettaan.
            torni.KaantymisAjastin = new Timer();
            torni.KaantymisAjastin.Interval = 0.1;
            torni.KaantymisAjastin.Timeout += delegate { torni.KaannaTykki(); };
            torni.KaantymisAjastin.Start();
        }
    }

    /// <summary>
    /// Muuttuja, joka seuraa mikä tykki on valittuna.
    /// </summary>
    private int valittuTykki = 1;

    private GameObject valinta;

    public void ValitseTykki(int tykki)
    {
        valittuTykki = tykki - 1;
        valinta.X = Level.Left + valittuTykki * 10 + 10;
        valinta.Y = Level.Top + 20;
    }

    /// <summary>
    /// Näyttää valittavissa olevat tykit yläkulmassa.
    /// </summary>
    public void NaytaTykit()
    {
        //Käydään läpi kaikki tykit ja piirretään niitä vastaavat neliöt (myöhemmin tekstuurit)
        //Ruudun ylänurkkaan nättiin riviin
        int tykkeja = tykit.GetLength(0);

        valinta = NaytaValinta();

        for (int i = 0; i < tykkeja; i++)
        {
            GameObject nappula = new GameObject(10, 10, Shape.Rectangle);
            nappula.X = Level.Left + i * 10 + 10;
            nappula.Y = Level.Top + 20;
            nappula.Image = (Image) tykit[i, 3];
            nappula.Tag = "Nappula";
            Add(nappula, 3);
        }
    }

    /// <summary>
    /// Laittaa valitun tykin ympärille keltaisen neliön.
    /// </summary>
    /// <param name="valinta">Valittu tykki</param>
    public GameObject NaytaValinta()
    {
        GameObject valinta = new GameObject(10, 10, Shape.Rectangle);
        valinta.Color = Color.Yellow;
        Add(valinta, 2);
        return valinta;
    }

    /// <summary>
    /// Aliohjelma joka etsii annettua tykkiä lähinnä olevan vihollisen
    /// </summary>
    /// <returns>Lähin vihollinen</returns>
    /// <param name="Torni">Torni</param>
    public PhysicsObject EtsiVihollinen(Cannon torni)
    {
        PhysicsObject kohde = null;

        double lyhin = double.MaxValue;
        //Etsitään lähin vihollinen
        foreach (PhysicsObject vihu in GetObjectsWithTag("Vihollinen"))
        {
            double etaisyys = Vector.Distance(vihu.Position, torni.Position);

            if (etaisyys < lyhin)
            {
                lyhin = etaisyys;
                kohde = vihu;
            }
        }

        return kohde;
    }
}