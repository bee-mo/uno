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

  public void NextActivePlayer(){
    active_player_ = GetNextActivePlayer();//mod ((active_player_ + direction_), (player_hands_.Count));


    active_player_text_.text = "Active Player ID: " + player_hands_.Keys.ToArray()[active_player_].ToString();
    if (active_player_ != main_player_id_){ //show the hand
      SetActiveEnemy(active_player_);
    }

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
    Debug.Assert(table_deck_);

    active_enemy_text_ = GameObject.Find("Active Enemy Text").transform.GetComponent<TMP_Text>();
    active_player_text_ = GameObject.Find("Active Player Text").transform.GetComponent<TMP_Text>();
    play_pile_.GetComponent<PlayPile>().DrawFromDeck();


    CreateMainPlayer(main_player_id_);


    int enemy_count = 5;
    for (int i = 1; i <= enemy_count; ++i) {
      if (active_enemy_ == -1) active_enemy_ = i;
      CreateEnemyPlayer(i);
    }
    active_enemy_text_.text =  "Current Active Enemy: " + active_enemy_.ToString();
    active_player_text_.text =  "Active Player ID: " + player_hands_.Keys.ToArray()[active_player_].ToString();

    UpdatePlayerCountInfo();
  }

  // Update is called once per frame
  void Update() {

    if (initial_draw_){

      if (player_hands_[main_player_id_].GetComponent<HandController>().GetCardCount() < initial_hand_size_){
        DrawMainPlayerCard();
      } else if (player_hands_[active_enemy_].GetComponent<HandController>().GetCardCount() < initial_hand_size_){
        DrawEnemyPlayerCard();
      } else {
        NextEnemy();
        if (player_hands_[active_enemy_].GetComponent<HandController>().GetCardCount() >= initial_hand_size_) initial_draw_ = false;

      }

    }

    if (cardsToDraw > 0 && drawingPlayer >= 0 && !table_deck_.GetComponent<GameDeck>().CheckCardDrawInProgess()){

      DrawToHand(drawingPlayer);
      cardsToDraw+= -1;

      if (cardsToDraw <= 0){
        drawingPlayer = -1;
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

  public void DrawMainPlayerCard() {
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

      deck.DrawMainPlayerCard(hand);

      if (!initial_draw_) NextActivePlayer();
    
  } else {
      Debug.Log("Not your turn");
    }

  }

  public void DrawEnemyPlayerCard() {
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
      deck.DrawEnemyPlayerCard(hand);
      if (!initial_draw_) NextActivePlayer();
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
    new_enemy_hand.transform.localScale = new Vector3(500.0f, 500.0f, 1.0f);

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
}
