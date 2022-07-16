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

  private bool is_player_ = false;

  private Transform playPile;

  private GameManager gameManager;
  private int playerID;
  
  //if the next card drawn will be playable instantly
  private bool next_card_playable_ = false; //This tells us that next card drawn will be playable
  private bool next_card_playable_only_ = false; //This tells us that a card has been drawn and only that drawn card is playable

  private Card playable_drawn_card_;


  private Transform uno_button_;
  private Transform catch_button_;

  private Transform challenge_button_;
  private Transform decline_button_;


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

    cardParent = transform.Find("Card Parent").gameObject;//new GameObject("Card Parent");
    //cardParent.transform.SetParent(handLayout, false); 

    uno_button_ = transform.Find("Uno Button");
    catch_button_ = transform.Find("Catch Button");
    challenge_button_ = transform.Find("Challenge Button");
    decline_button_ = transform.Find("Decline Button");
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
    
    cardCount = newCardCount;



    if (next_card_playable_){ 
      card.SetCardDrawnPlayable(true);
      next_card_playable_ = false;
      next_card_playable_only_ = true;
      playable_drawn_card_ = card;
      gameManager.SetUnoElements();
    }

    if (is_main_player_ || gameManager.show_hands_) card.FlipCard(Card.CardPosition.FRONT);
    else card.FlipCard(Card.CardPosition.BACK);


    

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

  public void SetPlayer(bool val){
    is_player_ = val;
  }

  public bool IsPlayer(){
    return is_player_;
  }

  public bool RemoveCard(Transform removedCard) {
    //Calculate positions of current cards based on the new amount of cards

    var card = removedCard.transform.GetComponent<Card>();
    var playPileComponent = playPile.GetComponent<PlayPile>();


    //Checks if only the drawn card is the playable card and checks if the selected card is also set as a drawn playable card
    if (next_card_playable_only_ && !card.GetCardDrawnPlayable() ) return false;


    //check if playing card is valid
    if (!CheckCardPlayValid(playPileComponent.GetTopCard().GetCardInfo(), card.GetCardInfo() )) return false;

    if (playerID != gameManager.GetActivePlayerID() || gameManager.ChallengeActive()) return false;

    //card play is valid, set next_card variables off
    if (next_card_playable_only_ ){
      next_card_playable_ = false;
      next_card_playable_only_ = false;
    }

    Card topCard = playPileComponent.GetTopCard();

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

    } else if (playedCardInfo.cardType.ToString() == "WILD"){

      playPileComponent.ActivateColorSelection();
      return true;
    } else if (playedCardInfo.cardType.ToString() == "WILD_DRAW_4"){

      //play pile handles color selection
      //Challenge event occurs (challenge events will record the participating players)
      // the challenge event will handle the drawing
      //Next we proceed as usual, but while challenge event is active thing will work differently
      //Set uno elements will still work for setting up catches, but will not a set up a prep.
      //Once challenge event resolves, we set next active player (the skip) and set uno elements
      //This set up will only be for prep as a prep is not set up so no new catch will be possible here 
      //gameManager.ForceDraw(4, gameManager.GetNextActivePlayer());
      playPileComponent.ActivateColorSelection();
      gameManager.StartChallenge(playerID, gameManager.GetNextActivePlayer(), topCard);
      return true;
    }
    gameManager.NextActivePlayer();
    gameManager.SetUnoElements();
    return true;

  }


  public bool CheckCardPlayValid(CardGenerator.CardInfo topCard, CardGenerator.CardInfo checkCard){

    if (checkCard.cardType.ToString() == "WILD_DRAW_4" || checkCard.cardType.ToString() == "WILD") return true;

    if (checkCard.cardColor == topCard.cardColor) return true;
    if (checkCard.cardType == topCard.cardType) return true;

    return false;

  }
  public bool CheckMatchingColor(CardGenerator.CardInfo topCard, CardGenerator.CardInfo checkCard){

    if (checkCard.cardType.ToString() == "WILD_DRAW_4" || checkCard.cardType.ToString() == "WILD") return false;

    if (checkCard.cardColor == topCard.cardColor) return true;

    return false;

  }


  public bool HandHasPlayable(){

    bool playable = false;
    var playPileComponent = playPile.GetComponent<PlayPile>();
    foreach (Transform child in cardParent.transform) {
        var card = child.GetComponent<Card>();

        //if we are in a state where only drawn card is playable, then card is automatically invalid
        if (next_card_playable_only_ && !card.GetCardDrawnPlayable() ) continue;

        if (CheckCardPlayValid(playPileComponent.GetTopCard().GetCardInfo(), card.GetCardInfo() ))
        {
          playable = true;
        }

    }

    return playable;


  }
  public Card GetPlayableCard(){

    var playPileComponent = playPile.GetComponent<PlayPile>();
    foreach (Transform child in cardParent.transform) {
        var card = child.GetComponent<Card>();

        //if we are in a state where only drawn card is playable, then card is automatically invalid
        if (next_card_playable_only_ && !card.GetCardDrawnPlayable() ) continue;

        if (CheckCardPlayValid(playPileComponent.GetTopCard().GetCardInfo(), card.GetCardInfo() ))
        {
          return card;
        }

    }
    return null;

  }


  public bool HandHasMatchingColor(Card topCard ){

    bool matchingColor = false;

    foreach (Transform child in cardParent.transform) {
        var card = child.GetComponent<Card>();

        //if we are in a state where only drawn card is playable, then card is automatically invalid
        if (next_card_playable_only_ && !card.GetCardDrawnPlayable() ) continue;

        if (CheckMatchingColor(topCard.GetCardInfo(), card.GetCardInfo() ))
        {
          matchingColor = true;
        }

    }

    return matchingColor;

  }

  public void SetPlayerID(int id){
    playerID = id;
  }

  public int GetCardCount(){
    return (int)cardCount;
  }

  public void ClearHand(){
    handLayout = transform;
    
    if (cardParent == null){
      cardParent = transform.Find("Card Parent").gameObject;
      //cardParent = new GameObject("Card Parent");
      //cardParent.transform.SetParent(handLayout, false); 
    }

    foreach (Transform child in cardParent.transform){
      if (child != null) GameObject.Destroy(child.gameObject);
    }

    //cardParent = new GameObject("Card Parent");
    //cardParent.transform.SetParent(handLayout, false); 
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

  public bool CheckCardDrawn(){
    return next_card_playable_only_;
  }

  public void SetUnoButton(bool val){
    uno_button_.gameObject.SetActive(val);
  }


  public void SetCatchButton(bool val){
    catch_button_.gameObject.SetActive(val);
  }

  public void SetChallengeButtons(bool val){
    challenge_button_.gameObject.SetActive(val);
    decline_button_.gameObject.SetActive(val);
  }





  public void CallUno(){
    gameManager.CallUno(playerID);
  }

  public void CatchPlayer(){
    gameManager.CatchPlayer(playerID);
  }

  public void AcceptChallenge(){
    gameManager.AcceptChallenge();
  }

  public void DeclineChallenge(){
    gameManager.DeclineChallenge();
  }


}




