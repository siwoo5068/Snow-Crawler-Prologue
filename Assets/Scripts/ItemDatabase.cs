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

    /// <summary>
    /// 가구별 안락도 기여 점수.
    /// 무겁고 큰 가구일수록 안락도가 높음 → 수집 전략이 생김.
    /// </summary>
    public static readonly Dictionary<ItemType, float> ComfortValue = new Dictionary<ItemType, float>
    {
        // 기존 (레거시 — 혹시 스폰될 경우 대비)
        { ItemType.OldChair,        1.0f },
        { ItemType.WoodenTable,     1.0f },
        { ItemType.Bookshelf,       1.5f },
        { ItemType.Lantern,         0.3f },
        { ItemType.HeavyCrate,      0.5f },
        { ItemType.Rug,             0.5f },
        { ItemType.WallClock,       0.3f },
        { ItemType.SmallDrawer,     0.8f },

        // Furniture Mega Pack
        { ItemType.Bed,             2.0f },   // 침대: 최고
        { ItemType.Sofa,            1.5f },   // 소파: 높음
        { ItemType.TableFurniture,  1.0f },   // 테이블: 보통
        { ItemType.Chair,           1.0f },   // 의자: 보통
        { ItemType.Cushion,         0.5f },   // 쿠션: 낮음
    };
}
