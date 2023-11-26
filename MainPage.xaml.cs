using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MEMO_Beadando_2023
{
    public class Kartya   //kártya osztály
    { 
        // propertik, osztály tulajdonságai
        public string Elolap { get; set; }  // kártya előlapja képei
        public string Hatlap { get; set; } = "hatlap7.png"; // hátlap
        public bool Fordul { get; set; }    // felvan-e fordulva a kártya
        public bool Passzol { get; set; }   // passzolnak-e a kártyák
        public int Id { get; set; } // sorszám alapján való párosítás   
    }

    public partial class MainPage : ContentPage
    {
        private const int Sorok = 7; // const=constans állandó az értéke
        private const int Oszlopok = 4;
        private List<Kartya> kartyalista = new List<Kartya>(); //üres konstruktor hívás a példányosításhoz
        private Random veletlen = new Random();
        private Kartya elsofordulas = null; // eltároljuk az első felfordtott kártyát
        
        // Időzítő beállítása
        private TimeSpan ido;
        private bool jatekfutas;
        private bool idozitofutasa = false;




        public MainPage()
        {          
            InitializeComponent();
            UjjatekClick(this, EventArgs.Empty); // Amikor belépünk ajátékra ne egy üres felület legyen, hanem tegye fel akártyákat

        }

        // Szenkvencia miatt kell ide a függvények a UjjatekClickből



        private void Jatektabla() // Az új játék gomb miatt törlünk
        {
            kartyalista.Clear();

            //feltöltjük (új menet)

            for (int i = 0; i < Sorok * Oszlopok / 2; i++) // 14 pár
            {
                var tarolo = new Kartya
                {
                    Elolap = $"kartya{ i + 1 }.png",
                    Id=i
                };

                // kartyalist feltöltése

                kartyalista.Add(tarolo);
                kartyalista.Add(new Kartya { Elolap = tarolo.Elolap, Id = tarolo.Id }); // kétszer adok hozzá egy kártyát, hogy meglegyen a pár
            }
        }

    // Keverés

        private void Keveres()
        {
            int n = kartyalista.Count;

            //Netről van Fisher-Yates féle keverő algoritmus

            while (n > 1) // Addig tart a ciklus amíg van elem alistába
            {
                n--; //csökkenteni az n-t 1-gyel: hogy a ciklus véget érjen
                int k = veletlen.Next(n + 1);
                // kártyacsere

                var ertek = kartyalista[k];
                kartyalista[k] = kartyalista[n];
                kartyalista[n] = ertek;
            }
        }



    // Kártyák tölése a tábláról
        
        private void ClearGrid()
        {
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();
        }





    //Felhelyezni újra a sorokat oszlopokat

        private void Ujrarakas()
        {
            // sorok feltöltése szám szerint (7 db)

            for (int i = 0; i < Sorok; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }


            // oszlpok feltöltése szám szerint (4 db)

            for (int i = 0; i < Oszlopok; i++)
            {
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
        }



    // Kártyák felhelyezése sorokra oszlopokra

        private void Kartyakfelrakasa()
        {
            for (int index = 0; index < kartyalista.Count; index++)
            {
                var tarolo = kartyalista[index];
                var keptarolo = new Image
                {
                    Source = tarolo.Hatlap, // Miden egyes kép forrása a kártya hátlapja lesz
                    Aspect = Aspect.AspectFit, // Keret kitöltése
                    ClassId = index.ToString() // A feltöltött képek ID-ját beállítjuk a az aktuális index ID-jára.
                };

                var erintesfelismero = new TapGestureRecognizer(); // Érintés beálltása
                erintesfelismero.Tapped += Erintesfigyelo;  //Nézi, hogy ez az esemény megvalósult-e érintésre
                                                            // += operátor segíségével az Erintesfigyelo függvényt feliratkoztstjuk az erintesfelismero.Tapped tulajdonságára (property)
                keptarolo.GestureRecognizers.Add(erintesfelismero);

                // A sor és oszlop pozíciókat a kártya indexe alapján kiszámol
                int sor = index / Oszlopok; // Index osztva az oszlopok száma
                int oszlop = index % Oszlopok; // Maradékos osztás az oszlopokon

                GameGrid.Children.Add(keptarolo, oszlop, sor); // hozzáadjuk a kártyát a Grid-hez
            }
        }




     // Időzítő beállítása

        private void StartTimer()
        {
            ido = TimeSpan.Zero; // 0-ról indul
            Timerlabel.Text = "00:00";
            jatekfutas = true;

            if (!idozitofutasa) // ha nem fut az időzítő
            {
                idozitofutasa = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), TimerCallBack); // 1 másodpercről indul
            }
        }


        private bool TimerCallBack()
        {
            if (!jatekfutas)    // ha nem fut a játék
            {
                idozitofutasa = false; // az időzítő nem fut tovább
                return false;
            }

            //Idő múlásának beállítása
            ido = ido.Add(TimeSpan.FromSeconds(1));
            Timerlabel.Text = ido.ToString(@"mm\:ss");
            return true;
        }




        private void UjjatekClick(object sender, EventArgs e) // új játék gomb
        {
            // Időzítőt megállítva lesz

            jatekfutas = false;

            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>  //Időzítő indításának függvénye 100 ms késleltetéssel
            {
                Jatektabla(); // Játék tábla felépítése
                Keveres(); // Kártyák keverése
                ClearGrid(); // Kártyák tölése a tábláról
                Ujrarakas(); //Felhelyezni újra a sorokat oszlopokat
                Kartyakfelrakasa(); // Kártyák felhelyezése sorokra oszlopokra
                StartTimer(); // rekúrzió
                return false;

            });      
        }

    // A kártyák megfordításához animáció beállítása

        private async Task Kartyaforditas(Image keptarolo, Kartya kartya) //async = több mindent tud egyszerre futatttani
        {
            await keptarolo.RotateTo(360, 100); //Elfordul akép 360 fokkal
            keptarolo.Source = kartya.Fordul ? kartya.Hatlap : kartya.Elolap; // Ha a kártya fel van fordulva, akkor a hátlap van, ha nincs aor az előlapot
            // Source = a képnek a forrása hátlapot vagy előlapot mutat
            await keptarolo.RotateTo(360, 100); // Visszafordul 360 fokkal
            kartya.Fordul = !kartya.Fordul; // visszafordítottnak állít a Fordul propertyt    


            //await keptarolo.ScaleTo(0,100); // 0=0% (0 és 1 közötti szám)
            //await keptarolo.ScaleTo(1,100); // 1=100%
        }


    // Maga a játék menete

        private async void Erintesfigyelo(object sender, EventArgs e)
        {
            /*
             Ha az első kártya megfordul, a kártya adatai elmentődnek az elsofordulas változóba,
             majd a 2. kártya fordulása után ellenőrzésre kerül, hogy megegyezik-e a két kártya.
             Ha igen, akkor a két kártya Passzol tulajdonság true-ra vált és a két kártya eltűnik.
             Ha nem eggyeznek, akkor a két kártya visszafordul egy rövid idő után és folytatódik a játék.           
             */

            var felforditottkep = (Image)sender;

            int kepindex = int.Parse(felforditottkep.ClassId); // A ClassId property-ből lekéri a kép indexét
            var felforditottkartya = kartyalista[kepindex]; // Az aktuális indexen lévő kártya

            if (felforditottkartya.Passzol || felforditottkartya.Fordul) // Levédem a kártyát, hogy ne lehessen többször forgatni, ha már fel van fordítva.
            {
                return;
            }

            await Kartyaforditas(felforditottkep, felforditottkartya); // kártyafordító függvényt meghívjuk

            if (elsofordulas == null)
            {
                elsofordulas = felforditottkartya;
            }

            else
            {
                if (elsofordulas.Id == felforditottkartya.Id) // egyeezne-e a kártyák?
                {
                    elsofordulas.Passzol = true;
                    felforditottkartya.Passzol = true;

                    var elsoforditottkep = GameGrid.Children.Cast<Image>().FirstOrDefault(img => img.ClassId == kartyalista.IndexOf(elsofordulas).ToString()); //var = nem lehet tudni mi a tipusa ez futás közben derül ki
                    var masodikforditottkep = felforditottkep;

                    if (elsoforditottkep != null)
                        elsoforditottkep.IsVisible = false; // eltűnik az első felfordított kép
                    if (masodikforditottkep != null)
                    {
                        masodikforditottkep.IsVisible = false; // eltűnik vele együtt a 2. felfordított kép                 
                    }

                    GameOver();
                }

                else // ha nincs pár
                {
                    await Task.Delay(500); //fél msp késleltetés
                    await Kartyaforditas(Kepkereso(elsofordulas), elsofordulas); // Visszafordul az első kártya
                    await Kartyaforditas(felforditottkep, felforditottkartya); // majd a 2. kártya is
                }

                    elsofordulas = null; // mive nem volt pár ezért nincs első fordulás semmi, minden visszafordul
            }
        }

    
        private Image Kepkereso(Kartya kartya)
        {
            int kepindex = kartyalista.IndexOf(kartya); //A kártyalista aktuális kártya indexének beállítása
            return GameGrid.Children.Cast<Image>().FirstOrDefault(img => img.ClassId == kepindex.ToString());
            // Az első találti érték helyét hasonlítja össze a képnek az indexével helyével és ezt az értéket adja vissza.
        }

        private void GameOver()
        {
            if (kartyalista.All( ideiglenesvaltozo => ideiglenesvaltozo.Passzol))
            {
                jatekfutas = false;
                DisplayAlert("Győzelem!", "Megtaláltad az összes kártyát!", "OK");
            }
        }

    // Kilépés a játékból
        private void ExitClick(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().CloseMainWindow();
        }
    }
}
