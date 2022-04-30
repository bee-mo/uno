using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

  // Store the player id and their associated hands' game objects
  private Dictionary<int, GameObject> player_hands_;
  private GameObject table_deck_;

  // placeholder ids
  private int main_player_id_ = 0;
  private int enemy_player_id_ = 1;

  // Start is called before the first frame update
  void Start() {
    player_hands_ = new Dictionary<int, GameObject>();

    CreateMainPlayer(main_player_id_);
    CreateEnemyPlayer(enemy_player_id_);

    table_deck_ = GameObject.Find("GameDeck");
    Debug.Assert(table_deck_);
  }

  // Update is called once per frame
  void Update() {

  }

  public void DrawMainPlayerCard() {
    GameObject main_go = player_hands_[main_player_id_];
    Debug.Assert(main_go);
    Debug.Assert(table_deck_);

    GameDeck deck = table_deck_.GetComponent<GameDeck>();
    HandController hand = main_go.GetComponent<HandController>();
    Debug.Assert(hand);

    deck.DrawMainPlayerCard(hand);
  }

  public void DrawEnemyPlayerCard() {
    GameObject enemy_go = player_hands_[enemy_player_id_];
    Debug.Assert(enemy_go);
    Debug.Assert(table_deck_);

    GameDeck deck = table_deck_.GetComponent<GameDeck>();
    HandController hand = enemy_go.GetComponent<HandController>();
    Debug.Assert(hand);

    deck.DrawEnemyPlayerCard(hand);
  }

  private void CreateMainPlayer(int id) {
    if (player_hands_.ContainsKey(id)) return;
    var main_go = GameObject.Find("Hand Layout");
    Debug.Assert(main_go);

    HandController hand = main_go.GetComponent<HandController>();
    hand.SetAsMainPlayer();

    player_hands_.Add(id, main_go);
  }

  private void CreateEnemyPlayer(int id) {
    if (player_hands_.ContainsKey(id)) return;
    var enemy_go = GameObject.Find("Enemy Hand Layout");
    Debug.Assert(enemy_go);

    HandController hand = enemy_go.GetComponent<HandController>();
    hand.SetAsEnemy();

    player_hands_.Add(id, enemy_go);
  }

  static GameManager GetSingleton() {
    var gm_go = GameObject.Find("Game Manager");
    Debug.Assert(gm_go);
    GameManager manager = gm_go.GetComponent<GameManager>();
    Debug.Assert(manager);
    return manager;
  }
}
