using GameCreator.Characters;
using GameCreator.Variables;
using StrongSideAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using static System.TimeZoneInfo;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] TMP_Text playerAlias;
    public List<string> playerStances = new List<string>();
    public string defaultStance = "";
    public static event Action<string, string> OnPlayerClicked;
    public static event Action<PlayerItem> OnPlayerSelected;
    public CharacterState currentStance;
    public Character character;
    public int index;
    bool changeStance = false;
    public List<CharacterState> characterStances = new List<CharacterState>();
    public OffensePersonnel offensePersonnel;


    private void Awake()
    {
        character = GetComponent<Character>();
    }
    public void SetPersonnel(OffensePersonnel personnel)
    {
        offensePersonnel = personnel;
       // offensePersonnel.positionIndex = offensePersonnel.positionIndex - 1;
        SetPlayerAlias(offensePersonnel.positionAlias);
    }

    public void SetPlayerAlias(string alias)
    {
        playerAlias.text = alias;
    }

    public void SetPlayerStances(List<string> stances, string defaultStance, int playerIndex)
    {
        index = playerIndex;
        this.defaultStance = defaultStance;
        foreach (var item in stances)
        {
            playerStances.Add(item);
        }
        FindAndSetStance(this.defaultStance);
    }

    private void FindAndSetStance(string stanceToChange)
    {
        character.enabled = true;
        var stance = characterStances.Find(sName => sName.name == stanceToChange);
        if (stance != null)
        {
            print("setting stancce " + stance);
            ChangePlayerState(stance);
        }
            
    }

    private void OnEnable()
    {
        AppManager.OnPlayerPosAliasStatus += AliasStatusActivation;
        AppManager.OnPlayerStanceStatus += StanceStatusActivation;
        AppManager.OnChosenPlayerStance += SetNewStance;
    }

    private void StanceStatusActivation(bool status)
    {
        playerAlias.text = !status ? string.Format("{0} \n {1}", offensePersonnel.positionAlias, defaultStance) : offensePersonnel.positionAlias;
    }

    private void SetNewStance(string obj, PlayerItem currentItem)
    {
        if (this.offensePersonnel.id == currentItem.offensePersonnel.id)
        {
            defaultStance = obj;
            playerAlias.text = string.Format("{0} \n {1}", offensePersonnel.positionAlias, defaultStance);
            FindAndSetStance(defaultStance);
        }
           
    }
    public void ChangePlayerState(CharacterState stateClip, AvatarMask avatarMask = null,
           float weight = 1f, float transitionTime = 0.25f, NumberProperty speed = null)
    {
        if (speed == null)
            speed = new NumberProperty(1.0f);
        if (character == null)
            character = GetComponent<Character>();

        if (character != null && stateClip != null)
            character.GetCharacterAnimator().SetState(
                stateClip,
                avatarMask,
                weight,
                transitionTime,
                speed.GetValue(character.gameObject),
                CharacterAnimation.Layer.Layer1);
    }

    private void OnMouseDown()
    {
        character.enabled = false;
        OnPlayerClicked?.Invoke(defaultStance, offensePersonnel.positionAlias);
       var c =  string.Format("{0} \n {1}", offensePersonnel.positionAlias, defaultStance);
        OnPlayerSelected?.Invoke(this);
    }

    private void OnMouseUp()
    {
        character.enabled = true;
    }


    public void AliasStatusActivation(bool status)
    {
        playerAlias.gameObject.SetActive(status);
    }
}
