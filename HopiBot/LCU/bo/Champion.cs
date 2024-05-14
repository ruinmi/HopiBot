namespace HopiBot.LCU.bo
{
    public class Champion
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
