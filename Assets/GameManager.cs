using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour {

  public GameObject enemy_hand_prefab_;

  // Store the player id and their associated hands' game objects
  private Dictionary<int, GameObject> player_hands_;


  private GameObject table_deck_;

  private GameObject play_pile_;

  private GameObject restart_button_;

  private GameObject enemy_row_;
  private Dictionary<int, int> enemy_id_to_index_;
  private int active_enemy_ = -1;


  // placeholder ids
  private int main_player_id_ = 0;
  private int enemy_player_id_ = 1;

  private LerpAnimator enemy_hands_lerper_;

  private int active_player_ = 0;
  private int direction_ = 1;
  private TMP_Text active_enemy_text_; 
  private TMP_Text active_player_text_; 

  private bool initial_draw_ = true;
  private int initial_hand_size_ = 3;

  private int cardsToDraw = 0;
  private int drawingPlayer = -1;

  private Button player_draw_button_;
  private Button enemy_draw_button_;

  private int uno_preparation_ = -1;
  private int uno_catch_ = -1;

  public bool show_hands_ = false;

  [SerializeField] private int catch_draw_count_ = 4; 

  private int challenged_player_ = -1;
  private int challenging_player_ = -1;
  private Card challenge_top_card_;

  private bool challenge_active_ = false;

  // Start is called before the first frame update
  void Start() {
    enemy_id_to_index_ = new Dictionary<int, int>();
    Debug.Assert(enemy_hand_prefab_);

    enemy_row_ = GameObject.Find("Enemy Hand Layout Row");
    Debug.Assert(enemy_row_);
    enemy_hands_lerper_ = gameObject.AddComponent<LerpAnimator>();
    enemy_hands_lerper_.SetEffected(enemy_row_);

    player_hands_ = new Dictionary<int, GameObject>();

    table_deck_ = GameObject.Find("GameDeck");
    play_pile_ = GameObject.Find("PlayPile");
    restart_button_ = GameObject.Find("Restart Button");
    Debug.Assert(table_deck_);

    active_enemy_text_ = GameObject.Find("Active Enemy Text").transform.GetComponent<TMP_Text>();
    active_player_text_ = GameObject.Find("Active Player Text").transform.GetComponent<TMP_Text>();
    //play_pile_.GetComponent<PlayPile>().DrawFromDeck();

    player_draw_button_ = GameObject.Find("Draw Card").transform.GetComponent<Button>();
    enemy_draw_button_ = GameObject.Find("Enemy Draw Card").transform.GetComponent<Button>();


    CreateMainPlayer(main_player_id_);


    int enemy_count = 3;
    for (int i = 1; i <= enemy_count; ++i) {
      if (active_enemy_ == -1) active_enemy_ = i;
      CreateEnemyPlayer(i);
    }
    active_enemy_text_.text =  "Current Active Enemy: " + active_enemy_.ToString();
    active_player_text_.text =  "Active Player ID: " + player_hands_.Keys.ToArray()[active_player_].ToString();

    UpdatePlayerCountInfo();
    ResetGame();
  }

  // Update is called once per frame
  void Update() {

    if (initial_draw_){

      if (player_hands_[main_player_id_].GetComponent<HandController>().GetCardCount() < initial_hand_size_){
        DrawMainPlayerCard(false);
      } else if (player_hands_[active_enemy_].GetComponent<HandController>().GetCardCount() < initial_hand_size_){
        DrawEnemyPlayerCard(false);
      } else {
        NextEnemy();
        if (player_hands_[active_enemy_].GetComponent<HandController>().GetCardCount() >= initial_hand_size_){
          initial_draw_ = false;
          SetUnoElements();
        }
      }

    }

    if (cardsToDraw > 0 && drawingPlayer >= 0 && !table_deck_.GetComponent<GameDeck>().CheckCardDrawInProgess()){

      DrawToHand(drawingPlayer);
      cardsToDraw+= -1;

      if (cardsToDraw <= 0){
        drawingPlayer = -1;

        if (active_player_ != main_player_id_){
          SetActiveEnemy(active_player_);
        }
        
      }

    }


  }

  public void NextEnemy() {
    int next_enemy = active_enemy_ + 1;
    if (!enemy_id_to_index_.ContainsKey(next_enemy)) {
      next_enemy = 1;
    }
    SetActiveEnemy(next_enemy);
  }


  public void DrawToHand(int handIndex){
    if (!CardGenerator.GetSingleton().HasCardsLeft()) {
      Debug.Log("No more cards to play");
    }

    GameObject hand_go = player_hands_[handIndex];

    GameDeck deck = table_deck_.GetComponent<GameDeck>();
    HandController hand = hand_go.GetComponent<HandController>();
    

    if (hand.IsMainPlayer()){
      deck.DrawMainPlayerCard(hand);  
    } else {
      deck.DrawEnemyPlayerCard(hand);  
    }
    
  }

  public void DrawMainPlayerCard(bool playable) {
    if (!CardGenerator.GetSingleton().HasCardsLeft()) {
      Debug.Log("No more cards to play");
    }

    GameObject main_go = player_hands_[main_player_id_];
    Debug.Assert(main_go);
    Debug.Assert(table_deck_);

    GameDeck deck = table_deck_.GetComponent<GameDeck>();
    HandController hand = main_go.GetComponent<HandController>();
    Debug.Assert(hand);

    if (player_hands_.Keys.ToArray()[active_player_] == main_player_id_ || initial_draw_){
      if (playable){
        hand.NextCardPlayable();
        SetDrawButtons(false);
      }

      deck.DrawMainPlayerCard(hand);



      //if (!initial_draw_) NextActivePlayer();


  } else {
      Debug.Log("Not your turn");
    }

  }

  public void DrawEnemyPlayerCard(bool playable) {
    if (!CardGenerator.GetSingleton().HasCardsLeft()) {
      Debug.Log("No more cards to play");
    }

    GameObject enemy_go = player_hands_[active_enemy_];
    Debug.Assert(enemy_go);
    Debug.Assert(table_deck_);

    GameDeck deck = table_deck_.GetComponent<GameDeck>();
    HandController hand = enemy_go.GetComponent<HandController>();
    Debug.Assert(hand);


    if (player_hands_.Keys.ToArray()[active_player_] == active_enemy_ || initial_draw_){
      
      if (playable){
        hand.NextCardPlayable();
        SetDrawButtons(false);
      }
      deck.DrawEnemyPlayerCard(hand);
      //if (!initial_draw_) NextActivePlayer();

    } else {
      Debug.Log("Not the active player's turn");
    }

    ///deck.DrawEnemyPlayerCard(hand);
  }

  private void SetActiveEnemy(int next_enemy_id) {
    Debug.Assert(enemy_id_to_index_.ContainsKey(next_enemy_id));
    Debug.Assert(player_hands_.ContainsKey(next_enemy_id));

    GameObject hand_go = player_hands_[next_enemy_id];

    enemy_hands_lerper_.LerpTo(new Vector3(
      enemy_row_.transform.position.x - hand_go.transform.position.x,
      enemy_row_.transform.position.y,
      enemy_row_.transform.position.z
    ), 3.0f);

    active_enemy_ = next_enemy_id;
    active_enemy_text_.text = "Current Active Enemy: " + active_enemy_.ToString();
  }

  private void CreateMainPlayer(int id) {
    if (player_hands_.ContainsKey(id)) return;
    var main_go = GameObject.Find("Hand Layout");
    Debug.Assert(main_go);

    HandController hand = main_go.GetComponent<HandController>();
    hand.SetAsMainPlayer();
    hand.SetPlayerID(id);
    player_hands_.Add(id, main_go);

  }

  private void CreateEnemyPlayer(int id) {
    if (player_hands_.ContainsKey(id)) return;

    var new_enemy_hand = Instantiate(enemy_hand_prefab_);
    Debug.Assert(new_enemy_hand);

    new_enemy_hand.SetActive(true);
    new_enemy_hand.name = "Enemy Hand #" + id;

    int enemy_index = enemy_id_to_index_.Count;
    float enemy_space_interval = Screen.width;

    new_enemy_hand.transform.SetParent(enemy_row_.transform);
    new_enemy_hand.transform.localPosition = new Vector3(enemy_space_interval * enemy_index, 0, -200.0f);
    new_enemy_hand.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

    // var enemy_go = GameObject.Find("Enemy Hand Layout");
    // Debug.Assert(enemy_go);

    HandController hand = new_enemy_hand.GetComponent<HandController>();
    hand.SetAsEnemy();
    hand.SetPlayerID(id);

    player_hands_.Add(id, new_enemy_hand);
    enemy_id_to_index_.Add(id, enemy_index);
  }

  private void UpdatePlayerCountInfo() {

    var txt = GameObject.Find("Player Count Text").GetComponent<TMPro.TextMeshProUGUI>();
    Debug.Assert(txt);
    txt.text = player_hands_.Count + " Players";
  }

  static GameManager GetSingleton() {
    var gm_go = GameObject.Find("Game Manager");
    Debug.Assert(gm_go);
    GameManager manager = gm_go.GetComponent<GameManager>();
    Debug.Assert(manager);
    return manager;
  }

  public void NextActivePlayer(){
    active_player_ = GetNextActivePlayer();//mod ((active_player_ + direction_), (player_hands_.Count));


    active_player_text_.text = "Active Player ID: " + player_hands_.Keys.ToArray()[active_player_].ToString();
    if (active_player_ != main_player_id_){ //show the hand
      SetActiveEnemy(active_player_);
    }

    SetDrawButtons(true);

  }

  public int GetNextActivePlayer(){
    return mod((active_player_ + direction_), (player_hands_.Count));
  }
  int mod(int x, int m) {
    return (x%m + m)%m;
  }

  public void ReverseDirection(){
    direction_ = -direction_;
  }

  public int GetActivePlayerID(){
    return player_hands_.Keys.ToArray()[active_player_];
  }

  public void ForceDraw(int count, int targetHand){
    cardsToDraw = count;

    drawingPlayer = targetHand;
    if (targetHand != main_player_id_){ //show the hand
      SetActiveEnemy(targetHand);
    }
  }

  public void ResetGame(){


    CardGenerator.GetSingleton().ResetDeck(); 
    PlayPile playPileComponent = play_pile_.GetComponent<PlayPile>();

    playPileComponent.ResetPlayPile();
    playPileComponent.ResetTopCard();
    playPileComponent.DrawFromDeck();

    for (int i = 0; i < player_hands_.Count; i++){
      player_hands_ [player_hands_.Keys.ToArray()[i]].GetComponent<HandController>().ClearHand();
  
    }

    initial_draw_ = true;
    direction_ = 1;
    active_player_ = 0;
    active_enemy_ = 1;

    SetRestartButton(false);


  }

  public void SetRestartButton(bool val){
    restart_button_.SetActive(val);
  }

  public void PassActivePlayerAction(){
    GameObject hand_go = player_hands_[active_player_];
    HandController hand = hand_go.GetComponent<HandController>();

    hand.PassAction();

    NextActivePlayer();
    SetUnoElements();

  }

  private void SetDrawButtons(bool val){

    player_draw_button_.interactable = val;
    enemy_draw_button_.interactable = val;

  }


  public void SetUnoElements(){

    if (uno_preparation_ < 0){ //Previous player was not able to enter uno OR, they cleared it
      uno_catch_ = -1;
    } else if (player_hands_ [player_hands_.Keys.ToArray()[uno_preparation_]].GetComponent<HandController>().GetCardCount() == 1){
      uno_catch_ = uno_preparation_;
    } else {
      uno_catch_ = -1;
    }

    int nextPlayer = active_player_;//GetNextActivePlayer(); //unless this was activated by a draw?

    GameObject hand_go = player_hands_[nextPlayer];
    HandController hand = hand_go.GetComponent<HandController>();

    //Check for playable uno position and a challenge is not occuring (they will become prep after the challenge)
    if (hand.GetCardCount() == 2 && hand.HandHasPlayable() && challenging_player_ < 0 && challenged_player_ < 0) { 
      uno_preparation_ = nextPlayer;
    } else {
      uno_preparation_ = -1;
    }

    UpdateUnoElements();





  }

  public void UpdateUnoElements(){

    for (int i = 0; i < player_hands_.Count; i++){

      if (uno_catch_ >= 0 && uno_catch_ != i){
        player_hands_ [player_hands_.Keys.ToArray()[i]].GetComponent<HandController>().SetCatchButton(true);
      } else {
        player_hands_ [player_hands_.Keys.ToArray()[i]].GetComponent<HandController>().SetCatchButton(false);
      }


      if (i == uno_catch_ || i == uno_preparation_){
        player_hands_ [player_hands_.Keys.ToArray()[i]].GetComponent<HandController>().SetUnoButton(true);
      } else {
        player_hands_ [player_hands_.Keys.ToArray()[i]].GetComponent<HandController>().SetUnoButton(false);
      }

  
    }

  }

  public void CallUno(int callingPlayer){

    //If the player who called uno is the catchable player, then the uno_prep is not cleared (the prep should still have the option to call uno)
    if (callingPlayer != uno_catch_)
    {
      uno_preparation_ = -1;
    }
    



    //if uno is called by anyone, then the current catchable player is cleared
    uno_catch_ = -1;

    UpdateUnoElements();

  }

  public void CatchPlayer(int catchingPlayer){

    //If someone catches themselves somehow return; 
    if (catchingPlayer == uno_catch_) return; 
 
    ForceDraw(catch_draw_count_, uno_catch_);

    //The caught player is no longer catchable
    uno_catch_ = -1;



    UpdateUnoElements();

  }

  public void StartChallenge(int challengable, int challenger, Card topCard){

    challenging_player_ = challenger;

    challenged_player_ = challengable;

    challenge_top_card_ = topCard;
    challenge_active_ = true;
    player_hands_ [player_hands_.Keys.ToArray()[challenger]].GetComponent<HandController>().SetChallengeButtons(true);

  }


  public void AcceptChallenge(){


    if (player_hands_ [player_hands_.Keys.ToArray()[challenged_player_]].GetComponent<HandController>().HandHasMatchingColor(challenge_top_card_)){
      ForceDraw(4, challenged_player_);
    } else {
      ForceDraw(6, challenging_player_);
    }

    player_hands_ [player_hands_.Keys.ToArray()[challenging_player_]].GetComponent<HandController>().SetChallengeButtons(false);
    uno_catch_ = -1;
    challenged_player_ = -1;
    challenging_player_ = -1;
    challenge_top_card_ = null;
    challenge_active_ = false; 
    NextActivePlayer();
    SetUnoElements();
  }


  public void DeclineChallenge(){
    ForceDraw(4, challenging_player_);


    player_hands_ [player_hands_.Keys.ToArray()[challenging_player_]].GetComponent<HandController>().SetChallengeButtons(false);

    uno_catch_ = -1;
    challenged_player_ = -1;
    challenging_player_ = -1;
    challenge_top_card_ = null;
    challenge_active_ = false;
    NextActivePlayer();
    SetUnoElements();
  }

  public bool ChallengeActive(){
    return challenge_active_;
  }

}
