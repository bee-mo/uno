using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayPile : MonoBehaviour
{
    private Card topCard; //the card that is checked against for valid plays
    public GameObject cardPrefab;

    private Transform cardColorText;   
    private GameObject colorSelectionBox;

    void Awake(){
        cardColorText = GameObject.Find("Card Color Text").transform;
        colorSelectionBox = GameObject.Find("Color Selection");
        colorSelectionBox.SetActive(false);
    }


    public void DrawFromDeck(){
        GameObject drawnCard = Instantiate(cardPrefab, transform);
        var card = drawnCard.transform.GetComponent<Card>();

        Transform gameDeck = GameObject.Find("GameDeck").transform;
        Vector3 gameDeckPosition = gameDeck.position;

        Vector3 gt = GameObject.Find("GameTable").transform.localScale;
        Vector3 gd = gameDeck.localScale;
        
        //position the new card's initial position to the top of the deck
        float deckOffset = (gameDeck.GetComponent<GameDeck>().GetDeckOffset() * 0.01f + drawnCard.GetComponent<RectTransform>().rect.height / 2.0f) * gd.y;
        drawnCard.transform.position = new Vector3(gameDeckPosition.x, gameDeckPosition.y + deckOffset, -2.0f);

        //Initial scale matches the top card on the deck
        drawnCard.transform.localScale = new Vector3(gd.x / gt.x, gd.y / gt.y, gd.z / gt.z);
          

        Vector3 newPos = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), -1.0f);
        Vector3 newRot = new Vector3(0.0f, 0.0f, Random.Range(-45.0f, 45.0f));
        card.SetPlayCardPositions(newPos, newRot);
        card.SetPlayCard(true);
        card.FlipCard(Card.CardPosition.FRONT);
        SetTopCard(card);
        
    }

    public void SetTopCard(Card newCard){
        topCard = newCard;

        CardGenerator.CardInfo newInfo = newCard.GetCardInfo();
        if (newInfo.cardType.ToString() != "WILD_DRAW_4" && newInfo.cardType.ToString() != "WILD"){
            //cardColorText = GameObject.Find("Card Color Text").transform;
            TMP_Text currentColorText = cardColorText.GetComponent<TMP_Text>();
            currentColorText.text = newInfo.cardColor.ToString();
        } else {
            colorSelectionBox.SetActive(true);
        }
    }

    public Card GetTopCard(){
        return topCard;
    }

    public void SetColor(int newColor){

        CardGenerator.CardColor colorToSet = (CardGenerator.CardColor)newColor;

        topCard.GetCardInfo().cardColor = colorToSet;

        TMP_Text currentColorText = cardColorText.GetComponent<TMP_Text>();
        currentColorText.text = topCard.GetCardInfo().cardColor.ToString();
        colorSelectionBox.SetActive(false);

    }

}
