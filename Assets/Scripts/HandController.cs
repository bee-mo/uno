using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandController : MonoBehaviour {
  private Transform handLayout;
  private GameObject cardParent;
  private float cardCount;
  private const float maxHandWidth = 6.0f; //the max number of cards before we start overlapping cards
  private float cardWidth;
  private float cardHeight;
  private float handWidth; //the calculated max hand width
  public GameObject cardPrefab;
  private Transform selectedCard_;
  private bool is_main_player_ = true;

  private Transform playPile;

  private GameManager gameManager;
  private int playerID;
  
  //if the next card drawn will be playable instantly
  private bool next_card_playable_ = false; 
  private bool next_card_playable_only_ = false;

  private Card playable_drawn_card_;


  // Start is called before the first frame update
  void Awake(){

  }

  void Start() {
    handLayout = transform;

    Debug.Assert(cardPrefab != null);
    cardWidth = cardPrefab.transform.GetComponent<RectTransform>().rect.width;
    cardHeight = cardPrefab.transform.GetComponent<RectTransform>().rect.height;
    handWidth = maxHandWidth * cardWidth;
    playPile = GameObject.Find("PlayPile").transform;

    gameManager = GameObject.Find("Game Manager").transform.GetComponent<GameManager>();

    cardParent = new GameObject("Card Parent");
    cardParent.transform.SetParent(handLayout, false); 
  }

  // Update is called once per frame
  void Update() { }


  public void DrawCard() {
    //Calculate positions of current cards based on the new amount of cards
    int newCardCount = cardParent.transform.childCount + 1; //newest card count

    float spacingIncrement = 0.0f;
    float leftEdge = -(newCardCount - 1) * 0.5f * cardWidth;

    float currentCardZ = -0.01f;
    if (newCardCount * cardWidth > handWidth) {
      spacingIncrement = cardWidth - ((cardWidth * newCardCount - handWidth) / newCardCount);

      leftEdge = -handWidth / 2.0f + cardWidth * 0.5f;
    } else {
      spacingIncrement = cardWidth;
    }

    //Each existing card will be moved to their new positions
    foreach (Transform child in cardParent.transform) {
      child.GetComponent<Card>().SetCardPosition(leftEdge, currentCardZ);
      leftEdge += spacingIncrement;

      currentCardZ += -0.01f;
    }

    //Add the new card
    GameObject drawnCard = Instantiate(cardPrefab, cardParent.transform);



    drawnCard.transform.localPosition = new Vector3(leftEdge, -cardHeight * 1.5f, currentCardZ);
    var card = drawnCard.transform.GetComponent<Card>();
    card.SetCardPosition(leftEdge, currentCardZ);
    card.SetHandController(handLayout.GetComponent<HandController>());
    
    if (next_card_playable_){ 
      card.SetCardDrawnPlayable(true);
      next_card_playable_ = false;
      next_card_playable_only_ = true;
      playable_drawn_card_ = card;
    }

    if (is_main_player_ || true) card.FlipCard(Card.CardPosition.FRONT);
    else card.FlipCard(Card.CardPosition.BACK);
    cardCount = newCardCount;
  }

  public void SetAsMainPlayer() {
    is_main_player_ = true;
  }

  public void SetAsEnemy() {
    is_main_player_ = false;
  }

  public bool IsMainPlayer(){
    return is_main_player_;
  }

  public bool RemoveCard(Transform removedCard) {
    //Calculate positions of current cards based on the new amount of cards

    var card = removedCard.transform.GetComponent<Card>();
    var playPileComponent = playPile.GetComponent<PlayPile>();


    if (next_card_playable_only_ && !card.GetCardDrawnPlayable() ) return false;


    //check if playing card is valid
    if (!CheckCardPlayValid(playPileComponent.GetTopCard().GetCardInfo(), card.GetCardInfo() )) return false;

    if (playerID != gameManager.GetActivePlayerID()) return false;

    //card play is valid, set next_card variables off
    if (next_card_playable_only_ ){
      next_card_playable_ = false;
      next_card_playable_only_ = false;
    }

    int newCardCount = cardParent.transform.childCount - 1; //newest card count

    if (newCardCount <= 0){
      gameManager.SetRestartButton(true);
      Debug.Log("winner");
    }

    float spacingIncrement = 0.0f;

    float leftEdge = -(newCardCount - 1) * 0.5f * cardWidth;


    if (newCardCount * cardWidth > handWidth) {
      spacingIncrement = cardWidth - ((cardWidth * newCardCount - handWidth) / newCardCount);

      leftEdge = -handWidth / 2.0f + cardWidth * 0.5f;
    } else {
      spacingIncrement = cardWidth;
    }
    //removedCard.SetParent();
    Transform displayCard = removedCard.Find("Display Image");
    Vector3 cardRotation = displayCard.eulerAngles;
    cardRotation.z += Random.Range(-45.0f, 45.0f);


//    removedCard.SetParent(playPile, true);

    playPileComponent.SetTopCard(card);


    float cardZ = (float)-playPileComponent.GetPlayPileCardCount()*0.01f - 1.0f;


    Vector3 newPos = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), cardZ);
    displayCard.localPosition = Vector3.zero;
    Vector3 newRot = cardRotation;
    removedCard.GetComponent<Button>().enabled = false;

    card.SetPlayCardPositions(newPos, newRot);
    float currentCardZ = -0.01f;

    //Each existing card will be moved to their new positions
    foreach (Transform child in cardParent.transform) {
      if (child != removedCard) {
        child.GetComponent<Card>().SetCardPosition(leftEdge, currentCardZ);
        leftEdge += spacingIncrement;
        currentCardZ += -0.01f;
      }
    }

    cardCount = newCardCount;

    CardGenerator.CardInfo playedCardInfo = card.GetCardInfo();

    if (playedCardInfo.cardType.ToString() == "SKIP"){
      gameManager.NextActivePlayer();
    } else if (playedCardInfo.cardType.ToString() == "REVERSE"){
      gameManager.ReverseDirection();
    } else if (playedCardInfo.cardType.ToString() == "DRAW_2"){
      gameManager.ForceDraw(2, gameManager.GetNextActivePlayer());
    } else if (playedCardInfo.cardType.ToString() == "WILD_DRAW_4"){
      gameManager.ForceDraw(4, gameManager.GetNextActivePlayer());
    }
    gameManager.NextActivePlayer();
    return true;

  }


  public bool CheckCardPlayValid(CardGenerator.CardInfo topCard, CardGenerator.CardInfo checkCard){

    if (checkCard.cardType.ToString() == "WILD_DRAW_4" || checkCard.cardType.ToString() == "WILD") return true;

    if (checkCard.cardColor == topCard.cardColor) return true;
    if (checkCard.cardType == topCard.cardType) return true;

    return false;

  }

  public void SetPlayerID(int id){
    playerID = id;
  }

  public int GetCardCount(){
    return (int)cardCount;
  }

  public void ClearHand(){
    handLayout = transform;
    Destroy(cardParent);
    cardParent = new GameObject("Card Parent");
    cardParent.transform.SetParent(handLayout, false); 
    cardCount = 0;
  }

  public void NextCardPlayable(){
    next_card_playable_ = true;
  }

  public void PassAction(){
    next_card_playable_ = false;
    next_card_playable_only_ = false;

    if (playable_drawn_card_ != null){
      playable_drawn_card_.SetCardDrawnPlayable(false);
      playable_drawn_card_ = null;
    }
  }

}




