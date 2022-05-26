using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {

  private Button button;
  private HandController handControl_;
  private float selected_position_y_;
  private float deselected_position_y_;
  private float select_lerp_ = 0.0f;
  private bool selecting_card_ = false;
  private float draw_speed_ = 5.0f;

  // draw the card into the deck the first time it is
  // instantiated.
  private bool start_draw_card_ = true;
  private float start_draw_card_lerp_ = 0.0f;


  private bool start_card_rearrange_ = false;
  private float start_card_rearrange_lerp_ = 0.0f;

  private float start_rearrange_position_x_;
  private float end_rearrange_position_x_;

  private Transform display_image_;
  private CardGenerator.CardInfo card_info_;

  private float card_height_;
  private bool is_card_back_ = false;

  private bool is_played = false;

  private Vector3 start_play_card_position_;
  private Vector3 end_play_card_position_;

  private Vector3 start_play_card_rotation_;
  private Vector3 end_play_card_rotation_;

  private Vector3 start_play_card_scale_;
  private Vector3 end_play_card_scale_;

  private float play_card_lerp = 0.0f;
  private bool play_card_complete = false;

  private bool card_drawn_playable_ = false; //by default, a card cannot be played after a draw action

  void Awake() {
    display_image_ = transform.Find("Display Image");
    Debug.Assert(display_image_ != null);

    SetToNextCardInDeck();
  }

  void Start() {
    button = GetComponent<Button>();
    button.onClick.AddListener(SelectCard);

    card_height_ = GetComponent<RectTransform>().rect.height;
    deselected_position_y_ = 0.0f;//transform.localPosition.y;

    selected_position_y_ = 0.6f * card_height_;
  }

  void Update() {
    HandleInitCardDraw();
    HandleCardSelection();
    HandleCardRearrange();
    HandleCardPlay();
  }

  public enum CardPosition { FRONT, BACK };
  public void FlipCard(CardPosition position) {
    switch (position) {
      case CardPosition.FRONT: {
          display_image_.GetComponent<SpriteRenderer>().sprite = card_info_.cardSprite;
          is_card_back_ = false;
          break;
        }
      case CardPosition.BACK: {
          display_image_.GetComponent<SpriteRenderer>().sprite = CardGenerator.GetSingleton().GetCardBackSprite();
          is_card_back_ = true;
          break;
        }
    }
  }

  public void SetHandController(HandController newController) {
    handControl_ = newController;
  }
  public void SelectCard() {
    selecting_card_ = true;
  }

  public void DeselectCard() {
    selecting_card_ = false;
  }

  private void SetToNextCardInDeck() {
    // card_info_ = CardGenerator.GetSingleton().GenerateRandomCard();
    card_info_ = CardGenerator.GetSingleton().GetNextCardFromDeck();
    if (card_info_ == null) {
      Debug.Log("No more cards in deck");
      return;
    }
    Debug.Assert(display_image_.GetComponent<SpriteRenderer>() != null);
    // display_image_.GetComponent<SpriteRenderer>().sprite = CardGenerator.GetSingleton().GetCardBackSprite();
    // display_image_.GetComponent<SpriteRenderer>().sprite = card_info_.cardSprite;
  }

  public void PlayCard() {
    if (selecting_card_) {

      if (handControl_.RemoveCard(transform)){

        
        
        FlipCard(CardPosition.FRONT);
        is_played = true;
      }
    }

  }

  public void SetPlayCardPositions(Vector3 newPosition, Vector3 newRotation) {

    start_play_card_position_ = transform.localPosition;
    start_play_card_rotation_ = transform.eulerAngles;

    end_play_card_position_ = newPosition;
    end_play_card_rotation_ = newRotation;


    start_play_card_scale_ = transform.localScale;
    end_play_card_scale_ = Vector3.one;

    play_card_lerp = 0.0f;

  }

  private void HandleCardPlay() {
    if (!is_played || play_card_complete) return;


    if (play_card_lerp < 1.0f) {

      transform.localPosition = Vector3.Lerp(start_play_card_position_, end_play_card_position_, play_card_lerp);
      transform.eulerAngles = Vector3.Lerp(start_play_card_rotation_, end_play_card_rotation_, play_card_lerp);
      transform.localScale = Vector3.Lerp(start_play_card_scale_, end_play_card_scale_, play_card_lerp);

      play_card_lerp = Mathf.Clamp(play_card_lerp + draw_speed_ * Time.deltaTime, 0.0f, 1.0f);
    } else {
      transform.localPosition = end_play_card_position_;
      transform.eulerAngles = end_play_card_rotation_;
      transform.localScale = end_play_card_scale_;
      play_card_complete = true;
    }


  }

  private void HandleInitCardDraw() {
    if (is_played) return;


    if (!start_draw_card_) return;
    if (start_draw_card_lerp_ == 0.0f) {

      //new cards do need to be set horizontally using rearrange
      start_card_rearrange_ = false;

      transform.localPosition = CreateDrawStartPosition();
      gameObject.SetActive(true);
    } else {
      if (start_draw_card_lerp_ == 1.0f) {
        start_draw_card_ = false;
        transform.localPosition = CreateDeselectedVectorPosition();
      } else {
        Vector3 start = CreateDrawStartPosition();
        Vector3 end = CreateDeselectedVectorPosition();

        Vector3 newPosition = transform.localPosition;
        newPosition = Vector3.Lerp(start, end, start_draw_card_lerp_);
        transform.localPosition = newPosition;
      }
    }

    start_draw_card_lerp_ = Mathf.Clamp(
      start_draw_card_lerp_ + Time.deltaTime * draw_speed_,
      0.0f, 1.0f);
  }

  private void HandleCardRearrange() {
    if (start_card_rearrange_ && !is_played) {

      if (start_card_rearrange_lerp_ == 1.0f) {
        start_card_rearrange_ = false;

      }

      Vector3 newPosition = transform.localPosition;
      newPosition.x = Mathf.Lerp(start_rearrange_position_x_, end_rearrange_position_x_, start_card_rearrange_lerp_);
      transform.localPosition = newPosition;

      start_card_rearrange_lerp_ = Mathf.Clamp(start_card_rearrange_lerp_ + draw_speed_ * Time.deltaTime, 0.0f, 1.0f);
    }
  }

  private void HandleCardSelection() {
    if (is_played) return;

    if (selecting_card_) {
      //if (select_lerp_ == 1.0f) return;
      if (start_draw_card_) { //if card draw was in process, complete that animation automatically
        start_draw_card_ = false;
        transform.localPosition = CreateDeselectedVectorPosition();
      }
      float start = deselected_position_y_;//CreateDeselectedVectorPosition();
      float end = selected_position_y_;//CreateSelectedVectorPosition();

      //float originalCardHeight = display_image.GetComponent<RectTransform>().sizeDelta.y;
      Vector2 boxSize = transform.GetComponent<RectTransform>().sizeDelta;

      boxSize.y = Mathf.Lerp(card_height_, card_height_ * 1.4f, select_lerp_);
      transform.GetComponent<RectTransform>().sizeDelta = boxSize;

      float boxDiff = (boxSize.y - card_height_) / 2.0f;
      transform.localPosition = new Vector3(transform.localPosition.x, boxDiff, transform.localPosition.z);

      display_image_.localPosition = new Vector3(0.0f, Mathf.Lerp(start, end, select_lerp_) - boxDiff, 0.0f);

      select_lerp_ = Mathf.Clamp(select_lerp_ + draw_speed_ * Time.deltaTime, 0.0f, 1.0f);
    } else {
      //if (select_lerp_ == 0.0f) return;
      if (start_draw_card_) return;//start_draw_card_ = false;
      float start = selected_position_y_;
      float end = deselected_position_y_;

      //float originalCardHeight = display_image.GetComponent<RectTransform>().sizeDelta.y;
      Vector2 boxSize = transform.GetComponent<RectTransform>().sizeDelta;

      boxSize.y = Mathf.Lerp(card_height_, card_height_ * 1.4f, select_lerp_);
      transform.GetComponent<RectTransform>().sizeDelta = boxSize;

      float boxDiff = (boxSize.y - card_height_) / 2.0f;
      transform.localPosition = new Vector3(transform.localPosition.x, boxDiff, transform.localPosition.z);

      //transform.localPosition = Vector3.Lerp(start, end, 1 - select_lerp_);
      display_image_.localPosition = new Vector3(0.0f, Mathf.Lerp(start, end, 1 - select_lerp_) - boxDiff, 0.0f);

      select_lerp_ = Mathf.Clamp(select_lerp_ - draw_speed_ * Time.deltaTime, 0.0f, 1.0f);
    }
  }


  private Vector3 CreateSelectedVectorPosition() {
    return new Vector3(
      transform.localPosition.x,
      selected_position_y_,
      transform.localPosition.z
    );
  }

  private Vector3 CreateDeselectedVectorPosition() {
    return new Vector3(
      transform.localPosition.x,
      deselected_position_y_,
      transform.localPosition.z
    );
  }

  private Vector3 CreateDrawStartPosition() {
    float card_height = GetComponent<RectTransform>().rect.height;
    return new Vector3(

      end_rearrange_position_x_,
      deselected_position_y_ - card_height * 1.5f,
      transform.localPosition.z
    );
  }

  public void SetCardPosition(float xPosition, float zPosition) {
    start_rearrange_position_x_ = transform.localPosition.x;

    Vector3 localPos = transform.localPosition;
    localPos.z = zPosition;
    transform.localPosition = localPos;

    end_rearrange_position_x_ = xPosition;
    start_card_rearrange_ = true;

    start_card_rearrange_lerp_ = 0.0f;

  }

  public void SetPlayCard(bool new_is_played){
    is_played = new_is_played;
  }

  public CardGenerator.CardInfo GetCardInfo(){
    return card_info_;
  }

  public void SetCardDrawnPlayable(bool val){
    card_drawn_playable_ = val;
  }

  public bool GetCardDrawnPlayable(){
    return card_drawn_playable_;
  }

}
