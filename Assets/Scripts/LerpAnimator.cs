using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpAnimator : MonoBehaviour {

  private GameObject effected_;
  private float lerp_speed_ = 1.0f;
  private Vector3 start_pos_;
  private Vector3 end_pos_;
  private bool lerping_;
  private float lerp_ = 0.0f;

  // Update is called once per frame
  void Update() {
    if (!effected_) return;
    if (!lerping_) return;

    effected_.transform.position = Vector3.Lerp(start_pos_, end_pos_, lerp_);
    lerp_ = Mathf.Clamp(lerp_ + Time.deltaTime * lerp_speed_, 0.0f, 1.0f);
    if (lerp_ == 1.0f) {
      lerping_ = false;
    }
  }

  public void SetEffected(GameObject obj) {
    effected_ = obj;
  }

  public void LerpTo(Vector3 destionation, float speed = 1.0f) {
    start_pos_ = effected_.transform.position;
    end_pos_ = destionation;
    lerp_speed_ = speed;
    lerping_ = true;
    lerp_ = 0.0f;
  }
}
