using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDeck : MonoBehaviour {

  private Sprite base_card_sprite_ = null;
  private Dictionary<float, Sprite> card_sprites_ = new Dictionary<float, Sprite>();
  private SpriteRenderer renderer_;

  // The drawable card is the card that creates the card drawing
  // illusion. Should be reset to its original position after 
  // each draw animation.
  private GameObject drawable_card_;
  private Vector3 lowest_deck_pos_;
  private Vector3 highest_deck_pos_;
  private bool card_draw_in_progress_ = false;
  private Vector3 draw_start_pos_, draw_end_pos_;
  private float card_draw_lerp_ = 0.0f;
  private float draw_speed_ = 2.5f;
  private bool draw_card_to_main_player_ = false;
  private HandController hand_drawing_;

  public float fullness_ = 1.0f;
  private const int kDeckThickness = 12;

  void Start() {
    renderer_ = GetComponent<SpriteRenderer>();
    Debug.Assert(renderer_ != null);
    Debug.Assert(renderer_.sprite);
    CreateDrawableCard();

    base_card_sprite_ = renderer_.sprite;
    InitializeCardSpriteStates();
    SetDeckFullness(1.0f);
  }

  void Update() {
    HandleCardDraw();
  }

  public void DrawMainPlayerCard(HandController hand) {
    // for now, draw card only for the main player
    card_draw_in_progress_ = true;
    draw_card_to_main_player_ = true;

    hand_drawing_ = hand;
  }

  public void DrawEnemyPlayerCard(HandController hand) {
    // for now, draw card only for the main player
    card_draw_in_progress_ = true;
    draw_card_to_main_player_ = false;

    hand_drawing_ = hand;
  }

  // @def Update the deck sprite to reflect the fullness of the deck.
  public void SetDeckFullness(float fullness) {
    fullness_ = Mathf.Clamp(fullness, 0.0f, 1.0f);
    Sprite selected_sprite = null;
    float best_fullness = -1;
    foreach (var entry in card_sprites_) {
      if (entry.Key <= fullness_ && entry.Key > best_fullness) {
        selected_sprite = entry.Value;
        best_fullness = entry.Key;
      }
    }
    if (selected_sprite == null) return;

    fullness_ = best_fullness;
    renderer_.sprite = selected_sprite;
  }

  private void HandleCardDraw() {
    if (card_draw_in_progress_) {
      if (draw_start_pos_ == Vector3.zero || draw_end_pos_ == Vector3.zero) {
        card_draw_lerp_ = 0.0f;
        drawable_card_.SetActive(true);
        draw_start_pos_ = drawable_card_.transform.position;
        draw_end_pos_ = drawable_card_.transform.position;
        if (draw_card_to_main_player_) draw_end_pos_.y -= 6.0f;
        else draw_end_pos_.y += 6.0f;
      } else if (card_draw_lerp_ != 1.0) {
        drawable_card_.transform.position = Vector3.Lerp(draw_start_pos_, draw_end_pos_, card_draw_lerp_);
        card_draw_lerp_ += Time.deltaTime * draw_speed_;
        card_draw_lerp_ = Mathf.Clamp(card_draw_lerp_, 0.0f, 1.0f);
      } else {
        // card draw is done
        drawable_card_.SetActive(false);
        drawable_card_.transform.position = draw_start_pos_;
        card_draw_in_progress_ = false;

        draw_start_pos_ = Vector3.zero;
        draw_end_pos_ = Vector3.zero;

        // Call the hand controller to draw the card now
        Debug.Assert(hand_drawing_);
        hand_drawing_.DrawCard();
      }
    }
  }

  // @desc Create different sprites for the card deck depending on how
  // many cards are left in the deck.
  // The less cards in the deck, the smaller the deck should look.
  private void InitializeCardSpriteStates() {
    for (int i = 0; i < kDeckThickness; ++i) {
      float fullness = ((float)i) / kDeckThickness;
      card_sprites_.Add(fullness, CreateDeckSprite(fullness));
    }
  }

  // @desc Create deck sprite dependin on how full the deck is.
  // @param [fullness] >= 0 and <= 1.0
  private Sprite CreateDeckSprite(float fullness) {
    Debug.Assert(base_card_sprite_ != null);
    fullness = Mathf.Clamp(fullness, 0.0f, 1.0f);

    int offset = (int)(kDeckThickness * fullness);
    Texture2D base_tex = base_card_sprite_.texture;
    Texture2D tex = new Texture2D(base_tex.width, base_tex.height + offset);

    for (int i = 0; i < base_tex.width; ++i) {
      for (int j = 0; j < base_tex.height; ++j) {
        tex.SetPixel(i, offset + j, base_tex.GetPixel(i, j));
      }
    }

    // fill in the offset
    Color c = new Color(0.75f, 0.75f, 0.75f);
    for (int i = 0; i < tex.width; ++i) {
      for (int j = 0; j < offset; ++j) {
        tex.SetPixel(i, j, c);
      }
    }

    tex.filterMode = FilterMode.Point;
    tex.Apply();
    // construct a sprite using this new texture
    // Set the pivot to the center fo rnow.
    return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.0f));
  }

  private void CreateDrawableCard() {
    // 0.321
    drawable_card_ = new GameObject();
    drawable_card_.name = "Drawable Card";
    var r = drawable_card_.AddComponent<SpriteRenderer>();
    r.transform.parent = transform.parent;

    Sprite source_spr = GetComponent<SpriteRenderer>().sprite;
    Sprite cpy_spr = Sprite.Create(
      source_spr.texture,
      new Rect(0.0f, 0.0f, source_spr.texture.width, source_spr.texture.height),
      new Vector2(0.5f, 0.0f));
    r.sprite = cpy_spr;

    drawable_card_.transform.position = new Vector3(
      transform.position.x,
      transform.position.y,
      -2);
    drawable_card_.transform.rotation = transform.rotation;
    drawable_card_.transform.localScale = transform.localScale;

    lowest_deck_pos_ = drawable_card_.transform.position;
    highest_deck_pos_ = drawable_card_.transform.position;
    highest_deck_pos_.y += 0.325f;

    UpdateDrawableCardPosition();
    drawable_card_.SetActive(false);
  }

  private void UpdateDrawableCardPosition() {
    Debug.Assert(drawable_card_ != null);
    float diff = highest_deck_pos_.y - lowest_deck_pos_.y;
    float delta = diff * fullness_;
    drawable_card_.transform.position = new Vector3(
      drawable_card_.transform.position.x,
      lowest_deck_pos_.y + delta,
      drawable_card_.transform.position.z
    );
  }

  public float GetDeckOffset(){
    return (kDeckThickness * fullness_);
  }

  public bool CheckCardDrawInProgess(){
    return card_draw_in_progress_;
  }

}
