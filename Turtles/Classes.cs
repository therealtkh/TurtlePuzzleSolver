using System.Drawing;

namespace Turtles
{
    // Original puzzle has five colors (where orange is some form of yellow)
    public enum TurtleColor { Red, Green, Yellow, Blue, Orange }
    
    // Cards are what we have in deck and what we place on the table
    public class Card
    {
        public string ID { get; }                       // ID will never change
        public TurtleColor TopColor { get; set; }       // Colors according to enum further up
        public int TopPart { get; set; }                // Bodypart is either 0 (torso) or 1 (butt)
        public TurtleColor BottomColor { get; set; }
        public int BottomPart { get; set; }
        public TurtleColor LeftColor { get; set; }
        public int LeftPart { get; set; }
        public TurtleColor RightColor { get; set; }
        public int RightPart { get; set; }
        public int Rotation { get; set; }               // Rotation will be 0, 1, 2 or 3 meaning how many times card has rotated

        // Rotation starts at 0 which means torsos are in positions right and bottom (and bottoms to left and up)
        public Card(int id, TurtleColor topColor, TurtleColor bottomColor, TurtleColor leftColor, TurtleColor rightColor)
        {
            if (id < 10)                        // Create card with int but using ID as a string in most places
                this.ID = "0" + id.ToString();  // except for a few comparisons, where type doesn't matter
            else                                // and add a 0 to all cards with ID < 10, this makes the
                this.ID = id.ToString();        // printed tables look better and symmetrical
            this.TopColor = topColor;
            this.BottomColor = bottomColor;
            this.LeftColor = leftColor;
            this.RightColor = rightColor;
            this.TopPart = 0;   // butt
            this.BottomPart = 1;// torso
            this.LeftPart = 0;  // butt
            this.RightPart = 1; // torso
            this.Rotation = 0;  // Rotation 0 is default - indicated by text rotation!
        }

        // Rotate card 90 degrees CW / right
        public void CardRotate()
        {
            TurtleColor tmpColor = this.TopColor;
            int tmpPart = this.TopPart;

            this.TopColor    = this.LeftColor;
            this.TopPart     = this.LeftPart;
            this.LeftColor   = this.BottomColor;
            this.LeftPart    = this.BottomPart;
            this.BottomColor = this.RightColor;
            this.BottomPart  = this.RightPart;
            this.RightColor  = tmpColor;
            this.RightPart   = tmpPart;

            this.Rotation++;
            if (this.Rotation > 3)     
                this.Rotation = 0;      // Keep rotation 0-3 
        }
    }

    // Instead of storing graphics and unused/default positions in separate lists or arrays...
    public class CardGraphics
    {
        public string ID { get; }       // ID can be directly compared with Card.ID
        public Bitmap Image { get; }
        public int UnusedPosX { get; }
        public int UnusedPosY { get; }

        public CardGraphics(int id, Bitmap image, int x, int y)
        {
            if (id < 10)
                this.ID = "0" + id.ToString();
            else
                this.ID = id.ToString();
            this.Image = image;
            this.UnusedPosX = x;
            this.UnusedPosY = y;
        }
    }

}
