namespace Nofun.Util
{
    public struct NRectangle
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public NRectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public bool Collide(NRectangle otherRect)
        {
            return (x < otherRect.x + otherRect.width) && (x + width > otherRect.x) && (y < otherRect.y + otherRect.height) && (y + height > otherRect.y);
        }
    }
}