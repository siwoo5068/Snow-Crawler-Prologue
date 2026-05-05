using System.Collections.Generic;

public enum ItemType
{
    // ── 기존 아이템 ──────────────────────────────
    OldChair,
    WoodenTable,
    Bookshelf,
    Lantern,
    HeavyCrate,
    Rug,
    WallClock,
    SmallDrawer,

    // ── Furniture Mega Pack 가구 ─────────────────
    Bed,             // 침대  (9.0 kg)
    Chair,           // 의자  (2.5 kg)
    Cushion,         // 쿠션  (0.5 kg)
    Sofa,            // 소파  (7.5 kg)
    TableFurniture,  // 테이블 (4.5 kg)
}

public static class ItemDatabase
{
    public static readonly Dictionary<ItemType, float> Weight = new Dictionary<ItemType, float>
    {
        // 기존
        { ItemType.OldChair,        2.0f },
        { ItemType.WoodenTable,     5.0f },
        { ItemType.Bookshelf,       7.0f },
        { ItemType.Lantern,         0.5f },
        { ItemType.HeavyCrate,      8.0f },
        { ItemType.Rug,             1.5f },
        { ItemType.WallClock,       1.0f },
        { ItemType.SmallDrawer,     3.0f },

        // Furniture Mega Pack
        { ItemType.Bed,             9.0f },
        { ItemType.Chair,           2.5f },
        { ItemType.Cushion,         0.5f },
        { ItemType.Sofa,            7.5f },
        { ItemType.TableFurniture,  4.5f },
    };
}
