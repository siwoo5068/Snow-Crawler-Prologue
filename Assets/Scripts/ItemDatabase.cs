using System.Collections.Generic;

public enum ItemType
{
    OldChair,
    WoodenTable,
    Bookshelf,
    Lantern,
    HeavyCrate,
    Rug,
    WallClock,
    SmallDrawer,
}

public static class ItemDatabase
{
    public static readonly Dictionary<ItemType, float> Weight = new Dictionary<ItemType, float>
    {
        { ItemType.OldChair,    2.0f },
        { ItemType.WoodenTable, 5.0f },
        { ItemType.Bookshelf,   7.0f },
        { ItemType.Lantern,     0.5f },
        { ItemType.HeavyCrate,  8.0f },
        { ItemType.Rug,         1.5f },
        { ItemType.WallClock,   1.0f },
        { ItemType.SmallDrawer, 3.0f },
    };
}
