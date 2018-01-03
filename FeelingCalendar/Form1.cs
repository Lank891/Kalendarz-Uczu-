using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FeelingCalendar
{

    public partial class Form1 : Form
    {
        //Tablica 372 dni
        private Day[] days = new Day[372];

        //Tablica z elementami menu wyboru kolorów - 10 elementów
        private PickMenu[] menu = new PickMenu[10];

        //Aktualnie wybrany element menu
        private int actualMenu;

        //Zmienna mówiąca, czy program zamyka się z zapisem (false) czy bez zapisu (true)
        private bool specialClose = false; 

        //Wydzielona funkcja wypełniająca menu, użyta tylko raz - dla przejrzystości kodu
        private void MakeMenu()
        {
            menu[0] = new PickMenu(60, 324, Color.White);
            menu[1] = new PickMenu(60, 354, Color.Yellow);
            menu[2] = new PickMenu(60, 384, Color.Violet);
            menu[3] = new PickMenu(60, 414, Color.SeaGreen);
            menu[4] = new PickMenu(60, 444, Color.Olive);
            menu[5] = new PickMenu(353, 324, Color.SpringGreen);
            menu[6] = new PickMenu(353, 354, Color.Blue);
            menu[7] = new PickMenu(353, 384, Color.Crimson);
            menu[8] = new PickMenu(353, 414, Color.Salmon);
            menu[9] = new PickMenu(353, 444, Color.SteelBlue);
        }

        //Błąd odczytu pliku - zwraca informację o tym czy został utworzony nowy plik, czy nie
        private bool ReadFault()
        {
            //Zapis informacji pobranej dialogiem
            DialogResult dr = MessageBox.Show("Plik kalendarza jest uszkodzony lub nie istnieje!\nCzy utworzyzć nowy plik?", "Błąd pliku kalendarza!", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);

            //Wykorzystanie informacji do zamknięcia programu lub stworzenia nowego pliku
            switch(dr)
            {
                case DialogResult.No:
                default:
                    {
                        //Zapis do specjalnej zmiennej, że wychodzimy bez zapisywanie
                        specialClose = true;
                        this.Close();
                        return false;
                    }
                case DialogResult.Yes:
                    {
                        try
                        {
                            //Otwarcie i wyczyszczenie pliku/utworzenie pliku
                            using (System.IO.StreamWriter sr = new System.IO.StreamWriter("Cal.cdc", false))
                            {
                                //Wpisanie do pliku 372 zer (pusty kalendarz)
                                for (int i = 0; i < 372; i++) sr.Write('0');
                                return true;
                            }
                        }
                        catch (Exception e)
                        {
                            //Jeżeli podczas tworzenia pliku napodka błąd, to program kończy działanie
                            System.Diagnostics.Debug.WriteLine("Błąd: " + e);
                            MessageBox.Show("Błąd podczas próby stworzenia nowego pliku kalendarza bądź nadpisywania uszkodzonego pliku!\nSprawdź, czy program może edytować pliki na dysku lub czy istniejący plik nie ma flagi \"Tylko do odczytu\"!", "Błąd tworzenia/edycji pliku!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            
                            //Zapis do specjalnej zmiennej, że wychodzimy bez zapisywanie
                            specialClose = true;
                            this.Close();
                            return false;
                        }
                        
                    }
            }

        }

        //Zapisane do pliku, jeżeli nastąpiły zmiany w kalendarzu, zwracany typ potrzebny jest jedynie do przerwania zamykania programu (wtedy zwraca true)
        private bool Save(bool amIExiting)
        {
            //Zmienna na stary plik
            String oldFile;
            //Zmienna na nowy plik
            String newFile = "";

            //Zapisanie treści nowego pliku
            foreach (Day day in days)
            {
                newFile += day.SaveType();
            }

            //Próba odczytania starego pliku
            try
            { 
                using (System.IO.StreamReader sr = new System.IO.StreamReader("Cal.cdc"))
                {
                    //Zapis pliku do Stringa i zamknięcie pliku
                    oldFile = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                //Jeżeli odczytanie się nie powiedzie, to traktuje sstary plik jako pusty
                System.Diagnostics.Debug.WriteLine("Błąd: " + e);
                oldFile = "";
            }

            //Jeżeli stary plik i potencjalnie nowy są takie same, to zwraca prawdę - nie było czego zapisywać
            if (oldFile.Equals(newFile))
            {
                //Jeżeli używamy przycisku, a nie wychodzenia, to pokazuje się informacja, że zapisana jest aktualna wersja już
                if (amIExiting == false) MessageBox.Show("Aktualna wersja jest już zapisana.", "Zapis", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                return false;
            }

            //Upewnienie się, czy plik ma zostać zapisany
            DialogResult doSave = MessageBox.Show("Zapisać aktualny kalendarz?", "Zapis kalendarza", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

            switch (doSave)
            {
                case DialogResult.No:
                default:
                    {
                        //Nie zapisujemy kalendarza - zwracamy fałsz
                        return false;
                    }
                case DialogResult.Yes:
                    {
                        //Próba otworzenia pliku do zapisu z usunięciem danych
                        try
                        {
                            using (System.IO.StreamWriter sr = new System.IO.StreamWriter("Cal.cdc", false))
                            {
                                //Zapis do pliku i potwierdzające zapisanie zakończzenie funkcji
                                sr.Write(newFile);
                                sr.Close();
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            //Błąd zapisu pliku wraz z pytaniem czy wyjść z aplikacji
                            System.Diagnostics.Debug.WriteLine("Błąd: " + e);
                            DialogResult fault = MessageBox.Show("Błąd podczas próby stworzenia nowego pliku kalendarza bądź nadpisywania uszkodzonego pliku!\nSprawdź, czy program może edytować pliki na dysku lub czy istniejący plik nie ma flagi \"Tylko do odczytu\"!\nWciśnij \"Tak\" aby wyjść z programu bez zapisywania zmian, albo \"Nie\", aby pozostawić program włączony.", "Błąd tworzenia/edycji pliku!", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);

                            switch(fault)
                            {
                                case DialogResult.No:
                                default:
                                    {
                                        //Dla odmowy wyjścia z programu anuluje funkcję zapisu
                                        return true;
                                    }
                                case DialogResult.Yes:
                                    {
                                        //Jeżeli wychodzimy z programu, to zwróć prawdę, aby kontynuować zamykanie
                                        if (amIExiting == true) return false;

                                        //Jeżeli nie wychodzimy z programu nadaj specjalnej zmiennej informację o tym, że program zamyka się bez próby zaposu zamknij program i zwróc fałsz
                                        specialClose = true;
                                        this.Close();
                                        return false;
                                    }
                            }
                        }
                    }
            }
        }

        //Nadpisanie funkcji zamykania zapisujące dodatkowo kalendarz
        protected override void OnClosing(CancelEventArgs e)
        {
            //Jeżeli kalendarz ma się zapisać
            if(specialClose == false)
            {
                //Jeżeli próba zapisu dała true, to program przestaje być zamykany
                if (Save(true)) e.Cancel = true;
            }

            //Kontynuowanie wyjścia
            base.OnClosing(e);
        }

        //Ładowanie z pliku i tworzenie kalendarza, zwraca false jeżeli tworzenie nie zostało ukończone i należy powtózyć tworzenie
        private bool CreateCallendar()
        {
            //Otworzenie pliku i wczytwanie dni
            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader("Cal.cdc"))
                {
                    //Zapis pliku do Stringa i zamknięcie pliku
                    String file = sr.ReadToEnd();
                    sr.Close();

                    //Sprawdzenie, czy plik miał odpowiednią ilość znaków
                    if (file.Length < 372)
                    {
                        if (ReadFault() == false) return true;
                        else return false;
                    }

                    //Stworzenie w niej dni
                    for (int i = 0; i < 12; i++)
                    {
                        for (int j = 0; j < 31; j++)
                        {
                            char sign = file.ElementAt(j + 31 * i);
                            days[j + 31 * i] = new Day(38 + 23 * j, 38 + 23 * i);
                            days[j + 31 * i].ChangeType(sign);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Błąd: " + e);
                if (ReadFault() == false) return true;
                else return false;
            }
        }
        public Form1()
        {
            //Próbuje stworzyć kalendarz aż się powiedzie, albo aplikacja się zamknie
            bool checkNewCallendar = false;
            while (checkNewCallendar == false) checkNewCallendar = CreateCallendar();

            //Tworzenie menu i wybranie domyślnej opcji
            MakeMenu();
            actualMenu = 0;

            //Inne komponenty
            InitializeComponent();
        }

        //Przeciążenie rysowania
        protected override void OnPaint(PaintEventArgs e)
        {
            //Rysowanie tła siatki dni
            SolidBrush pen = new SolidBrush(Color.Black);
            Rectangle rect = new Rectangle(35, 35, 716, 279);
            e.Graphics.FillRectangle(pen, rect);

            //Rysowanie dni
            foreach(Day day in days)
            {
                day.Draw(e);
            }

            //Rysowanie kwadratu pod wybranym elementem menu
            Rectangle choosenRect = menu[actualMenu].GetRect();
            Rectangle borderRect = new Rectangle(choosenRect.Left - 3, choosenRect.Top - 3, 26, 26);
            //Pędzel z odpowiednim kolorem na obwódkę
            SolidBrush borderPen = new SolidBrush(Color.SlateGray);
            //Rysowanie obwódkowego kwadratu pod elementami menu
            e.Graphics.FillRectangle(borderPen, borderRect);

            //Rysowanie menu
            foreach (PickMenu pm in menu)
            {
                pm.Draw(e);
            }
        }

        //Naciśnięcie na przycisk zapisu dnia
        private void button1_Click(object sender, EventArgs e)
        {
            //Funckja zapisu z informacją, że nie jest to zapis podczas wychodzenia z programu
            Save(false);
        }

        //Przycisk resetu kalendarza - w prosty sposób tworzy nowe, puste (type = '0') dni
        private void button2_Click(object sender, EventArgs e)
        {

            //Pytanie czy na pewno chce się zresetować zmiany
            DialogResult dr = MessageBox.Show("Czy na pewno chcesz zresetować cały kalendarz?\nZmiana jest odwracalna jedynie poprzez restart programu bez zapisywania zmian!", "Potwierdź chęć wyczyszczenia kalendarza", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            switch(dr)
            {
                case DialogResult.No:
                default:
                    {
                        break;
                    }
                case DialogResult.Yes:
                    {
                        //Tworzy zamiast dni nowe dni, o domyślnym typie '0' (pusty)
                        for (int i = 0; i < 12; i++)
                        {
                            for (int j = 0; j < 31; j++)
                            {
                                days[j + 31 * i] = new Day(38 + 23 * j, 38 + 23 * i);
                            }
                        }

                        //Następnie aktualizuje zmiany
                        this.Invalidate(true);
                        break;
                    }
            }
        }

        //Nadpisanie funkcji kliknięcia myszki
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            //Działamy tylko lewym przyciskiem myszy, więc kliknięcie innym zwracamy od razu
            if (e.Button != MouseButtons.Left) return;

            //Sprawdzanie, czy przypadkiem nie kliknięto w kwadracik z menu 
            for (int i = 0; i < 10; i++)
            {
                //Jeżeli sprawdzany kwadrat będzie wybranym to ustawiamy na niego aktywne menu, rysujemy od nowa i wychodzimy
                if(menu[i].GetRect().Contains(e.Location))
                {
                    actualMenu = i;
                    this.Invalidate(true);
                    return;
                }
            }

            //Jezeli nie trafiliśmy w menu, to sprawdźmy dni i ew. zamalujmy odpowiedni, przerysujmy od nowa i wyjdźmy z funkcji
            foreach (Day day in days)
            {
                if (day.GetRect().Contains(e.Location))
                {
                    day.ChangeType((char)(actualMenu + 48));
                    System.Diagnostics.Debug.WriteLine((char)(actualMenu + 48));
                    this.Invalidate(true);
                    return;
                }
            }
        }
    }

    //Klasa dnia
    class Day
    {
        //Pędzel (kolor), kwadrat, typ dnia jako char
        private SolidBrush pen;
        private Rectangle rect;
        private char type;

        //Zdobycie danych do zapisu - zwraca typ
        public char SaveType() { return type; }

        //Zdobycie kwadratu
        public Rectangle GetRect() { return rect; }

        //Konstruktor - położenie kwadratu i jego typ
        public Day(int x, int y)
        {
            type = '0';
            rect = new Rectangle(x, y, 20, 20);
            ChangePen();
        }

        //Zmiana typu dnia
        public void ChangeType(char sign)
        {
            type = sign;
            ChangePen();
        }

        //Change pen - zmienia kolor pędzla, używane po zmianach typu
        private void ChangePen()
        {
            //Domyślnie kolor biały - pusty
            Color color = Color.White;
            //W zależności od typu ustawia kolor
            switch (type)
            {
                case '0':
                    {
                        color = Color.White; //Pusty
                        break;
                    }
                case '1':
                    {
                        color = Color.Yellow; //Fantastyczny
                        break;
                    }
                case '2':
                    {
                        color = Color.Violet; //Szczęśliwy
                        break;
                    }
                case '3':
                    {
                        color = Color.SeaGreen; //Zwykły
                        break;
                    }
                case '4':
                    {
                        color = Color.Olive; //Męczący
                        break;
                    }
                case '5':
                    {
                        color = Color.SpringGreen; //Smutny
                        break;
                    }
                case '6':
                    {
                        color = Color.Blue; //Tragiczny
                        break;
                    }
                case '7':
                    {
                        color = Color.Crimson; //Denerwujący
                        break;
                    }
                case '8':
                    {
                        color = Color.Salmon; //Stresujący
                        break;
                    }
                case '9':
                    {
                        color = Color.SteelBlue; //Leniwy
                        break;
                    }
                default:
                    {
                        color = Color.White; //Gdyby coś było inaczej to pusty (biały)
                        break;
                    }
            }
            //Przypisuje pędzlowi kolor
            pen = new SolidBrush(color);
        }

        //Rysownia kwadratu pędzlem
        public void Draw(PaintEventArgs e)
        {
            //Narysowanie pędzlem prostokąta
            e.Graphics.FillRectangle(pen, rect);
        }
    }

    //Klasa ikony menu
    class PickMenu
    {
        //Pędzle do rysowania
        private SolidBrush colorPen;
        private SolidBrush blackPen;
        //Kwadraty
        private Rectangle colorRect;
        private Rectangle blacRect;

        //Zddobycie większego kwadratu
        public Rectangle GetRect() { return blacRect; }

        //Konstrutkor - współrzędne dużego kwadratu (obwódki) i kolor środka
        public PickMenu(int x, int y, Color col)
        {
            blacRect = new Rectangle(x, y, 20, 20);
            colorRect = new Rectangle(x + 2, y + 2, 16, 16);

            blackPen = new SolidBrush(Color.Black);
            colorPen = new SolidBrush(col);
        }

        //Rysowanie całego tworu
        public void Draw(PaintEventArgs e)
        {
            //Najpierw czarny, a potem na nim kolorowy
            e.Graphics.FillRectangle(blackPen, blacRect);
            e.Graphics.FillRectangle(colorPen, colorRect);
        }
    }
}
