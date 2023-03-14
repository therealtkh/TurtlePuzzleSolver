using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace Turtles
{
    public partial class Turtles : Form
    {
        //int tests = 0;            // Count test calls, for debug mostly
        readonly int fieldSize = 4; // For this puzzle, field is fixed 4x4 but for debug this could be changed (to smaller value)
        List<CardGraphics> graphics = new List<CardGraphics>(); // The graphics and positions in the picturebox for unused cards
        List<string[]> sequences = new List<string[]>();    // Store all sequences as arrays of strings instead of strings to avoid having to split them later
        List<int[]> rotations = new List<int[]>();          // Strongly connected to sequences and stores rotation of each card for each sequence
        int incompleteCombinations = 0;       // Partly debug, partly fun. Amount of incomplete chains tested
        BackgroundWorker _backgroundWorker;
        bool abort = false;         // If user press the Stop button in UI, this is used in background worker thread to cancel work
        bool holdThread = false;    // Set when pausing backgroundworker until UI task has completed (like drawing the field)
        int currentSolution = -1;   // When drawing solutions, keep track of which one we're showing, setting to -1 is mostly for debug
        public string result { get; set; }      // Used to store result in Carls algorithm

        public Turtles()
        {
            InitializeComponent();
            InitGraphics();     // Prepare our cards, could be done every time we use but as we read from file I only wanted to do it once

            _backgroundWorker = new BackgroundWorker();             // Background worker that will run in its own thread
            _backgroundWorker.WorkerReportsProgress = true;         // Allow us to send info from worker thread to UI thread
            _backgroundWorker.WorkerSupportsCancellation = true;    // Allow us to indicate if we have cancelled the operation
            _backgroundWorker.DoWork += RunBackgroundWorker;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;

            pb_1.BackgroundImage = new Bitmap("grid_big.png");      // Make sure to have file in same folder
            Bitmap bmp = new Bitmap(pb_1.Size.Width, pb_1.Size.Height);      // Prepare the field
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);             // picturebox already has a background image with grid/outlines
            foreach (CardGraphics cg in graphics)
            {
                g.DrawImage(cg.Image, cg.UnusedPosX, cg.UnusedPosY);    // Draw all cards in respective "base" position
            }
            pb_1.Image = bmp;

            tb_1.Text = "Welcome to the Turtle Puzzle Solver by Jonas O!" + System.Environment.NewLine;

            //SolveAll();   //Moved to background thread instead, not suitable to run from here anymore
            //CallesVersion();
        }

        // Is there a better or smarter way to move cursor to the bottom of TextBox? 
        private void CursorToBottom()
        {
            tb_1.SelectionStart = tb_1.Text.Length;
            tb_1.SelectionLength = 0;
            tb_1.ScrollToCaret();
        }

        // Create the deck of cards (put them in a List). More compact than having in the code...
        private List<Card> InitCards()
        {
            List<Card> deck = new List<Card>();
            deck.Add(new Card(00, TurtleColor.Red,    TurtleColor.Yellow, TurtleColor.Yellow, TurtleColor.Green));
            deck.Add(new Card(01, TurtleColor.Green,  TurtleColor.Red,    TurtleColor.Green,  TurtleColor.Yellow));
            deck.Add(new Card(02, TurtleColor.Green,  TurtleColor.Red,    TurtleColor.Yellow, TurtleColor.Green));
            deck.Add(new Card(03, TurtleColor.Red,    TurtleColor.Yellow, TurtleColor.Green,  TurtleColor.Blue));
            deck.Add(new Card(04, TurtleColor.Yellow, TurtleColor.Blue,   TurtleColor.Blue,   TurtleColor.Yellow));
            deck.Add(new Card(05, TurtleColor.Red,    TurtleColor.Blue,   TurtleColor.Yellow, TurtleColor.Orange));
            deck.Add(new Card(06, TurtleColor.Red,    TurtleColor.Blue,   TurtleColor.Orange, TurtleColor.Green));
            deck.Add(new Card(07, TurtleColor.Yellow, TurtleColor.Blue,   TurtleColor.Green,  TurtleColor.Red));
            deck.Add(new Card(08, TurtleColor.Blue,   TurtleColor.Green,  TurtleColor.Yellow, TurtleColor.Green));
            deck.Add(new Card(09, TurtleColor.Blue,   TurtleColor.Red,    TurtleColor.Green,  TurtleColor.Green));
            deck.Add(new Card(10, TurtleColor.Blue,   TurtleColor.Red,    TurtleColor.Green,  TurtleColor.Yellow));
            deck.Add(new Card(11, TurtleColor.Blue,   TurtleColor.Green,  TurtleColor.Yellow, TurtleColor.Red));
            deck.Add(new Card(12, TurtleColor.Green,  TurtleColor.Red,    TurtleColor.Blue,   TurtleColor.Yellow));
            deck.Add(new Card(13, TurtleColor.Red,    TurtleColor.Blue,   TurtleColor.Yellow, TurtleColor.Green));
            deck.Add(new Card(14, TurtleColor.Red,    TurtleColor.Blue,   TurtleColor.Green,  TurtleColor.Yellow));
            deck.Add(new Card(15, TurtleColor.Green,  TurtleColor.Red,    TurtleColor.Yellow, TurtleColor.Blue));
            return deck;
        }

        // Create the deck of graphics (a List of objects with graphics and positions). 
        private void InitGraphics()
        {
            int tileSize = 81;      // 81 pixels per card side, directly connected to the files with graphics
            Bitmap inputBitmap = new Bitmap("cards_big_all.png");   // Make sure to have file in same folder
            System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.DontCare;    // Unclear how important PixelFormat is...
            // Original image has a black grid, therefore the coordinates add 1,2,3,4 to the starting points
            graphics.Add(new CardGraphics(00, inputBitmap.Clone(new Rectangle(0 * tileSize + 1, 0 * tileSize + 1, tileSize, tileSize), format), 1,   103));
            graphics.Add(new CardGraphics(01, inputBitmap.Clone(new Rectangle(1 * tileSize + 2, 0 * tileSize + 1, tileSize, tileSize), format), 1,   1));
            graphics.Add(new CardGraphics(02, inputBitmap.Clone(new Rectangle(2 * tileSize + 3, 0 * tileSize + 1, tileSize, tileSize), format), 103, 1));
            graphics.Add(new CardGraphics(03, inputBitmap.Clone(new Rectangle(3 * tileSize + 4, 0 * tileSize + 1, tileSize, tileSize), format), 205, 1));
            graphics.Add(new CardGraphics(04, inputBitmap.Clone(new Rectangle(0 * tileSize + 1, 1 * tileSize + 2, tileSize, tileSize), format), 307, 1));
            graphics.Add(new CardGraphics(05, inputBitmap.Clone(new Rectangle(1 * tileSize + 2, 1 * tileSize + 2, tileSize, tileSize), format), 409, 1));
            graphics.Add(new CardGraphics(06, inputBitmap.Clone(new Rectangle(2 * tileSize + 3, 1 * tileSize + 2, tileSize, tileSize), format), 511, 1));
            graphics.Add(new CardGraphics(07, inputBitmap.Clone(new Rectangle(3 * tileSize + 4, 1 * tileSize + 2, tileSize, tileSize), format), 511, 103));
            graphics.Add(new CardGraphics(08, inputBitmap.Clone(new Rectangle(0 * tileSize + 1, 2 * tileSize + 3, tileSize, tileSize), format), 511, 349));
            graphics.Add(new CardGraphics(09, inputBitmap.Clone(new Rectangle(1 * tileSize + 2, 2 * tileSize + 3, tileSize, tileSize), format), 511, 451));
            graphics.Add(new CardGraphics(10, inputBitmap.Clone(new Rectangle(2 * tileSize + 3, 2 * tileSize + 3, tileSize, tileSize), format), 409, 451));
            graphics.Add(new CardGraphics(11, inputBitmap.Clone(new Rectangle(3 * tileSize + 4, 2 * tileSize + 3, tileSize, tileSize), format), 307, 451));
            graphics.Add(new CardGraphics(12, inputBitmap.Clone(new Rectangle(0 * tileSize + 1, 3 * tileSize + 4, tileSize, tileSize), format), 205, 451));
            graphics.Add(new CardGraphics(13, inputBitmap.Clone(new Rectangle(1 * tileSize + 2, 3 * tileSize + 4, tileSize, tileSize), format), 103, 451));
            graphics.Add(new CardGraphics(14, inputBitmap.Clone(new Rectangle(2 * tileSize + 3, 3 * tileSize + 4, tileSize, tileSize), format), 1,   451));
            graphics.Add(new CardGraphics(15, inputBitmap.Clone(new Rectangle(3 * tileSize + 4, 3 * tileSize + 4, tileSize, tileSize), format), 1,   349));
        }

        // Kick off the algorithm. It used to be longer... remaining code could be moved to the place where we start it from
        private void SolveAll()
        {
            List<Card> deck = new List<Card>(InitCards());      // Prepare deck
            Card[,] playfield = new Card[fieldSize, fieldSize]; // Prepare field (slots will be null)
            TestNextCard(deck, playfield);                      // Change here to use the current or old version
        }
                
        // Recursive function to place card, return true if card could be placed?
        private void TestNextCard(List<Card> deckSelection, Card[,] field)
        {
            if (abort)
                return;

            //tests++;    // For debug - or for fun

            for (int y = 0; y < fieldSize; y++)         // We will count through this every time we call the function 
            {                                           // even though some slots are already located. Unnesessary???
                for (int x = 0; x < fieldSize; x++)     // Well, yes. But it's a very small grid, so it's quite fast.
                {                                       // Could be solved if we send to the function where we came from...
                    if (field[y, x] == null)            // First occurance of a slot where we can place a card
                    {
                        for (int i = 0; i < 4; i++)     // 4 as in four rotations of the card
                        {
                            List<Card> subSelection = new List<Card>();

                            if (y == 0 && x == 0)   // First slot will have all cards to chose from
                            {
                                subSelection = new List<Card>(deckSelection);
                            }
                            else if (y == 0)   // Top row only checks left
                            {
                                subSelection = (from cardsLeft in deckSelection
                                                where cardsLeft.LeftColor == field[y, x - 1].RightColor
                                                where (cardsLeft.LeftPart ^ field[y, x - 1].RightPart) == 1
                                                orderby cardsLeft.ID
                                                select cardsLeft).ToList();
                            }
                            else if (x == 0)    // First time this will occur is at y = 1, and then only once per row
                            {
                                subSelection = (from cardsLeft in deckSelection
                                                where cardsLeft.TopColor == field[y - 1, x].BottomColor
                                                where (cardsLeft.TopPart ^ field[y - 1, x].BottomPart) == 1
                                                orderby cardsLeft.ID
                                                select cardsLeft).ToList();
                            }
                            else                // Where we check both top and left
                            {
                                subSelection = (from cardsLeft in deckSelection
                                                where cardsLeft.TopColor == field[y - 1, x].BottomColor
                                                where (cardsLeft.TopPart ^ field[y - 1, x].BottomPart) == 1
                                                where cardsLeft.LeftColor == field[y, x - 1].RightColor
                                                where (cardsLeft.LeftPart ^ field[y, x - 1].RightPart) == 1
                                                orderby cardsLeft.ID
                                                select cardsLeft).ToList();
                            }
                            
                            if (subSelection.Count > 0)
                            {
                                for (int j = 0; j < subSelection.Count; j++)    // Cycle through all matching cards
                                {
                                    field[y, x] = subSelection[j];      // Place the card on the field (where we had Card == null)
                                    
                                    if (y == (fieldSize - 1) && x == (fieldSize - 1)) // Should not need to check if field[y, x] != null
                                    {   // We have a full board, wohoo!!!!
                                        SaveSequence(field);
                                        holdThread = true;
                                        _backgroundWorker.ReportProgress(2, field); // Write board to log
                                        while (holdThread) { } 
                                    }
                                    else
                                    {
                                        List<Card> newDeckSelection = new List<Card>();     // The deck for next iteration
                                        foreach (Card c in deckSelection)   // All the cards we had left in deck
                                        {
                                            if (c != subSelection[j])       // Add all cards but the one we just placed
                                                newDeckSelection.Add(c);    // Can be done with making a copy of the deck and .Remove(c) as well
                                        }
                                        Card[,] branchedField = field.Clone() as Card[,];   // Make a copy of our field to branch with
                                        TestNextCard(newDeckSelection, branchedField);
                                    }
                                }
                            }
                            else    // There are no matching cards left to try
                            {
                                incompleteCombinations++;   
                                if (incompleteCombinations % 10000 == 0 && incompleteCombinations != 0)
                                {
                                    holdThread = true;
                                    _backgroundWorker.ReportProgress(1);    // 1 means write amount of incomplete combinations we have tested
                                    while (holdThread) { }
                                }
                            }

                            foreach (Card c in deckSelection)
                            {
                                c.CardRotate();
                            }
                        }
                        
                        if (incompleteCombinations % 200 == 0)             // Don't draw every card we place
                        {                                       // ...only every so often complete chain
                            holdThread = true;                  // Flag to pause thread while we report... 
                            _backgroundWorker.ReportProgress(0, field);  // 0 means "draw board"
                            while (holdThread) { }              // Wait for draw to finish. Add Thread.Sleep(50); after report to see what's going on
                        }

                        return;     // We will actualy never place two cards after each other, the function is called again instead.
                    }
                }
            }
        }

        // Fully functional algorithm before starting with rotation!
        private void TestNextCardOld(List<Card> deckSelection, Card[,] field)
        {
            if (abort)
                return;

            //tests++;    // For debug - or for fun

            for (int y = 0; y < fieldSize; y++)         // We will count through this every time we call the function 
            {                                           // even though some slots are already located. Unnesessary???
                for (int x = 0; x < fieldSize; x++)     // Well, yes. But it's a very small grid, so it's quite fast.
                {
                    if (field[y, x] == null) // First occurance of a slot where we can place a card
                    {
                        List<Card> subSelection = new List<Card>();
                        if (y == 0 && x == 0)   // First slot will have all cards to chose from
                        {
                            subSelection = new List<Card>(deckSelection);
                        }
                        else if (y == 0)   // Top row only checks left
                        {
                            subSelection = (from cardsLeft in deckSelection
                                            where cardsLeft.LeftColor == field[y, x - 1].RightColor
                                            orderby cardsLeft.ID
                                            select cardsLeft).ToList();
                        }
                        else if (x == 0)    // First time this will occur is at y = 1, and then only once per row
                        {
                            subSelection = (from cardsLeft in deckSelection
                                            where cardsLeft.TopColor == field[y - 1, x].BottomColor
                                            orderby cardsLeft.ID
                                            select cardsLeft).ToList();
                        }
                        else                // Where we check both top and left
                        {
                            subSelection = (from cardsLeft in deckSelection
                                            where cardsLeft.TopColor == field[y - 1, x].BottomColor
                                            where cardsLeft.LeftColor == field[y, x - 1].RightColor
                                            orderby cardsLeft.ID
                                            select cardsLeft).ToList();
                        }
                        
                        if (subSelection.Count > 0)
                        {
                            for (int i = 0; i < subSelection.Count; i++)
                            {
                                field[y, x] = subSelection[i];      // Place the card on the field
                                if (y == fieldSize - 1 && x == fieldSize - 1) // Should not need to check if field[y, x] != null
                                {   // We have a full board, wohoo!!!!
                                    SaveSequence(field);
                                    holdThread = true;
                                    _backgroundWorker.ReportProgress(2, field); // Write board to log
                                    while (holdThread) { }
                                }
                                else
                                {
                                    List<Card> newDeckSelection = new List<Card>();
                                    foreach (Card c in deckSelection)
                                    {
                                        if (c != subSelection[i])
                                            newDeckSelection.Add(c);
                                    }
                                    Card[,] branchedField = field.Clone() as Card[,];
                                    TestNextCardOld(newDeckSelection, branchedField);
                                }
                            }
                        }
                        else    // There are no matching cards left to try
                        {
                            incompleteCombinations++;
                            if (incompleteCombinations % 1000 == 0 && incompleteCombinations != 0)
                            {
                                holdThread = true;
                                _backgroundWorker.ReportProgress(1);    // 1 means write amount of incomplete combinations we have tested
                                while (holdThread) { }
                            }
                        }

                        if (incompleteCombinations % 50 == 0)             // Don't draw every card we place
                        {                                       // ...only every so often complete chain
                            holdThread = true;                  // Flag to pause thread while we report... 
                            _backgroundWorker.ReportProgress(0, field);  // 0 means "draw board"
                            while (holdThread) { }              // Wait for draw to finish. Add Thread.Sleep(50); after report to see what's going on
                        }
                        return;     // We will actualy never place two cards after each other, the function is called again instead.
                    }
                }
            }
        }

        // Debug function to print all cards in provided List. To show the entire selection of cards to choose from, for example.
        // Will print the List in this style: <01,05,10,15>
        private void PrintList(List<Card> cards)
        {
            if (cards.Count == 0)
            {
                tb_1.Text += "<no cards>" + System.Environment.NewLine;     // Should not happen, right?
            }
            else
            {
                tb_1.Text += "<";
                for (int i = 0; i < cards.Count; i++)
                {
                    tb_1.Text += cards[i].ID;
                    if (i != cards.Count - 1)
                        tb_1.Text += ",";
                }

                tb_1.Text += ">" + System.Environment.NewLine;
            }
        }

        // Print entire field, unless for debug, it will be only full boards (4x4 cards)
        private void PrintField(Card[,] field)
        {
            tb_1.Text += "---full----" + System.Environment.NewLine;
            for (int y = 0; y < fieldSize; y++)
            {
                for (int x = 0; x < fieldSize; x++)
                {
                    if (field[y, x] == null)    // This will only happen if board is NOT full/complete, so mostly for debug
                    {
                        tb_1.Text += System.Environment.NewLine + "--notfull--" + System.Environment.NewLine;
                        return;
                    }
                    else
                    {
                        tb_1.Text += field[y, x].ID + " ";
                    }
                }
                tb_1.Text += System.Environment.NewLine;
            }
            tb_1.Text += "---board---" + System.Environment.NewLine;
            CursorToBottom();
            tb_1.Refresh();
        }

        // When a full sequence is located, save into the List of sequences (and rotations!)
        private void SaveSequence(Card[,] field)
        {
            string[] tmpStr = new string[16];   // Using an array of strings instead of one long string
            int[] cardRotations = new int[16];  // Every card has a rotation, 0 to 3
            for (int y = 0; y < fieldSize; y++)
            {
                for (int x = 0; x < fieldSize; x++)
                {
                    tmpStr[y * 4 + x] = field[y, x].ID;
                    cardRotations[y * 4 + x] = field[y, x].Rotation;
                }
            }
            sequences.Add(tmpStr);
            rotations.Add(cardRotations);
        }

        // Draw entire field (used cards in "field" and unused cards in its home slot)
        private void DrawField(Card[,] fieldToDraw)
        {
            List<CardGraphics> cardsGraphicsLeft = new List<CardGraphics>(graphics);
            Bitmap bmp = new Bitmap(pb_1.Size.Width, pb_1.Size.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            int offsetX = 133;  // Starting positions for the first card
            int offsetY = 103;  // After that, every card is 81 pixels + 1 pixel for grid
            for (int y = 0; y < fieldSize; y++)
            {
                for (int x = 0; x < fieldSize; x++)
                {
                    if (fieldToDraw[y, x] != null)
                    {
                        Bitmap rotatedCard = new Bitmap(cardsGraphicsLeft.Where(card => card.ID == fieldToDraw[y, x].ID).FirstOrDefault().Image);
                        for (int i = 0; i < fieldToDraw[y, x].Rotation; i++)    // Turn card 0-3 times depending on stored rotation value
                        {
                            rotatedCard.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }
                        g.DrawImage(rotatedCard, offsetX + x * 82, offsetY + y * 82);
                        cardsGraphicsLeft.Remove(cardsGraphicsLeft.Where(card => card.ID == fieldToDraw[y, x].ID).FirstOrDefault());
                    }
                }
            }
            foreach (CardGraphics cg in cardsGraphicsLeft)
            {
                g.DrawImage(cg.Image, cg.UnusedPosX, cg.UnusedPosY);    // All remaining cards are drawn in their "base" positions
            }
            pb_1.Image = bmp;
        }

        // Handles when we report progress. Usually as a percent of progress but we can use integer as an instruction and send an object in e.UserState
        // This is on UI thread!
        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)   // Check "instruction"
            {
                case 0:         // Most often we will just draw board in its current state, full or not
                    DrawField((Card[,])e.UserState);
                    break;
                case 1:         // Report how many incomplete combinations we've tested
                    tb_1.Text += "Incomplete: " + incompleteCombinations.ToString() + System.Environment.NewLine;
                    CursorToBottom();
                    tb_1.Refresh();
                    break;
                case 2:         // Typically only full boards!
                    PrintField((Card[,])e.UserState);   
                    break;
                default:
                    break;
            }
            
            holdThread = false;
        }

        // Starts the background worker thread - this one has no access to GUI such as picturebox or textbox
        private void RunBackgroundWorker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            SolveAll();     // As SolveAll is shorter now, all code could be moved here instead
            if (worker.CancellationPending == true)     // Did we exit the thread with the cancel flag set?
            {
                e.Cancel = true;
            }
        }   // No need for more code here, when we're done we will run _RunWorkerCompleted

        // This part is run when background worker has completed, regardless of outcome
        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                tb_1.Text += "Operation aborted!" + System.Environment.NewLine;
                tb_1.Text += "Ended after " + (incompleteCombinations + sequences.Count).ToString() + " tested combinations." + System.Environment.NewLine;
            }
            else if (e.Error != null)   // Never tested this, not sure if this is correct
            {
                tb_1.Text += "An error occured!!! " + System.Environment.NewLine + e.Error.Message + System.Environment.NewLine;
            }
            else
            {
                tb_1.Text += "Operation completed!" + System.Environment.NewLine;
                tb_1.Text += "Total incomplete combinations tested: " + (incompleteCombinations + sequences.Count).ToString() + System.Environment.NewLine;
            }
            tb_1.Text += "Solutions found: " + sequences.Count + System.Environment.NewLine;
            //tb_1.Text += "Tests: " + tests.ToString() + System.Environment.NewLine;   // For debug
            CursorToBottom();
            tb_1.Refresh();
            btn_start.Enabled = true;
            btn_stop.Enabled = false;
            if (sequences.Count > 0)
            {
                RemoveDuplicates();
                btn_solutions.Enabled = true;
                currentSolution = 0;
                ShowSolution(0);    // If we found solutions, show the first one (index 0 in sequences)
            }
        }

        // Pressing the Start button - clear all old stuff and start anew
        private void btn_start_Click(object sender, EventArgs e)
        {
            tb_1.Text = "";         // Clear log
            incompleteCombinations = 0;       // Clear combinations
            //tests = 0;            // Clear (debug) test counter
            sequences.Clear();      // Clear successful chains
            rotations.Clear();      // Clear corresponding rotation
            abort = false;          // Clear abort flag (for when pressing stop)
            btn_start.Enabled = false;
            btn_stop.Enabled = true;
            btn_solutions.Enabled = false;
            btn_solutions.Text = "Solutions";
            currentSolution = -1;
            _backgroundWorker.RunWorkerAsync();
        }
        
        // Pressing the Stop button
        private void btn_stop_Click(object sender, EventArgs e)
        {
            _backgroundWorker.CancelAsync();        // Report that we clicked cancel
            abort = true;                           // Set abort flag so recursive function can abort (step out)
            btn_start.Enabled = true;               
            btn_stop.Enabled = false;
        }

        // Pressing the Solutions button - show and cycle through all solutions we found
        private void btn_solutions_Click(object sender, EventArgs e)
        {
            currentSolution++;
            if (currentSolution >= sequences.Count)
            {
                currentSolution = 0;
            }
            if (sequences.Count > 0)
                ShowSolution(currentSolution);
        }

        // Draw a complete solution [pos] from sequence. Note that pos is 0-based index but is presented as 1-based.
        private void ShowSolution(int pos)
        {
            if (currentSolution < 0)        // Should never happen as this should only be called if sequence.count > 0
                return;

            btn_solutions.Text = "Solution " + (pos + 1).ToString() + "/" + sequences.Count.ToString();
            btn_solutions.Refresh();        // Refresh so we get correct text while cards are being placed
            Bitmap bmp = new Bitmap(pb_1.Size.Width, pb_1.Size.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);     // Clear background image, but it has an background image (the grid) that won't change
            int offsetX = 133;  // Starting positions for the first card
            int offsetY = 103;  // After that, every card is 81 pixels + 1 pixel for grid
            foreach (CardGraphics cg in graphics)
            {
                g.DrawImage(cg.Image, cg.UnusedPosX, cg.UnusedPosY);    // First all cards in their "unused" positions
            }

            for (int y = 0; y < fieldSize; y++)
            {
                for (int x = 0; x < fieldSize; x++)
                {
                    Bitmap rotatedCard = new Bitmap(graphics.Where(card => card.ID == sequences[pos][y * 4 + x]).FirstOrDefault().Image);
                    for (int j = 0; j < rotations[pos][y * 4 + x]; j++)
                    {
                        rotatedCard.RotateFlip(RotateFlipType.Rotate90FlipNone);    // Rotate card if needed
                    }
                    g.DrawImage(rotatedCard, offsetX + x * 82, offsetY + y * 82);   // Draw image in its place in the field / solution
                    g.SetClip(new Rectangle(graphics[Convert.ToInt32(sequences[pos][y * 4 + x])].UnusedPosX,    // Mark area for old position of card
                           graphics[Convert.ToInt32(sequences[pos][y * 4 + x])].UnusedPosY, 81, 81));           // This was tricky to get right...
                    g.Clear(Color.Transparent);     // Clear the old card position
                    g.ResetClip();                  // Reset clip back to "full image"
                    pb_1.Image = bmp;
                    pb_1.Refresh(); 
                    Thread.Sleep(150);
                }
            }
        }

        // Default solution algorithm will give us every solution four times because we rotate all cards. 
        // Only run this for full solutions with 16 cards!
        private void RemoveDuplicates()
        {
            for (int seq = 0; seq < sequences.Count; seq++)
            {
                if (sequences[seq].Length != 16)
                    break;      // This should not be needed as long as we only run it with full sequences
                for (int i = 0; i < 16; i++)    // We don't know in advance what position card 5 is in
                {
                    if (sequences[seq][i] == "05")  // Use card 05 as refrence card to place in rotation 0
                    {
                        if (rotations[seq][i] != 0)     // Position i has card 05
                        {
                            int rots = 4 - rotations[seq][i];   // New int as rotation[seq][i] will change
                            for (int j = 0; j < rots; j++)      // Changing rotations is done in this loop
                            {       // Change position in sequences but also change BOTH value and position in rotations
                                sequences[seq] = RotatePositions(sequences[seq]);
                                rotations[seq] = RotateRotations(rotations[seq]);
                            }
                        }
                        break;  // Once we have changed order we must probably break out or we'll find the 05 in some other position?
                    }
                }
            }       // Now we have looked through the entire sequence and have rotated it correctly

            List<int> duplicates = new List<int>();     
            for (int seq = 0; seq < sequences.Count; seq++)     // Look through all sequences (solutions)
            {
                for (int rest = (seq + 1); rest < sequences.Count; rest++)  // Start with next sequence and look through the rest
                {
                    if (sequences[seq].SequenceEqual(sequences[rest]))      // Sequence exists! But we also need to check rotation
                    {
                        if (rotations[seq].SequenceEqual(rotations[rest]))  // NOW we have a duplicate (index)
                        {
                            if (!duplicates.Contains(rest))     // Check if we have found it already
                            {
                                duplicates.Add(rest);           
                                //tb_1.Text += "Found dup. in index: " + rest.ToString() + System.Environment.NewLine;
                                //CursorToBottom();
                            }
                        }
                    }
                }
            }

            duplicates.Sort();          // We must sort them high to low and delete from Lists in that order
            duplicates.Reverse();       // Otherwise we will end up out of bounds! 
            for (int i = 0; i < duplicates.Count; i++)
            {
                sequences.RemoveAt(duplicates[i]);
                rotations.RemoveAt(duplicates[i]);
            }
            tb_1.Text += "After duplicate check there are " + sequences.Count.ToString() + " unique solutions!" + System.Environment.NewLine;
            CursorToBottom();
        }

        /* Rotate positions 90 degrees CW. Look at the 16 values as a 4x4 field and then move values accordingly
        00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15

        00 01 02 03 
        04 05 06 07 
        08 09 10 11 
        12 13 14 15

        12 08 04 00 13 09 05 01 14 10 06 02 15 11 07 03

        12 08 04 00
        13 09 05 01
        14 10 06 02
        15 11 07 03 */
        private string[] RotatePositions(string[] inStr)
        {
            string[] outStr = new string[16];
            outStr[0]  = inStr[12];
            outStr[1]  = inStr[8];
            outStr[2]  = inStr[4];
            outStr[3]  = inStr[0];
            outStr[4]  = inStr[13];
            outStr[5]  = inStr[9];
            outStr[6]  = inStr[5];
            outStr[7]  = inStr[1];
            outStr[8]  = inStr[14];
            outStr[9]  = inStr[10];
            outStr[10] = inStr[6];
            outStr[11] = inStr[2];
            outStr[12] = inStr[15];
            outStr[13] = inStr[11];
            outStr[14] = inStr[7];
            outStr[15] = inStr[3];
            return outStr;
        }

        // Same as above but also changing values to plus one as it means rotating 90 degrees CW. It's used to draw the correct rotation later!
        private int[] RotateRotations(int[] inInts)
        {
            int[] outInts = new int[16];
            outInts[0]  = RecalculateRotation(inInts[12]);
            outInts[1]  = RecalculateRotation(inInts[8]);
            outInts[2]  = RecalculateRotation(inInts[4]);
            outInts[3]  = RecalculateRotation(inInts[0]);
            outInts[4]  = RecalculateRotation(inInts[13]);
            outInts[5]  = RecalculateRotation(inInts[9]);
            outInts[6]  = RecalculateRotation(inInts[5]);
            outInts[7]  = RecalculateRotation(inInts[1]);
            outInts[8]  = RecalculateRotation(inInts[14]);
            outInts[9]  = RecalculateRotation(inInts[10]);
            outInts[10] = RecalculateRotation(inInts[6]);
            outInts[11] = RecalculateRotation(inInts[2]);
            outInts[12] = RecalculateRotation(inInts[15]);
            outInts[13] = RecalculateRotation(inInts[11]);
            outInts[14] = RecalculateRotation(inInts[7]);
            outInts[15] = RecalculateRotation(inInts[3]);
            return outInts;
        }

        // Only used in RotateRotations, making sure that rotation 4 goes back to 0 to keep everything consistent!
        private int RecalculateRotation(int rot)
        {
            rot++;
            if (rot > 3)
                rot = 0;
            return rot;
        }

        // When clicking the picture, save the field into ClipBoard. Easier and faster than screenshots or similar. For debug or sharing!
        private void pb_1_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(329, 329);      // Size of entire field including grids
            Graphics g = Graphics.FromImage(bmp);   // However, the grid will not follow as it's a "background" and not part of the image.
            g.DrawImage(pb_1.Image, 0, 0, new Rectangle(132, 102, 329, 329), GraphicsUnit.Pixel);   // Take image in picturebox and copy a section of it
            Clipboard.SetImage(bmp);        // Place image in ClipBoard to easily paste into Paint or whatever
            tb_1.Text += "Copied solution " + (currentSolution + 1).ToString() + System.Environment.NewLine;
            CursorToBottom();
        }


        #region Carls version

        // Carls version of the solution finder (not rotating cards!)
        private void CallesVersion()
        {
            List<Card> deck = new List<Card>(InitCards());  // Prepare deck
            List<Card> cleanTable = new List<Card>();       // The table with cards picked from the deck
            PickCard(deck, cleanTable);                     // Start recursive search
            MessageBox.Show(result);                        // Show the result
        }

        // Carls recursive function
        private void PickCard(List<Card> CallesDeck, List<Card> table)
        {
            //tests++;
            foreach (Card c in CallesDeck)
            {
                List<Card> localDeck = new List<Card>(CallesDeck);  // Make a new deck clone (with only the cards that are left from before)
                localDeck.Remove(c);                                // Remove a card from the new deck...

                List<Card> localTable = new List<Card>(table);      // Make a new table clone (with all cards that lie there from before)
                localTable.Add(c);                                  // ...and place the card on the new table

                if (CheckIfWorthContinuing(localTable))             // Check if this table (card combination) is worth continuing, if colors match
                    PickCard(localDeck, localTable);                // If so, continue with another card
            }

            if (table.Count == 16)                                  // Only log solutions that make it all the way to 16 cards
                result = result + string.Join(",", table.Select(x => x.ID).ToList()) + "\n";
        }

        // Carls condition checker called from resursive function
        private bool CheckIfWorthContinuing(List<Card> localTable)
        {
            bool worthIt = true;            // Say that colors match, unless...

            if (localTable.Count > 1 && (localTable.Count - 1) % 4 != 0)                    // Avoid first column
                if (localTable.Last().LeftColor != localTable[localTable.Count - 2].RightColor)       // Check if color matches with card to the left
                    worthIt = false;

            if (localTable.Count > 4)                                                       // Avoid first row
                if (localTable.Last().TopColor != localTable[localTable.Count - 5].BottomColor)       // Check if color matches with card above
                    worthIt = false;

            return worthIt;
        }

        #endregion

    }
}
