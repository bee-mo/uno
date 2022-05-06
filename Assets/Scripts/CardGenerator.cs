using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// @brief Generate cards of a given type.
public class CardGenerator : MonoBehaviour {

  public class CardInfo {
    private CardType type_;
    private CardColor color_;
    private Sprite card_sprite_;

    public CardType cardType {
      get { return type_; }
    }
    public CardColor cardColor {
      get { return color_; }
      set { color_ = value; }
    }

    public Sprite cardSprite {
      get { return card_sprite_; }
    }

    public CardInfo(CardType type, CardColor color) {
      type_ = type;
      color_ = color;

      card_sprite_ = CardGenerator.GetSingleton().GenerateCardSprite(this);
      Debug.Assert(card_sprite_ != null);
    }

    public override bool Equals(object o) => this.Equals(o as CardInfo);

    public bool Equals(CardInfo b) {
      return type_ == b.type_ && color_ == b.color_;
    }
  }; // end CardInfo

  private List<CardInfo> unplayed_cards_;
  private Dictionary<CardType, Sprite> card_type_sprite_map_;
  private GameDeck deck_;

  void Start() {
    GameObject deck_go = GameObject.Find("GameDeck");
    Debug.Assert(deck_go != null);
    deck_ = deck_go.GetComponent<GameDeck>();
    Debug.Assert(deck_);

    card_type_sprite_map_ = new Dictionary<CardType, Sprite>();
    RegisterCardSprites();

    unplayed_cards_ = new List<CardInfo>();
    InitializeUnplayedCards();
    UpdateDeck();
  }

  static CardGenerator generator_ = null;
  private Sprite back_card_spite_ = null;

  public static CardGenerator GetSingleton() {
    // if (generator_ == null) generator_ = new CardGenerator();
    var cfg = GameObject.Find("Cards Config");
    Debug.Assert(cfg != null);
    return cfg.GetComponent<CardGenerator>();
  }

  // Card Color
  public enum CardColor { Red = 0, Yellow, Blue, Green };
  // Card Type
  public enum CardType {
    NUM_0 = 0, NUM_1, NUM_2, NUM_3, NUM_4, NUM_5, NUM_6,
    NUM_7, NUM_8, NUM_9, SKIP, REVERSE, WILD, DRAW_2,
    WILD_DRAW_4, BACKGROUND
  };

  private void UpdateDeck() {
    float fullness = unplayed_cards_.Count / 108.0f;
    deck_.SetDeckFullness(fullness);
  }

  public CardInfo GenerateRandomCard() {
    CardColor color = (CardColor)(int)Random.Range(0, 3);
    CardType type = (CardType)(int)Random.Range(0, 14);
    return new CardInfo(type, color);
  }

  public bool HasCardsLeft() {
    return unplayed_cards_.Count > 0;
  }

  public CardInfo GetNextCardFromDeck() {
    if (unplayed_cards_.Count == 0) return null;

    int card_index = Random.Range(0, unplayed_cards_.Count - 1);
    var card = unplayed_cards_[card_index];

    unplayed_cards_.RemoveAt(card_index);
    UpdateDeck();
    return card;
  }

  public Sprite GetCardBackSprite() {
    if (back_card_spite_) return back_card_spite_;

    var back = transform.Find("Uno Card Back").gameObject;
    Debug.Assert(back);

    Image img = back.GetComponent<Image>();
    Debug.Assert(img);
    Texture2D tex = CloneTexture(img.sprite.texture);
    back_card_spite_ = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    return back_card_spite_;
  }

  private void InitializeUnplayedCards() {
    unplayed_cards_.Clear();

    // There are 8 of each number (1-9), 2 of each color
    // There are only 4 zeroes tho.
    for (int i = 0; i < 8; ++i) {
      for (int j = 0; j <= 9; ++j) {
        if (j == 0 && i >= 4) continue;
        CardType type = (CardType)j;
        CardColor color = (CardColor)(i % 4);
        unplayed_cards_.Add(new CardInfo(type, color));
      }
    }

    // 8 of each: skip, reverse, draw 2
    // 2 of each color
    List<CardType> g2 = new List<CardType>(){
      CardType.SKIP, CardType.REVERSE, CardType.DRAW_2};
    for (int i = 0; i < 8; ++i) {
      foreach (var type in g2) {
        CardColor color = (CardColor)(i % 4);
        unplayed_cards_.Add(new CardInfo(type, color));
      }
    }

    // 4 wild, and 4 wild + draw 4
    List<CardType> g3 = new List<CardType>(){
      CardType.WILD, CardType.WILD_DRAW_4};
    for (int i = 0; i < 4; ++i) {
      CardColor color = (CardColor)i;
      foreach (var type in g3) {
        unplayed_cards_.Add(new CardInfo(type, color));
      }
    }
    Debug.Assert(unplayed_cards_.Count == 108);
  }

  private void RegisterCardSprites() {
    // TODO: Load the sprites from the sprite sheet and
    // store them in card_type_sprite_map_.

    GameObject card_spritesheet_go = GameObject.Find("Card Spritesheet");
    Debug.Assert(card_spritesheet_go != null);
    Image spritesheet_image = card_spritesheet_go.GetComponent<Image>();
    Debug.Assert(spritesheet_image != null);

    List<Sprite> sub_sprites = TraverseAndFindAllSubimages(spritesheet_image);
    Debug.Assert(sub_sprites.Count == 16);
    card_type_sprite_map_[CardType.WILD] = sub_sprites[0];
    card_type_sprite_map_[CardType.BACKGROUND] = sub_sprites[1];
    card_type_sprite_map_[CardType.NUM_8] = sub_sprites[2];
    card_type_sprite_map_[CardType.NUM_9] = sub_sprites[3];
    card_type_sprite_map_[CardType.NUM_4] = sub_sprites[4];
    card_type_sprite_map_[CardType.NUM_5] = sub_sprites[5];
    card_type_sprite_map_[CardType.NUM_6] = sub_sprites[6];
    card_type_sprite_map_[CardType.NUM_7] = sub_sprites[7];
    card_type_sprite_map_[CardType.NUM_0] = sub_sprites[8];
    card_type_sprite_map_[CardType.NUM_1] = sub_sprites[9];
    card_type_sprite_map_[CardType.NUM_2] = sub_sprites[10];
    card_type_sprite_map_[CardType.NUM_3] = sub_sprites[11];
    card_type_sprite_map_[CardType.REVERSE] = sub_sprites[12];
    card_type_sprite_map_[CardType.SKIP] = sub_sprites[13];
    card_type_sprite_map_[CardType.DRAW_2] = sub_sprites[14];
    card_type_sprite_map_[CardType.WILD_DRAW_4] = sub_sprites[15];

    // Debug
    // {
    //   CardInfo card_info = new CardInfo(CardType.NUM_3, CardColor.Red);
    //   Debug_ReplaceSprite(card_info.cardSprite);
    // }

  }

  private void Debug_ReplaceSprite(Sprite s) {
    GameObject target = GameObject.Find("PlaceholderSquare");
    Debug.Assert(target != null);
    SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
    Debug.Assert(renderer != null);

    // replace sprite
    renderer.sprite = s;
  }

  private Sprite GenerateCardSprite(CardInfo info) {
    if (!card_type_sprite_map_.ContainsKey(info.cardType)) return null;

    Sprite center = card_type_sprite_map_[info.cardType];
    Sprite cardBackground = CreateCardWithColor(info.cardColor);

    return CreateCenteredSprite(cardBackground, center);
  }

  Sprite CreateCardWithColor(CardColor color) {
    Sprite bg = card_type_sprite_map_[CardType.BACKGROUND];
    Texture2D bg_tex = bg.texture;

    Color red = ColorFromRgb(229, 129, 129);
    Color green = ColorFromRgb(129, 229, 171);
    Color yellow = ColorFromRgb(235, 230, 136);
    Color blue = ColorFromRgb(129, 161, 229);

    Color bg_color = Color.black;
    switch (color) {
      case CardColor.Red:
        bg_color = red;
        break;
      case CardColor.Yellow:
        bg_color = yellow;
        break;
      case CardColor.Green:
        bg_color = green;
        break;
      case CardColor.Blue:
        bg_color = blue;
        break;
    }

    Texture2D clr_tex = new Texture2D(bg_tex.width, bg_tex.height);

    for (int i = 0; i < clr_tex.width; ++i) {
      for (int j = 0; j < clr_tex.height; ++j) {
        // border: fill source color
        if (i == 0 || j == 0 || i == clr_tex.width - 1 || j == clr_tex.height - 1) {
          clr_tex.SetPixel(i, j, bg_tex.GetPixel(i, j));
        } else {
          clr_tex.SetPixel(i, j, bg_color);
        }
      }
    }

    clr_tex.filterMode = FilterMode.Point;
    clr_tex.Apply();
    return Sprite.Create(clr_tex, new Rect(0.0f, 0.0f, clr_tex.width, clr_tex.height), new Vector2(0.5f, 0.5f));
  }

  Sprite CreateCenteredSprite(in Sprite background, in Sprite content) {
    Texture2D bg_tex = background.texture;
    Texture2D content_tex = content.texture;
    Debug.Assert(bg_tex.width >= content_tex.width);
    Debug.Assert(bg_tex.height >= content_tex.height);

    Texture2D combined_tex = new Texture2D(bg_tex.width, bg_tex.height);
    int start_x = bg_tex.width / 2 - content_tex.width / 2;
    int start_y = bg_tex.height / 2 - content_tex.height / 2;

    Texture2D card_tex = CloneTexture(bg_tex);
    for (int i = 0; i < content_tex.width; ++i) {
      for (int j = 0; j < content_tex.height; ++j) {
        Color c = content_tex.GetPixel(i, j);
        if (c.a != 0) {
          card_tex.SetPixel(i + start_x, j + start_y, c);
        }
      }
    }
    card_tex.filterMode = FilterMode.Point;
    card_tex.Apply();
    return Sprite.Create(card_tex, new Rect(0.0f, 0.0f, card_tex.width, card_tex.height), new Vector2(0.5f, 0.5f));
  }

  Texture2D CloneTexture(in Texture2D t) {
    Texture2D otex = new Texture2D(t.width, t.height);
    for (int i = 0; i < t.width; ++i) {
      for (int j = 0; j < t.height; ++j) {
        otex.SetPixel(i, j, t.GetPixel(i, j));
      }
    }
    otex.filterMode = FilterMode.Point;
    otex.Apply();
    return otex;
  }

  List<Sprite> TraverseAndFindAllSubimages(in Image source) {
    List<Sprite> sprites = new List<Sprite>();

    // start traversing from the first non-transparent color pixel
    Texture2D source_tex = source.sprite.texture;

    for (int j = 0; j < source_tex.height; ++j) {
      for (int i = 0; i < source_tex.width; ++i) {

        Color clr = source_tex.GetPixel(i, j);
        if (clr.a != 0) {

          // the parsing must start from the first top-left opaque pixel
          // in the subimage.
          if (j == 0 || source_tex.GetPixel(i, j - 1).a == 0) {
            Sprite sub_image_sprite = ParseAndGenerateSubimage(source_tex, i, j);
            if (sub_image_sprite != null) sprites.Add(sub_image_sprite);
          }

          // skip to the next alpha in the row
          while (i < source_tex.width && source_tex.GetPixel(i, j).a != 0) ++i;
        }

      }
    }

    return sprites;
  }

  Sprite ParseAndGenerateSubimage(in Texture2D tex, int x, int y) {
    // find the bottom-right opaque pixel startying from (x, y)
    int end_x = x, end_y = y;

    for (; end_x < tex.width && tex.GetPixel(end_x, y).a != 0; ++end_x) { }
    for (; end_y < tex.height && tex.GetPixel(x, end_y).a != 0; ++end_y) { }
    if (end_x - x <= 0 || end_y - y <= 0) return null;

    Color color_to_ignore = ColorFromRgb(194, 163, 163);
    Texture2D sub_img_tex = new Texture2D(end_x - x, end_y - y);
    for (int i = x; i < end_x; ++i) {
      for (int j = y; j < end_y; ++j) {
        Color c = tex.GetPixel(i, j);

        // exclude color: rgb(194, 163, 163)
        if (c == color_to_ignore) {
          c.a = 0;
        }

        sub_img_tex.SetPixel(i - x, j - y, c);
      }
    }
    sub_img_tex.filterMode = FilterMode.Point;
    sub_img_tex.Apply();

    Sprite subimg_sprite = Sprite.Create(sub_img_tex,
      new Rect(0.0f, 0.0f, sub_img_tex.width, sub_img_tex.height),
      new Vector2(0.5f, 0.5f));

    return subimg_sprite;
  }

  static Color ColorFromRgb(int r, int g, int b) {
    return new Color(
      Mathf.Clamp((float)r / 255, 0.0f, 1.0f),
      Mathf.Clamp((float)g / 255, 0.0f, 1.0f),
      Mathf.Clamp((float)b / 255, 0.0f, 1.0f));
  }
}
