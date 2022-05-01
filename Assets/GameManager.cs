using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

  public GameObject enemy_hand_prefab_;

  // Store the player id and their associated hands' game objects
  private Dictionary<int, GameObject> player_hands_;
  private GameObject table_deck_;
  private GameObject enemy_row_;
  private Dictionary<int, int> enemy_id_to_index_;
  private int active_enemy_ = -1;

  // placeholder ids
  private int main_player_id_ = 0;
  private int enemy_player_id_ = 1;

  private LerpAnimator enemy_hands_lerper_;

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
    Debug.Assert(table_deck_);

    CreateMainPlayer(main_player_id_);

    int enemy_count = 5;
    for (int i = 1; i <= enemy_count; ++i) {
      if (active_enemy_ == -1) active_enemy_ = i;
      CreateEnemyPlayer(i);
    }

    UpdatePlayerCountInfo();
  }

  // Update is called once per frame
  void Update() {

  }

  public void NextEnemy() {
    int next_enemy = active_enemy_ + 1;
    if (!enemy_id_to_index_.ContainsKey(next_enemy)) {
      next_enemy = 1;
    }
    SetActiveEnemy(next_enemy);
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

    deck.DrawMainPlayerCard(hand);
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

    deck.DrawEnemyPlayerCard(hand);
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
