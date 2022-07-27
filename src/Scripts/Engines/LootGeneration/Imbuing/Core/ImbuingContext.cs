namespace Server
{
    public class ImbuingContext
    {
	    private Mobile Player { get; }
        public Item LastImbued { get; set; }
        public int ImbueMod { get; set; }
        public int ImbueModInt { get; set; }
        public int ImbueModVal { get; set; }
        public int ImbMenuCat { get; set; }

        public ImbuingContext(Mobile mob)
        {
            Player = mob;
        }

        public ImbuingContext(Mobile owner, GenericReader reader)
        {
            var v = reader.ReadInt();

            Player = owner;

            switch (v)
            {
                case 1:
                    LastImbued = reader.ReadItem();
                    ImbueMod = reader.ReadInt();
                    ImbueModInt = reader.ReadInt();
                    ImbueModVal = reader.ReadInt();
                    ImbMenuCat = reader.ReadInt();
                    break;
                case 0:
                    LastImbued = reader.ReadItem();
                    ImbueMod = reader.ReadInt();
                    ImbueModInt = reader.ReadInt();
                    ImbueModVal = reader.ReadInt();
                    reader.ReadInt();
                    ImbMenuCat = reader.ReadInt();
                    reader.ReadInt();
                    break;
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1);

            writer.Write(LastImbued);
            writer.Write(ImbueMod);
            writer.Write(ImbueModInt);
            writer.Write(ImbueModVal);
            writer.Write(ImbMenuCat);
        }
    }
}
